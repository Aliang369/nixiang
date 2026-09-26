using System.Collections.Generic;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct stSkillSubPrefab
{
	public static Dictionary<int, string> InitSkillSubPrefab = new Dictionary<int, string>
	{
		{ 10001, "prefab/combat/hero/commsub/sub_10001.prefab" },
		{ 10002, "prefab/combat/hero/commsub/sub_10002.prefab" },
		{ 10005, "prefab/combat/hero/commsub/sub_10005.prefab" },
		{ 10008, "prefab/combat/hero/commsub/sub_10008.prefab" },
		{ 10102, "prefab/combat/hero/commsub/sub_10102.prefab" },
		{ 10103, "prefab/combat/hero/commsub/sub_10103.prefab" },
		{ 10104, "prefab/combat/hero/commsub/sub_10104.prefab" },
		{ 10105, "prefab/combat/hero/commsub/sub_10105.prefab" },
		{ 10106, "prefab/combat/hero/commsub/sub_10106.prefab" },
		{ 10108, "prefab/combat/hero/commsub/sub_10108.prefab" },
		{ 10110, "prefab/combat/hero/commsub/sub_10110.prefab" },
		{ 10120, "prefab/combat/hero/commsub/sub_10120.prefab" },
		{ 10121, "prefab/combat/hero/commsub/sub_10121.prefab" },
		{ 10130, "prefab/combat/hero/commsub/sub_10130.prefab" },
		{ 10201, "prefab/combat/hero/commsub/sub_10201.prefab" },
		{ 10202, "prefab/combat/hero/commsub/sub_10202.prefab" },
		{ 10203, "prefab/combat/hero/commsub/sub_10203.prefab" },
		{ 10204, "prefab/combat/hero/commsub/sub_10204.prefab" },
		{ 10205, "prefab/combat/hero/commsub/sub_10205.prefab" },
		{ 10210, "prefab/combat/hero/commsub/sub_10210.prefab" },
		{ 10211, "prefab/combat/hero/commsub/sub_10211.prefab" },
		{ 10301, "prefab/combat/hero/commsub/sub_10301.prefab" },
		{ 10302, "prefab/combat/hero/commsub/sub_10302.prefab" },
		{ 10303, "prefab/combat/hero/commsub/sub_10303.prefab" },
		{ 10401, "prefab/combat/hero/commsub/sub_10401.prefab" },
		{ 10404, "prefab/combat/hero/commsub/sub_10404.prefab" },
		{ 10405, "prefab/combat/hero/commsub/sub_10405.prefab" },
		{ 10501, "prefab/combat/hero/commsub/sub_10501.prefab" },
		{ 10502, "prefab/combat/hero/commsub/sub_10502.prefab" },
		{ 10503, "prefab/combat/hero/commsub/sub_10503.prefab" },
		{ 10601, "prefab/combat/hero/commsub/sub_10601.prefab" },
		{ 10603, "prefab/combat/hero/commsub/sub_10603.prefab" },
		{ 10701, "prefab/combat/hero/commsub/sub_10701.prefab" },
		{ 10702, "prefab/combat/hero/commsub/sub_10702.prefab" },
		{ 10703, "prefab/combat/hero/commsub/sub_10703.prefab" },
		{ 10704, "prefab/combat/hero/commsub/sub_10704.prefab" },
		{ 10705, "prefab/combat/hero/commsub/sub_10705.prefab" },
		{ 10801, "prefab/combat/hero/commsub/sub_10801.prefab" },
		{ 10802, "prefab/combat/hero/commsub/sub_10802.prefab" },
		{ 10803, "prefab/combat/hero/commsub/sub_10803.prefab" },
		{ 10804, "prefab/combat/hero/commsub/sub_10804.prefab" },
		{ 10901, "prefab/combat/hero/commsub/sub_10901.prefab" },
		{ 11001, "prefab/combat/hero/commsub/sub_11001.prefab" },
		{ 11002, "prefab/combat/hero/commsub/sub_11002.prefab" },
		{ 11003, "prefab/combat/hero/commsub/sub_11003.prefab" },
		{ 11101, "prefab/combat/hero/commsub/sub_11101.prefab" },
		{ 11102, "prefab/combat/hero/commsub/sub_11102.prefab" },
		{ 11106, "prefab/combat/hero/commsub/sub_11106.prefab" },
		{ 11107, "prefab/combat/hero/commsub/sub_11107.prefab" },
		{ 11201, "prefab/combat/hero/commsub/sub_11201.prefab" },
		{ 11207, "prefab/combat/hero/commsub/sub_11207.prefab" },
		{ 11301, "prefab/combat/hero/commsub/sub_11301.prefab" },
		{ 11302, "prefab/combat/hero/commsub/sub_11302.prefab" },
		{ 11303, "prefab/combat/hero/commsub/sub_11303.prefab" },
		{ 11401, "prefab/combat/hero/commsub/sub_11401.prefab" },
		{ 11402, "prefab/combat/hero/commsub/sub_11402.prefab" },
		{ 11403, "prefab/combat/hero/commsub/sub_11403.prefab" },
		{ 11407, "prefab/combat/hero/commsub/sub_11407.prefab" },
		{ 11410, "prefab/combat/hero/commsub/sub_11410.prefab" },
		{ 20001, "prefab/combat/hero/commsub/sub_20001.prefab" },
		{ 20002, "prefab/combat/hero/commsub/sub_20002.prefab" },
		{ 20003, "prefab/combat/hero/commsub/sub_20003.prefab" },
		{ 20004, "prefab/combat/hero/commsub/sub_20004.prefab" },
		{ 20005, "prefab/combat/hero/commsub/sub_20005.prefab" },
		{ 30201, "prefab/combat/hero/commsub/sub_30201.prefab" },
		{ 30203, "prefab/combat/hero/commsub/sub_30203.prefab" },
		{ 30204, "prefab/combat/hero/commsub/sub_30204.prefab" },
		{ 40101, "prefab/combat/hero/commsub/sub_40101.prefab" },
		{ 40102, "prefab/combat/hero/commsub/sub_40102.prefab" },
		{ 40103, "prefab/combat/hero/commsub/sub_40103.prefab" },
		{ 40201, "prefab/combat/hero/commsub/sub_40201.prefab" },
		{ 40202, "prefab/combat/hero/commsub/sub_40202.prefab" },
		{ 40203, "prefab/combat/hero/commsub/sub_40203.prefab" },
		{ 40301, "prefab/combat/hero/commsub/sub_40301.prefab" },
		{ 40303, "prefab/combat/hero/commsub/sub_40303.prefab" },
		{ 40401, "prefab/combat/hero/commsub/sub_40401.prefab" },
		{ 40402, "prefab/combat/hero/commsub/sub_40402.prefab" },
		{ 40403, "prefab/combat/hero/commsub/sub_40403.prefab" },
		{ 40501, "prefab/combat/hero/commsub/sub_40501.prefab" },
		{ 40502, "prefab/combat/hero/commsub/sub_40502.prefab" },
		{ 40503, "prefab/combat/hero/commsub/sub_40503.prefab" },
		{ 40601, "prefab/combat/hero/commsub/sub_40601.prefab" },
		{ 40602, "prefab/combat/hero/commsub/sub_40602.prefab" },
		{ 40603, "prefab/combat/hero/commsub/sub_40603.prefab" },
		{ 40701, "prefab/combat/hero/commsub/sub_40701.prefab" },
		{ 40801, "prefab/combat/hero/commsub/sub_40801.prefab" },
		{ 40901, "prefab/combat/hero/commsub/sub_40901.prefab" },
		{ 41001, "prefab/combat/hero/commsub/sub_41001.prefab" },
		{ 41101, "prefab/combat/hero/commsub/sub_41101.prefab" },
		{ 41201, "prefab/combat/hero/commsub/sub_41201.prefab" },
		{ 41301, "prefab/combat/hero/commsub/sub_41301.prefab" },
		{ 41401, "prefab/combat/hero/commsub/sub_41401.prefab" }
	};

	private static Dictionary<int, string> _prefabCache = new Dictionary<int, string>();

	public static string GetSkillSubPrefab(int subSkillId)
	{
		if (_prefabCache.TryGetValue(subSkillId, out var value))
		{
			return value;
		}
		zbSkillSubCsvRecord recordByKey = zbDataConfig.Instance.skillSubCsvTable.GetRecordByKey(subSkillId);
		string value2 = "";
		if (recordByKey != null)
		{
			InitSkillSubPrefab.TryGetValue(recordByKey.subIndex, out value2);
		}
		_prefabCache[subSkillId] = value2;
		return value2;
	}
}
