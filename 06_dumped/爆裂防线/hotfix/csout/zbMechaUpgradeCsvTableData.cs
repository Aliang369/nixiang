using System;

[Serializable]
public class zbMechaUpgradeCsvTableData
{
	public zbMechaUpgradeCsvRecord[] records;

	public static zbMechaUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaUpgradeCsvTableData zbMechaUpgradeCsvTableData2 = new zbMechaUpgradeCsvTableData();
		zbMechaUpgradeCsvRecord[] array = new zbMechaUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbMechaUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbMechaUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbMechaUpgradeCsvTableData2;
	}
}
