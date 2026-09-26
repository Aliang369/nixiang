using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterEyeEffect : MonoBehaviour
{
	[NonSerialized]
	public zbCombatData combatData;

	[NonSerialized]
	public SkeletonAnimation skeletonAnimation;

	[NonSerialized]
	private int monsterUUID;

	[NonSerialized]
	public zbMonsterSkillCsvRecord monsterSkillRecord;

	[NonSerialized]
	public zbMonsterSkillIndexCsvRecord monsterSkillIndexRecord;

	[NonSerialized]
	public zbSpineGpuAnimPlayer gpuPlayer;

	[NonSerialized]
	public bool m_spineGpuTakenOver;

	private const string kEyeSpinePath = "animation/effect/monster/monster_eye/monster_eye_SkeletonData.asset";

	protected virtual void OnEnable()
	{
	}

	private void OnDisable()
	{
		zbSpineGpuAnimRuntime.ReleaseEffect(gpuPlayer);
		m_spineGpuTakenOver = false;
	}

	private void Reset()
	{
		combatData = null;
	}

	public void InitData(zbCombatData _combatData, int _monsterUUID)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		}
		combatData = _combatData;
		((Component)this).gameObject.transform.SetParent(combatData.nodeRootMonsterSkillEffect);
		monsterUUID = _monsterUUID;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			((Component)this).transform.localScale = ((Component)monsterById).transform.localScale;
			SetThisPosition(monsterById);
			zbCombatSpineAssetUtil.RefreshSpine(skeletonAnimation, "animation/effect/monster/monster_eye/monster_eye_SkeletonData.asset", "animation", loop: true);
			if (TryConvertGpuEffect())
			{
				gpuPlayer.Advance(0f);
			}
		}
		else
		{
			Free();
		}
	}

	private bool TryConvertGpuEffect()
	{
		m_spineGpuTakenOver = false;
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			return false;
		}
		zbSpineGpuAnimRuntime.RestoreDormantEffect(skeletonAnimation);
		if (gpuPlayer == null)
		{
			gpuPlayer = new zbSpineGpuAnimPlayer();
		}
		m_spineGpuTakenOver = zbSpineGpuAnimRuntime.TryConvertEffect(gpuPlayer, skeletonAnimation, "animation/effect/monster/monster_eye/monster_eye_SkeletonData.asset", combatData);
		if (!m_spineGpuTakenOver)
		{
			return false;
		}
		TrackEntry val = ((skeletonAnimation.AnimationState != null) ? skeletonAnimation.AnimationState.GetCurrent(0) : null);
		if ((val == null || val.Animation == null || !(val.Animation.Name != "<empty>") || !gpuPlayer.PlayClip(val.Animation.Name, val.Loop)) && !gpuPlayer.PlayClip("animation", loop: true))
		{
			m_spineGpuTakenOver = false;
			gpuPlayer.Unbind();
			zbSpineGpuAnimRuntime.RestoreDormantEffect(skeletonAnimation);
			return false;
		}
		return true;
	}

	public virtual void Update()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (combatData == null)
		{
			return;
		}
		if (m_spineGpuTakenOver && gpuPlayer != null)
		{
			gpuPlayer.Advance(Time.deltaTime);
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			SetThisPosition(monsterById);
			if (combatData.GetSceneAlertRange() + combatData.defense.GetAlertPositionY() > ((Component)monsterById).transform.position.y)
			{
				Free();
			}
		}
		else
		{
			Free();
		}
	}

	public virtual void SetThisPosition(zbMonster targetMonster)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 curPosition = targetMonster.GetCurPosition();
		((Component)this).transform.position = new Vector3(curPosition.x, curPosition.y, -6f);
		((Component)this).transform.localScale = ((Component)targetMonster).transform.localScale;
	}

	public void ReturnToPool()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			combatData.monsterEyeEffectPoolMgr.ReturnMonsterEyeEffect(this);
		}
	}

	public virtual void Free()
	{
		ReturnToPool();
	}
}
