using System;
using System.Collections.Generic;

[Serializable]
public class zbTeamAttrCsvTableData
{
	public Dictionary<int, zbTeamAttrCsvRecord> records = new Dictionary<int, zbTeamAttrCsvRecord>();

	public static zbTeamAttrCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTeamAttrCsvTableData zbTeamAttrCsvTableData2 = new zbTeamAttrCsvTableData();
		Dictionary<int, zbTeamAttrCsvRecord> dictionary = new Dictionary<int, zbTeamAttrCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTeamAttrCsvRecord.Read(zbBinReader2);
		}
		zbTeamAttrCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTeamAttrCsvTableData2;
	}
}
