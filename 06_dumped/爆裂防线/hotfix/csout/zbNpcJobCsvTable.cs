public class zbNpcJobCsvTable
{
	private zbNpcJobCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/npc/npcJobCsv", zbNpcJobCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbNpcJobCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}

	public zbNpcJobCsvRecord GetRecordByKey(int key)
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
