using System;

[Serializable]
public class zbSceneCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string path { get; set; }

	public int sceneType { get; set; }

	public string sequenceName { get; set; }

	public int needWaterEffect { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSceneCsvRecord Read(zbBinReader r)
	{
		return new zbSceneCsvRecord
		{
			id = r.ReadInt(),
			path = r.ReadString(),
			sceneType = r.ReadInt(),
			sequenceName = r.ReadString(),
			needWaterEffect = r.ReadInt()
		};
	}
}
