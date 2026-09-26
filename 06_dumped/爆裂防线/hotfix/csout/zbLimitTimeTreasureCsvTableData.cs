using System;

[Serializable]
public class zbLimitTimeTreasureCsvTableData
{
	public zbLimitTimeTreasureCsvRecord[] records;

	public static zbLimitTimeTreasureCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLimitTimeTreasureCsvTableData zbLimitTimeTreasureCsvTableData2 = new zbLimitTimeTreasureCsvTableData();
		zbLimitTimeTreasureCsvRecord[] array = new zbLimitTimeTreasureCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbLimitTimeTreasureCsvRecord.Read(zbBinReader2);
		}
		zbLimitTimeTreasureCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbLimitTimeTreasureCsvTableData2;
	}
}
