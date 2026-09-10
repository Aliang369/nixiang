using System.Collections.Generic;
using System.Linq;

public class GameSkillPreset
{
	[SaveField(null, null)]
	public List<GameSkillSlot> gameSkillSlots;

	[SaveField(null, null)]
	public PlayerCharacterDef characterDef;

	public List<GameSkill> Skills => (from s in gameSkillSlots
		where s.gameSkill != null
		select s.gameSkill).ToList();

	public GameSkillPreset()
	{
		gameSkillSlots = new List<GameSkillSlot>();
		characterDef = null;
	}

	public GameSkillPreset(ProfileSkillPreset preset)
	{
		gameSkillSlots = preset.AllSkillSlots.Select((PresetSkillSlot s) => new GameSkillSlot(s)).ToList();
		characterDef = preset.character.def;
	}
}
