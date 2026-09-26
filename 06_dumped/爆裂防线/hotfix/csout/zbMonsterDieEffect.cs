using System.Collections;
using Spine.Unity;
using UnityEngine;

public class zbMonsterDieEffect : MonoBehaviour
{
	private zbCombatData combatData;

	private SkeletonAnimation skeletonAnimation;

	private int monsterUUID;

	private float m_returnPoolTime;

	private float m_returnPoolTimeAccumTime;

	private const float FLY_START_DELAY = 0.5f;

	private const float MIN_FLY_DURATION = 0.4f;

	private const float BASE_SPEED = 6f;

	private Vector3 m_initialScale;

	private void Reset()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		combatData = null;
		m_returnPoolTime = float.MaxValue;
		m_returnPoolTimeAccumTime = 0f;
		((Component)this).transform.localScale = Vector3.one;
	}

	public void InitData(zbCombatData _combatData, Vector3 targetVector, Vector3 localScale)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		}
		combatData = _combatData;
		((Component)this).gameObject.transform.SetParent(combatData.nodeRootMonsterSkillEffect);
		SetThisPosition(targetVector, localScale);
		m_initialScale = ((Component)this).transform.localScale;
		Vector3 position = ((Component)this).transform.position;
		zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		Rect worldBound = zbCombatWnd2.m_killCount.worldBound;
		Vector3 val = zbUtil.UIToolkitToWorldSpace(((Rect)(ref worldBound)).position, zbCameraHelper.Instance.mainCamera, zbCombatWnd2.m_killCount);
		((MonoBehaviour)this).StartCoroutine(DelayThenFly(position, new Vector3(val.x + 0.2f, val.y - 0.25f, val.z)));
	}

	private IEnumerator DelayThenFly(Vector3 startPos, Vector3 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		yield return null;
		if ((Object)(object)skeletonAnimation != (Object)null && skeletonAnimation.AnimationState != null)
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "animation", true);
		}
		yield return (object)new WaitForSeconds(0.5f);
		yield return FlyBezierToTarget(((Component)this).transform.position, targetPos, 6f);
	}

	private IEnumerator FlyBezierToTarget(Vector3 startPos, Vector3 targetPos, float baseSpeed)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		startPos = ((Component)this).transform.position;
		float num = Vector3.Distance(startPos, targetPos);
		float duration = Mathf.Max(num / baseSpeed, 0.4f);
		Vector3 val = targetPos - startPos;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(0f - normalized.y, normalized.x, 0f);
		float num2 = Rand.Range(2f, 3f) * num / 10f;
		float num3 = Rand.Range(-1f, 1f);
		float num4 = Rand.Range(-0.5f, 0.5f);
		float num5 = 1f;
		if (startPos.x > 0f && startPos.x < 3.6f)
		{
			num5 = -1f;
		}
		else if (startPos.x < 0f && startPos.x > -3.6f)
		{
			num5 = 1f;
		}
		float num6 = Rand.Range(0.15f, 0.35f);
		Vector3 controlPos = startPos + (targetPos - startPos) * num6;
		controlPos += val2 * num2 * num5;
		controlPos.x += num3;
		controlPos.z += num4;
		float elapsed = 0f;
		float num7 = 180f;
		float totalRotation = Rand.Range(2f, 4f) * num7;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float num8 = EaseInQuad(t);
			Vector3 position = CalculateBezierPoint(num8, startPos, controlPos, targetPos);
			((Component)this).transform.position = position;
			((Component)this).transform.localScale = m_initialScale;
			if ((Object)(object)skeletonAnimation != (Object)null)
			{
				float num9 = Mathf.Lerp(0f, totalRotation, num8);
				((Component)skeletonAnimation).transform.localEulerAngles = new Vector3(0f, 0f, num9);
			}
			yield return null;
		}
		((Component)this).transform.position = targetPos;
		((Component)this).transform.localScale = m_initialScale;
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			((Component)skeletonAnimation).transform.localEulerAngles = Vector3.zero;
		}
		(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd).PlayIconScaleAnim();
		ReturnToPool(0.01f);
	}

	private float EaseInQuad(float t)
	{
		return t * t;
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		return num * num * p0 + 2f * num * t * p1 + t * t * p2;
	}

	public void SetThisPosition(Vector3 targetVector, Vector3 localScale)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = new Vector3(targetVector.x, targetVector.y, -6f);
		((Component)this).transform.localScale = localScale;
	}

	public void ReturnToPool(float delayTime = 0f)
	{
		if (((Component)this).gameObject.activeSelf)
		{
			m_returnPoolTime = delayTime;
			m_returnPoolTimeAccumTime = 0f;
			if (delayTime == 0f)
			{
				ReturnToPoolImmediate();
			}
		}
	}

	private void ReturnToPoolImmediate()
	{
		if (((Component)this).gameObject.activeSelf && combatData != null && combatData.monsterDieEffectPoolMgr != null)
		{
			combatData.monsterDieEffectPoolMgr.ReturnMonsterDieEffect(this);
		}
	}

	private void Update()
	{
		m_returnPoolTimeAccumTime += Time.deltaTime;
		if (m_returnPoolTimeAccumTime >= m_returnPoolTime)
		{
			ReturnToPoolImmediate();
		}
	}
}
