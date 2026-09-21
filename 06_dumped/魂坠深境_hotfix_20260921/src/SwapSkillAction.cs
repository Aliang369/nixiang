using System.Linq;

public class SwapSkillAction : GameAction
{
	public GameSkill newSkill;

	public GameSkillSlot oldSkillSlot;

	public SwapSkillAction(GameSkillSlot oldSkillSlot, GameSkill newSkill)
	{
		this.oldSkillSlot = oldSkillSlot;
		this.newSkill = newSkill;
	}

	protected override void Update()
	{
		GameSkill gameSkill = oldSkillSlot.gameSkill;
		gameState.gameEventManager.TriggerSwapSkill(gameSkill, newSkill);
		gameState.actionManager.EnqueueAction(new ChangeSkillAction(gameSkill, ChangeSkillAction.Type.Remove));
		if (gameSkill.ReplaceRecycle)
		{
			gameState.playerState.prepareSkillsPreset.gameSkillSlots.FirstOrDefault((GameSkillSlot s) => s.gameSkill == null)?.SetSkill(gameSkill);
		}
		else
		{
			gameState.gameEventManager.TriggerConsumeSkill(gameSkill);
			gameState.playerState.consumedSkills.Add(gameSkill);
		}
		EnqueueAction(new ActivateSkillAction(newSkill, oldSkillSlot));
		FinishAction();
	}
}
