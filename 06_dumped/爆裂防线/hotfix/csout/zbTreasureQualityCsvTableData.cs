using System;

[Serializable]
public class zbTreasureQualityCsvTableData
{
	public zbTreasureQualityCsvRecord[] records;

	public static zbTreasureQualityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTreasureQualityCsvTableData zbTreasureQualityCsvTableData2 = new zbTreasureQualityCsvTableData();
		zbTreasureQualityCsvRecord[] array = new zbTreasureQualityCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbTreasureQualityCsvRecord.Read(zbBinReader2);
		}
		zbTreasureQualityCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbTreasureQualityCsvTableData2;
	}
}
