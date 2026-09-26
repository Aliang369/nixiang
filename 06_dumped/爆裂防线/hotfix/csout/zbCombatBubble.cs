using UnityEngine;

public class zbCombatBubble
{
	private zbCombatData m_combatData;

	public zbBubblePool m_bubblePool;

	public void Init(zbCombatData combatData)
	{
		m_combatData = combatData;
		m_bubblePool = new zbBubblePool();
		m_bubblePool.Init(m_combatData);
	}

	public void Free()
	{
		m_bubblePool.Free();
	}

	public void ShowBubble(Vector3 bubblePosition, ECombatBubble bubbleType, long num)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			if (m_combatData.dataIndex == 1)
			{
				((Vector3)(ref bubblePosition)).Set((bubblePosition.x - m_combatData.nodeRootPositonX) * 0.8f, (bubblePosition.y - m_combatData.nodeRootPositonY + 8.4f) * 0.8f - 8.4f, bubblePosition.z);
			}
			else
			{
				((Vector3)(ref bubblePosition)).Set(0f - (bubblePosition.x - m_combatData.nodeRootPositonX) * 0.8f, 0f - ((bubblePosition.y - m_combatData.nodeRootPositonY + 8.4f) * 0.8f - 8.4f) + 1f, bubblePosition.z);
			}
		}
		else
		{
			((Vector3)(ref bubblePosition)).Set(bubblePosition.x - m_combatData.nodeRootPositonX, bubblePosition.y - m_combatData.nodeRootPositonY, bubblePosition.z);
		}
		m_bubblePool.AddRequest(bubblePosition, bubbleType, num);
	}

	public void ReturnBubble(zbBubbleNumber bubble)
	{
		m_bubblePool.ReturnBubble(bubble);
	}

	public void Update()
	{
		if (!zbCombatSystem.Instance.IsPaused())
		{
			m_bubblePool.Update();
		}
	}
}
