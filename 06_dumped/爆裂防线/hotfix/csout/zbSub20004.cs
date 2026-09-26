using System.Collections;
using Proto.Enums;
using UnityEngine;

public class zbSub20004 : zbSkillBase
{
	private int runState = 1;

	private zbMonster monster20004;

	private float duration20004;

	private float duration;

	protected override void OnInited()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "open", false);
			m_skeletonAnimation.AnimationState.AddAnimation(0, "idle", true, 0f);
		}
		float num = GetAttrValueByType(EAttrSkillType.DamageRange) / 2f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		duration = GetAttrValueByType(EAttrSkillType.Duration);
		runState = 1;
		monster20004 = null;
		duration20004 = 0f;
	}

	protected override void Update()
	{
		base.Update();
		duration20004 += Time.deltaTime;
		if (duration20004 >= duration && (Object)(object)monster20004 == (Object)null)
		{
			ReturnToPool();
			duration20004 = 0f;
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)monster20004 == (Object)null && runState == 1)
		{
			zbMonster component = ((Component)collider).GetComponent<zbMonster>();
			if ((Object)(object)component != (Object)null && (long)component.curHp > 0 && component.GetAttrValueByType(EAttrMonsterType.Mass) <= skillSubRecord.param[0])
			{
				monster20004 = component;
				monster20004.SetBoxColliderEnabel(bEnabale: false);
				((Component)monster20004).transform.position = new Vector3(combatData.nodeRootPositonX, 100f + combatData.nodeRootPositonY, ((Component)monster20004).transform.position.z);
				((MonoBehaviour)this).StartCoroutine(ActionRoutine());
				runState = 2;
			}
		}
	}

	private IEnumerator ActionRoutine()
	{
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "close", false);
		}
		((Component)monster20004).gameObject.SetActive(false);
		yield return (object)new WaitForSeconds(13f / 30f);
		float y = Mathf.Max(((Component)this).transform.position.y - 5f, -5.5f);
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, y, ((Component)this).transform.position.z);
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "open", false);
		}
		yield return (object)new WaitForSeconds(13f / 30f);
		((Component)monster20004).transform.position = new Vector3(((Component)this).transform.position.x, y, ((Component)monster20004).transform.position.z);
		((Component)monster20004).gameObject.SetActive(true);
		monster20004.SetBoxColliderEnabel(bEnabale: true);
		duration20004 = 0f;
		monster20004 = null;
		ReturnToPool(7f / 30f);
	}
}
