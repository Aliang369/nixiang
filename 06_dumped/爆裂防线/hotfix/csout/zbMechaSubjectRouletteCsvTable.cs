public class zbMechaSubjectRouletteCsvTable
{
	private zbMechaSubjectRouletteCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/mechaSubject/mechaSubjectRouletteCsv", zbMechaSubjectRouletteCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbMechaSubjectRouletteCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
