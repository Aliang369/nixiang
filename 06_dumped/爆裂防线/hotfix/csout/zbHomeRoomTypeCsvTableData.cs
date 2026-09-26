using System;
using System.Collections.Generic;

[Serializable]
public class zbHomeRoomTypeCsvTableData
{
	public Dictionary<int, zbHomeRoomTypeCsvRecord> records = new Dictionary<int, zbHomeRoomTypeCsvRecord>();

	public static zbHomeRoomTypeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHomeRoomTypeCsvTableData zbHomeRoomTypeCsvTableData2 = new zbHomeRoomTypeCsvTableData();
		Dictionary<int, zbHomeRoomTypeCsvRecord> dictionary = new Dictionary<int, zbHomeRoomTypeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHomeRoomTypeCsvRecord.Read(zbBinReader2);
		}
		zbHomeRoomTypeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHomeRoomTypeCsvTableData2;
	}
}
