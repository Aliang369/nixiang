using System.Collections.Generic;

public class NodeDisplayInfo
{
	public class Element<T>
	{
		public bool valid => value != null;

		public T value { get; }

		public Element(T value)
		{
			this.value = value;
		}
	}

	public Element<AddSpawnItemsInfo> addItemInfo;

	public Element<int?> baseValue;

	public Element<bool> damage;

	public Element<int?> deltaTimes;

	public Element<int?> deltaValue;

	public Element<bool?> outputBool;

	public Element<Pray> pray;

	public Element<QualityLevel> qualityLevel;

	public Element<List<EnchantDef>> refEnchantDefs;

	public Element<List<Enchant>> refEnchants;

	public Element<List<Influence>> refInfluences;

	public Element<List<ItemDefinition>> refItemDefs;

	public Element<List<Item>> refItems;

	public Element<List<RuneDef>> refRuneDefs;

	public Element<List<Rune>> refRunes;

	public Element<List<GameSkill>> refSkills;

	public Element<List<ItemTag>> refTags;

	public Element<Stance> refStance;

	public Element<StanceDef> refStanceDef;

	public Element<string> str;

	public Element<int?> times;

	public Element<int?> value;
}
