public class zbChapterTwoBarPoolCsvTable
{
	private zbChapterTwoBarPoolCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/journey/chapterTwo/chapterTwoBarPoolCsv", zbChapterTwoBarPoolCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbChapterTwoBarPoolCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
