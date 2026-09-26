using System;

[Serializable]
public class zbSkillAttrCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int valueType { get; set; }

	public int showPercent { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSkillAttrCsvRecord Read(zbBinReader r)
	{
		return new zbSkillAttrCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			valueType = r.ReadInt(),
			showPercent = r.ReadInt()
		};
	}
}
