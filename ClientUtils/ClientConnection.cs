
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MCPSDK.ClientUtils
{
    public class ClientConnection : IDisposable
    {

        private readonly string host;
        private readonly int port;
        private TcpClient tcpClient;
        private NetworkStream stream;
        private readonly byte[] buffer = new byte[8192];
        private readonly StringBuilder receiveBuffer = new StringBuilder();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> responseCallbacks
            = new();

        private bool isReading;

        public bool IsConnected => tcpClient?.Connected ?? false;


        public ClientConnection(string host, int port)
        {
            this.host = host;
            this.port = port;
        }


        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            stream = tcpClient.GetStream();

            if (!isReading)
            {
                isReading = true;
                _ = Task.Run(() => ReadLoopAsync()); // start receiving
            }
        }

        public void Disconnect()
        {
            tcpClient?.Close();
            stream?.Close();
        }


        public void Dispose()
        {
            Disconnect();
        }


        private async Task ReadLoopAsync()
        {
            try
            {
                while (IsConnected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        receiveBuffer.Append(data);

                        // Try parsing complete JSON response
                        ProcessBuffer();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket read error: " + ex.Message);
            }
        }


        private void ProcessBuffer()
        {
            try
            {
                var data = receiveBuffer.ToString();

                // Try parse complete JSON (naive assumption: whole JSON object comes at once)
                var json = JsonDocument.Parse(data);
                HandleResponse(data);

                receiveBuffer.Clear(); // Clear buffer on success
            }
            catch (JsonException)
            {
                // Incomplete message, wait for more data
            }
        }


        private void HandleResponse(string jsonString)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("id", out var idElement))
                {
                    var requestId = idElement.GetString();
                    if (requestId != null && responseCallbacks.TryRemove(requestId, out var tcs))
                    {
                        tcs.SetResult(jsonString);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to handle response: " + ex.Message);
            }
        }


        private string GenerateRequestId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        public async Task<JsonElement> SendCommandAsync(string method, object parameters)
        {
            await ConnectAsync();

            var requestId = GenerateRequestId();

            var request = new
            {
                jsonrpc = "2.0",
                method,
                @params = parameters,
                id = requestId
            };

            string json = JsonSerializer.Serialize(request);
            byte[] bytes = Encoding.UTF8.GetBytes(json + "\n"); // optional newline

            var tcs = new TaskCompletionSource<string>();
            responseCallbacks[requestId] = tcs;

            await stream.WriteAsync(bytes, 0, bytes.Length);

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            using (cts.Token.Register(() =>
            {
                if (responseCallbacks.TryRemove(requestId, out var callback))
                {
                    callback.SetException(new TimeoutException($"Command '{method}' timed out."));
                }
            }))
            {
                string result = await tcs.Task;
                var jsonResponse = JsonDocument.Parse(result).RootElement;

                if (jsonResponse.TryGetProperty("error", out var error))
                {
                    throw new Exception(error.GetProperty("message").GetString());
                }

                return jsonResponse.GetProperty("result");
            }
        }

    }
}
