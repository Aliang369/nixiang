using System;
using System.Collections.Generic;

[Serializable]
public class zbEquipQualityCsvTableData
{
	public Dictionary<int, zbEquipQualityCsvRecord> records = new Dictionary<int, zbEquipQualityCsvRecord>();

	public static zbEquipQualityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbEquipQualityCsvTableData zbEquipQualityCsvTableData2 = new zbEquipQualityCsvTableData();
		Dictionary<int, zbEquipQualityCsvRecord> dictionary = new Dictionary<int, zbEquipQualityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbEquipQualityCsvRecord.Read(zbBinReader2);
		}
		zbEquipQualityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbEquipQualityCsvTableData2;
	}
}
