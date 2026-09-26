using System;

[Serializable]
public class zbRoomUpgradeEffectCsvTableData
{
	public zbRoomUpgradeEffectCsvRecord[] records;

	public static zbRoomUpgradeEffectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRoomUpgradeEffectCsvTableData zbRoomUpgradeEffectCsvTableData2 = new zbRoomUpgradeEffectCsvTableData();
		zbRoomUpgradeEffectCsvRecord[] array = new zbRoomUpgradeEffectCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbRoomUpgradeEffectCsvRecord.Read(zbBinReader2);
		}
		zbRoomUpgradeEffectCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbRoomUpgradeEffectCsvTableData2;
	}
}
