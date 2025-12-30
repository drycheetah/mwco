using System;

namespace MWCO.Shared.Packets;

/// <summary>
/// Low-frequency vehicle configuration updates
/// Fuel, damage, tire pressure, etc.
/// ~5Hz update rate
/// </summary>
public struct VehicleConfigPacket
{
    public PacketHeader Header;
    public ushort VehicleId;

    // Fuel system
    public float FuelLevel;           // Liters
    public float FuelConsumption;     // L/100km

    // Tire pressures (bar)
    public float TirePressureFL;
    public float TirePressureFR;
    public float TirePressureRL;
    public float TirePressureRR;

    // Damage state (0-1, where 1 = fully damaged)
    public float EngineDamage;
    public float BodyDamage;

    public const int Size = PacketHeader.Size + 2 + 32;

    public VehicleConfigPacket(ushort vehicleId, uint tick)
    {
        Header = new PacketHeader(PacketType.FuelUpdate, tick);
        VehicleId = vehicleId;
        FuelLevel = 0;
        FuelConsumption = 0;
        TirePressureFL = TirePressureFR = TirePressureRL = TirePressureRR = 2.0f;
        EngineDamage = BodyDamage = 0;
    }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[Size];
        int offset = 0;

        Buffer.BlockCopy(Header.ToBytes(), 0, bytes, offset, PacketHeader.Size);
        offset += PacketHeader.Size;

        BitConverter.GetBytes(VehicleId).CopyTo(bytes, offset);
        offset += 2;

        BitConverter.GetBytes(FuelLevel).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(FuelConsumption).CopyTo(bytes, offset);
        offset += 4;

        BitConverter.GetBytes(TirePressureFL).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(TirePressureFR).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(TirePressureRL).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(TirePressureRR).CopyTo(bytes, offset);
        offset += 4;

        BitConverter.GetBytes(EngineDamage).CopyTo(bytes, offset);
        offset += 4;
        BitConverter.GetBytes(BodyDamage).CopyTo(bytes, offset);

        return bytes;
    }

    public static VehicleConfigPacket FromBytes(byte[] bytes, int startOffset = 0)
    {
        var packet = new VehicleConfigPacket();
        int offset = startOffset;

        packet.Header = PacketHeader.FromBytes(bytes, offset);
        offset += PacketHeader.Size;

        packet.VehicleId = BitConverter.ToUInt16(bytes, offset);
        offset += 2;

        packet.FuelLevel = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.FuelConsumption = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        packet.TirePressureFL = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.TirePressureFR = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.TirePressureRL = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.TirePressureRR = BitConverter.ToSingle(bytes, offset);
        offset += 4;

        packet.EngineDamage = BitConverter.ToSingle(bytes, offset);
        offset += 4;
        packet.BodyDamage = BitConverter.ToSingle(bytes, offset);

        return packet;
    }
}
