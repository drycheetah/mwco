using System.Net;
using System.Net.Sockets;
using MWCO.Shared;
using MWCO.Shared.Packets;

namespace MWCO.Server;

/// <summary>
/// Basic UDP server for MWCO multiplayer
/// Handles client connections and packet routing
/// </summary>
public class UdpServer
{
    private readonly UdpClient _udpClient;
    private readonly Dictionary<IPEndPoint, ConnectedClient> _clients;
    private readonly int _port;
    private uint _currentTick;
    private bool _running;
    private ushort _nextPlayerId = 1;
    private ushort _nextVehicleId = 1;

    public UdpServer(int port = NetworkConfig.DefaultPort)
    {
        _port = port;
        _udpClient = new UdpClient(port);
        _clients = new Dictionary<IPEndPoint, ConnectedClient>();
        _currentTick = 0;
    }

    public async Task StartAsync()
    {
        _running = true;
        Console.WriteLine($"[MWCO Server] Starting on port {_port}...");
        Console.WriteLine($"[MWCO Server] Protocol version: {NetworkConfig.ProtocolVersion}");
        Console.WriteLine($"[MWCO Server] Physics tick rate: {NetworkConfig.PhysicsTickRate}Hz");

        // Start server loop
        var receiveTask = ReceiveLoopAsync();
        var tickTask = TickLoopAsync();

        await Task.WhenAll(receiveTask, tickTask);
    }

    public void Stop()
    {
        _running = false;
        _udpClient.Close();
        Console.WriteLine("[MWCO Server] Server stopped.");
    }

