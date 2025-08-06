

namespace MCPSDK.ClientUtils
{
    public static class ConnectionUtils
    {
        public static async Task<T> WithClientConnectionAsync<T>(Func<ClientConnection, Task<T>> operation,
            string serverUrl = "localhost", int port = 8080)
        {
            var client = new ClientConnection(serverUrl, port);

            try
            {
                if (!client.IsConnected) { 
                
                    await client.ConnectAsync();

                    // Optionally wait for a successful connection
                    var start = DateTime.UtcNow;
                    while (!client.IsConnected)
                    {
                        await Task.Delay(50);
                        if ((DateTime.UtcNow - start).TotalSeconds > 5)
                        {
                            throw new TimeoutException("连接到Revit客户端失败 (Timeout after 5s)");
                        }
                    }
                }

                // Perform the user-defined operation
                return await operation(client);
            }
            finally
            {

                // Always disconnect afterwards
                client.Disconnect();
            }
        }
    }
}
