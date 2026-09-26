using System;
using System.Collections.Generic;

[Serializable]
public class zbPeerageTaskCsvTableData
{
	public Dictionary<int, zbPeerageTaskCsvRecord> records = new Dictionary<int, zbPeerageTaskCsvRecord>();

	public static zbPeerageTaskCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPeerageTaskCsvTableData zbPeerageTaskCsvTableData2 = new zbPeerageTaskCsvTableData();
		Dictionary<int, zbPeerageTaskCsvRecord> dictionary = new Dictionary<int, zbPeerageTaskCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPeerageTaskCsvRecord.Read(zbBinReader2);
		}
		zbPeerageTaskCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPeerageTaskCsvTableData2;
	}
}
