using System;
using System.Text;

namespace MWCO.Shared.Packets;

/// <summary>
/// Server response to connection request
/// </summary>
public struct ConnectionResponsePacket
{
    public PacketHeader Header;
    public ushort AssignedPlayerId;
    public ushort AssignedVehicleId;
    public byte Accepted;  // 1 = accepted, 0 = denied
    public string Message;

    public const int MaxMessageLength = 128;

    public ConnectionResponsePacket(bool accepted, ushort playerId, ushort vehicleId, string message, uint tick)
    {
        Header = new PacketHeader(accepted ? PacketType.ConnectionAccepted : PacketType.ConnectionDenied, tick);
        AssignedPlayerId = playerId;
        AssignedVehicleId = vehicleId;
        Accepted = (byte)(accepted ? 1 : 0);
        Message = message.Length > MaxMessageLength ? message.Substring(0, MaxMessageLength) : message;
    }

    public byte[] ToBytes()
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(Message);
        byte messageLength = (byte)Math.Min(messageBytes.Length, MaxMessageLength);

        byte[] bytes = new byte[PacketHeader.Size + 2 + 2 + 1 + 1 + messageLength];
        int offset = 0;

        // Header
        byte[] headerBytes = Header.ToBytes();
        Buffer.BlockCopy(headerBytes, 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        // Player and vehicle IDs
        BitConverter.GetBytes(AssignedPlayerId).CopyTo(bytes, offset);
        offset += 2;
        BitConverter.GetBytes(AssignedVehicleId).CopyTo(bytes, offset);
        offset += 2;

        // Accepted flag
        bytes[offset++] = Accepted;

        // Message length + message
        bytes[offset++] = messageLength;
        Buffer.BlockCopy(messageBytes, 0, bytes, offset, messageLength);

        return bytes;
    }

    public static ConnectionResponsePacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new ConnectionResponsePacket();
        int offset = startOffset;

        // Header
        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        // Player and vehicle IDs
        packet.AssignedPlayerId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;
        packet.AssignedVehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        // Accepted flag
        packet.Accepted = bytes[offset++];

        // Message
        byte messageLength = bytes[offset++];
        packet.Message = Encoding.UTF8.GetString(bytes, offset, messageLength);

        return packet;
    }
}
