public class zbBarPoolSpecialCsvTable
{
	private zbBarPoolSpecialCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/bar/barPoolSpecialCsv", zbBarPoolSpecialCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbBarPoolSpecialCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
