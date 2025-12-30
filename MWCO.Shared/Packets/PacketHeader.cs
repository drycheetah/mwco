using System;
using System.Runtime.InteropServices;

namespace MWCO.Shared.Packets;

/// <summary>
/// Standard packet header for all MWCO network packets
/// Fixed size: 8 bytes
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketHeader
{
    /// <summary>
    /// Protocol version to ensure compatibility
    /// </summary>
    public ushort ProtocolVersion;

    /// <summary>
    /// Type of packet (see PacketType enum)
    /// </summary>
    public PacketType PacketType;

    /// <summary>
    /// Flags for packet options (reliable, ordered, etc.)
    /// </summary>
    public byte Flags;

    /// <summary>
    /// Server tick number when packet was created
    /// </summary>
    public uint Tick;

    public const int Size = 8;

    public PacketHeader(PacketType packetType, uint tick, byte flags = 0)
    {
        ProtocolVersion = NetworkConfig.ProtocolVersion;
        PacketType = packetType;
        Flags = flags;
        Tick = tick;
    }

    /// <summary>
    /// Serializes the header to a byte array
    /// </summary>
    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        BitConverter.GetBytes(ProtocolVersion).CopyTo(bytes, 0);
        bytes[2] = (byte)PacketType;
        bytes[3] = Flags;
        BitConverter.GetBytes(Tick).CopyTo(bytes, 4);
        return bytes;
    }

    /// <summary>
    /// Deserializes a header from a byte array
    /// </summary>
    public static PacketHeader FromBytes(byte[] bytes, int offset = 0)
    {
        if (bytes.Length - offset < Size)
            throw new ArgumentException($"Buffer too small. Need at least {Size} bytes.");

        return new PacketHeader
        {
            ProtocolVersion = BitConverter.ToUInt16(bytes, offset),
            PacketType = (PacketType)bytes[offset + 2],
            Flags = bytes[offset + 3],
            Tick = BitConverter.ToUInt32(bytes, offset + 4)
        };
    }
}
