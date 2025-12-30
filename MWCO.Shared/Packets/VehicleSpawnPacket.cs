using System;
using System.Text;

namespace MWCO.Shared.Packets;

/// <summary>
/// Vehicle spawn packet - sent when a new vehicle enters the game
/// Contains full initial state
/// </summary>
public struct VehicleSpawnPacket
{
    public PacketHeader Header;
    public ushort VehicleId;
    public ushort OwnedByPlayerId;
    public string VehicleModel;

    // Initial transform
    public float PosX, PosY, PosZ;
    public float RotX, RotY, RotZ, RotW;

    // Initial state
    public float RPM;
    public sbyte Gear;
    public byte EngineRunning;
    public float FuelLevel;

    public const int MaxModelNameLength = 32;

    public VehicleSpawnPacket(ushort vehicleId, ushort playerId, string model, uint tick)
    {
        Header = new PacketHeader(PacketType.VehicleSpawn, tick);
        VehicleId = vehicleId;
        OwnedByPlayerId = playerId;
        VehicleModel = model.Length > MaxModelNameLength ? model.Substring(0, MaxModelNameLength) : model;
        PosX = PosY = PosZ = 0;
        RotX = RotY = RotZ = 0;
        RotW = 1;
        RPM = 0;
        Gear = 1;
        EngineRunning = 0;
        FuelLevel = 0;
    }

    public byte[] ToBytes()
    {
        byte[] modelBytes = Encoding.UTF8.GetBytes(VehicleModel);
        byte modelLength = (byte)Math.Min(modelBytes.Length, MaxModelNameLength);

        byte[] bytes = new byte[PacketHeader.Size + 2 + 2 + 1 + modelLength + 44];
        int offset = 0;

        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        BitConverter.GetBytes(VehicleId).CopyTo(bytes, offset);
        offset += 2;
        BitConverter.GetBytes(OwnedByPlayerId).CopyTo(bytes, offset);
        offset += 2;

        bytes[offset++] = modelLength;
        Buffer.BlockCopy(modelBytes, 0, bytes, offset, modelLength);
        offset += modelLength;

        BitConverter.GetBytes(PosX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(PosY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(PosZ).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(RotX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(RotY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(RotZ).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(RotW).CopyTo(bytes, offset);
        offset += 4;

        BitConverter.GetBytes(RPM).CopyTo(bytes, offset);
        offset += 4;
        bytes[offset++] = (byte)Gear;
        bytes[offset++] = EngineRunning;
        BitConverter.GetBytes(FuelLevel).CopyTo(bytes, offset);

        return bytes;
    }

    public static VehicleSpawnPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new VehicleSpawnPacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;
        packet.OwnedByPlayerId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        byte modelLength = bytes[offset++];
        packet.VehicleModel = Encoding.UTF8.GetString(bytes, offset, modelLength);
        offset += modelLength;

        packet.PosX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.PosY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.PosZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.RotX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.RotY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.RotZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.RotW = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        packet.RPM = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.Gear = (sbyte)bytes[offset++];
        packet.EngineRunning = bytes[offset++];
        packet.FuelLevel = BitConverter.ToSingle(bytes, offset);

        return packet;
    }
}

/// <summary>
/// Vehicle despawn packet
/// </summary>
public struct VehicleDespawnPacket
{
    public PacketHeader Header;
    public ushort VehicleId;
    public byte Reason;  // 0=disconnect, 1=destroyed, 2=other

    public const int Size = PacketHeader.Size + 3;

    public VehicleDespawnPacket(ushort vehicleId, byte reason, uint tick)
    {
        Header = new PacketHeader(PacketType.VehicleDespawn, tick);
        VehicleId = vehicleId;
        Reason = reason;
    }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        int offset = 0;

        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        BitConverter.GetBytes(VehicleId).CopyTo(bytes, offset);
        offset += 2;
        bytes[offset] = Reason;

        return bytes;
    }

    public static VehicleDespawnPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new VehicleDespawnPacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;
        packet.Reason = bytes[offset];

        return packet;
    }
}
