public class zbRoomUpgradeUtilityCsvTable
{
	private zbRoomUpgradeUtilityCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/homelandV2/roomUpgradeUtilityCsv", zbRoomUpgradeUtilityCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbRoomUpgradeUtilityCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
