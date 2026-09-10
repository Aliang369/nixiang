using System.Collections.Generic;

public class ChangSkillSlotCountAction : ExecuteAction
{
	public enum Type
	{
		Add,
		Remove
	}

	private Type type;

	private int count;

	public ChangSkillSlotCountAction(int count, Type type)
	{
		this.count = count;
		this.type = type;
	}

	protected override void Update()
	{
		switch (type)
		{
		case Type.Add:
		{
			for (int j = 0; j < count; j++)
			{
				gameState.playerState.activeSkills_inSlot.Add(new GameSkillSlot());
			}
			break;
		}
		case Type.Remove:
		{
			for (int i = 0; i < count; i++)
			{
				List<GameSkillSlot> activeSkills_inSlot = gameState.playerState.activeSkills_inSlot;
				GameSkillSlot gameSkillSlot = activeSkills_inSlot[activeSkills_inSlot.Count - 1];
				gameState.playerState.activeSkills_inSlot.Remove(gameSkillSlot);
				if (gameSkillSlot.gameSkill != null)
				{
					gameState.playerState.activatedSkills_outSlot.Add(gameSkillSlot.gameSkill);
				}
			}
			break;
		}
		}
	}
}
