using System;

[Serializable]
public class zbRoomUpgradeUtilityCsvTableData
{
	public zbRoomUpgradeUtilityCsvRecord[] records;

	public static zbRoomUpgradeUtilityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRoomUpgradeUtilityCsvTableData zbRoomUpgradeUtilityCsvTableData2 = new zbRoomUpgradeUtilityCsvTableData();
		zbRoomUpgradeUtilityCsvRecord[] array = new zbRoomUpgradeUtilityCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbRoomUpgradeUtilityCsvRecord.Read(zbBinReader2);
		}
		zbRoomUpgradeUtilityCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbRoomUpgradeUtilityCsvTableData2;
	}
}
