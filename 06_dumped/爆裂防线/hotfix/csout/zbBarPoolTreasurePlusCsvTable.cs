public class zbBarPoolTreasurePlusCsvTable
{
	private zbBarPoolTreasurePlusCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/bar/barPoolTreasurePlusCsv", zbBarPoolTreasurePlusCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbBarPoolTreasurePlusCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
