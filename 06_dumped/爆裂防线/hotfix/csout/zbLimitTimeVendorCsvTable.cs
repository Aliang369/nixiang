public class zbLimitTimeVendorCsvTable
{
	private zbLimitTimeVendorCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/limitTimeVendor/limitTimeVendorCsv", zbLimitTimeVendorCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbLimitTimeVendorCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
