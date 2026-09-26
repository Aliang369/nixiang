using System;

[Serializable]
public class zbPlayerGunUpgradeCsvTableData
{
	public zbPlayerGunUpgradeCsvRecord[] records;

	public static zbPlayerGunUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerGunUpgradeCsvTableData zbPlayerGunUpgradeCsvTableData2 = new zbPlayerGunUpgradeCsvTableData();
		zbPlayerGunUpgradeCsvRecord[] array = new zbPlayerGunUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbPlayerGunUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbPlayerGunUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbPlayerGunUpgradeCsvTableData2;
	}
}
