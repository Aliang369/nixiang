using System;

[Serializable]
public class zbTacticAttrRatioCsvTableData
{
	public zbTacticAttrRatioCsvRecord[] records;

	public static zbTacticAttrRatioCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTacticAttrRatioCsvTableData zbTacticAttrRatioCsvTableData2 = new zbTacticAttrRatioCsvTableData();
		zbTacticAttrRatioCsvRecord[] array = new zbTacticAttrRatioCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbTacticAttrRatioCsvRecord.Read(zbBinReader2);
		}
		zbTacticAttrRatioCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbTacticAttrRatioCsvTableData2;
	}
}
