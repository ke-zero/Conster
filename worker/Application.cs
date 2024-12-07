using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Byter;
using Conster.Core;
using Netly;
using Netly.Interfaces;

namespace Conster.Worker;

public static class Application
{
    private const string HEADER_TOKEN = "TOKEN";
    private static readonly HTTP.Server _server = new();

    private static readonly Dictionary<string, ConnectionData> Connections = new();

    private static string GetHeaderValue(in IHTTP.ServerRequest request)
    {
        var value = request.Headers.FirstOrDefault(x => x.Key.Equals(HEADER_TOKEN, StringComparison.OrdinalIgnoreCase));

        return value.Value ?? string.Empty;
    }


    public static void Initialize()
    {
        StringExtension.Default = Encoding.UTF8;

        _server.On.Open(() => Console.WriteLine($"Server Started At: {_server.Host}"));
        _server.On.Error(e => Console.WriteLine($"Server Error: {e}"));
        _server.On.Close(() =>
        {
            Console.WriteLine($"Server Closed At: {_server.Host}");
            Connections.Clear();
        });

        // Check Auth Token
        _server.Middleware.Add((request, response, next) =>
        {
            if (!request.IsWebSocket) response.Headers["Content-Type"] = "application/json";

            // close all connection that not provide token
            if (string.IsNullOrWhiteSpace(GetHeaderValue(request)))
            {
                response.Send((int)HttpStatusCode.Unauthorized);
                return;
            }

            next();
        });

        _server.Map.WebSocket("/lobby", (request, socket) =>
        {
            var token = GetHeaderValue(request);

            if (token.Equals(Config.API_KEY))
            {
                HandleMasterSocket(request, socket);
                return;
            }

            if (JWTSolver.Solve(token, Config.API_KEY, out LobbyToken? lobbyToken) && lobbyToken != null &&
                lobbyToken.IsValid())
            {
                HandleClientSocket(request, socket, lobbyToken);
                return;
            }

            Console.WriteLine("Error on websocket Connection");
            // Invalid Token.
            socket.To.Close(WebSocketCloseStatus.PolicyViolation);
        });

        _server.Map.Post("/lobby/status", (request, response) =>
        {
            if (!GetHeaderValue(request).Equals(Config.API_KEY))
            {
                response.Send((int)HttpStatusCode.Unauthorized);
                return;
            }

            WorkerClientIDParametersRequest? requestData;

            try
            {
                requestData = JsonSerializer.Deserialize<WorkerClientIDParametersRequest>(request.Body.Text);
            }
            catch
            {
                requestData = new WorkerClientIDParametersRequest();
            }

            var responseData = new WorkerClientStatusResponse();

            if (requestData is { IDs.Count: > 0 })
                foreach (var id in requestData.IDs)
                {
                    var data = Connections.FirstOrDefault(x => x.Id == id && !x.IsMaster);
                    var success = Connections.TryGetValue(id, out var data);

                    responseData.Data.Add(new WorkerClientStatusResponse.StatusData
                    {
                        Id = id,
                        Success = success,
                        At = data?.CreatedAt ?? DateTime.UtcNow
                    });
                }

            response.Send((int)HttpStatusCode.OK, JsonSerializer.Serialize(responseData));
        });

        _server.Map.Post("/lobby/disconnect", (request, response) =>
        {
            if (!GetHeaderValue(request).Equals(Config.API_KEY))
            {
                response.Send((int)HttpStatusCode.Unauthorized);
                return;
            }

            WorkerClientIDParametersRequest requestData;

            try
            {
                requestData = JsonSerializer.Deserialize<WorkerClientIDParametersRequest>(request.Body.Text) ??
                              new WorkerClientIDParametersRequest();
            }
            catch
            {
                requestData = new WorkerClientIDParametersRequest();
            }

            var responseData = new WorkerClientSuccessStatusResponse();

            foreach (var id in requestData.IDs)
            {
                var success = Connections.TryGetValue(id, out var data);

                responseData.Data.Add(new WorkerClientSuccessStatusResponse.SuccessStatusData
                {
                    Id = id,
                    Success = success
                });

                if (success) data?.Socket.To.Close();
            }

            response.Send((int)HttpStatusCode.OK, JsonSerializer.Serialize(responseData));
        });

        _server.Map.Post("/lobby/message", (request, response) =>
        {
            if (!GetHeaderValue(request).Equals(Config.API_KEY))
            {
                response.Send((int)HttpStatusCode.Unauthorized);
                return;
            }

            WorkerClientMessageRequest? requestData;

            try
            {
                requestData = JsonSerializer.Deserialize<WorkerClientMessageRequest>(request.Body.Text);
            }
            catch
            {
                requestData = null;
            }

            var responseData = new WorkerClientSuccessStatusResponse();

            if (requestData != null && !string.IsNullOrWhiteSpace(requestData.Name))
            {
                var buffer = JsonSerializer.Serialize
                (
                    new WorkerClientMessageData { Name = requestData.Name, Message = requestData.Message }
                );

                // send message to all connected clients
                if (requestData.IDs.Count <= 0)
                {
                    if (string.IsNullOrWhiteSpace(requestData.Server)) // send message to all server
                    {
                        foreach (var client in Connections.Where(x => !x.Value.IsMaster))
                            client.Value.Socket.To.Data(buffer, HTTP.MessageType.Text);
                    }
                    else // limit connection by lobby name (ServerName)
                    {
                        var clients = Connections.Where
                        (
                            x => !x.Value.IsMaster &&
                                 x.Value.Server.Equals(requestData.Server, StringComparison.OrdinalIgnoreCase)
                        );

                        foreach (var client in clients) client.Value.Socket.To.Data(buffer, HTTP.MessageType.Text);
                    }
                }
                else
                {
                    foreach (var id in requestData.IDs)
                    {
                        bool success;
                        ConnectionData? data;

                        if (string.IsNullOrWhiteSpace(requestData.Server))
                        {
                            success = Connections.TryGetValue(id, out data);
                        }
                        else
                        {
                            var result = Connections.Where(x =>
                                !x.Value.IsMaster &&
                                x.Value.Server.Equals(requestData.Server, StringComparison.CurrentCultureIgnoreCase) &&
                                x.Key.Equals(id)
                            ).ToArray();

                            data = result.Length > 0 ? result[0].Value : null;
                            success = data != null;
                        }

                        responseData.Data.Add(new WorkerClientSuccessStatusResponse.SuccessStatusData
                        {
                            Id = id,
                            Success = success
                        });

                        if (success) data?.Socket.To.Data(buffer, HTTP.MessageType.Text);
                    }
                }
            }

            response.Send((int)HttpStatusCode.OK, JsonSerializer.Serialize(responseData));
        });
    }

