using System;
using System.Collections.Generic;

[Serializable]
public class zbDefenseTypeCsvTableData
{
	public Dictionary<int, zbDefenseTypeCsvRecord> records = new Dictionary<int, zbDefenseTypeCsvRecord>();

	public static zbDefenseTypeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDefenseTypeCsvTableData zbDefenseTypeCsvTableData2 = new zbDefenseTypeCsvTableData();
		Dictionary<int, zbDefenseTypeCsvRecord> dictionary = new Dictionary<int, zbDefenseTypeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDefenseTypeCsvRecord.Read(zbBinReader2);
		}
		zbDefenseTypeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDefenseTypeCsvTableData2;
	}
}
