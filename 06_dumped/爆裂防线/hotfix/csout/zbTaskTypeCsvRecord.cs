using System;

[Serializable]
public class zbTaskTypeCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string link { get; set; }

	public string funcType { get; set; }

	public string linkUrl { get; set; }

	public string funcId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTaskTypeCsvRecord Read(zbBinReader r)
	{
		return new zbTaskTypeCsvRecord
		{
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			link = r.ReadString(),
			funcType = r.ReadString(),
			linkUrl = r.ReadString(),
			funcId = r.ReadString()
		};
	}
}
