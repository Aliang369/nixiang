public class zbHeroAwakenCsvTable
{
	private zbHeroAwakenCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/hero/heroAwakenCsv", zbHeroAwakenCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbHeroAwakenCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
