using System;
using System.Collections.Generic;

[Serializable]
public class zbLadderTargetAwardCsvTableData
{
	public Dictionary<int, zbLadderTargetAwardCsvRecord> records = new Dictionary<int, zbLadderTargetAwardCsvRecord>();

	public static zbLadderTargetAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLadderTargetAwardCsvTableData zbLadderTargetAwardCsvTableData2 = new zbLadderTargetAwardCsvTableData();
		Dictionary<int, zbLadderTargetAwardCsvRecord> dictionary = new Dictionary<int, zbLadderTargetAwardCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLadderTargetAwardCsvRecord.Read(zbBinReader2);
		}
		zbLadderTargetAwardCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLadderTargetAwardCsvTableData2;
	}
}
