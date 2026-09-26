public class zbWorldLairKillAwardCsvTable
{
	private zbWorldLairKillAwardCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/world/worldLairKillAwardCsv", zbWorldLairKillAwardCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbWorldLairKillAwardCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
