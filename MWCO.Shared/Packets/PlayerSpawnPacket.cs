using System;
using System.Runtime.InteropServices;

namespace MWCO.Shared.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerSpawnPacket
{
    public PacketHeader Header;
    public ushort PlayerId;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] PlayerName;

    public float SpawnPositionX;
    public float SpawnPositionY;
    public float SpawnPositionZ;

    public PlayerSpawnPacket(ushort playerId, string playerName)
    {
        Header = new PacketHeader(PacketType.PlayerSpawn, 0);
        PlayerId = playerId;
        PlayerName = new byte[32];

        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(playerName);
        int copyLen = Math.Min(nameBytes.Length, 31);
        Array.Copy(nameBytes, PlayerName, copyLen);

        SpawnPositionX = SpawnPositionY = SpawnPositionZ = 0f;
    }

    public string GetPlayerName()
    {
        int len = Array.IndexOf(PlayerName, (byte)0);
        if (len < 0) len = PlayerName.Length;
        return System.Text.Encoding.UTF8.GetString(PlayerName, 0, len);
    }

    public static PlayerSpawnPacket FromBytes(byte[] data)
    {
        int size = Marshal.SizeOf(typeof(PlayerSpawnPacket));
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, 0, ptr, Math.Min(size, data.Length));
        PlayerSpawnPacket packet = (PlayerSpawnPacket)Marshal.PtrToStructure(ptr, typeof(PlayerSpawnPacket));
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
