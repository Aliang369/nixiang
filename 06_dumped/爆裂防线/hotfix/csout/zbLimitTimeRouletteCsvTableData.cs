using System;

[Serializable]
public class zbLimitTimeRouletteCsvTableData
{
	public zbLimitTimeRouletteCsvRecord[] records;

	public static zbLimitTimeRouletteCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLimitTimeRouletteCsvTableData zbLimitTimeRouletteCsvTableData2 = new zbLimitTimeRouletteCsvTableData();
		zbLimitTimeRouletteCsvRecord[] array = new zbLimitTimeRouletteCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbLimitTimeRouletteCsvRecord.Read(zbBinReader2);
		}
		zbLimitTimeRouletteCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbLimitTimeRouletteCsvTableData2;
	}
}
