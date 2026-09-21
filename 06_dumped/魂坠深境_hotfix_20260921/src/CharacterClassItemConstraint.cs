using System.Collections.Generic;
using System.Linq;

public class CharacterClassItemConstraint : ItemConstraint
{
	public List<CharacterClassDef> characterClasses = new List<CharacterClassDef>();

	public bool requireExclusiveMatch;

	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		List<CharacterClassType> list = item.GetDef()?.generationInfo?.attachClass;
		if (list == null || characterClasses == null)
		{
			return false;
		}
		int num = (from characterClass in characterClasses
			where characterClass != null
			select characterClass.mClass).Distinct().Count(list.Contains);
		return requireExclusiveMatch ? (num == 1) : (num > 0);
	}
}
