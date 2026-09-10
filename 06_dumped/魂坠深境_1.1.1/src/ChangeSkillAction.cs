using System.Linq;

public class ChangeSkillAction : ExecuteAction
{
	public enum Type
	{
		AddOutSlot,
		Remove,
		AddInSlot
	}

	private readonly GameSkill skill;

	private readonly GameSkillSlot skillSlot;

	private readonly Type type;

	public ChangeSkillAction(GameSkill skill, Type type, GameSkillSlot skillSlot = null)
	{
		this.skill = skill;
		this.type = type;
		this.skillSlot = skillSlot;
	}

	protected override void Update()
	{
		switch (type)
		{
		case Type.AddOutSlot:
			if (!gameState.playerState.ActivatedSkills.Contains(skill))
			{
				gameState.playerState.activatedSkills_outSlot.Add(skill);
			}
			skill.owner = gameState.playerState.bft;
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerAddSkill(skill);
			});
			break;
		case Type.Remove:
		{
			bool flag = gameState.playerState.activatedSkills_outSlot.Remove(skill);
			if (!flag)
			{
				GameSkillSlot gameSkillSlot = gameState.playerState.activeSkills_inSlot.Find((GameSkillSlot s) => s.gameSkill == skill);
				if (gameSkillSlot != null)
				{
					flag = true;
					gameSkillSlot.SetSkill(null);
				}
			}
			if (flag)
			{
				gameState.gameEventManager.TriggerRemoveSkill(skill);
			}
			skill.owner = null;
			break;
		}
		case Type.AddInSlot:
			if (skillSlot != null)
			{
				if (!gameState.playerState.ActivatedSkills.Contains(skill))
				{
					skillSlot.SetSkill(skill);
				}
				skill.owner = gameState.playerState.bft;
				EnqueueAction(delegate
				{
					gameState.gameEventManager.TriggerAddSkill(skill);
				});
			}
			break;
		}
	}
}
