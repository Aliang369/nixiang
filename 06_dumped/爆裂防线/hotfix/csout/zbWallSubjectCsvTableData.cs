using System;
using System.Collections.Generic;

[Serializable]
public class zbWallSubjectCsvTableData
{
	public Dictionary<int, zbWallSubjectCsvRecord> records = new Dictionary<int, zbWallSubjectCsvRecord>();

	public static zbWallSubjectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallSubjectCsvTableData zbWallSubjectCsvTableData2 = new zbWallSubjectCsvTableData();
		Dictionary<int, zbWallSubjectCsvRecord> dictionary = new Dictionary<int, zbWallSubjectCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWallSubjectCsvRecord.Read(zbBinReader2);
		}
		zbWallSubjectCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWallSubjectCsvTableData2;
	}
}
