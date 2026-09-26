public class zbRestaurantBuyCsvTable
{
	private zbRestaurantBuyCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/restaurant/restaurantBuyCsv", zbRestaurantBuyCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbRestaurantBuyCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
