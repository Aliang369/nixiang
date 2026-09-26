using System.Collections.Generic;
using UnityEngine;

public class zbMechaHuo01Sub : MonoBehaviour
{
	public HashSet<int> m_monstersInRange = new HashSet<int>();

	protected void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null && (long)component.curHp > 0)
		{
			m_monstersInRange.Add(component.GetUUID());
		}
	}

	protected void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null && (long)component.curHp > 0)
		{
			m_monstersInRange.Remove(component.GetUUID());
		}
	}
}
