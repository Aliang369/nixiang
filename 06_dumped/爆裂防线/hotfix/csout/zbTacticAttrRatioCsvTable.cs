public class zbTacticAttrRatioCsvTable
{
	private zbTacticAttrRatioCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/tactic/tacticAttrRatioCsv", zbTacticAttrRatioCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbTacticAttrRatioCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
