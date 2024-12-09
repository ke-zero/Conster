using System.Net;
using System.Text.Json;
using Conster.Core.Worker;
using Conster.Worker.Interfaces;
using Netly;
using Netly.Interfaces;

namespace Conster.Worker;

public class ApplicationLobbyManager(ref Application application) : IAddons
{
    private const bool IS_MASTER = true, IS_CLIENT = false;

    private const string
        MASTER_ZONE = "",
        EVENT_CLIENT_OPEN = nameof(EVENT_CLIENT_OPEN),
        EVENT_CLIENT_CLOSE = nameof(EVENT_CLIENT_CLOSE);

    public IApplication Application { get; } = application;

    public void OnInitialize()
    {
        Application.Server.Middleware.Add("/lobby", (request, response, next) =>
        {
            if
            (
                !Application.IsNotMaster(request, response, false, out _)
                ||
                !Application.IsNotClient(request, response, false, out _)
            )
            {
                next();
                return;
            }

            response.Send(401);
        });

        Application.Server.Map.WebSocket("/lobby", (request, socket) =>
        {
            if (!Application.IsNotMaster(request, null!, false, out _))
                HandleManagerSocket(ref socket);
            else if (!Application.IsNotClient(request, null!, false, out var token))
                HandleClientSocket(ref socket, token);
            else
                socket.To.Close();
        });

        Application.Server.Map.Post("/lobby/status", (request, response) =>
        {
            if (Application.IsNotMaster(request, response, true, out _)) return;
            HandleStatus(ref request, ref response);
        });

        Application.Server.Map.Post("/lobby/disconnect", (request, response) =>
        {
            if (Application.IsNotMaster(request, response, true, out _)) return;
            HandleDisconnect(ref request, ref response);
        });

        Application.Server.Map.Post("/lobby/message", (request, response) =>
        {
            if (Application.IsNotMaster(request, response, true, out _)) return;
            HandleMessage(ref request, ref response);
        });
    }

    public void OnStart()
    {
    }

    public void OnStop()
    {
    }

    private void HandleManagerSocket(ref IHTTP.WebSocket websocket)
    {
        var myConnection = new Connection(Guid.NewGuid().ToString(), IS_MASTER, MASTER_ZONE, ref websocket);
        var socket = websocket;

        socket.On.Open(() =>
        {
            Console.WriteLine($"[Lobby] Master socket connected: {myConnection.ID} ({myConnection.IDHash})");
            Application.Connections.Add(myConnection.IDHash, myConnection);

            var clients = Application.Connections
                .Where(x => !x.Value.IsMaster)
                .Select(y => new WorkerData.LobbyToken { Id = y.Value.ID, Zone = y.Value.Zone })
                .ToArray();

            if (clients.Length > 0)
                socket.To.Event(EVENT_CLIENT_OPEN, JsonSerializer.Serialize(clients), HTTP.MessageType.Text);
        });

        socket.On.Close(() =>
        {
            Console.WriteLine($"[Lobby] Master socket disconnected: {myConnection.ID} ({myConnection.IDHash})");
            Application.Connections.Remove(myConnection.IDHash);
        });
    }

    private void HandleClientSocket(ref IHTTP.WebSocket websocket, WorkerData.LobbyToken token)
    {
        var myConnection = new Connection(token.Id, IS_CLIENT, token.Zone, ref websocket);
        var socket = websocket;

        socket.On.Open(() =>
        {
            Console.WriteLine($"[Lobby] Client socket connected: {myConnection.ID} ({myConnection.IDHash})");
            Application.Connections.Add(myConnection.IDHash, myConnection);

            WorkerData.LobbyToken[] message = [token];
            BroadcastToMaster(EVENT_CLIENT_OPEN, JsonSerializer.Serialize(message));
        });

        socket.On.Close(() =>
        {
            Console.WriteLine($"[Lobby] Client socket disconnected: {myConnection.ID} ({myConnection.IDHash})");
            Application.Connections.Remove(myConnection.IDHash);

            WorkerData.LobbyToken[] message = [token];
            BroadcastToMaster(EVENT_CLIENT_CLOSE, JsonSerializer.Serialize(message));
        });

        return;

        void BroadcastToMaster(string name, string message)
        {
            foreach (var connection in Application.Connections.Where(x => x.Value.IsMaster))
                connection.Value.WebSocket.To.Event(name, message, HTTP.MessageType.Text);
        }
    }

    private void HandleStatus(ref IHTTP.ServerRequest request, ref IHTTP.ServerResponse response)
    {
        response.Send((int)HttpStatusCode.NotImplemented);
    }

    private void HandleDisconnect(ref IHTTP.ServerRequest request, ref IHTTP.ServerResponse response)
    {
        response.Send((int)HttpStatusCode.NotImplemented);
    }

    private void HandleMessage(ref IHTTP.ServerRequest request, ref IHTTP.ServerResponse response)
    {
        response.Send((int)HttpStatusCode.NotImplemented);
    }
}