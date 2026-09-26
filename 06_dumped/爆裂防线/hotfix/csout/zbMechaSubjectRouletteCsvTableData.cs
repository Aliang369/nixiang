using System;

[Serializable]
public class zbMechaSubjectRouletteCsvTableData
{
	public zbMechaSubjectRouletteCsvRecord[] records;

	public static zbMechaSubjectRouletteCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaSubjectRouletteCsvTableData zbMechaSubjectRouletteCsvTableData2 = new zbMechaSubjectRouletteCsvTableData();
		zbMechaSubjectRouletteCsvRecord[] array = new zbMechaSubjectRouletteCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbMechaSubjectRouletteCsvRecord.Read(zbBinReader2);
		}
		zbMechaSubjectRouletteCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbMechaSubjectRouletteCsvTableData2;
	}
}
