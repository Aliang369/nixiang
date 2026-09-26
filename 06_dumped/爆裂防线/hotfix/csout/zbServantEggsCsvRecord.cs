using System;

[Serializable]
public class zbServantEggsCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int incubateTime { get; set; }

	public int poolId { get; set; }

	public string anim { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantEggsCsvRecord Read(zbBinReader r)
	{
		return new zbServantEggsCsvRecord
		{
			id = r.ReadInt(),
			incubateTime = r.ReadInt(),
			poolId = r.ReadInt(),
			anim = r.ReadString()
		};
	}
}
