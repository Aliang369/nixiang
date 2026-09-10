using System.Collections.Generic;
using System.Linq;

public class UpgradeCharacterSkill : GameConfigEffect
{
	public class CharacterSkillUpgradePreviewNode : GameConfigEffect
	{
		public int upgradeCount;

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
			nodeDisplayInfo.refSkills = new NodeDisplayInfo.Element<List<GameSkill>>(new List<GameSkill>());
			if (eve.nodeTempValues.TryGetValue("character") is PlayerCharacterState playerCharacterState)
			{
				IEnumerable<GameSkill> collection = from ps in playerCharacterState.GetCharacterCurrentInnateProfileSkillsOfUpgradeCount(upgradeCount, showLocked: true)
					select new GameSkill(ps);
				nodeDisplayInfo.refSkills.value.AddRange(collection);
			}
			return nodeDisplayInfo;
		}
	}

	public CharacterSkillUpgradePreviewNode beforeUpgradePreviewNode;

	public CharacterSkillUpgradePreviewNode afterUpgradePreviewNode;

	public override string GenerateTalentDisplayDescription(out TextDataGenerator generator, PlayerCharacterState character = null, TalentDef def = null)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.Display, GameEvent.TriggerType.OutBattleDisplay);
		gameEvent.gameState = TextDataGenerator.GenerateGameState(base.applicationState);
		gameEvent.nodeTempValues.Add("character", character);
		return TextDataPreset.FormatEffectObjectDescription(def, def.effectDescription, gameEvent, out generator);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { beforeUpgradePreviewNode, afterUpgradePreviewNode };
	}
}
