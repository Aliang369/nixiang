public class zbLegionBossKillAwardCsvTable
{
	private zbLegionBossKillAwardCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/legion/legionBossKillAwardCsv", zbLegionBossKillAwardCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbLegionBossKillAwardCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
