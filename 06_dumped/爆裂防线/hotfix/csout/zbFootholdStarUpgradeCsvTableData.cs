using System;

[Serializable]
public class zbFootholdStarUpgradeCsvTableData
{
	public zbFootholdStarUpgradeCsvRecord[] records;

	public static zbFootholdStarUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFootholdStarUpgradeCsvTableData zbFootholdStarUpgradeCsvTableData2 = new zbFootholdStarUpgradeCsvTableData();
		zbFootholdStarUpgradeCsvRecord[] array = new zbFootholdStarUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbFootholdStarUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbFootholdStarUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbFootholdStarUpgradeCsvTableData2;
	}
}
