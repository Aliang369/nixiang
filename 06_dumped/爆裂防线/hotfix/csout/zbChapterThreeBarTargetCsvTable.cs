public class zbChapterThreeBarTargetCsvTable
{
	private zbChapterThreeBarTargetCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/journey/chapterThree/chapterThreeBarTargetCsv", zbChapterThreeBarTargetCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbChapterThreeBarTargetCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
