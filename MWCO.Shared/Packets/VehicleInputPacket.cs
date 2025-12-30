using System;
using System.Runtime.InteropServices;

namespace MWCO.Shared.Packets;

/// <summary>
/// Client input packet - raw player inputs before smoothing
/// Sent from client to server at input rate (~50-60Hz)
/// Total size: ~24 bytes (8 header + 16 data)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VehicleInputPacket
{
    public PacketHeader Header;

    // Vehicle identification
    public ushort VehicleId;

    // Raw Input Values (before smoothing)
    public float SteerInput;
    public float ThrottleInput;
    public float BrakeInput;
    public float HandbrakeInput;
    public float ClutchInput;

    // Input Flags (1 byte for multiple booleans)
    public byte InputFlags;

    // Target gear for manual transmission (-1 for no change)
    public sbyte TargetGear;

    public const int Size = PacketHeader.Size + 26;

    // Input flag bit positions
    public const byte FLAG_START_ENGINE = 1 << 0;
    public const byte FLAG_SHIFT_UP = 1 << 1;
    public const byte FLAG_SHIFT_DOWN = 1 << 2;
    public const byte FLAG_HORN = 1 << 3;
    public const byte FLAG_LIGHTS = 1 << 4;

    public VehicleInputPacket(ushort vehicleId, uint tick)
    {
        Header = new PacketHeader(PacketType.VehicleInputUpdate, tick);
        VehicleId = vehicleId;
        SteerInput = 0;
        ThrottleInput = 0;
        BrakeInput = 0;
        HandbrakeInput = 0;
        ClutchInput = 0;
        InputFlags = 0;
        TargetGear = -1;  // No gear change
    }

    public bool GetFlag(byte flag)
    {
        return (InputFlags & flag) != 0;
    }

    public void SetFlag(byte flag, bool value)
    {
        if (value)
            InputFlags |= flag;
        else
            InputFlags &= (byte)~flag;
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

        // Inputs
        BitConverter.GetBytes(SteerInput).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(ThrottleInput).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(BrakeInput).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(HandbrakeInput).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(ClutchInput).CopyTo(bytes, offset);
        offset += 4;

        // Flags and gear
        bytes[offset++] = InputFlags;
        bytes[offset] = (byte)TargetGear;

        return bytes;
    }

    public static VehicleInputPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        if (bytes.Length - startOffset < Size)
            throw new ArgumentException($"Buffer too small. Need at least {Size} bytes.");

        var packet = new VehicleInputPacket();
        int offset = startOffset;

        // Header
        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        // Vehicle ID
        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        // Inputs
        packet.SteerInput = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.ThrottleInput = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.BrakeInput = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.HandbrakeInput = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.ClutchInput = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        // Flags and gear
        packet.InputFlags = bytes[offset++];
        packet.TargetGear = (sbyte)bytes[offset];

        return packet;
    }
}
