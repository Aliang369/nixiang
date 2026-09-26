using UnityEngine;

public class zbBuffDieTodo : zbBuffBase
{
	public override void RefreshProperty()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			for (int num = monsterById.buffDieTodo.Count - 1; num >= 0; num--)
			{
				zbBuffBase obj = monsterById.buffDieTodo[num];
				obj.Update();
				if (obj.buffRecord.id == buffRecord.id)
				{
					monsterById.buffDieTodo.RemoveAt(num);
					break;
				}
			}
			monsterById.buffDieTodo.Add(this);
		}
		durationTime = 999999f;
	}
}
