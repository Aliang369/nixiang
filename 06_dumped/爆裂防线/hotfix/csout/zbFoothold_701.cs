using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zbFoothold_701 : zbFootholdBase
{
	public stFootholdIndex701 footholdIndex701Data = new stFootholdIndex701();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private int[] metroIndex = new int[5] { -4, -2, 0, 2, 4 };

	private const float Interval = 0.1f;

	private static readonly WaitForSeconds WaitInterval;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex701Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex701Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex701Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex701Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex701Data.dazeTime = footholdEffectRecord.param[num];
		num++;
		m_damageInterval = footholdIndex701Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex701Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			TriggerDamage();
		}
	}

	private void TriggerDamage()
	{
		List<int> randomItems = Rand.GetRandomItems(metroIndex, footholdIndex701Data.triggerNumber);
		for (int i = 0; i < footholdIndex701Data.triggerNumber; i++)
		{
			float y = ((i > 4) ? (Rand.Range(-4f, 3f) + combatData.nodeRootPositonY) : ((float)randomItems[i] + combatData.nodeRootPositonY));
			((MonoBehaviour)this).StartCoroutine(TriggerNumber(y));
		}
	}

	public IEnumerator TriggerNumber(float y)
	{
		Vector3 spawnPostion = default(Vector3);
		for (int i = 0; i < 5; i++)
		{
			((Vector3)(ref spawnPostion))._002Ector((float)(i - 2) * 1.5f + combatData.nodeRootPositonX, y);
			((zbFootholdIndex701)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex701.prefab", this, spawnPostion, Vector2.up)).SetFootholdAtr(footholdIndex701Data, footholdEffectRecord);
			yield return WaitInterval;
		}
	}

	static zbFoothold_701()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		WaitInterval = new WaitForSeconds(0.1f);
	}
}
