public class zbLimitTimeTreasureCsvTable
{
	private zbLimitTimeTreasureCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/operate/limitTimeTreasure/limitTimeTreasureCsv", zbLimitTimeTreasureCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbLimitTimeTreasureCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
