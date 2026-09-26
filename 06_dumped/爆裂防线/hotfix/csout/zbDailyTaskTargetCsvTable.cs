public class zbDailyTaskTargetCsvTable
{
	private zbDailyTaskTargetCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/task/dailyTaskTargetCsv", zbDailyTaskTargetCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbDailyTaskTargetCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
