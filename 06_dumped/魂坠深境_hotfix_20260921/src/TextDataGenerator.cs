using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextDataGenerator
{
	private static readonly Dictionary<string, Regex> cachedRegex = new Dictionary<string, Regex>();

	public GameEvent displayGameEvent;

	public GameStateInfo gameState;

	public List<object> refLinkObjects = new List<object>();

	public bool showUncollectedStatus;

	public IEffectObjectDisplay textSource;

	private TextDataGenerator()
	{
	}

	public static TextDataGenerator Create(GameStateInfo gameState, IEffectObjectDisplay caster, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		GameEvent gameEvent = Effect.GenerateDisplayEvent(gameState, caster, null, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay);
		return new TextDataGenerator
		{
			textSource = caster,
			gameState = gameState,
			displayGameEvent = gameEvent,
			showUncollectedStatus = showUncollectedStatus
		};
	}

	public static TextDataGenerator Create(ApplicationStateInfo applicationState, IEffectObjectDisplay caster, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		return Create(GenerateGameState(applicationState), caster, isBattleDisplay, showUncollectedStatus);
	}

	public static TextDataGenerator Create(GameEvent displayEvent, IEffectObjectDisplay caster, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		return new TextDataGenerator
		{
			textSource = caster,
			gameState = displayEvent.gameState,
			displayGameEvent = displayEvent,
			showUncollectedStatus = showUncollectedStatus
		};
	}

	public static TextDataGenerator Create(ApplicationStateInfo applicationState, Item item, SpellCastInfo castInfo = null, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		GameStateInfo gameStateInfo = GenerateGameState(applicationState);
		GameEvent gameEvent = Effect.GenerateDisplayEvent(gameStateInfo, item, castInfo, isBattleDisplay ? GameEvent.TriggerType.InBattleDisplay : GameEvent.TriggerType.OutBattleDisplay);
		return new TextDataGenerator
		{
			textSource = item,
			gameState = gameStateInfo,
			displayGameEvent = gameEvent,
			showUncollectedStatus = showUncollectedStatus
		};
	}

	public static GameStateInfo GenerateGameState(ApplicationStateInfo applicationState)
	{
		GameStateInfo gameStateInfo = applicationState?.runningGameState;
		if (gameStateInfo == null)
		{
			gameStateInfo = new GameStateInfo();
			gameStateInfo.applicationState = applicationState;
			gameStateInfo.gameEventManager = GameEventManager.GenerateMockGameEventMangerForTextDisplay(gameStateInfo);
		}
		return gameStateInfo;
	}

	public string Parse(string rawStr)
	{
		try
		{
			if (rawStr == null)
			{
				return string.Empty;
			}
			return ParseText(rawStr);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, $"文本解析错误【{rawStr}】 - caster:{displayGameEvent.Caster}\n", ThinkingAnalyticsExtends.ExceptionType.Text);
			return rawStr + "!!!节点文本错误!!!";
		}
	}

	private string ParseText(string rawStr, string content = null)
	{
		if (content == null)
		{
			content = rawStr;
		}
		IOrderedEnumerable<Match> orderedEnumerable = from g in TextDataPreset.groupRegex.Matches(content)
			orderby g.Index descending
			select g;
		bool flag = false;
		foreach (Match item2 in orderedEnumerable)
		{
			flag = true;
			string text = item2.ToString();
			string content2 = text.Substring(1, text.Length - 2);
			string replace = ParseText(text, content2);
			content = ReplaceLast(content, text, replace);
		}
		IEnumerable<(Match, TextDataPreset.TextTagPreset)> matchTagPresets = GetMatchTagPresets(content);
		bool flag2 = false;
		using (IEnumerator<(Match, TextDataPreset.TextTagPreset)> enumerator2 = matchTagPresets.GetEnumerator())
		{
			if (enumerator2.MoveNext())
			{
				(Match, TextDataPreset.TextTagPreset) current2 = enumerator2.Current;
				flag2 = true;
				Match item = current2.Item1;
				string replace2 = current2.Item2.textReplacer(this, item.Groups);
				return ReplaceLast(content, item.Value, replace2);
			}
		}
		if (!flag2 && !flag)
		{
			return rawStr;
		}
		return content;
	}

	private static string ReplaceLast(string text, string search, string replace)
	{
		int num = text.LastIndexOf(search, StringComparison.Ordinal);
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num) + replace + text.Substring(num + search.Length);
	}

	private IEnumerable<(Match, TextDataPreset.TextTagPreset)> GetMatchTagPresets(string rawStr)
	{
		if (string.IsNullOrEmpty(rawStr))
		{
			yield break;
		}
		TextDataPreset.TextTagPreset[] textTagPresets = TextDataPreset.textTagPresets;
		foreach (TextDataPreset.TextTagPreset preset in textTagPresets)
		{
			Regex matchRegex = GetMatchRegex(preset.matchRegexStr);
			foreach (Match item3 in matchRegex.Matches(rawStr))
			{
				yield return (item3, preset);
			}
		}
		if (displayGameEvent.gameState == null)
		{
			yield break;
		}
		foreach (KeywordDef asset in displayGameEvent.gameState.applicationState.assetCatalog.keywordCatalog.assets)
		{
			foreach (Match item4 in asset.cachedRegex.Matches(rawStr))
			{
				yield return (item4, TextDataPreset.keywordTagPreset);
			}
		}
	}

	private static Regex GetMatchRegex(string str)
	{
		if (cachedRegex.TryGetValue(str, out var value))
		{
			return value;
		}
		Regex regex = new Regex(str, RegexOptions.Compiled);
		cachedRegex.Add(str, regex);
		return regex;
	}

	public string GetTagsText(IEnumerable<ItemTag> tags)
	{
		string text = "";
		if (tags == null)
		{
			return text;
		}
		foreach (ItemTag tag in tags)
		{
			if (!(tag == null))
			{
				string text2 = ((tag.textSpriteIcon == null) ? "" : tag.textSpriteIcon.ToTMPSpriteText().SizeTextPercent(130));
				text = text + " " + (text2 + tag.GetNameText().ColorText(tag.textColor)).NoBreak() + " ";
			}
		}
		return text.ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkTagColor).NoBreak();
	}

	public string GetSkillsText(IEnumerable<GameSkill> skills)
	{
		string text = "";
		foreach (GameSkill skill in skills)
		{
			text += skill.NameWithQualityAndUpgrade.LinkReferenceObject(refLinkObjects.Count).ViewText().ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkEnchantColor)
				.NoBreak();
			refLinkObjects.Add(skill);
		}
		return text;
	}

	public string GetInfluencesText(IEnumerable<Influence> influences)
	{
		string text = "";
		foreach (Influence influence in influences)
		{
			text += influence.def.InfluenceName.LinkReferenceObject(refLinkObjects.Count).ViewText().ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkEnchantColor)
				.NoBreak();
			refLinkObjects.Add(influence);
		}
		return text;
	}

	public string GetEnchantsText(IEnumerable<Enchant> enchants, int? deltaValue = null)
	{
		if (enchants == null)
		{
			return "";
		}
		List<string> list = new List<string>();
		foreach (Enchant enchant in enchants)
		{
			refLinkObjects.Add(enchant);
			string text = "";
			text = ((!enchant.def.stacking) ? (enchant.def.Name ?? "") : ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_tilde_arg1.FormatWith(enchant.def.Name, enchant.value));
			text = text.LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkEnchantColor)
				.NoBreak();
			if (deltaValue > 0)
			{
				text += ScriptableObjectSingleton<GameDataManager>.instance.increaseTextIcon.ToTMPSpriteText();
			}
			list.Add(text);
		}
		return string.Join("、", list);
	}

	public string GetRunesText(IEnumerable<Rune> runes)
	{
		string text = "";
		if (runes == null)
		{
			return text;
		}
		foreach (Rune rune in runes)
		{
			refLinkObjects.Add(rune);
			string text2 = rune.def.Name;
			if (rune.def.stacking)
			{
				text2 = $"{text2}{rune.value}";
			}
			text2 = text2.ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkRuneColor).LinkReferenceObject(refLinkObjects.Count - 1).ViewText()
				.NoBreak();
			text += text2;
		}
		return text;
	}

	public string GetItemDefsText(IEnumerable<ItemDefinition> defs)
	{
		string text = "";
		if (defs == null)
		{
			return text;
		}
		Dictionary<ItemDefinition, int> dictionary = new Dictionary<ItemDefinition, int>();
		foreach (ItemDefinition def in defs)
		{
			if (!(def == null))
			{
				if (dictionary.ContainsKey(def))
				{
					dictionary[def]++;
				}
				else
				{
					dictionary.Add(def, 1);
				}
			}
		}
		foreach (KeyValuePair<ItemDefinition, int> item in dictionary)
		{
			text = ((item.Value != 1) ? (text + GetItemDefinitionText(item.Key) + "？" + item.Value) : (text + GetItemDefinitionText(item.Key)));
		}
		return text;
	}

	public string GetItemsText(IEnumerable<Item> items)
	{
		if (items == null)
		{
			return "";
		}
		List<(Item, int)> list = new List<(Item, int)>();
		Func<Item, Item, bool> DisplayItemEqualer = (Item a, Item b) => a.GetDef() == b.GetDef() && (int)a.counter == (int)b.counter;
		foreach (Item item in items)
		{
			if (item != null)
			{
				int num = list.FindIndex(((Item, int) i) => DisplayItemEqualer(i.Item1, item));
				if (num != -1)
				{
					list[num] = (list[num].Item1, list[num].Item2 + 1);
				}
				else
				{
					list.Add((item, 1));
				}
			}
		}
		return GenerateItemsCountText(list);
	}

	public string GetAddItemText(AddSpawnItemsInfo addInfo)
	{
		List<AddSpawnItemsInfo.AddSingleItemInfo> list = addInfo?.GetAddItemInfos();
		if (list == null)
		{
			return "";
		}
		List<(AddSpawnItemsInfo.AddSingleItemInfo, int)> list2 = new List<(AddSpawnItemsInfo.AddSingleItemInfo, int)>();
		Func<AddSpawnItemsInfo.AddSingleItemInfo, AddSpawnItemsInfo.AddSingleItemInfo, bool> DisplayItemEqualer = (AddSpawnItemsInfo.AddSingleItemInfo a, AddSpawnItemsInfo.AddSingleItemInfo b) => a.item.GetDef() == b.item.GetDef() && (int)a.GenerateFinalItem(addInfo).counter == (int)b.GenerateFinalItem(addInfo).counter;
		foreach (AddSpawnItemsInfo.AddSingleItemInfo info in list)
		{
			if (info.item != null)
			{
				int num = list2.FindIndex(((AddSpawnItemsInfo.AddSingleItemInfo, int) i) => DisplayItemEqualer(i.Item1, info));
				if (num != -1)
				{
					list2[num] = (list2[num].Item1, list2[num].Item2 + 1);
				}
				else
				{
					list2.Add((info, 1));
				}
			}
		}
		return GenerateItemsCountText((from p in list2
			select (p.Item1.GenerateFinalItem(addInfo), p.Item2) into p
			where p.Item1 != null
			select p).ToList());
	}

	private string GenerateItemsCountText(List<(Item, int)> itemCountList)
	{
		string text = "";
		foreach (var (item, num) in itemCountList)
		{
			if (item == null)
			{
				continue;
			}
			string counterText = item.GetCounterText();
			string displayItemName = GetDisplayItemName(item);
			TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
			string text2 = ((counterText.IsNullOrEmpty() || instance == null) ? displayItemName : instance.term_arg0_tilde_arg1.FormatWith(displayItemName, counterText));
			if (num == 1)
			{
				if (!text.IsNullOrEmpty())
				{
					text += " ";
				}
				text += GetItemDisplayReferenceText(item, text2);
				continue;
			}
			if (!text.IsNullOrEmpty())
			{
				text += " ";
			}
			text += GetItemDisplayReferenceText(item, ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_count_card_arg1.FormatWith(num, text2));
		}
		return text;
	}

	public string GetDamageText(int? damage, int? deltaDamage, int? finalTimes, int? deltaTimes)
	{
		Color color = new Color(1f, 0.4f, 0.4f);
		if (!damage.HasValue)
		{
			return "？".ColorText(color);
		}
		if (deltaDamage.HasValue)
		{
			damage += deltaDamage;
		}
		if (!finalTimes.HasValue)
		{
			finalTimes = 1;
		}
		string text = damage.ToString();
		text = ((finalTimes == 1) ? (text ?? "").ColorText(color) : (text + $"X{finalTimes}".SizeTextPercent(75)).ColorText(color));
		if (deltaDamage != 0 || deltaTimes != 0)
		{
			if (deltaDamage >= 0 && deltaTimes >= 0)
			{
				text += ScriptableObjectSingleton<GameDataManager>.instance.increaseTextIcon.ToTMPSpriteText();
			}
			else if (deltaDamage <= 0 && deltaTimes <= 0)
			{
				text += ScriptableObjectSingleton<GameDataManager>.instance.decreaseTextIcon.ToTMPSpriteText();
			}
			else
			{
				text += ScriptableObjectSingleton<GameDataManager>.instance.increaseTextIcon.ToTMPSpriteText();
				text += ScriptableObjectSingleton<GameDataManager>.instance.decreaseTextIcon.ToTMPSpriteText();
			}
		}
		return text.NoBreak();
	}

	public string GetItemDefinitionText(ItemDefinition def)
	{
		return GetItemDefinitionDisplayReferenceText(def);
	}

	public string GetEnchantDefText(IEnumerable<EnchantDef> defs)
	{
		string text = "";
		if (defs == null)
		{
			return text;
		}
		foreach (EnchantDef def in defs)
		{
			refLinkObjects.Add(def);
			string text2 = def.Name.LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor)
				.NoBreak();
			text += text2;
		}
		return text;
	}

	public string GetStanceText(Stance stance, int? deltaValue)
	{
		refLinkObjects.Add(stance);
		string text = "";
		Color stanceColor = stance.def.stanceColor;
		if (deltaValue.HasValue)
		{
			int valueOrDefault = deltaValue.GetValueOrDefault();
			if (valueOrDefault <= 0)
			{
				if (valueOrDefault == 0)
				{
					text += $"{stance.def.stanceName}{stance.value}".LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(stanceColor)
						.NoBreak();
				}
				else
				{
					text += $"{stance.def.stanceName}{stance.value}".LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(stanceColor)
						.NoBreak();
					text += ScriptableObjectSingleton<GameDataManager>.instance.decreaseTextIcon.ToTMPSpriteText();
				}
			}
			else
			{
				text += $"{stance.def.stanceName}{stance.value}".LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(stanceColor)
					.NoBreak();
				text += ScriptableObjectSingleton<GameDataManager>.instance.increaseTextIcon.ToTMPSpriteText();
			}
		}
		return text;
	}

	public string GetStanceDefText(StanceDef stanceDef)
	{
		refLinkObjects.Add(stanceDef);
		Color stanceColor = stanceDef.stanceColor;
		return (stanceDef.stanceName ?? "").LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(stanceColor)
			.NoBreak();
	}

	public bool ShouldHideItemName(Item item)
	{
		return UncollectedItemDisplayHelper.ShouldHideItemName(gameState?.applicationState, item, showUncollectedStatus);
	}

	public bool ShouldHideItemName(ItemDefinition itemDef)
	{
		return UncollectedItemDisplayHelper.ShouldHideItemName(gameState?.applicationState, itemDef, showUncollectedStatus);
	}

	public string GetDisplayItemName(Item item)
	{
		return UncollectedItemDisplayHelper.GetDisplayItemName(gameState?.applicationState, item, showUncollectedStatus);
	}

	public string GetDisplayItemName(ItemDefinition itemDef)
	{
		return UncollectedItemDisplayHelper.GetDisplayItemName(gameState?.applicationState, itemDef, showUncollectedStatus);
	}

	public string GetItemDisplayReferenceText(Item item, string rawItemText = null)
	{
		if (rawItemText == null)
		{
			rawItemText = GetDisplayItemName(item);
		}
		if (ShouldHideItemName(item))
		{
			return rawItemText.ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor).NoBreak();
		}
		refLinkObjects.Add(item);
		return rawItemText.LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor)
			.NoBreak();
	}

	public string GetItemDefinitionDisplayReferenceText(ItemDefinition itemDef, string rawItemText = null)
	{
		if (rawItemText == null)
		{
			rawItemText = GetDisplayItemName(itemDef);
		}
		if (ShouldHideItemName(itemDef))
		{
			return rawItemText.ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor).NoBreak();
		}
		refLinkObjects.Add(itemDef);
		return rawItemText.LinkReferenceObject(refLinkObjects.Count - 1).ViewText().ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor)
			.NoBreak();
	}
}
