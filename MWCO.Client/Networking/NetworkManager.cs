using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using MWCO.Shared;
using MWCO.Shared.Packets;

namespace MWCO.Client.Networking
{
    /// <summary>
    /// Main network manager - handles connection and packet routing
    /// Runs as a MonoBehaviour on a persistent GameObject
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        // Singleton
        public static NetworkManager Instance { get; private set; }

        // Connection state
        public bool IsConnected { get; private set; }
        public ushort LocalPlayerId { get; private set; }
        public ushort LocalVehicleId { get; private set; }
        public uint CurrentTick => currentTick;

        // Networking
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private uint currentTick;

        // Vehicle tracking
        private Dictionary<ushort, RemoteVehicle> remoteVehicles = new Dictionary<ushort, RemoteVehicle>();
        private LocalVehicleController localVehicleController;

        // Player tracking
        private Dictionary<ushort, RemotePlayer> remotePlayers = new Dictionary<ushort, RemotePlayer>();
        private PlayerController localPlayerController;

        // Config
        private string serverAddress = "127.0.0.1";
        private int serverPort = NetworkConfig.DefaultPort;
        private string playerName = "Player";

        // Update timers
        private float highPriorityTimer = 0f;
        private float mediumPriorityTimer = 0f;
        private float lowPriorityTimer = 0f;
        private float heartbeatTimer = 0f;

