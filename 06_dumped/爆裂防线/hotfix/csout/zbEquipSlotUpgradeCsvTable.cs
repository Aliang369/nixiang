public class zbEquipSlotUpgradeCsvTable
{
	private zbEquipSlotUpgradeCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/equip/equipSlotUpgradeCsv", zbEquipSlotUpgradeCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbEquipSlotUpgradeCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
