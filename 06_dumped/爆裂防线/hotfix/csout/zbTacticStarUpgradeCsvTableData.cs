using System;

[Serializable]
public class zbTacticStarUpgradeCsvTableData
{
	public zbTacticStarUpgradeCsvRecord[] records;

	public static zbTacticStarUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTacticStarUpgradeCsvTableData zbTacticStarUpgradeCsvTableData2 = new zbTacticStarUpgradeCsvTableData();
		zbTacticStarUpgradeCsvRecord[] array = new zbTacticStarUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbTacticStarUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbTacticStarUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbTacticStarUpgradeCsvTableData2;
	}
}
