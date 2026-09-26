public class zbLegionUnlockCsvTable
{
	private zbLegionUnlockCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/legion/legionUnlockCsv", zbLegionUnlockCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbLegionUnlockCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}

	public zbLegionUnlockCsvRecord GetRecordByKey(int key)
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
