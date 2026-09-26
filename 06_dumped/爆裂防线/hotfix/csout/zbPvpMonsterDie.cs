using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPvpMonsterDie
{
	private class zbBurstSpawnItem
	{
		public int monsterTid;

		public Vector3 position;

		public Transform parent;

		public zbCombatMonster monsters;
	}

	private zbBaseWnd m_wndParent;

	private SpineVisualElement spineP;

	private VisualElement m_pProgress;

	private zbCombatData combatData;

	private readonly List<zbBurstSpawnItem> m_pendingSpawns = new List<zbBurstSpawnItem>();

	private const int MONSTER_SPAWN_PER_FRAME = 5;

	public zbPvpMonsterDie(VisualElement root, zbBaseWnd wndParent)
	{
		m_pProgress = UQueryExtensions.Q<VisualElement>(root, "bar_killProgress", (string)null);
		spineP = UQueryExtensions.Q<SpineVisualElement>(root, "spine_kill", (string)null);
		zbUtil.SetSpineVisibleWithFreeze(spineP, visible: false);
		zbFontReplacer.Instance.ReplaceFontInUI(root);
	}

	public void Refresh(zbCombatData _combatData)
	{
		combatData = _combatData;
		m_pendingSpawns.Clear();
	}

	public void UpdateUI()
	{
		zbUtil.SetWidthIfChanged(m_pProgress, (float)(120 * combatData.pvpMonsterDieNumber) / (float)combatData.pvpMonsterDieNumberMax);
	}

	public void TriggerCall()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		UpdateUI();
		DrainPendingSpawns();
		if (combatData == null || combatData.pvpMonsterDieNumber < combatData.pvpMonsterDieNumberMax)
		{
			return;
		}
		zbCombatData pvpCombatData = zbSelf.Instance.combatHandler.GetPvpCombatData(combatData);
		if (pvpCombatData == null)
		{
			return;
		}
		spineP.AnimationState.SetAnimation(0, "animation", false);
		zbUtil.SetSpineVisibleWithFreeze(spineP, visible: true);
		float num = Rand.Range(-2f, 2f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num + pvpCombatData.nodeRootPositonX, ((Component)pvpCombatData.combatScene.top).transform.position.y + pvpCombatData.nodeRootPositonY - 2f, 0f);
		combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(pvpCombatData.dataIndex + 10).InitData(combatData, pvpCombatData.dataIndex + 10, val, "animation");
		foreach (KeyValuePair<int, int> pvpMonsterDie in combatData.pvpMonsterDieList)
		{
			for (int i = 0; i < pvpMonsterDie.Value; i++)
			{
				m_pendingSpawns.Add(new zbBurstSpawnItem
				{
					monsterTid = pvpMonsterDie.Key,
					position = val,
					parent = ((Component)pvpCombatData.monsterSpawner).transform.parent,
					monsters = pvpCombatData.monsters
				});
			}
		}
		combatData.pvpMonsterDieNumber = 0;
		combatData.pvpMonsterDieList.Clear();
	}

	private void DrainPendingSpawns()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (m_pendingSpawns.Count != 0)
		{
			int num = Math.Min(5, m_pendingSpawns.Count);
			for (int i = 0; i < num; i++)
			{
				zbBurstSpawnItem zbBurstSpawnItem2 = m_pendingSpawns[i];
				zbMonster obj = zbBurstSpawnItem2.monsters.SpawnMonsterAtPosition(zbBurstSpawnItem2.monsterTid, zbBurstSpawnItem2.position, zbBurstSpawnItem2.parent, isPvpEnemy: true);
				float num2 = Rand.Range(0f, (float)Math.PI * 2f);
				zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
				zbExtraDirection2.direction = Vector2.op_Implicit(new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)));
				zbExtraDirection2.duration = 0.5f;
				zbExtraDirection2.speed = Rand.Range(-1f, 1f) / zbExtraDirection2.duration;
				obj.AddExtraDirectionList(zbExtraDirection2);
			}
			m_pendingSpawns.RemoveRange(0, num);
		}
	}
}
