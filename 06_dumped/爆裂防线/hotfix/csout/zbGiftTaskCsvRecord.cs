using System;

[Serializable]
public class zbGiftTaskCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int goodsId { get; set; }

	public int growthHour { get; set; }

	public string animPath { get; set; }

	public string iconPath { get; set; }

	public string bonus { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbGiftTaskCsvRecord Read(zbBinReader r)
	{
		return new zbGiftTaskCsvRecord
		{
			id = r.ReadInt(),
			goodsId = r.ReadInt(),
			growthHour = r.ReadInt(),
			animPath = r.ReadString(),
			iconPath = r.ReadString(),
			bonus = r.ReadString()
		};
	}
}