    private async Task ReceiveLoopAsync()
    {
        Console.WriteLine("[MWCO Server] Receive loop started.");

        while (_running)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                _ = Task.Run(() => ProcessPacket(result.Buffer, result.RemoteEndPoint));
            }
            catch (ObjectDisposedException)
            {
                // Server is shutting down
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MWCO Server] Receive error: {ex.Message}");
            }
        }

        Console.WriteLine("[MWCO Server] Receive loop stopped.");
    }

    private async Task TickLoopAsync()
    {
        Console.WriteLine("[MWCO Server] Tick loop started.");
        var tickInterval = TimeSpan.FromSeconds(1.0 / NetworkConfig.PhysicsTickRate);

        while (_running)
        {
            var tickStart = DateTime.UtcNow;

            // Process game tick
            ProcessTick();
            _currentTick++;

            // Sleep until next tick
            var elapsed = DateTime.UtcNow - tickStart;
            var remaining = tickInterval - elapsed;

            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining);
            }
            else
            {
                Console.WriteLine($"[MWCO Server] Warning: Tick took {elapsed.TotalMilliseconds:F2}ms (target: {tickInterval.TotalMilliseconds:F2}ms)");
            }
        }

        Console.WriteLine("[MWCO Server] Tick loop stopped.");
    }

    private void ProcessTick()
    {
        float deltaTime = 1.0f / NetworkConfig.PhysicsTickRate;

        // Integrate server-side physics for all vehicles
        foreach (var client in _clients.Values)
        {
            // Apply last received input
            if (client.LastInput.HasValue)
            {
                client.VehicleState.ApplyInput(client.LastInput.Value);
            }

            // Integrate physics
            client.VehicleState.Integrate(deltaTime);
            client.VehicleState.LastUpdateTick = _currentTick;

            client.LastSeenTick = _currentTick;
        }

        // Broadcast vehicle states to all clients
        BroadcastVehicleStates();

        // Check for timeouts
        CheckTimeouts();
    }

    private void ProcessPacket(byte[] data, IPEndPoint remoteEndPoint)
    {
        if (data.Length < PacketHeader.Size)
        {
            Console.WriteLine($"[MWCO Server] Received packet too small from {remoteEndPoint}");
            return;
        }

        try
        {
            var header = PacketHeader.FromBytes(data);

            // Verify protocol version
            if (header.ProtocolVersion != NetworkConfig.ProtocolVersion)
            {
                Console.WriteLine($"[MWCO Server] Protocol version mismatch from {remoteEndPoint}: {header.ProtocolVersion} vs {NetworkConfig.ProtocolVersion}");
                SendConnectionDenied(remoteEndPoint, "Protocol version mismatch");
                return;
            }

            switch (header.PacketType)
            {
                case PacketType.ConnectionRequest:
                    HandleConnectionRequest(data, remoteEndPoint);
                    break;

                case PacketType.VehicleInputUpdate:
                    HandleVehicleInput(data, remoteEndPoint);
                    break;

                case PacketType.Heartbeat:
                    HandleHeartbeat(remoteEndPoint);
                    break;

                case PacketType.Disconnect:
                    HandleDisconnect(remoteEndPoint);
                    break;

                default:
                    Console.WriteLine($"[MWCO Server] Unknown packet type {header.PacketType} from {remoteEndPoint}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MWCO Server] Error processing packet from {remoteEndPoint}: {ex.Message}");
        }
    }

    private void HandleConnectionRequest(byte[] data, IPEndPoint remoteEndPoint)
    {
        var packet = ConnectionRequestPacket.FromBytes(data);
        Console.WriteLine($"[MWCO Server] Connection request from {remoteEndPoint}, player: {packet.PlayerName}");

        // Check if already connected
        if (_clients.ContainsKey(remoteEndPoint))
        {
            Console.WriteLine($"[MWCO Server] Client {remoteEndPoint} already connected");
            return;
        }

        // Assign IDs
        ushort playerId = _nextPlayerId++;
        ushort vehicleId = _nextVehicleId++;

        // Create client with initialized vehicle state
        var client = new ConnectedClient
        {
            EndPoint = remoteEndPoint,
            PlayerId = playerId,
            VehicleId = vehicleId,
            PlayerName = packet.PlayerName,
            ConnectedTick = _currentTick,
            LastSeenTick = _currentTick,
            VehicleState = new VehicleState
            {
                VehicleId = vehicleId,
                PlayerId = playerId,
                LastUpdateTick = _currentTick
            }
        };

        _clients[remoteEndPoint] = client;

        // Send acceptance
        var response = new ConnectionResponsePacket(
            accepted: true,
            playerId: playerId,
            vehicleId: vehicleId,
            message: $"Welcome to MWCO Server! You are player {playerId}",
            tick: _currentTick
        );

        SendPacket(response.ToBytes(), remoteEndPoint);
        Console.WriteLine($"[MWCO Server] Client {remoteEndPoint} connected as {packet.PlayerName} (Player ID: {playerId}, Vehicle ID: {vehicleId})");
    }

    private void SendConnectionDenied(IPEndPoint remoteEndPoint, string reason)
    {
        var response = new ConnectionResponsePacket(
            accepted: false,
            playerId: 0,
            vehicleId: 0,
            message: reason,
            tick: _currentTick
        );

        SendPacket(response.ToBytes(), remoteEndPoint);
    }

    private void HandleVehicleInput(byte[] data, IPEndPoint remoteEndPoint)
    {
        if (!_clients.TryGetValue(remoteEndPoint, out var client))
        {
            Console.WriteLine($"[MWCO Server] Vehicle input from unknown client {remoteEndPoint}");
            return;
        }

        var packet = VehicleInputPacket.FromBytes(data);

        // Update client's vehicle state from input
        client.LastInput = packet;
        client.LastSeenTick = _currentTick;

        // TODO: Process input and update authoritative vehicle state
        // For now, just acknowledge receipt
    }

    private void HandleHeartbeat(IPEndPoint remoteEndPoint)
    {
        if (_clients.TryGetValue(remoteEndPoint, out var client))
        {
            client.LastSeenTick = _currentTick;
        }
    }

    private void HandleDisconnect(IPEndPoint remoteEndPoint)
    {
        if (_clients.TryGetValue(remoteEndPoint, out var client))
        {
            Console.WriteLine($"[MWCO Server] Client {client.PlayerName} disconnected");
            _clients.Remove(remoteEndPoint);
        }
    }

    private void BroadcastVehicleStates()
    {
        // For each connected vehicle, broadcast its server-authoritative state to all other clients
        foreach (var client in _clients.Values)
        {
            // Get server-side authoritative state
            var statePacket = client.VehicleState.ToPacket();
            statePacket.Header.Tick = _currentTick;

            // Broadcast to all OTHER clients
            foreach (var otherClient in _clients.Values)
            {
                if (otherClient.PlayerId != client.PlayerId)
                {
                    SendPacket(statePacket.ToBytes(), otherClient.EndPoint);
                }
            }
        }
    }

    private void CheckTimeouts()
    {
        var timeoutTicks = (uint)(NetworkConfig.ConnectionTimeoutSeconds * NetworkConfig.PhysicsTickRate);
        var toRemove = new List<IPEndPoint>();

        foreach (var (endPoint, client) in _clients)
        {
            if (_currentTick - client.LastSeenTick > timeoutTicks)
            {
                Console.WriteLine($"[MWCO Server] Client {client.PlayerName} timed out");
                toRemove.Add(endPoint);
            }
        }

        foreach (var endPoint in toRemove)
        {
            _clients.Remove(endPoint);
        }
    }

    private void SendPacket(byte[] data, IPEndPoint destination)
    {
        try
        {
            _udpClient.Send(data, data.Length, destination);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MWCO Server] Error sending to {destination}: {ex.Message}");
        }
    }
}

/// <summary>
/// Represents a connected client
/// </summary>
public class ConnectedClient
{
    public required IPEndPoint EndPoint { get; set; }
    public required ushort PlayerId { get; set; }
    public required ushort VehicleId { get; set; }
    public required string PlayerName { get; set; }
    public required uint ConnectedTick { get; set; }
    public required uint LastSeenTick { get; set; }

    public VehicleInputPacket? LastInput { get; set; }
    public VehicleState VehicleState { get; set; } = new VehicleState();
}
