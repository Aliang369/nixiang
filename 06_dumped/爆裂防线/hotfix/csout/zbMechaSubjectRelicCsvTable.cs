public class zbMechaSubjectRelicCsvTable
{
	private zbMechaSubjectRelicCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/mechaSubject/mechaSubjectRelicCsv", zbMechaSubjectRelicCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbMechaSubjectRelicCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
