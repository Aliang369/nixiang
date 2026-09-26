public class zbChapterOneBarProbabilityCsvTable
{
	private zbChapterOneBarProbabilityCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/journey/chapterOne/chapterOneBarProbabilityCsv", zbChapterOneBarProbabilityCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbChapterOneBarProbabilityCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
