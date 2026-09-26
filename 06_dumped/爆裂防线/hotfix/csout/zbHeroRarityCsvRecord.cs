using System;

[Serializable]
public class zbHeroRarityCsvRecord : zbTableBaseRecord
{
	public int rarity { get; set; }

	public int quality { get; set; }

	public int frag { get; set; }

	public int soul { get; set; }

	public int upgradeFrag { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHeroRarityCsvRecord Read(zbBinReader r)
	{
		return new zbHeroRarityCsvRecord
		{
			rarity = r.ReadInt(),
			quality = r.ReadInt(),
			frag = r.ReadInt(),
			soul = r.ReadInt(),
			upgradeFrag = r.ReadInt()
		};
	}
}
