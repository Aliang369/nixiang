using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionBossChallengeCsvTableData
{
	public Dictionary<int, zbLegionBossChallengeCsvRecord> records = new Dictionary<int, zbLegionBossChallengeCsvRecord>();

	public static zbLegionBossChallengeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionBossChallengeCsvTableData zbLegionBossChallengeCsvTableData2 = new zbLegionBossChallengeCsvTableData();
		Dictionary<int, zbLegionBossChallengeCsvRecord> dictionary = new Dictionary<int, zbLegionBossChallengeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionBossChallengeCsvRecord.Read(zbBinReader2);
		}
		zbLegionBossChallengeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionBossChallengeCsvTableData2;
	}
}
