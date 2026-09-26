using System.Collections.Generic;

public class zbWallSkillDataBase
{
	public int[] saveRecord = new int[5];

	public float sumRecord;

	public virtual void RefreshProperty()
	{
	}

	public virtual void TriggerAction(zbCombatData combatData)
	{
	}

	public virtual void UnRestoreData()
	{
	}

	public virtual void Restore(List<int> list)
	{
	}

	public virtual bool IsZuobi()
	{
		return false;
	}
}
