public class zbHomeRoomTypeCsvTable
{
	private zbHomeRoomTypeCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/homelandV2/homeRoomTypeCsv", zbHomeRoomTypeCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbHomeRoomTypeCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}

	public zbHomeRoomTypeCsvRecord GetRecordByKey(int key)
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
