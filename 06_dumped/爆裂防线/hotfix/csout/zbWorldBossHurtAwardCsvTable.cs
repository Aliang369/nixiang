public class zbWorldBossHurtAwardCsvTable
{
	private zbWorldBossHurtAwardCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/worldBoss/worldBossHurtAwardCsv", zbWorldBossHurtAwardCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbWorldBossHurtAwardCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
