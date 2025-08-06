using MCPSDK.API.Commands;
using MCPSDK.Models.JsonRPC;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Core;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace MCPSDK
{
    public abstract class SocketServiceBase<T>
    {
        private TcpListener _listener;
        private Thread? _listenerThread;
        private bool _isRunning;
        private int _port;
        private ICommandRegistry _commandRegistry;
        private CommandExecutor _commandExecutor;

        private ILogger _logger;

        private T _app;

        public bool IsRunning => _isRunning;

        public ICommandRegistry CommandRegistry => _commandRegistry;   

        public int Port
        {
            get => _port;
            set => _port = value;
        }

        public virtual void Initialize(ILogger<Logger> logger,int port,T app)
        {
            _app = app;
            _port = port;
            _logger = logger;
            _commandRegistry = new CommandRegistry();

            _commandExecutor = new CommandExecutor(_commandRegistry, _logger);
        }

        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();

                _listenerThread = new Thread(ListenForClients)
                {
                    IsBackground = true
                };
                _listenerThread.Start();

                _logger.LogInformation($"Socket service initialized on port {_port}");
            }
            catch (Exception)
            {
                _isRunning = false;
            }
        }

        private void ListenForClients()
        {
            try
            {
                while (_isRunning)
                {
                    TcpClient client = _listener.AcceptTcpClient();

                    Thread clientThread = new Thread(HandleClientCommunication)
                    {
                        IsBackground = true
                    };
                    clientThread.Start(client);
                }
            }
            catch (SocketException)
            {

            }
            catch (Exception)
            {
                // log
            }
        }

        private void HandleClientCommunication(object? clientObj)
        {
            if (clientObj == null) return;

            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream stream = tcpClient.GetStream();

            try
            {
                byte[] buffer = new byte[8192];

                while (_isRunning && tcpClient.Connected)
                {
                    int bytesRead = 0;

                    try
                    {
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                    }
                    catch (IOException)
                    {

                        break;
                    }

                    if (bytesRead == 0)
                    {

                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    System.Diagnostics.Trace.WriteLine($"message on TCP: {message}");

                    string response = ProcessJsonRPCRequest(message);

                    // 发送响应
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            catch (Exception)
            {
                // log
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private string ProcessJsonRPCRequest(string requestJson)
        {
            JsonRPCRequest? request;

            try
            {

                request = JsonConvert.DeserializeObject<JsonRPCRequest>(requestJson);

                if (request == null || !request.IsValid())
                {
                    return JsonRPCErrorResponse.CreateErrorResponse(
                        null,
                        JsonRPCErrorCodes.InvalidRequest,
                        "Invalid JSON-RPC request"
                    );
                }

                var result = _commandExecutor.ExecuteCommand(request);

                return result;

            }
            catch (Exception ex)
            {

                return JsonRPCErrorResponse.CreateErrorResponse(null,
                    JsonRPCErrorCodes.InternalError,
                    $"Internal error: ${ex.Message}");
            }

        }

    }
}
