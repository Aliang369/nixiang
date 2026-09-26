public class zbLegionBossChallengeCsvTable
{
	private zbLegionBossChallengeCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/legion/legionBossChallengeCsv", zbLegionBossChallengeCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbLegionBossChallengeCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}

	public zbLegionBossChallengeCsvRecord GetRecordByKey(int key)
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		if (tableData.records.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}
}