        private const float HEARTBEAT_INTERVAL = 1.0f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[MWCO] NetworkManager initialized");
        }

        void Start()
        {
            // Load config
            LoadConfig();
        }

        void Update()
        {
            if (!IsConnected)
                return;

            // Update timers
            float deltaTime = Time.deltaTime;
            currentTick++;

            highPriorityTimer += deltaTime;
            mediumPriorityTimer += deltaTime;
            lowPriorityTimer += deltaTime;
            heartbeatTimer += deltaTime;

            // High priority updates (50Hz)
            if (highPriorityTimer >= 1.0f / NetworkConfig.HighPriorityUpdateRate)
            {
                SendHighPriorityUpdates();
                highPriorityTimer = 0f;
            }

            // Medium priority updates (20Hz)
            if (mediumPriorityTimer >= 1.0f / NetworkConfig.MediumPriorityUpdateRate)
            {
                SendMediumPriorityUpdates();
                mediumPriorityTimer = 0f;
            }

            // Low priority updates (5Hz)
            if (lowPriorityTimer >= 1.0f / NetworkConfig.LowPriorityUpdateRate)
            {
                SendLowPriorityUpdates();
                lowPriorityTimer = 0f;
            }

            // Heartbeat
            if (heartbeatTimer >= HEARTBEAT_INTERVAL)
            {
                SendHeartbeat();
                heartbeatTimer = 0f;
            }

            // Receive packets
            ReceivePackets();
        }

        void OnDestroy()
        {
            Disconnect();
        }

        private void LoadConfig()
        {
            // TODO: Load from config file
            playerName = SystemInfo.deviceName;
        }

        public void Connect(string address = null, int port = 0)
        {
            if (IsConnected)
            {
                Debug.LogWarning("[MWCO] Already connected!");
                return;
            }

            try
            {
                if (address != null) serverAddress = address;
                if (port > 0) serverPort = port;

                serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
                udpClient = new UdpClient();

                Debug.Log($"[MWCO] Connecting to {serverAddress}:{serverPort}...");

                // Send connection request
                var request = new ConnectionRequestPacket(playerName, currentTick);
                SendPacket(request.ToBytes());

                Debug.Log("[MWCO] Connection request sent");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Connection failed: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            try
            {
                // Send disconnect packet
                var header = new PacketHeader(PacketType.Disconnect, currentTick);
                SendPacket(header.ToBytes());

                udpClient?.Close();
                IsConnected = false;

                Debug.Log("[MWCO] Disconnected from server");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error during disconnect: {ex.Message}");
            }
        }

        public void SendPacket(byte[] data)
        {
            try
            {
                udpClient.Send(data, data.Length, serverEndPoint);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Failed to send packet: {ex.Message}");
            }
        }

        public void SendPacket<T>(T packet) where T : struct
        {
            try
            {
                int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
                byte[] data = new byte[size];
                IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
                System.Runtime.InteropServices.Marshal.StructureToPtr(packet, ptr, false);
                System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, size);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
                SendPacket(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Failed to send packet: {ex.Message}");
            }
        }

        private void ReceivePackets()
        {
            try
            {
                while (udpClient.Available > 0)
                {
                    IPEndPoint remoteEndPoint = null;
                    byte[] data = udpClient.Receive(ref remoteEndPoint);

                    if (data.Length >= PacketHeader.Size)
                    {
                        ProcessPacket(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error receiving packets: {ex.Message}");
            }
        }

        private void ProcessPacket(byte[] data)
        {
            try
            {
                var header = PacketHeader.FromBytes(data);

                switch (header.PacketType)
                {
                    case PacketType.ConnectionAccepted:
                        HandleConnectionAccepted(data);
                        break;

                    case PacketType.ConnectionDenied:
                        HandleConnectionDenied(data);
                        break;

                    case PacketType.VehicleStateUpdate:
                        HandleVehicleState(data);
                        break;

                    case PacketType.VehicleSpawn:
                        HandleVehicleSpawn(data);
                        break;

                    case PacketType.VehicleDespawn:
                        HandleVehicleDespawn(data);
                        break;

                    case PacketType.PlayerState:
                        HandlePlayerState(data);
                        break;

                    case PacketType.PlayerSpawn:
                        HandlePlayerSpawn(data);
                        break;

                    case PacketType.PlayerDespawn:
                        HandlePlayerDespawn(data);
                        break;

                    case PacketType.WheelStateUpdate:
                        HandleWheelState(data);
                        break;

                    case PacketType.GearChange:
                    case PacketType.EngineStart:
                    case PacketType.EngineStop:
                    case PacketType.LightToggle:
                    case PacketType.HornTrigger:
                        HandleVehicleEvent(data);
                        break;

                    case PacketType.FuelUpdate:
                        HandleVehicleConfig(data);
                        break;

                    case PacketType.PartAttach:
                    case PacketType.PartDetach:
                        HandlePartSync(data);
                        break;

                    case PacketType.TimeWeatherSync:
                        HandleTimeWeather(data);
                        break;

                    default:
                        Debug.LogWarning($"[MWCO] Unhandled packet type: {header.PacketType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error processing packet: {ex.Message}");
            }
        }

        private void HandleConnectionAccepted(byte[] data)
        {
            var response = ConnectionResponsePacket.FromBytes(data);
            LocalPlayerId = response.AssignedPlayerId;
            LocalVehicleId = response.AssignedVehicleId;
            IsConnected = true;

            Debug.Log($"[MWCO] Connected! Player ID: {LocalPlayerId}, Vehicle ID: {LocalVehicleId}");
            Debug.Log($"[MWCO] Server message: {response.Message}");

            // Initialize local vehicle controller
            localVehicleController = gameObject.AddComponent<LocalVehicleController>();
            localVehicleController.Initialize(LocalVehicleId);

            // Initialize local player controller
            localPlayerController = gameObject.AddComponent<PlayerController>();
            localPlayerController.Initialize(playerName);
        }

        private void HandleConnectionDenied(byte[] data)
        {
            var response = ConnectionResponsePacket.FromBytes(data);
            Debug.LogError($"[MWCO] Connection denied: {response.Message}");
        }

        private void HandleVehicleState(byte[] data)
        {
            var packet = VehicleStatePacket.FromBytes(data);

            // Ignore our own vehicle
            if (packet.VehicleId == LocalVehicleId)
                return;

            // Update or create remote vehicle
            if (!remoteVehicles.TryGetValue(packet.VehicleId, out var vehicle))
            {
                Debug.LogWarning($"[MWCO] Received state for unknown vehicle {packet.VehicleId}");
                return;
            }

            vehicle.UpdateState(packet);
        }

        private void HandleVehicleSpawn(byte[] data)
        {
            var packet = VehicleSpawnPacket.FromBytes(data);

            // Ignore our own vehicle
            if (packet.VehicleId == LocalVehicleId)
                return;

            Debug.Log($"[MWCO] Spawning remote vehicle {packet.VehicleId} (owner: Player {packet.OwnedByPlayerId})");

            // Create remote vehicle
            var vehicleObj = new GameObject($"RemoteVehicle_{packet.VehicleId}");
            var remoteVehicle = vehicleObj.AddComponent<RemoteVehicle>();
            remoteVehicle.Initialize(packet);

            remoteVehicles[packet.VehicleId] = remoteVehicle;
        }

        private void HandleVehicleDespawn(byte[] data)
        {
            var packet = VehicleDespawnPacket.FromBytes(data);

            if (remoteVehicles.TryGetValue(packet.VehicleId, out var vehicle))
            {
                Debug.Log($"[MWCO] Despawning vehicle {packet.VehicleId}");
                Destroy(vehicle.gameObject);
                remoteVehicles.Remove(packet.VehicleId);
            }
        }

        private void HandleWheelState(byte[] data)
        {
            var packet = WheelStatePacket.FromBytes(data);

            if (remoteVehicles.TryGetValue(packet.VehicleId, out var vehicle))
            {
                vehicle.UpdateWheelState(packet);
            }
        }

        private void HandleVehicleEvent(byte[] data)
        {
            var packet = VehicleEventPacket.FromBytes(data);

            if (remoteVehicles.TryGetValue(packet.VehicleId, out var vehicle))
            {
                vehicle.HandleEvent(packet);
            }
        }

        private void HandleVehicleConfig(byte[] data)
        {
            var packet = VehicleConfigPacket.FromBytes(data);

            if (remoteVehicles.TryGetValue(packet.VehicleId, out var vehicle))
            {
                vehicle.UpdateConfig(packet);
            }
        }

        private void HandlePartSync(byte[] data)
        {
            var packet = PartSyncPacket.FromBytes(data);

            if (remoteVehicles.TryGetValue(packet.VehicleId, out var vehicle))
            {
                vehicle.UpdatePart(packet);
            }
        }

        private void HandleTimeWeather(byte[] data)
        {
            var packet = TimeWeatherPacket.FromBytes(data);
            // TODO: Update game time and weather
            Debug.Log($"[MWCO] Time update: {packet.Hour}:{packet.Minute:D2}, Weather: {packet.WeatherType}");
        }

        private void SendHighPriorityUpdates()
        {
            if (localVehicleController != null && localVehicleController.IsReady)
            {
                // Send input packet
                var inputPacket = localVehicleController.GetInputPacket(currentTick);
                SendPacket(inputPacket.ToBytes());
            }
        }

        private void SendMediumPriorityUpdates()
        {
            // Medium priority updates handled by local vehicle controller
        }

        private void SendLowPriorityUpdates()
        {
            // Low priority updates handled by local vehicle controller
        }

        private void SendHeartbeat()
        {
            var header = new PacketHeader(PacketType.Heartbeat, currentTick);
            SendPacket(header.ToBytes());
        }

        private void HandlePlayerState(byte[] data)
        {
            var packet = PlayerStatePacket.FromBytes(data);

            // Ignore our own player
            if (packet.PlayerId == LocalPlayerId)
                return;

            // Update or create remote player
            if (!remotePlayers.TryGetValue(packet.PlayerId, out var player))
            {
                Debug.LogWarning($"[MWCO] Received state for unknown player {packet.PlayerId}");
                return;
            }

            Vector3 position = new Vector3(packet.PositionX, packet.PositionY, packet.PositionZ);
            Quaternion rotation = new Quaternion(packet.RotationX, packet.RotationY, packet.RotationZ, packet.RotationW);
            player.UpdateState(position, rotation);
        }

        private void HandlePlayerSpawn(byte[] data)
        {
            var packet = PlayerSpawnPacket.FromBytes(data);

            // Ignore our own player
            if (packet.PlayerId == LocalPlayerId)
                return;

            Debug.Log($"[MWCO] Spawning remote player {packet.PlayerId}: {packet.GetPlayerName()}");

            // Create remote player
            var playerObj = new GameObject($"RemotePlayer_{packet.PlayerId}");
            var remotePlayer = playerObj.AddComponent<RemotePlayer>();
            remotePlayer.Initialize(packet.PlayerId, packet.GetPlayerName());

            Vector3 spawnPos = new Vector3(packet.SpawnPositionX, packet.SpawnPositionY, packet.SpawnPositionZ);
            playerObj.transform.position = spawnPos;

            remotePlayers[packet.PlayerId] = remotePlayer;
        }

        private void HandlePlayerDespawn(byte[] data)
        {
            var packet = PlayerDespawnPacket.FromBytes(data);

            if (remotePlayers.TryGetValue(packet.PlayerId, out var player))
            {
                Debug.Log($"[MWCO] Despawning player {packet.PlayerId}");
                Destroy(player.gameObject);
                remotePlayers.Remove(packet.PlayerId);
            }
        }
    }
}
