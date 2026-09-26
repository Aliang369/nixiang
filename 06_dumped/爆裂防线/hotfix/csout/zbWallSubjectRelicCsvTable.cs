public class zbWallSubjectRelicCsvTable
{
	private zbWallSubjectRelicCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/wallSubject/wallSubjectRelicCsv", zbWallSubjectRelicCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbWallSubjectRelicCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
