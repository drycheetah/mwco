using System;

namespace MWCO.Shared.Packets;

/// <summary>
/// Generic vehicle event packet for discrete events
/// (gear changes, engine start/stop, horn, lights, etc.)
/// </summary>
public struct VehicleEventPacket
{
    public PacketHeader Header;
    public ushort VehicleId;
    public byte EventData;  // Event-specific data

    public const int Size = PacketHeader.Size + 3;

    public VehicleEventPacket(ushort vehicleId, PacketType eventType, byte eventData, uint tick)
    {
        Header = new PacketHeader(eventType, tick);
        VehicleId = vehicleId;
        EventData = eventData;
    }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        int offset = 0;

        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        BitConverter.GetBytes(VehicleId).CopyTo(bytes, offset);
        offset += 2;

        bytes[offset] = EventData;

        return bytes;
    }

    public static VehicleEventPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new VehicleEventPacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        packet.EventData = bytes[offset];

        return packet;
    }
}

/// <summary>
/// Gear change event
/// EventData = new gear number (signed)
/// </summary>
public struct GearChangeEvent
{
    public static VehicleEventPacket Create(ushort vehicleId, sbyte gear, uint tick)
    {
        return new VehicleEventPacket(vehicleId, PacketType.GearChange, (byte)gear, tick);
    }

    public static sbyte GetGear(VehicleEventPacket packet)
    {
        return (sbyte)packet.EventData;
    }
}

/// <summary>
/// Engine start/stop events
/// EventData = 1 for start, 0 for stop
/// </summary>
public struct EngineEvent
{
    public static VehicleEventPacket CreateStart(ushort vehicleId, uint tick)
    {
        return new VehicleEventPacket(vehicleId, PacketType.EngineStart, 1, tick);
    }

    public static VehicleEventPacket CreateStop(ushort vehicleId, uint tick)
    {
        return new VehicleEventPacket(vehicleId, PacketType.EngineStop, 0, tick);
    }
}

/// <summary>
/// Light toggle event
/// EventData bits: 0=headlights, 1=brake lights, 2=reverse lights, 3=turn signal left, 4=turn signal right
/// </summary>
public struct LightToggleEvent
{
    public const byte HEADLIGHTS = 1 << 0;
    public const byte BRAKE_LIGHTS = 1 << 1;
    public const byte REVERSE_LIGHTS = 1 << 2;
    public const byte TURN_SIGNAL_LEFT = 1 << 3;
    public const byte TURN_SIGNAL_RIGHT = 1 << 4;

    public static VehicleEventPacket Create(ushort vehicleId, byte lightFlags, uint tick)
    {
        return new VehicleEventPacket(vehicleId, PacketType.LightToggle, lightFlags, tick);
    }
}

/// <summary>
/// Horn trigger event
/// EventData = 1 for pressed, 0 for released
/// </summary>
public struct HornEvent
{
    public static VehicleEventPacket Create(ushort vehicleId, bool pressed, uint tick)
    {
        return new VehicleEventPacket(vehicleId, PacketType.HornTrigger, (byte)(pressed ? 1 : 0), tick);
    }
}
