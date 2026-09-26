using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterOnceEffect : MonoBehaviour
{
	[NonSerialized]
	public zbCombatData combatData;

	[NonSerialized]
	public SkeletonAnimation skeletonAnimation;

	[NonSerialized]
	public zbMonsterEffectCsvRecord monsterEffectRecord;

	[NonSerialized]
	private float m_returnPoolTime;

	[NonSerialized]
	private float m_returnPoolTimeAccumTime;

	private bool activeFlag;

	private string aniamtionName;

	private bool selfControl;

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

	public void InitData(zbCombatData _combatData, int monsterEffectId, Vector3 vector3, string _aniamtionName, bool _selfControl = false)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		InitGameObject();
		selfControl = _selfControl;
		combatData = _combatData;
		aniamtionName = _aniamtionName;
		((Component)this).gameObject.transform.SetParent(combatData.nodeRootMonsterSkillEffect);
		monsterEffectRecord = zbDataConfig.Instance.monsterEffectCsvTable.GetRecordByKey(monsterEffectId);
		m_returnPoolTime = float.MaxValue;
		m_returnPoolTimeAccumTime = 0f;
		((Component)this).transform.position = vector3 - Vector3.forward * 2f;
		string spinePath = monsterEffectRecord.spinePath + "_SkeletonData.asset";
		zbCombatSpineAssetUtil.RefreshSpine(skeletonAnimation, spinePath);
		skeletonAnimation.UpdateTiming = (UpdateTiming)(!zbCombatSystem.Instance.combatFPS.IsSpriteReplaceMode());
		TryConvertGpuEffect(spinePath);
		if (!selfControl)
		{
			((MonoBehaviour)this).StartCoroutine(PlayAction());
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

	public IEnumerator PlayAction()
	{
		TrackEntry val = PlayEffectAnim(aniamtionName, loop: false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		Free();
	}

	public virtual void Update()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
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
		}
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
		combatData.monsterOnceEffectPoolMgr.ReturnMonsterSkillEffect(this);
	}

	public virtual void Free()
	{
		ReturnToPool();
	}
}
