using System;

[Serializable]
public class zbVipCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int vipExp { get; set; }

	public int highSpeed { get; set; }

	public int worldRecon2 { get; set; }

	public int fightModRefresh { get; set; }

	public int idleSweep { get; set; }

	public int legionBossSweep { get; set; }

	public int worldBossSweep { get; set; }

	public int quickBuy { get; set; }

	public int autoFight { get; set; }

	public int tiliBuy { get; set; }

	public int worldLairBuy { get; set; }

	public int legionBossBuy { get; set; }

	public int worldBossBuy { get; set; }

	public int endlessHordeBuy { get; set; }

	public int elementTrialBuy { get; set; }

	public int eliteQuestBuy { get; set; }

	public int pvpModRefresh { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipCsvRecord Read(zbBinReader r)
	{
		return new zbVipCsvRecord
		{
			level = r.ReadInt(),
			vipExp = r.ReadInt(),
			highSpeed = r.ReadInt(),
			worldRecon2 = r.ReadInt(),
			fightModRefresh = r.ReadInt(),
			idleSweep = r.ReadInt(),
			legionBossSweep = r.ReadInt(),
			worldBossSweep = r.ReadInt(),
			quickBuy = r.ReadInt(),
			autoFight = r.ReadInt(),
			tiliBuy = r.ReadInt(),
			worldLairBuy = r.ReadInt(),
			legionBossBuy = r.ReadInt(),
			worldBossBuy = r.ReadInt(),
			endlessHordeBuy = r.ReadInt(),
			elementTrialBuy = r.ReadInt(),
			eliteQuestBuy = r.ReadInt(),
			pvpModRefresh = r.ReadInt()
		};
	}
}
