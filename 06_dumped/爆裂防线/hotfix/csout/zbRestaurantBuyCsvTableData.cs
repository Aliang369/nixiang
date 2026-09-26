using System;

[Serializable]
public class zbRestaurantBuyCsvTableData
{
	public zbRestaurantBuyCsvRecord[] records;

	public static zbRestaurantBuyCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRestaurantBuyCsvTableData zbRestaurantBuyCsvTableData2 = new zbRestaurantBuyCsvTableData();
		zbRestaurantBuyCsvRecord[] array = new zbRestaurantBuyCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbRestaurantBuyCsvRecord.Read(zbBinReader2);
		}
		zbRestaurantBuyCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbRestaurantBuyCsvTableData2;
	}
}
