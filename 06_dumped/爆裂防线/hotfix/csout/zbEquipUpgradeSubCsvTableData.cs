using System;

[Serializable]
public class zbEquipUpgradeSubCsvTableData
{
	public zbEquipUpgradeSubCsvRecord[] records;

	public static zbEquipUpgradeSubCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbEquipUpgradeSubCsvTableData zbEquipUpgradeSubCsvTableData2 = new zbEquipUpgradeSubCsvTableData();
		zbEquipUpgradeSubCsvRecord[] array = new zbEquipUpgradeSubCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbEquipUpgradeSubCsvRecord.Read(zbBinReader2);
		}
		zbEquipUpgradeSubCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbEquipUpgradeSubCsvTableData2;
	}
}
