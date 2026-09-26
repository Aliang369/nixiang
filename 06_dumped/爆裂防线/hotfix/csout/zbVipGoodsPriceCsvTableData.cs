using System;
using System.Collections.Generic;

[Serializable]
public class zbVipGoodsPriceCsvTableData
{
	public Dictionary<int, zbVipGoodsPriceCsvRecord> records = new Dictionary<int, zbVipGoodsPriceCsvRecord>();

	public static zbVipGoodsPriceCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipGoodsPriceCsvTableData zbVipGoodsPriceCsvTableData2 = new zbVipGoodsPriceCsvTableData();
		Dictionary<int, zbVipGoodsPriceCsvRecord> dictionary = new Dictionary<int, zbVipGoodsPriceCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipGoodsPriceCsvRecord.Read(zbBinReader2);
		}
		zbVipGoodsPriceCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipGoodsPriceCsvTableData2;
	}
}
