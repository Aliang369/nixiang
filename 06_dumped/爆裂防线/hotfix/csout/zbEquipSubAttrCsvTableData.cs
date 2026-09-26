using System;

[Serializable]
public class zbEquipSubAttrCsvTableData
{
	public zbEquipSubAttrCsvRecord[] records;

	public static zbEquipSubAttrCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbEquipSubAttrCsvTableData zbEquipSubAttrCsvTableData2 = new zbEquipSubAttrCsvTableData();
		zbEquipSubAttrCsvRecord[] array = new zbEquipSubAttrCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbEquipSubAttrCsvRecord.Read(zbBinReader2);
		}
		zbEquipSubAttrCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbEquipSubAttrCsvTableData2;
	}
}
