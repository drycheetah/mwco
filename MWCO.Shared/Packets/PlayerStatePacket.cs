using System;
using System.Runtime.InteropServices;

namespace MWCO.Shared.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerStatePacket
{
    public PacketHeader Header;
    public ushort PlayerId;
    public uint Tick;

    // Transform
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float RotationX;
    public float RotationY;
    public float RotationZ;
    public float RotationW;

    // Animation states
    public byte IsWalking;
    public byte IsRunning;
    public byte IsCrouching;
    public byte IsInVehicle;

    public PlayerStatePacket(ushort playerId, uint tick)
    {
        Header = new PacketHeader(PacketType.PlayerState, tick);
        PlayerId = playerId;
        Tick = tick;
        PositionX = PositionY = PositionZ = 0f;
        RotationX = RotationY = RotationZ = 0f;
        RotationW = 1f;
        IsWalking = IsRunning = IsCrouching = IsInVehicle = 0;
    }

    public static PlayerStatePacket FromBytes(byte[] data)
    {
        int size = Marshal.SizeOf(typeof(PlayerStatePacket));
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, 0, ptr, Math.Min(size, data.Length));
        PlayerStatePacket packet = (PlayerStatePacket)Marshal.PtrToStructure(ptr, typeof(PlayerStatePacket));
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
