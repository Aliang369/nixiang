public class zbWallSubjectRouletteCsvTable
{
	private zbWallSubjectRouletteCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/wallSubject/wallSubjectRouletteCsv", zbWallSubjectRouletteCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbWallSubjectRouletteCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
