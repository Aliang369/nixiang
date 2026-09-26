using System;

[Serializable]
public class zbHeroFollowCsvTableData
{
	public zbHeroFollowCsvRecord[] records;

	public static zbHeroFollowCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHeroFollowCsvTableData zbHeroFollowCsvTableData2 = new zbHeroFollowCsvTableData();
		zbHeroFollowCsvRecord[] array = new zbHeroFollowCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbHeroFollowCsvRecord.Read(zbBinReader2);
		}
		zbHeroFollowCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbHeroFollowCsvTableData2;
	}
}
