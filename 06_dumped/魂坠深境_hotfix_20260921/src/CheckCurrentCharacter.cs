public class CheckCurrentCharacter : CheckNode
{
	public CharacterClassDef character;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState == null || character == null)
		{
			return null;
		}
		return eve.gameState.playerState.CharacterDef.attachClass == character;
	}
}
