public class zbChapterThreeDrawCountCsvTable
{
	private zbChapterThreeDrawCountCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/journey/chapterThree/chapterThreeDrawCountCsv", zbChapterThreeDrawCountCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbChapterThreeDrawCountCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}

	public zbChapterThreeDrawCountCsvRecord GetRecordByKey(int key)
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
