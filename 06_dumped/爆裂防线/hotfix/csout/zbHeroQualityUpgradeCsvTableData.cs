using System;

[Serializable]
public class zbHeroQualityUpgradeCsvTableData
{
	public zbHeroQualityUpgradeCsvRecord[] records;

	public static zbHeroQualityUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHeroQualityUpgradeCsvTableData zbHeroQualityUpgradeCsvTableData2 = new zbHeroQualityUpgradeCsvTableData();
		zbHeroQualityUpgradeCsvRecord[] array = new zbHeroQualityUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbHeroQualityUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbHeroQualityUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbHeroQualityUpgradeCsvTableData2;
	}
}
