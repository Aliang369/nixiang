using System.Linq;

public class ActivateSkillAction : GameAction
{
	private readonly GameSkill skill;

	private readonly GameSkillSlot skillSlot;

	public ActivateSkillAction(GameSkill skill, GameSkillSlot skillSlot)
	{
		this.skill = skill;
		this.skillSlot = skillSlot;
	}

	protected override void Update()
	{
		gameState.playerState.prepareSkillsPreset.gameSkillSlots.FirstOrDefault((GameSkillSlot s) => s.gameSkill == skill)?.SetSkill(null);
		EnqueueAction(new ChangeSkillAction(skill, ChangeSkillAction.Type.AddInSlot, skillSlot));
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerActivateSkill(skill);
		});
		gameState.recordManager.TrackJournalRecord(new ActivateSkillRecord(skill));
		ThinkingAnalyticsExtends.TrackActivateSkill(gameState, skill);
		FinishAction();
	}
}
