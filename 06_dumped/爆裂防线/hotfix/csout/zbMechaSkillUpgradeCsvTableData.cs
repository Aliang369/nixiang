using System;

[Serializable]
public class zbMechaSkillUpgradeCsvTableData
{
	public zbMechaSkillUpgradeCsvRecord[] records;

	public static zbMechaSkillUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaSkillUpgradeCsvTableData zbMechaSkillUpgradeCsvTableData2 = new zbMechaSkillUpgradeCsvTableData();
		zbMechaSkillUpgradeCsvRecord[] array = new zbMechaSkillUpgradeCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbMechaSkillUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbMechaSkillUpgradeCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbMechaSkillUpgradeCsvTableData2;
	}
}
