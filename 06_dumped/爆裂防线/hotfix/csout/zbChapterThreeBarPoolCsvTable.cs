public class zbChapterThreeBarPoolCsvTable
{
	private zbChapterThreeBarPoolCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/journey/chapterThree/chapterThreeBarPoolCsv", zbChapterThreeBarPoolCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbChapterThreeBarPoolCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
