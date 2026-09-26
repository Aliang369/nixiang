using System;

[Serializable]
public class zbSkillTypeAttrCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int attrId { get; set; }

	public int show { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSkillTypeAttrCsvRecord Read(zbBinReader r)
	{
		return new zbSkillTypeAttrCsvRecord
		{
			id = r.ReadInt(),
			attrId = r.ReadInt(),
			show = r.ReadInt()
		};
	}
}
