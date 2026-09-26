using System;

[Serializable]
public class zbLegionBossKillAwardCsvTableData
{
	public zbLegionBossKillAwardCsvRecord[] records;

	public static zbLegionBossKillAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionBossKillAwardCsvTableData zbLegionBossKillAwardCsvTableData2 = new zbLegionBossKillAwardCsvTableData();
		zbLegionBossKillAwardCsvRecord[] array = new zbLegionBossKillAwardCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbLegionBossKillAwardCsvRecord.Read(zbBinReader2);
		}
		zbLegionBossKillAwardCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbLegionBossKillAwardCsvTableData2;
	}
}
