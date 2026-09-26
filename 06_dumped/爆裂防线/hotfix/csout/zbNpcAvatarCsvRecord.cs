using System;

[Serializable]
public class zbNpcAvatarCsvRecord : zbTableBaseRecord
{
	public int avatarId { get; set; }

	public int genderId { get; set; }

	public int bodyId { get; set; }

	public int eyeColorId { get; set; }

	public int[] faceId { get; set; }

	public int hairId { get; set; }

	public int[] suitId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcAvatarCsvRecord Read(zbBinReader r)
	{
		return new zbNpcAvatarCsvRecord
		{
			avatarId = r.ReadInt(),
			genderId = r.ReadInt(),
			bodyId = r.ReadInt(),
			eyeColorId = r.ReadInt(),
			faceId = r.ReadIntArray(),
			hairId = r.ReadInt(),
			suitId = r.ReadIntArray()
		};
	}
}
