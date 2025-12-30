using System;
using System.Text;

namespace MWCO.Shared.Packets;

/// <summary>
/// Initial connection request from client to server
/// </summary>
public struct ConnectionRequestPacket
{
    public PacketHeader Header;
    public string PlayerName;
    public ushort ClientVersion;

    public const int MaxNameLength = 32;

    public ConnectionRequestPacket(string playerName, uint tick = 0)
    {
        Header = new PacketHeader(PacketType.ConnectionRequest, tick);
        PlayerName = playerName.Length > MaxNameLength ? playerName.Substring(0, MaxNameLength) : playerName;
        ClientVersion = NetworkConfig.ProtocolVersion;
    }

    public byte[] ToBytes()
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(PlayerName);
        byte nameLength = (byte)Math.Min(nameBytes.Length, MaxNameLength);

        byte[] bytes = new byte[PacketHeader.Size + 2 + 1 + nameLength];
        int offset = 0;

        // Header
        byte[] headerBytes = Header.ToBytes();
        Buffer.BlockCopy(headerBytes, 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        // Client version
        BitConverter.GetBytes(ClientVersion).CopyTo(bytes, offset);
        offset += 2;

        // Name length + name
        bytes[offset++] = nameLength;
        Buffer.BlockCopy(nameBytes, 0, bytes, offset, nameLength);

        return bytes;
    }

    public static ConnectionRequestPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new ConnectionRequestPacket();
        int offset = startOffset;

        // Header
        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        // Client version
        packet.ClientVersion = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        // Name
        byte nameLength = bytes[offset++];
        packet.PlayerName = Encoding.UTF8.GetString(bytes, offset, nameLength);

        return packet;
    }
}
