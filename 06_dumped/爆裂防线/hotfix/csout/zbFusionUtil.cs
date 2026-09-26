public class zbFusionUtil
{
	public static zbFusionLevelCsvRecord GetLevelRecordByExp(int exp)
	{
		zbFusionLevelCsvTableData records = zbDataConfig.Instance.fusionLevelCsvTable.GetRecords();
		for (int num = records.records.Count - 1; num >= 0; num--)
		{
			if (exp >= records.records[num].exp)
			{
				return records.records[num];
			}
		}
		return null;
	}
}
