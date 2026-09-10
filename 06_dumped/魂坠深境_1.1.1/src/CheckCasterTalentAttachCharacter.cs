public class CheckCasterTalentAttachCharacter : CheckNode
{
	public CharacterClassDef character;

	public override bool? Check(GameEvent eve)
	{
		if (character == null)
		{
			return null;
		}
		TalentDef eveObj = eve.GetEveObj<TalentDef>(GameEvent.EveObjType.Caster);
		if (eveObj == null)
		{
			return null;
		}
		return eveObj.attachCharacter == character;
	}
}
