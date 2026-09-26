using System;

[Serializable]
public class zbVipShopChannelCsvTableData
{
	public zbVipShopChannelCsvRecord[] records;

	public static zbVipShopChannelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipShopChannelCsvTableData zbVipShopChannelCsvTableData2 = new zbVipShopChannelCsvTableData();
		zbVipShopChannelCsvRecord[] array = new zbVipShopChannelCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbVipShopChannelCsvRecord.Read(zbBinReader2);
		}
		zbVipShopChannelCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbVipShopChannelCsvTableData2;
	}
}
