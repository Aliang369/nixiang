using System;
using System.Collections.Generic;

[Serializable]
public class zbWallSubjectZhanLingCsvTableData
{
	public Dictionary<int, zbWallSubjectZhanLingCsvRecord> records = new Dictionary<int, zbWallSubjectZhanLingCsvRecord>();

	public static zbWallSubjectZhanLingCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallSubjectZhanLingCsvTableData zbWallSubjectZhanLingCsvTableData2 = new zbWallSubjectZhanLingCsvTableData();
		Dictionary<int, zbWallSubjectZhanLingCsvRecord> dictionary = new Dictionary<int, zbWallSubjectZhanLingCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWallSubjectZhanLingCsvRecord.Read(zbBinReader2);
		}
		zbWallSubjectZhanLingCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWallSubjectZhanLingCsvTableData2;
	}
}
