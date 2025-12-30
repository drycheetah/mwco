using System;
using System.Runtime.InteropServices;

namespace MWCO.Shared.Packets;

/// <summary>
/// Wheel physics state for all 4 wheels
/// Medium priority update (~20Hz)
/// Total size: ~72 bytes (8 header + 64 data)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WheelStatePacket
{
    public PacketHeader Header;
    public ushort VehicleId;

    // 4 wheels x 14 bytes each = 56 bytes
    public WheelData FrontLeft;
    public WheelData FrontRight;
    public WheelData RearLeft;
    public WheelData RearRight;

    public const int Size = PacketHeader.Size + 2 + (WheelData.Size * 4);

    public WheelStatePacket(ushort vehicleId, uint tick)
    {
        Header = new PacketHeader(PacketType.WheelStateUpdate, tick);
        VehicleId = vehicleId;
        FrontLeft = new WheelData();
        FrontRight = new WheelData();
        RearLeft = new WheelData();
        RearRight = new WheelData();
    }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        int offset = 0;

        // Header
        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        // Vehicle ID
        BitConverter.GetBytes(VehicleId).CopyTo(bytes, offset);
        offset += 2;

        // Wheels
        Buffer.BlockCopy(FrontLeft.ToBytes(), 0, bytes, offset, WheelData.Size);
        offset += WheelData.Size;
        Buffer.BlockCopy(FrontRight.ToBytes(), 0, bytes, offset, WheelData.Size);
        offset += WheelData.Size;
        Buffer.BlockCopy(RearLeft.ToBytes(), 0, bytes, offset, WheelData.Size);
        offset += WheelData.Size;
        Buffer.BlockCopy(RearRight.ToBytes(), 0, bytes, offset, WheelData.Size);

        return bytes;
    }

    public static WheelStatePacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new WheelStatePacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        packet.FrontLeft = WheelData.FromBytes(bytes, offset);
        offset += WheelData.Size;
        packet.FrontRight = WheelData.FromBytes(bytes, offset);
        offset += WheelData.Size;
        packet.RearLeft = WheelData.FromBytes(bytes, offset);
        offset += WheelData.Size;
        packet.RearRight = WheelData.FromBytes(bytes, offset);

        return packet;
    }
}

/// <summary>
/// Individual wheel physics data
/// 14 bytes per wheel
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WheelData
{
    public float AngularVelocity;     // Wheel rotation speed (rad/s)
    public float Compression;         // Suspension compression (0-1)
    public float SteeringAngle;       // Current steering angle
    public byte OnGround;             // Is wheel touching ground (bool as byte)
    public byte TirePuncture;         // Is tire punctured (bool as byte)

    public const int Size = 14;

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        int offset = 0;

        BitConverter.GetBytes(AngularVelocity).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(Compression).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(SteeringAngle).CopyTo(bytes, offset);
        offset += 4;
        bytes[offset++] = OnGround;
        bytes[offset] = TirePuncture;

        return bytes;
    }

    public static WheelData FromBytes(byte[] bytes, int startOffset)
    {
        int offset = startOffset;
        var data = new WheelData
        {
            AngularVelocity = BitConverter.ToSingle(bytes, offset),
        };
        offset += 4;
        data.Compression = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        data.SteeringAngle = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        data.OnGround = bytes[offset++];
        data.TirePuncture = bytes[offset];

        return data;
    }
}
