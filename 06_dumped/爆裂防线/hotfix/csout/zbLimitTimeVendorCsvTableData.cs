using System;

[Serializable]
public class zbLimitTimeVendorCsvTableData
{
	public zbLimitTimeVendorCsvRecord[] records;

	public static zbLimitTimeVendorCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLimitTimeVendorCsvTableData zbLimitTimeVendorCsvTableData2 = new zbLimitTimeVendorCsvTableData();
		zbLimitTimeVendorCsvRecord[] array = new zbLimitTimeVendorCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbLimitTimeVendorCsvRecord.Read(zbBinReader2);
		}
		zbLimitTimeVendorCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbLimitTimeVendorCsvTableData2;
	}
}
