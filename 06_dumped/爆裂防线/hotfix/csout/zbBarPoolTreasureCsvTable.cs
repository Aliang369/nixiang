public class zbBarPoolTreasureCsvTable
{
	private zbBarPoolTreasureCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/bar/barPoolTreasureCsv", zbBarPoolTreasureCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbBarPoolTreasureCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
