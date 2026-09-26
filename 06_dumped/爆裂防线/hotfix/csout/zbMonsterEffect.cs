using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterEffect : MonoBehaviour
{
	[NonSerialized]
	public zbCombatData combatData;

	[NonSerialized]
	public SkeletonAnimation skeletonAnimation;

	[NonSerialized]
	private int monsterUUID;

	[NonSerialized]
	public zbMonsterEffectCsvRecord monsterEffectRecord;

	[NonSerialized]
	private float m_returnPoolTime;

	[NonSerialized]
	private float m_returnPoolTimeAccumTime;

	private bool activeFlag;

	[NonSerialized]
	public zbSpineGpuAnimPlayer gpuPlayer;

	[NonSerialized]
	public bool m_spineGpuTakenOver;

	protected virtual void OnEnable()
	{
		InitGameObject();
	}

	private void OnDisable()
	{
		zbSpineGpuAnimRuntime.ReleaseEffect(gpuPlayer);
		m_spineGpuTakenOver = false;
	}

	protected virtual void InitGameObject()
	{
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		}
	}

	private void Reset()
	{
		combatData = null;
		activeFlag = false;
		if ((Object)(object)skeletonAnimation != (Object)null && (Object)(object)((Component)skeletonAnimation).gameObject != (Object)null && ((Component)skeletonAnimation).gameObject.activeSelf)
		{
			((Component)skeletonAnimation).gameObject.SetActive(false);
		}
	}

	public void InitData(zbCombatData _combatData, int _monsterUUID, int monsterEffectId, float damageRange = 1f)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		InitGameObject();
		combatData = _combatData;
		((Component)this).gameObject.transform.SetParent(combatData.nodeRootMonsterSkillEffect);
		monsterUUID = _monsterUUID;
		monsterEffectRecord = zbDataConfig.Instance.monsterEffectCsvTable.GetRecordByKey(monsterEffectId);
		((Component)this).transform.localScale = new Vector3(damageRange, damageRange, 1f);
		m_returnPoolTime = float.MaxValue;
		m_returnPoolTimeAccumTime = 0f;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null && (Object)(object)skeletonAnimation != (Object)null)
		{
			SetThisPosition(monsterById);
			string spinePath = monsterEffectRecord.spinePath + "_SkeletonData.asset";
			zbCombatSpineAssetUtil.RefreshSpine(skeletonAnimation, spinePath);
			skeletonAnimation.UpdateTiming = (UpdateTiming)(!zbCombatSystem.Instance.combatFPS.IsSpriteReplaceMode());
			TryConvertGpuEffect(spinePath);
		}
		else
		{
			Free();
		}
	}

	public TrackEntry PlayEffectAnim(string clipName, bool loop)
	{
		if (m_spineGpuTakenOver && gpuPlayer != null)
		{
			gpuPlayer.PlayClip(clipName, loop);
		}
		return skeletonAnimation.AnimationState.SetAnimation(0, clipName, loop);
	}

	public void QueueEffectAnim(string clipName, bool loop)
	{
		if (m_spineGpuTakenOver && gpuPlayer != null)
		{
			gpuPlayer.QueueClip(clipName, loop);
		}
		skeletonAnimation.AnimationState.AddAnimation(0, clipName, loop, 0f);
	}

	protected bool TryConvertGpuEffect(string spinePath)
	{
		m_spineGpuTakenOver = false;
		zbSpineGpuAnimRuntime.RestoreDormantEffect(skeletonAnimation);
		if (gpuPlayer == null)
		{
			gpuPlayer = new zbSpineGpuAnimPlayer();
		}
		m_spineGpuTakenOver = zbSpineGpuAnimRuntime.TryConvertEffect(gpuPlayer, skeletonAnimation, spinePath, combatData);
		return m_spineGpuTakenOver;
	}

	public virtual void Update()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if (combatData == null)
		{
			return;
		}
		if (m_spineGpuTakenOver && gpuPlayer != null && (int)skeletonAnimation.UpdateTiming == 1)
		{
			gpuPlayer.Advance(Time.deltaTime);
		}
		if (m_returnPoolTimeAccumTime > 0f && !activeFlag)
		{
			activeFlag = true;
			if ((Object)(object)skeletonAnimation != (Object)null && (Object)(object)((Component)skeletonAnimation).gameObject != (Object)null)
			{
				((Component)skeletonAnimation).gameObject.SetActive(true);
			}
		}
		m_returnPoolTimeAccumTime += Time.deltaTime;
		if (m_returnPoolTimeAccumTime >= m_returnPoolTime)
		{
			ReturnToPoolImmediate();
			return;
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			SetThisPosition(monsterById);
		}
		else
		{
			Free();
		}
	}

	public virtual void SetThisPosition(zbMonster targetMonster)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.forward;
		if (monsterEffectRecord.front == 0)
		{
			val *= -1f;
		}
		((Component)this).transform.position = targetMonster.GetCurPosition() + val;
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
		combatData.monsterEffectPoolMgr.ReturnMonsterSkillEffect(this);
	}

	public virtual void Free()
	{
		ReturnToPool();
	}
}
