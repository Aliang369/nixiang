using System;
using System.Collections.Generic;

[Serializable]
public class zbMoneyExchangeCsvTableData
{
	public Dictionary<int, zbMoneyExchangeCsvRecord> records = new Dictionary<int, zbMoneyExchangeCsvRecord>();

	public static zbMoneyExchangeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMoneyExchangeCsvTableData zbMoneyExchangeCsvTableData2 = new zbMoneyExchangeCsvTableData();
		Dictionary<int, zbMoneyExchangeCsvRecord> dictionary = new Dictionary<int, zbMoneyExchangeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMoneyExchangeCsvRecord.Read(zbBinReader2);
		}
		zbMoneyExchangeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMoneyExchangeCsvTableData2;
	}
}
