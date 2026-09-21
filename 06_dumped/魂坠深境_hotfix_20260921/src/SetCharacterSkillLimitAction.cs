public class SetCharacterSkillLimitAction : ExecuteAction
{
	private readonly int value;

	public SetCharacterSkillLimitAction(int value)
	{
		this.value = value;
	}

	protected override void Update()
	{
		for (int i = gameState.playerState.skillLimit; i < value; i++)
		{
			gameState.playerState.activeSkills_inSlot.Add(new GameSkillSlot());
		}
		for (int j = value; j < gameState.playerState.skillLimit; j++)
		{
			gameState.playerState.activeSkills_inSlot.RemoveAt(gameState.playerState.activeSkills_inSlot.Count - 1);
		}
		gameState.playerState.skillLimit = value;
	}
}
