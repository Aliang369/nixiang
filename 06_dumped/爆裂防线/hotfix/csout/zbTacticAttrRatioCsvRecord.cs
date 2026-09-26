using System;

[Serializable]
public class zbTacticAttrRatioCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int star { get; set; }

	public float ratio { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTacticAttrRatioCsvRecord Read(zbBinReader r)
	{
		return new zbTacticAttrRatioCsvRecord
		{
			id = r.ReadInt(),
			star = r.ReadInt(),
			ratio = r.ReadFloat()
		};
	}
}
