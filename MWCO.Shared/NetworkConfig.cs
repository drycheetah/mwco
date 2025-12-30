namespace MWCO.Shared;

/// <summary>
/// Network configuration constants for MWCO
/// </summary>
public static class NetworkConfig
{
    /// <summary>
    /// Default UDP port for MWCO multiplayer
    /// </summary>
    public const int DefaultPort = 1999;

    /// <summary>
    /// Protocol version - increment when breaking changes are made
    /// </summary>
    public const int ProtocolVersion = 1;

    /// <summary>
    /// Maximum packet size in bytes (MTU - IP/UDP headers)
    /// </summary>
    public const int MaxPacketSize = 1400;

    /// <summary>
    /// Physics tick rate (Hz) - matches game's 0.02s timestep
    /// </summary>
    public const int PhysicsTickRate = 50;

    /// <summary>
    /// High priority update rate (Hz) - vehicle transforms and inputs
    /// </summary>
    public const int HighPriorityUpdateRate = 50;

    /// <summary>
    /// Medium priority update rate (Hz) - wheel states, engine state
    /// </summary>
    public const int MediumPriorityUpdateRate = 20;

    /// <summary>
    /// Low priority update rate (Hz) - fuel, damage, parts
    /// </summary>
    public const int LowPriorityUpdateRate = 5;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public const float ConnectionTimeoutSeconds = 10.0f;
}
