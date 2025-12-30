using System;
using System.Runtime.InteropServices;

namespace MWCO.Shared.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerDespawnPacket
{
    public PacketHeader Header;
    public ushort PlayerId;

    public PlayerDespawnPacket(ushort playerId)
    {
        Header = new PacketHeader(PacketType.PlayerDespawn, 0);
        PlayerId = playerId;
    }

    public static PlayerDespawnPacket FromBytes(byte[] data)
    {
        int size = Marshal.SizeOf(typeof(PlayerDespawnPacket));
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, 0, ptr, Math.Min(size, data.Length));
        PlayerDespawnPacket packet = (PlayerDespawnPacket)Marshal.PtrToStructure(ptr, typeof(PlayerDespawnPacket));
        Marshal.FreeHGlobal(ptr);
        return packet;
    }

    public byte[] ToBytes()
    {
        int size = Marshal.SizeOf(this);
        byte[] data = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(this, ptr, false);
        Marshal.Copy(ptr, data, 0, size);
        Marshal.FreeHGlobal(ptr);
        return data;
    }
}
