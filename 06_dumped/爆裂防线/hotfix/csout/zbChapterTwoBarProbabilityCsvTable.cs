public class zbChapterTwoBarProbabilityCsvTable
{
	private zbChapterTwoBarProbabilityCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/journey/chapterTwo/chapterTwoBarProbabilityCsv", zbChapterTwoBarProbabilityCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbChapterTwoBarProbabilityCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
