using System;

[Serializable]
public class zbMechaAvatarCsvRecord : zbTableBaseRecord
{
	public int avatarId { get; set; }

	public string icon { get; set; }

	public string battleIcon { get; set; }

	public string fightBone { get; set; }

	public string shadow { get; set; }

	public int hitTime { get; set; }

	public int boxWidth { get; set; }

	public int boxHeight { get; set; }

	public int boxOffset { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMechaAvatarCsvRecord Read(zbBinReader r)
	{
		return new zbMechaAvatarCsvRecord
		{
			avatarId = r.ReadInt(),
			icon = r.ReadString(),
			battleIcon = r.ReadString(),
			fightBone = r.ReadString(),
			shadow = r.ReadString(),
			hitTime = r.ReadInt(),
			boxWidth = r.ReadInt(),
			boxHeight = r.ReadInt(),
			boxOffset = r.ReadInt()
		};
	}
}
