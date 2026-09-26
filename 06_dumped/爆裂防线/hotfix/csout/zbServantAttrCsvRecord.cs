using System;

[Serializable]
public class zbServantAttrCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int attrType { get; set; }

	public int[] attr { get; set; }

	public float attrVal1 { get; set; }

	public float attrVal2 { get; set; }

	public float attrVal3 { get; set; }

	public float attrVal4 { get; set; }

	public float attrVal5 { get; set; }

	public float attrVal6 { get; set; }

	public float attrVal7 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantAttrCsvRecord Read(zbBinReader r)
	{
		return new zbServantAttrCsvRecord
		{
			id = r.ReadInt(),
			attrType = r.ReadInt(),
			attr = r.ReadIntArray(),
			attrVal1 = r.ReadFloat(),
			attrVal2 = r.ReadFloat(),
			attrVal3 = r.ReadFloat(),
			attrVal4 = r.ReadFloat(),
			attrVal5 = r.ReadFloat(),
			attrVal6 = r.ReadFloat(),
			attrVal7 = r.ReadFloat()
		};
	}
}
