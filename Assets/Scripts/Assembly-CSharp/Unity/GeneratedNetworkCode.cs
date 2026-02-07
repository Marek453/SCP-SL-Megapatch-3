// Unity.GeneratedNetworkCode
using System.Runtime.InteropServices;
using UnityEngine.Networking;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class GeneratedNetworkCode
{

    public static PlayerStats.HitInfo _ReadHitInfo_PlayerStats(NetworkReader reader)
    {
        PlayerStats.HitInfo result = default(PlayerStats.HitInfo);
        result.amount = reader.ReadSingle();
        result.tool = reader.ReadString();
        result.time = (int)reader.ReadPackedUInt32();
        result.attacker = reader.ReadString();
        result.plyID = (int)reader.ReadPackedUInt32();
        return result;
    }

    public static void _WriteHitInfo_PlayerStats(NetworkWriter writer, PlayerStats.HitInfo value)
    {
        writer.Write(value.amount);
        writer.Write(value.tool);
        writer.WritePackedUInt32((uint)value.time);
        writer.Write(value.attacker);
        writer.WritePackedUInt32((uint)value.plyID);
    }

    public static void _WritePickupInfo_Pickup(NetworkWriter writer, Pickup.PickupInfo value)
    {
        writer.Write(value.position);
        writer.Write(value.rotation);
        writer.WritePackedUInt32((uint)value.itemId);
        writer.Write(value.durability);
        writer.WritePackedUInt32((uint)value.ownerPlayerID);
    }

    public static Pickup.PickupInfo _ReadPickupInfo_Pickup(NetworkReader reader)
    {
        Pickup.PickupInfo result = default(Pickup.PickupInfo);
        result.position = reader.ReadVector3();
        result.rotation = reader.ReadQuaternion();
        result.itemId = (int)reader.ReadPackedUInt32();
        result.durability = reader.ReadSingle();
        result.ownerPlayerID = (int)reader.ReadPackedUInt32();
        return result;
    }

    public static PlayerPositionData _ReadPlayerPositionData_None(NetworkReader reader)
    {
        PlayerPositionData result = default(PlayerPositionData);
        result.position = reader.ReadVector3();
        result.rotation = reader.ReadSingle();
        result.playerID = (int)reader.ReadPackedUInt32();
        return result;
    }

    public static PlayerPositionData[] _ReadArrayPlayerPositionData_None(NetworkReader reader)
    {
        int num = reader.ReadUInt16();
        if (num == 0)
        {
            return new PlayerPositionData[0];
        }
        PlayerPositionData[] array = new PlayerPositionData[num];
        for (int i = 0; i < num; i++)
        {
            array[i] = _ReadPlayerPositionData_None(reader);
        }
        return array;
    }

    public static void _WritePlayerPositionData_None(NetworkWriter writer, PlayerPositionData value)
    {
        writer.Write(value.position);
        writer.Write(value.rotation);
        writer.WritePackedUInt32((uint)value.playerID);
    }

    public static void _WriteArrayPlayerPositionData_None(NetworkWriter writer, PlayerPositionData[] value)
    {
        if (value == null)
        {
            writer.Write((ushort)0);
            return;
        }
        ushort value2 = (ushort)value.Length;
        writer.Write(value2);
        for (ushort num = 0; num < value.Length; num = (ushort)(num + 1))
        {
            _WritePlayerPositionData_None(writer, value[num]);
        }
    }

    public static void _WriteInfo_Ragdoll(NetworkWriter writer, Ragdoll.Info value)
    {
        writer.Write(value.ownerHLAPI_id);
        writer.Write(value.steamClientName);
        _WriteHitInfo_PlayerStats(writer, value.deathCause);
        writer.WritePackedUInt32((uint)value.charclass);
    }

    public static Ragdoll.Info _ReadInfo_Ragdoll(NetworkReader reader)
    {
        Ragdoll.Info result = default(Ragdoll.Info);
        result.ownerHLAPI_id = reader.ReadString();
        result.steamClientName = reader.ReadString();
        result.deathCause = _ReadHitInfo_PlayerStats(reader);
        result.charclass = (int)reader.ReadPackedUInt32();
        return result;
    }

    public static RoundSummary.SumInfo_ClassList _ReadSumInfo_ClassList_RoundSummary(NetworkReader reader)
    {
        RoundSummary.SumInfo_ClassList result = default(RoundSummary.SumInfo_ClassList);
        result.class_ds = (int)reader.ReadPackedUInt32();
        result.scientists = (int)reader.ReadPackedUInt32();
        result.chaos_insurgents = (int)reader.ReadPackedUInt32();
        result.mtf_and_guards = (int)reader.ReadPackedUInt32();
        result.scps_except_zombies = (int)reader.ReadPackedUInt32();
        result.zombies = (int)reader.ReadPackedUInt32();
        result.warhead_kills = (int)reader.ReadPackedUInt32();
        result.time = (int)reader.ReadPackedUInt32();
        return result;
    }

    public static void _WriteSumInfo_ClassList_RoundSummary(NetworkWriter writer, RoundSummary.SumInfo_ClassList value)
    {
        writer.WritePackedUInt32((uint)value.class_ds);
        writer.WritePackedUInt32((uint)value.scientists);
        writer.WritePackedUInt32((uint)value.chaos_insurgents);
        writer.WritePackedUInt32((uint)value.mtf_and_guards);
        writer.WritePackedUInt32((uint)value.scps_except_zombies);
        writer.WritePackedUInt32((uint)value.zombies);
        writer.WritePackedUInt32((uint)value.warhead_kills);
        writer.WritePackedUInt32((uint)value.time);
    }

    public static void _WriteOffset_None(NetworkWriter writer, Offset value)
    {
        writer.Write(value.position);
        writer.Write(value.rotation);
        writer.Write(value.scale);
    }

    public static Offset _ReadOffset_None(NetworkReader reader)
    {
        Offset result = default(Offset);
        result.position = reader.ReadVector3();
        result.rotation = reader.ReadVector3();
        result.scale = reader.ReadVector3();
        return result;
    }
}
