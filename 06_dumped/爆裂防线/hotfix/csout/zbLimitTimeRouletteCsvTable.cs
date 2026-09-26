public class zbLimitTimeRouletteCsvTable
{
	private zbLimitTimeRouletteCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/limitTimeRoulette/limitTimeRouletteCsv", zbLimitTimeRouletteCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbLimitTimeRouletteCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
