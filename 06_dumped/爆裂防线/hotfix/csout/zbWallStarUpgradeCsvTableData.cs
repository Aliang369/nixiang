using System;

[Serializable]
public class zbWallStarUpgradeCsvTableData
{
	public zbWallStarUpgradeCsvRecord[] records;

	public static zbWallStarUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallStarUpgradeCsvTableData zbWallStarUpgradeCsvTableData2 = new zbWallStarUpgradeCsvTableData();
		zbWallStarUpgradeCsvRecord[] array = new zbWallStarUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbWallStarUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbWallStarUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbWallStarUpgradeCsvTableData2;
	}
}
