public class zbRoomUpgradeEffectCsvTable
{
	private zbRoomUpgradeEffectCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/homelandV2/roomUpgradeEffectCsv", zbRoomUpgradeEffectCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbRoomUpgradeEffectCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
