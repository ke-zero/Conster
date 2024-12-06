using System.Net;
using System.Text.Json;
using Conster.Core;
using Netly;
using Netly.Interfaces;

namespace Conster.Worker;

public static class Application
{
    private const string HEADER_TOKEN = "TOKEN";
    private static readonly HTTP.Server _server = new();

    private class ConnectionData(string id, bool isMaster, in string server, ref IHTTP.WebSocket socket)
    {
        public string Id { get; } = id;
        public bool IsMaster { get; } = isMaster;
        public string Server { get; } = server;
        public IHTTP.WebSocket Socket { get; } = socket;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }

    private static readonly List<ConnectionData> Connections = new();


    public static void Initialize()
    {
        _server.On.Open(() => Console.WriteLine($"Server Started At: {_server.Host}"));
        _server.On.Error(e => { Console.WriteLine($"Server Error: {e}"); });
        _server.On.Close(() =>
        {
            Console.WriteLine($"Server Closed At: {_server.Host}");

            lock (Connections)
            {
                Connections.Clear();
            }
        });

        // Check Auth Token
        _server.Middleware.Add((request, response, next) =>
        {
            // close all connection that not provide token
            if (!request.Headers.TryGetValue(HEADER_TOKEN, out var token) || string.IsNullOrWhiteSpace(token))
            {
                if (request.IsWebSocket) response.Close();
                else response.Send((int)HttpStatusCode.Unauthorized);
                return;
            }

            next();
        });

        _server.Map.WebSocket("/lobby", (request, socket) =>
        {
            var token = request.Headers[HEADER_TOKEN] ?? string.Empty;

            if (token.Equals(Config.API_KEY))
            {
                HandleMasterSocket(request, socket);
                return;
            }

            if (JWTSolver.Solve(token, Config.API_KEY, out LobbyToken? lobbyToken) && lobbyToken != null)
            {
                HandleClientSocket(request, socket, lobbyToken);
                return;
            }

            // Invalid Token.
            socket.To.Close();
        });

        _server.Map.Post("/lobby/status", (request, response) =>
        {
            if (!request.Headers[HEADER_TOKEN].Equals(Config.API_KEY))
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
                requestData = new();
            }

            var responseData = new WorkerClientStatusResponse();

            if (requestData is { IDs.Count: > 0 })
            {
                foreach (var id in requestData.IDs)
                {
                    var data = Connections.FirstOrDefault(x => x.Id == id && !x.IsMaster);

                    responseData.Data.Add(new()
                    {
                        Id = id,
                        Success = data != null,
                        At = data?.CreatedAt ?? DateTime.UtcNow
                    });
                }
            }

            response.Send((int)HttpStatusCode.OK, JsonSerializer.Serialize(responseData));
        });

        _server.Map.Post("/lobby/disconnect", (request, response) =>
        {
            if (!request.Headers[HEADER_TOKEN].Equals(Config.API_KEY))
            {
                response.Send((int)HttpStatusCode.Unauthorized);
                return;
            }

            WorkerClientIDParametersRequest requestData;

            try
            {
                requestData = JsonSerializer.Deserialize<WorkerClientIDParametersRequest>(request.Body.Text) ?? new();
            }
            catch
            {
                requestData = new();
            }

            var responseData = new WorkerClientSuccessStatusResponse();

            foreach (var id in requestData.IDs)
            {
                var data = Connections.FirstOrDefault(x => x.Id == id && !x.IsMaster);

                responseData.Data.Add(new()
                {
                    Id = id,
                    Success = data != null,
                });

                data?.Socket.To.Close();
            }

            response.Send((int)HttpStatusCode.OK, JsonSerializer.Serialize(responseData));
        });

        _server.Map.Post("/lobby/message", (request, response) =>
        {
            if (!request.Headers[HEADER_TOKEN].Equals(Config.API_KEY))
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
                        foreach (var client in Connections.Where(x => !x.IsMaster))
                            client.Socket.To.Data(buffer, HTTP.MessageType.Text);
                    }
                    else // limit connection by lobby name (ServerName)
                    {
                        var clients = Connections.Where
                        (
                            x => !x.IsMaster && x.Server.Equals(requestData.Server, StringComparison.OrdinalIgnoreCase)
                        );

                        foreach (var client in clients) client.Socket.To.Data(buffer, HTTP.MessageType.Text);
                    }
                }
                else
                {
                    foreach (var id in requestData.IDs)
                    {
                        ConnectionData? data;

                        if (string.IsNullOrWhiteSpace(requestData.Server))
                        {
                            data = Connections.FirstOrDefault(x => x.Id == id && !x.IsMaster);
                        }
                        else
                        {
                            data = Connections.FirstOrDefault
                            (
                                x => x.Id == id && !x.IsMaster && x.Server.Equals(requestData.Server,
                                    StringComparison.CurrentCultureIgnoreCase)
                            );
                        }

                        responseData.Data.Add(new()
                        {
                            Id = id,
                            Success = data != null,
                        });

                        data?.Socket.To.Data(buffer, HTTP.MessageType.Text);
                    }
                }
            }

            response.Send((int)HttpStatusCode.OK, JsonSerializer.Serialize(responseData));
        });
    }

    private static void HandleMasterSocket(in IHTTP.ServerRequest _, IHTTP.WebSocket socket)
    {
        var ID = Guid.NewGuid().ToString();

        socket.On.Open(() =>
        {
            lock (Connections)
            {
                Connections.Add(new(ID, true, string.Empty, ref socket));
            }

            Console.WriteLine($"{nameof(Connections)} Connected: {ID} (Master: {true})");

            var data = new WorkerClientIDParametersRequest();
            foreach (var client in Connections.Where(x => !x.IsMaster)) data.IDs.Add(client.Id);

            if (data.IDs.Count > 0)
                socket.To.Event("CLIENT_CONNECTED_REFRESH", JsonSerializer.Serialize(data), HTTP.MessageType.Text);
        });

        socket.On.Close(() =>
        {
            lock (Connections)
            {
                var element = Connections.FirstOrDefault(x => x.Id == ID && x.IsMaster);
                Connections.Remove(element!);
            }

            Console.WriteLine($"{nameof(Connections)} Disconnected: {ID} (Master: {true})");
        });

        // socket.On.Event((name, data, type) => { });
    }

    private static void HandleClientSocket(in IHTTP.ServerRequest _, IHTTP.WebSocket socket, LobbyToken token)
    {
        socket.On.Open(() =>
        {
            lock (Connections)
            {
                Connections.Add(new(token.Id, false, token.Server, ref socket));
            }

            Console.WriteLine($"{nameof(Connections)} Connected: {token.Id} (Client: {true})");

            var message = JsonSerializer.Serialize(new WorkerClientIDParametersRequest() { IDs = [token.Id] });

            foreach (var master in Connections.Where(x => x.IsMaster))
                master.Socket.To.Event("CLIENT_CONNECTED", JsonSerializer.Serialize(message),
                    HTTP.MessageType.Text);
        });

        socket.On.Close(() =>
        {
            lock (Connections)
            {
                var element = Connections.FirstOrDefault(x => x.Id == token.Id);
                Connections.Remove(element!);
            }

            Console.WriteLine($"{nameof(Connections)} Disconnected: {token.Id} (Client: {true})");

            var message = JsonSerializer.Serialize(new WorkerClientIDParametersRequest() { IDs = [token.Id] });

            foreach (var master in Connections.Where(x => x.IsMaster))
                master.Socket.To.Event("CLIENT_DISCONNECT", JsonSerializer.Serialize(message),
                    HTTP.MessageType.Text);
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
}