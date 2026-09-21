using System.Collections.Generic;

public class IntermissionPlayActivityDef : ActivityDef
{
	public int dailyDrawCount = 6;

	public MatterDef dailyTicketMatter;

	public MatterDef legacyTicketMatter;

	public MatterDef carryTicketMatter;

	public MatterDef otherworldGiftMatter;

	public PlayerCharacterDef dancerCharacter;

	public int dailyTaskCount = 2;

	public int dailyFreeTicketCount = 1;

	public int paidGiftThreshold = 60;

	public int reclaimGiftPerTicket = 10;

	public Dictionary<string, int> paidGiftOverrides = new Dictionary<string, int>();

	public List<IntermissionPlayDailyPool> dailyPools = new List<IntermissionPlayDailyPool>();
}
