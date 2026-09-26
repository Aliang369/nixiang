public class zbPlayerGunUpgradeCsvTable
{
	private zbPlayerGunUpgradeCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/player/playerGunUpgradeCsv", zbPlayerGunUpgradeCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbPlayerGunUpgradeCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
