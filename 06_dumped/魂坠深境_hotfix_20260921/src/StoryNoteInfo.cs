using System;

public class StoryNoteInfo
{
	public string content;

	public string signature;

	public Item item;

	public GameSkill skill;

	public ClavisDef clavisDef;

	public bool gatherGet;

	public bool needConfirm;

	public bool canBack;

	public ForerunnerLegacy forerunnerLegacy;

	public bool toGetForerunnerLegacy;

	public Action<StoryNoteFeedback> afterStoryNoteFinish;

	private StoryNoteInfo()
	{
	}

	public static StoryNoteInfo GetForerunnerLegacy(ForerunnerLegacy forerunnerLegacy, Action<StoryNoteFeedback> afterStoryNoteFinish = null)
	{
		return new StoryNoteInfo
		{
			content = ForerunnerPresetMessageResolver.ResolveText(forerunnerLegacy),
			signature = forerunnerLegacy.signature,
			item = forerunnerLegacy.legacyItem?.DisplayItem,
			skill = forerunnerLegacy.legacySkill?.DisplaySkill,
			gatherGet = true,
			needConfirm = true,
			canBack = true,
			forerunnerLegacy = forerunnerLegacy,
			toGetForerunnerLegacy = (!forerunnerLegacy.storyOnly && !string.IsNullOrEmpty(forerunnerLegacy.guid)),
			afterStoryNoteFinish = afterStoryNoteFinish
		};
	}

	public static StoryNoteInfo DisplayForerunnerLegacy(ForerunnerLegacy forerunnerLegacy)
	{
		return new StoryNoteInfo
		{
			content = ForerunnerPresetMessageResolver.ResolveText(forerunnerLegacy),
			signature = forerunnerLegacy.signature,
			item = forerunnerLegacy.legacyItem?.DisplayItem,
			skill = forerunnerLegacy.legacySkill?.DisplaySkill,
			gatherGet = false,
			needConfirm = false,
			canBack = true,
			forerunnerLegacy = forerunnerLegacy,
			toGetForerunnerLegacy = false
		};
	}

	public static StoryNoteInfo PlainNote(string note, string signature, Item item, GameSkill skill, ClavisDef clavisDef, bool gatherGet)
	{
		return new StoryNoteInfo
		{
			content = note,
			signature = signature,
			item = item,
			skill = skill,
			canBack = false,
			needConfirm = false,
			clavisDef = clavisDef,
			gatherGet = gatherGet
		};
	}
}
