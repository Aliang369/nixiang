using System;

[Serializable]
public class zbEquipSlotUpgradeCsvTableData
{
	public zbEquipSlotUpgradeCsvRecord[] records;

	public static zbEquipSlotUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbEquipSlotUpgradeCsvTableData zbEquipSlotUpgradeCsvTableData2 = new zbEquipSlotUpgradeCsvTableData();
		zbEquipSlotUpgradeCsvRecord[] array = new zbEquipSlotUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbEquipSlotUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbEquipSlotUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbEquipSlotUpgradeCsvTableData2;
	}
}
