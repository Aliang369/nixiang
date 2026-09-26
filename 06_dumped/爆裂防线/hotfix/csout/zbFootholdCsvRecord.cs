using System;

[Serializable]
public class zbFootholdCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string mainScene { get; set; }

	public string icon { get; set; }

	public int sceneId { get; set; }

	public int quality { get; set; }

	public int footholdEffectId1 { get; set; }

	public int footholdEffectId2 { get; set; }

	public int footholdEffectId3 { get; set; }

	public int footholdEffectId4 { get; set; }

	public int footholdEffectId5 { get; set; }

	public int footholdEffectId6 { get; set; }

	public int footholdEffectId7 { get; set; }

	public int footholdEffectId8 { get; set; }

	public int footholdEffectId9 { get; set; }

	public int footholdEffectId10 { get; set; }

	public string skillIcon { get; set; }

	public int effectId { get; set; }

	public int open { get; set; }

	public int propId { get; set; }

	public int convertPropId { get; set; }

	public int upgradeId { get; set; }

	public float scoreFactor { get; set; }

	public string banner { get; set; }

	public int show { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFootholdCsvRecord Read(zbBinReader r)
	{
		return new zbFootholdCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			mainScene = r.ReadString(),
			icon = r.ReadString(),
			sceneId = r.ReadInt(),
			quality = r.ReadInt(),
			footholdEffectId1 = r.ReadInt(),
			footholdEffectId2 = r.ReadInt(),
			footholdEffectId3 = r.ReadInt(),
			footholdEffectId4 = r.ReadInt(),
			footholdEffectId5 = r.ReadInt(),
			footholdEffectId6 = r.ReadInt(),
			footholdEffectId7 = r.ReadInt(),
			footholdEffectId8 = r.ReadInt(),
			footholdEffectId9 = r.ReadInt(),
			footholdEffectId10 = r.ReadInt(),
			skillIcon = r.ReadString(),
			effectId = r.ReadInt(),
			open = r.ReadInt(),
			propId = r.ReadInt(),
			convertPropId = r.ReadInt(),
			upgradeId = r.ReadInt(),
			scoreFactor = r.ReadFloat(),
			banner = r.ReadString(),
			show = r.ReadInt()
		};
	}
}
