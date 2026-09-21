using System.Collections.Generic;

public class CharacterGameInitConfig
{
	[SaveField(null, null)]
	public PlayerCharacterDef characterDef;

	[SaveField(null, null)]
	public CharacterMastery mastery;

	[SaveField(null, null)]
	public List<Item> initDeck;

	[SaveField(null, null)]
	public List<Influence> initInfluence;

	[SaveField(null, null)]
	public int initHealth;

	[SaveField(null, null)]
	public int initCrystal;

	[SaveField(null, null)]
	public int initCoin;

	[SaveField(null, null)]
	public int initWillPower;

	[SaveField(null, null)]
	public int itemSlotCount;

	[SaveField(null, null)]
	public int turnDrawCount;

	[SaveField(null, null)]
	public int overrideYin;

	[SaveField(null, null)]
	public int overrideYan;

	[SaveField(null, null)]
	public int unlockedSkillPresetSlotCount;

	[SaveField(null, null)]
	public int activateSkillSlotCount;
}