    private static void HandleMasterSocket(in IHTTP.ServerRequest _, IHTTP.WebSocket socket)
    {
        var ID = Guid.NewGuid().ToString();
        Console.WriteLine($"{nameof(HandleMasterSocket)}, ID: {ID}");

        socket.On.Open(() =>
        {
            Connections.Add(ID, new ConnectionData(ID, true, string.Empty, ref socket));

            Console.WriteLine($"{nameof(Connections)} Connected: {ID} (Master: {true})");

            var data = new WorkerClientIDParametersRequest();

            foreach (var client in Connections.Where(x => !x.Value.IsMaster))
                data.IDs.Add(client.Value.Id);

            if (data.IDs.Count <= 0) return;

            var message = JsonSerializer.Serialize(data);
            foreach (var client in Connections.Where(x => x.Value.IsMaster))
                client.Value.Socket.To.Event("CLIENT_CONNECTED_REFRESH", message, HTTP.MessageType.Text);
        });

        socket.On.Close(() =>
        {
            Connections.Remove(ID);
            Console.WriteLine($"{nameof(Connections)} Disconnected: {ID} (Master: {true})");
        });

        // socket.On.Event((name, data, type) => { });
    }

    private static void HandleClientSocket(in IHTTP.ServerRequest _, IHTTP.WebSocket socket, LobbyToken token)
    {
        Console.WriteLine($"{nameof(HandleClientSocket)}, Token: {JsonSerializer.Serialize(token)}");

        socket.On.Open(() =>
        {
            Connections.Add(token.Id, new ConnectionData(token.Id, false, token.Server, ref socket));

            Console.WriteLine($"{nameof(Connections)} Connected: {token.Id} (Client: {true})");

            var message = JsonSerializer.Serialize(new WorkerClientIDParametersRequest { IDs = [token.Id] });

            foreach (var master in Connections.Where(x => x.Value.IsMaster))
                master.Value.Socket.To.Event("CLIENT_CONNECTED", message, HTTP.MessageType.Text);
        });

        socket.On.Close(() =>
        {
            Connections.Remove(token.Id);

            Console.WriteLine($"{nameof(Connections)} Disconnected: {token.Id} (Client: {true})");

            var message = JsonSerializer.Serialize(new WorkerClientIDParametersRequest { IDs = [token.Id] });

            foreach (var master in Connections.Where(x => x.Value.IsMaster))
                master.Value.Socket.To.Event("CLIENT_DISCONNECT", message, HTTP.MessageType.Text);
        });

        // socket.On.Event((name, data, type) => { });
    }

    public static void Start()
    {
        _server.To.Open(new Uri($"https://{new Host(Config.HOST, Config.PORT)}"));
    }

    public static void Stop()
    {
        _server.To.Close();
    }

    public static void Freeze()
    {
        while (true)
        {
            var key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Q) continue;

            var quit = false;
            bool reading;

            var time = DateTime.Now;
            Console.WriteLine($"\nWanna you quit? `{time}`\n\t Press: `y` (Yes) or `n` (No)");

            do
            {
                var next = Console.ReadKey(true);

                switch (next.Key)
                {
                    case ConsoleKey.Y:
                    {
                        reading = false;
                        quit = true;
                        Console.WriteLine("`y` Selected. Program will be destroyed...");
                        break;
                    }
                    case ConsoleKey.N:
                    {
                        reading = false;
                        quit = false;
                        Console.WriteLine("`n` Selected. Program will continue...");
                        break;
                    }
                    default:
                    {
                        reading = true;
                        break;
                    }
                }
            } while (reading);

            if (quit) break;
        }
    }

    private class ConnectionData(string id, bool isMaster, in string server, ref IHTTP.WebSocket socket)
    {
        public string Id { get; } = id;
        public bool IsMaster { get; } = isMaster;
        public string Server { get; } = server;
        public IHTTP.WebSocket Socket { get; } = socket;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}