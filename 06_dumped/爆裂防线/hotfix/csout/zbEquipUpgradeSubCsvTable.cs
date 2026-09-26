public class zbEquipUpgradeSubCsvTable
{
	private zbEquipUpgradeSubCsvTableData tableData;

	public void Init()
	{
		tableData = zbTableLoader.Instance.LoadTableConfig("config/equip/equipUpgradeSubCsv", zbEquipUpgradeSubCsvTableData.ReadBin);
		if (tableData != null)
		{
			_ = tableData.records;
		}
	}

	public zbEquipUpgradeSubCsvTableData GetRecords()
	{
		if (tableData == null || tableData.records == null)
		{
			return null;
		}
		return tableData;
	}
}
