using System;
using System.Text;

namespace MWCO.Shared.Packets;

/// <summary>
/// World object synchronization
/// NPCs, items, environmental objects
/// </summary>
public struct WorldObjectPacket
{
    public PacketHeader Header;
    public uint ObjectId;
    public string ObjectName;
    public byte ObjectType;  // 0=static, 1=physics, 2=NPC, 3=item

    // Transform
    public float PosX, PosY, PosZ;
    public float RotX, RotY, RotZ, RotW;
    public float VelX, VelY, VelZ;

    public const int MaxNameLength = 64;

    public WorldObjectPacket(uint objectId, string objectName, byte objectType, PacketType packetType, uint tick)
    {
        Header = new PacketHeader(packetType, tick);
        ObjectId = objectId;
        ObjectName = objectName.Length > MaxNameLength ? objectName.Substring(0, MaxNameLength) : objectName;
        ObjectType = objectType;
        PosX = PosY = PosZ = 0;
        RotX = RotY = RotZ = 0;
        RotW = 1;
        VelX = VelY = VelZ = 0;
    }

    public byte[] ToBytes()
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(ObjectName);
        byte nameLength = (byte)Math.Min(nameBytes.Length, MaxNameLength);

        byte[] bytes = new byte[PacketHeader.Size + 4 + 1 + nameLength + 1 + 40];
        int offset = 0;

        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        BitConverter.GetBytes(ObjectId).CopyTo(bytes, offset);
        offset += 4;

        bytes[offset++] = nameLength;
        Buffer.BlockCopy(nameBytes, 0, bytes, offset, nameLength);
        offset += nameLength;

        bytes[offset++] = ObjectType;

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
        BitConverter.GetBytes(VelX).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(VelY).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(VelZ).CopyTo(bytes, offset);

        return bytes;
    }

    public static WorldObjectPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new WorldObjectPacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.ObjectId = BitConverter.ToUInt32(bytes, offset);
        offset += 4;

        byte nameLength = bytes[offset++];
        packet.ObjectName = Encoding.UTF8.GetString(bytes, offset, nameLength);
        offset += nameLength;

        packet.ObjectType = bytes[offset++];

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
        packet.VelX = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.VelY = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.VelZ = BitConverter.ToSingle(bytes, offset);

        return packet;
    }
}

/// <summary>
/// Time and weather synchronization
/// </summary>
public struct TimeWeatherPacket
{
    public PacketHeader Header;

    // Time (game time)
    public uint GameTimeTicks;    // Total ticks since game start
    public byte Hour;             // 0-23
    public byte Minute;           // 0-59
    public byte DayOfWeek;        // 0-6

    // Weather
    public byte WeatherType;      // 0=clear, 1=cloudy, 2=rain, 3=storm, 4=snow
    public float Temperature;     // Celsius
    public float Visibility;      // 0-1

    public const int Size = PacketHeader.Size + 15;

    public TimeWeatherPacket(uint tick)
    {
        Header = new PacketHeader(PacketType.TimeWeatherSync, tick);
        GameTimeTicks = 0;
        Hour = 12;
        Minute = 0;
        DayOfWeek = 0;
        WeatherType = 0;
        Temperature = 20.0f;
        Visibility = 1.0f;
    }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        int offset = 0;

        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        BitConverter.GetBytes(GameTimeTicks).CopyTo(bytes, offset);
        offset += 4;
        bytes[offset++] = Hour;
        bytes[offset++] = Minute;
        bytes[offset++] = DayOfWeek;
        bytes[offset++] = WeatherType;
        BitConverter.GetBytes(Temperature).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(Visibility).CopyTo(bytes, offset);

        return bytes;
    }

    public static TimeWeatherPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new TimeWeatherPacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.GameTimeTicks = BitConverter.ToUInt32(bytes, offset);
        offset += 4;
        packet.Hour = bytes[offset++];
        packet.Minute = bytes[offset++];
        packet.DayOfWeek = bytes[offset++];
        packet.WeatherType = bytes[offset++];
        packet.Temperature = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.Visibility = BitConverter.ToSingle(bytes, offset);

        return packet;
    }
}
