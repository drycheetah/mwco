namespace MWCO.Shared;

/// <summary>
/// Packet type identifiers for MWCO network protocol
/// </summary>
public enum PacketType : byte
{
    // Connection Management (0-9)
    ConnectionRequest = 0,
    ConnectionAccepted = 1,
    ConnectionDenied = 2,
    Disconnect = 3,
    Heartbeat = 4,

    // Player/Vehicle State (10-29)
    VehicleStateUpdate = 10,
    VehicleInputUpdate = 11,
    VehicleSpawn = 12,
    VehicleDespawn = 13,
    PlayerInfo = 14,
    PlayerState = 15,
    PlayerSpawn = 16,
    PlayerDespawn = 17,

    // Vehicle Events (30-49)
    GearChange = 30,
    EngineStart = 31,
    EngineStop = 32,
    LightToggle = 33,
    HornTrigger = 34,

    // Wheel/Physics State (50-69)
    WheelStateUpdate = 50,
    SuspensionUpdate = 51,

    // World Sync (70-89)
    WorldObjectUpdate = 70,
    WorldObjectSpawn = 71,
    WorldObjectDespawn = 72,
    TimeWeatherSync = 73,

    // Vehicle Configuration (90-109)
    FuelUpdate = 90,
    DamageUpdate = 91,
    PartAttach = 92,
    PartDetach = 93,
    TirePressureUpdate = 94,

    // Server Messages (110-119)
    ServerMessage = 110,
    ChatMessage = 111,

    // Diagnostics (120-127)
    Ping = 120,
    Pong = 121,
}
