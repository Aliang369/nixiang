using System.Collections.Generic;

public class stRecordDefense
{
	public long maxHp;

	public long curHp;

	public long shieldHp;

	public int damageImmunity;

	public int damageDecr;

	public Dictionary<int, List<int>> wallSkillIndexsP1 = new Dictionary<int, List<int>>();

	public Dictionary<int, List<int>> wallSkillIndexsP2 = new Dictionary<int, List<int>>();
}
