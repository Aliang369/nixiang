using System;

[Serializable]
public class zbPlayerSuitUpgradeCsvTableData
{
	public zbPlayerSuitUpgradeCsvRecord[] records;

	public static zbPlayerSuitUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerSuitUpgradeCsvTableData zbPlayerSuitUpgradeCsvTableData2 = new zbPlayerSuitUpgradeCsvTableData();
		zbPlayerSuitUpgradeCsvRecord[] array = new zbPlayerSuitUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbPlayerSuitUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbPlayerSuitUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbPlayerSuitUpgradeCsvTableData2;
	}
}
