using System;

[Serializable]
public class zbWallSubjectRouletteCsvTableData
{
	public zbWallSubjectRouletteCsvRecord[] records;

	public static zbWallSubjectRouletteCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallSubjectRouletteCsvTableData zbWallSubjectRouletteCsvTableData2 = new zbWallSubjectRouletteCsvTableData();
		zbWallSubjectRouletteCsvRecord[] array = new zbWallSubjectRouletteCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbWallSubjectRouletteCsvRecord.Read(zbBinReader2);
		}
		zbWallSubjectRouletteCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbWallSubjectRouletteCsvTableData2;
	}
}
