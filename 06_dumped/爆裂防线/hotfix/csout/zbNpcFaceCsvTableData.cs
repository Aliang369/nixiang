using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcFaceCsvTableData
{
	public Dictionary<int, zbNpcFaceCsvRecord> records = new Dictionary<int, zbNpcFaceCsvRecord>();

	public static zbNpcFaceCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcFaceCsvTableData zbNpcFaceCsvTableData2 = new zbNpcFaceCsvTableData();
		Dictionary<int, zbNpcFaceCsvRecord> dictionary = new Dictionary<int, zbNpcFaceCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcFaceCsvRecord.Read(zbBinReader2);
		}
		zbNpcFaceCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcFaceCsvTableData2;
	}
}
