using System;

[Serializable]
public class zbBarPoolTreasureCsvTableData
{
	public zbBarPoolTreasureCsvRecord[] records;

	public static zbBarPoolTreasureCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBarPoolTreasureCsvTableData zbBarPoolTreasureCsvTableData2 = new zbBarPoolTreasureCsvTableData();
		zbBarPoolTreasureCsvRecord[] array = new zbBarPoolTreasureCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbBarPoolTreasureCsvRecord.Read(zbBinReader2);
		}
		zbBarPoolTreasureCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbBarPoolTreasureCsvTableData2;
	}
}
