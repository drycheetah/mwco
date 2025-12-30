using System;
using System.Text;

namespace MWCO.Shared.Packets;

/// <summary>
/// Part attachment/detachment synchronization
/// For My Summer Car's part assembly system
/// </summary>
public struct PartSyncPacket
{
    public PacketHeader Header;
    public ushort VehicleId;
    public ushort PartId;
    public string PartName;
    public byte Attached;         // 1 = attached, 0 = detached
    public byte BoltCount;        // Number of bolts installed
    public byte MaxBolts;         // Maximum bolts for this part

    // Transform when attached
    public float LocalPosX;
    public float LocalPosY;
    public float LocalPosZ;
    public float LocalRotX;
    public float LocalRotY;
    public float LocalRotZ;
    public float LocalRotW;

    public const int MaxPartNameLength = 64;

    public PartSyncPacket(ushort vehicleId, ushort partId, string partName, bool attached, uint tick)
    {
        Header = new PacketHeader(attached ? PacketType.PartAttach : PacketType.PartDetach, tick);
        VehicleId = vehicleId;
        PartId = partId;
        PartName = partName.Length > MaxPartNameLength ? partName.Substring(0, MaxPartNameLength) : partName;
        Attached = (byte)(attached ? 1 : 0);
        BoltCount = 0;
        MaxBolts = 0;
        LocalPosX = LocalPosY = LocalPosZ = 0;
        LocalRotX = LocalRotY = LocalRotZ = 0;
        LocalRotW = 1;
    }

    public byte[] ToBytes()
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(PartName);
        byte nameLength = (byte)Math.Min(nameBytes.Length, MaxPartNameLength);

        byte[] bytes = new byte[PacketHeader.Size + 2 + 2 + 1 + nameLength + 1 + 1 + 1 + 28];
        int offset = 0;

        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        BitConverter.GetBytes(VehicleId).CopyTo(bytes, offset);
        offset += 2;
        BitConverter.GetBytes(PartId).CopyTo(bytes, offset);
        offset += 2;

        bytes[offset++] = nameLength;
        Buffer.BlockCopy(nameBytes, 0, bytes, offset, nameLength);
        offset += nameLength;

        bytes[offset++] = Attached;
        bytes[offset++] = BoltCount;
        bytes[offset++] = MaxBolts;

        BitConverter.GetBytes(LocalPosX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(LocalPosY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(LocalPosZ).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(LocalRotX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(LocalRotY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(LocalRotZ).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(LocalRotW).CopyTo(bytes, offset);

        return bytes;
    }

    public static PartSyncPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new PartSyncPacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;
        packet.PartId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        byte nameLength = bytes[offset++];
        packet.PartName = Encoding.UTF8.GetString(bytes, offset, nameLength);
        offset += nameLength;

        packet.Attached = bytes[offset++];
        packet.BoltCount = bytes[offset++];
        packet.MaxBolts = bytes[offset++];

        packet.LocalPosX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.LocalPosY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.LocalPosZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.LocalRotX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.LocalRotY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.LocalRotZ = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.LocalRotW = BitConverter.ToSingle(bytes, offset);

        return packet;
    }
}
