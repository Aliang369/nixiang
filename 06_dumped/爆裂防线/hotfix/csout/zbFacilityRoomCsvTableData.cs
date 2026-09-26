using System;
using System.Collections.Generic;

[Serializable]
public class zbFacilityRoomCsvTableData
{
	public Dictionary<int, zbFacilityRoomCsvRecord> records = new Dictionary<int, zbFacilityRoomCsvRecord>();

	public static zbFacilityRoomCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFacilityRoomCsvTableData zbFacilityRoomCsvTableData2 = new zbFacilityRoomCsvTableData();
		Dictionary<int, zbFacilityRoomCsvRecord> dictionary = new Dictionary<int, zbFacilityRoomCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFacilityRoomCsvRecord.Read(zbBinReader2);
		}
		zbFacilityRoomCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFacilityRoomCsvTableData2;
	}
}
