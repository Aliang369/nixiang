using System;

[Serializable]
public class zbBarPoolTreasurePlusCsvTableData
{
	public zbBarPoolTreasurePlusCsvRecord[] records;

	public static zbBarPoolTreasurePlusCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBarPoolTreasurePlusCsvTableData zbBarPoolTreasurePlusCsvTableData2 = new zbBarPoolTreasurePlusCsvTableData();
		zbBarPoolTreasurePlusCsvRecord[] array = new zbBarPoolTreasurePlusCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbBarPoolTreasurePlusCsvRecord.Read(zbBinReader2);
		}
		zbBarPoolTreasurePlusCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbBarPoolTreasurePlusCsvTableData2;
	}
}
