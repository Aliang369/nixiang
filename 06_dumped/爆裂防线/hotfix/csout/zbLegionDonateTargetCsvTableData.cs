using System;

[Serializable]
public class zbLegionDonateTargetCsvTableData
{
	public zbLegionDonateTargetCsvRecord[] records;

	public static zbLegionDonateTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionDonateTargetCsvTableData zbLegionDonateTargetCsvTableData2 = new zbLegionDonateTargetCsvTableData();
		zbLegionDonateTargetCsvRecord[] array = new zbLegionDonateTargetCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbLegionDonateTargetCsvRecord.Read(zbBinReader2);
		}
		zbLegionDonateTargetCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbLegionDonateTargetCsvTableData2;
	}
}
