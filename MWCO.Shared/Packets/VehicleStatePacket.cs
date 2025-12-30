using System;
using System.Runtime.InteropServices;

namespace MWCO.Shared.Packets;

/// <summary>
/// High-frequency vehicle state update packet
/// Contains essential transform and physics data
/// Target rate: 50Hz (every physics tick)
/// Total size: ~84 bytes (8 header + 76 data)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VehicleStatePacket
{
    public PacketHeader Header;

    // Vehicle Identification
    public ushort VehicleId;

    // Transform Data (48 bytes)
    public float PositionX;
    public float PositionY;
    public float PositionZ;

    public float RotationX;  // Quaternion
    public float RotationY;
    public float RotationZ;
    public float RotationW;

    public float VelocityX;
    public float VelocityY;
    public float VelocityZ;

    public float AngularVelocityX;
    public float AngularVelocityY;
    public float AngularVelocityZ;

    // Engine/Drivetrain State (10 bytes)
    public float RPM;
    public sbyte Gear;
    public byte EngineRunning;  // bool as byte

    // Input State (12 bytes) - current processed values
    public float Steering;
    public float Throttle;
    public float Brake;

    public const int Size = PacketHeader.Size + 78;

    public VehicleStatePacket(ushort vehicleId, uint tick)
    {
        Header = new PacketHeader(PacketType.VehicleStateUpdate, tick);
        VehicleId = vehicleId;

        PositionX = PositionY = PositionZ = 0;
        RotationX = RotationY = RotationZ = 0;
        RotationW = 1;
        VelocityX = VelocityY = VelocityZ = 0;
        AngularVelocityX = AngularVelocityY = AngularVelocityZ = 0;

        RPM = 0;
        Gear = 1;  // Neutral+1
        EngineRunning = 0;

        Steering = Throttle = Brake = 0;
    }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        int offset = 0;

        // Header
        byte[] headerBytes = Header.ToBytes();
        Buffer.BlockCopy(headerBytes, 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        // Vehicle ID
        BitConverter.GetBytes(VehicleId).CopyTo(bytes, offset);
        offset += 2;

        // Transform
        BitConverter.GetBytes(PositionX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(PositionY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(PositionZ).CopyTo(bytes, offset);
        offset += 4;

        BitConverter.GetBytes(RotationX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(RotationY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(RotationZ).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(RotationW).CopyTo(bytes, offset);
        offset += 4;

        BitConverter.GetBytes(VelocityX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(VelocityY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(VelocityZ).CopyTo(bytes, offset);
        offset += 4;

        BitConverter.GetBytes(AngularVelocityX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(AngularVelocityY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(AngularVelocityZ).CopyTo(bytes, offset);
        offset += 4;

        // Engine/Drivetrain
        BitConverter.GetBytes(RPM).CopyTo(bytes, offset);
        offset += 4;
        bytes[offset++] = (byte)Gear;
        bytes[offset++] = EngineRunning;

        // Input
        BitConverter.GetBytes(Steering).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(Throttle).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(Brake).CopyTo(bytes, offset);

        return bytes;
    }

    public static VehicleStatePacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        if (bytes.Length - startOffset < Size)
            throw new ArgumentException($"Buffer too small. Need at least {Size} bytes.");

        var packet = new VehicleStatePacket();
        int offset = startOffset;

        // Header
        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        // Vehicle ID
        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        // Transform
        packet.PositionX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.PositionY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.PositionZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        packet.RotationX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.RotationY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.RotationZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.RotationW = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        packet.VelocityX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.VelocityY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.VelocityZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        packet.AngularVelocityX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.AngularVelocityY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.AngularVelocityZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        // Engine/Drivetrain
        packet.RPM = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.Gear = (sbyte)bytes[offset++];
        packet.EngineRunning = bytes[offset++];

        // Input
        packet.Steering = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.Throttle = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.Brake = BitConverter.ToSingle(bytes, offset);

        return packet;
    }
}
