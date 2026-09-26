using System;
using System.Collections.Generic;

public class zbModRegister
{
	private Dictionary<int, Func<zbModBase>> modRegistry = new Dictionary<int, Func<zbModBase>>();

	private void RegisterMod(int modIndex, Func<zbModBase> modConstructor)
	{
		if (!modRegistry.ContainsKey(modIndex))
		{
			modRegistry.Add(modIndex, modConstructor);
		}
	}

	public zbModBase CreateMod(int modId)
	{
		zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(modId);
		if (recordByKey == null)
		{
			return null;
		}
		if (modRegistry.ContainsKey(recordByKey.modIndex))
		{
			return modRegistry[recordByKey.modIndex]();
		}
		return null;
	}

	public void Init()
	{
		RegisterMod(0, () => new zbModIndex0());
		RegisterMod(1000, () => new zbModIndex1000());
		RegisterMod(1001, () => new zbModIndex1001());
		RegisterMod(1002, () => new zbModIndex1002());
		RegisterMod(1003, () => new zbModIndex1003());
		RegisterMod(1004, () => new zbModIndex1004());
		RegisterMod(1005, () => new zbModIndex1005());
		RegisterMod(1007, () => new zbModIndex1007());
		RegisterMod(1008, () => new zbModIndex1008());
		RegisterMod(1009, () => new zbModIndex1009());
		RegisterMod(1010, () => new zbModIndex1010());
		RegisterMod(1012, () => new zbModIndex1012());
		RegisterMod(1015, () => new zbModIndex1015());
		RegisterMod(1016, () => new zbModIndex1016());
		RegisterMod(1019, () => new zbModIndex1019());
		RegisterMod(1022, () => new zbModIndex1022());
		RegisterMod(1023, () => new zbModIndex1023());
		RegisterMod(1024, () => new zbModIndex1024());
		RegisterMod(1025, () => new zbModIndex1025());
		RegisterMod(1026, () => new zbModIndex1026());
		RegisterMod(1027, () => new zbModIndex1027());
		RegisterMod(1028, () => new zbModIndex1028());
		RegisterMod(1029, () => new zbModIndex1029());
		RegisterMod(1030, () => new zbModIndex1030());
		RegisterMod(1031, () => new zbModIndex1031());
		RegisterMod(1032, () => new zbModIndex1032());
		RegisterMod(1033, () => new zbModIndex1033());
		RegisterMod(1034, () => new zbModIndex1034());
		RegisterMod(1035, () => new zbModIndex1035());
		RegisterMod(1036, () => new zbModIndex1036());
		RegisterMod(1037, () => new zbModIndex1037());
		RegisterMod(1039, () => new zbModIndex1039());
		RegisterMod(1040, () => new zbModIndex1040());
		RegisterMod(1041, () => new zbModIndex1041());
		RegisterMod(1042, () => new zbModIndex1042());
		RegisterMod(1043, () => new zbModIndex1043());
		RegisterMod(1044, () => new zbModIndex1044());
		RegisterMod(1045, () => new zbModIndex1045());
		RegisterMod(1046, () => new zbModIndex1046());
		RegisterMod(1047, () => new zbModIndex1047());
		RegisterMod(1048, () => new zbModIndex1048());
		RegisterMod(1049, () => new zbModIndex1049());
		RegisterMod(1050, () => new zbModIndex1050());
		RegisterMod(1051, () => new zbModIndex1051());
		RegisterMod(1052, () => new zbModIndex1052());
		RegisterMod(1053, () => new zbModIndex1053());
		RegisterMod(1054, () => new zbModIndex1054());
		RegisterMod(1055, () => new zbModIndex1055());
		RegisterMod(1056, () => new zbModIndex1056());
		RegisterMod(1057, () => new zbModIndex1057());
		RegisterMod(1058, () => new zbModIndex1058());
		RegisterMod(1059, () => new zbModIndex1059());
		RegisterMod(1060, () => new zbModIndex1060());
		RegisterMod(1062, () => new zbModIndex1062());
		RegisterMod(1063, () => new zbModIndex1063());
		RegisterMod(1064, () => new zbModIndex1064());
		RegisterMod(1065, () => new zbModIndex1065());
		RegisterMod(1066, () => new zbModIndex1066());
		RegisterMod(1067, () => new zbModIndex1067());
		RegisterMod(1068, () => new zbModIndex1068());
		RegisterMod(1069, () => new zbModIndex1069());
		RegisterMod(1076, () => new zbModIndex1076());
		RegisterMod(1077, () => new zbModIndex1077());
		RegisterMod(1079, () => new zbModIndex1079());
		RegisterMod(1080, () => new zbModIndex1080());
		RegisterMod(1081, () => new zbModIndex1081());
		RegisterMod(1082, () => new zbModIndex1082());
		RegisterMod(2011, () => new zbModIndex2011());
		RegisterMod(2012, () => new zbModIndex2012());
		RegisterMod(2013, () => new zbModIndex2013());
		RegisterMod(2014, () => new zbModIndex2014());
		RegisterMod(2021, () => new zbModIndex2021());
		RegisterMod(2022, () => new zbModIndex2022());
		RegisterMod(2041, () => new zbModIndex2041());
		RegisterMod(2061, () => new zbModIndex2061());
		RegisterMod(2071, () => new zbModIndex2071());
		RegisterMod(2081, () => new zbModIndex2081());
		RegisterMod(2082, () => new zbModIndex2082());
		RegisterMod(2091, () => new zbModIndex2091());
		RegisterMod(2092, () => new zbModIndex2092());
		RegisterMod(2093, () => new zbModIndex2093());
		RegisterMod(2101, () => new zbModIndex2101());
		RegisterMod(2111, () => new zbModIndex2111());
		RegisterMod(2112, () => new zbModIndex2112());
		RegisterMod(2113, () => new zbModIndex2113());
		RegisterMod(2114, () => new zbModIndex2114());
		RegisterMod(2121, () => new zbModIndex2121());
		RegisterMod(2122, () => new zbModIndex2122());
		RegisterMod(2123, () => new zbModIndex2123());
		RegisterMod(2124, () => new zbModIndex2124());
		RegisterMod(2132, () => new zbModIndex2132());
		RegisterMod(2133, () => new zbModIndex2133());
		RegisterMod(2134, () => new zbModIndex2134());
		RegisterMod(2141, () => new zbModIndex2141());
		RegisterMod(2142, () => new zbModIndex2142());
		RegisterMod(2143, () => new zbModIndex2143());
		RegisterMod(2144, () => new zbModIndex2144());
	}
}
