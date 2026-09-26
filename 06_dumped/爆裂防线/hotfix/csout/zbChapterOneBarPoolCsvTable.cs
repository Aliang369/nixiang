public class zbChapterOneBarPoolCsvTable
{
	private zbChapterOneBarPoolCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/journey/chapterOne/chapterOneBarPoolCsv", zbChapterOneBarPoolCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbChapterOneBarPoolCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
