using System;
using Spine.Unity;
using UnityEngine;

public class zbBuffEffect : MonoBehaviour
{
	[NonSerialized]
	public zbCombatData combatData;

	[NonSerialized]
	public SkeletonAnimation skeletonAnimation;

	[NonSerialized]
	public SpriteRenderer buffSprite;

	private bool isUsingSprite;

	private GameObject buffSpriteGO;

	private GameObject skeletonAnimationGO;

	[NonSerialized]
	public zbSpineGpuAnimPlayer gpuPlayer;

	[NonSerialized]
	public bool m_spineGpuTakenOver;

	[NonSerialized]
	private int monsterUUID;

	[NonSerialized]
	public zbSkillBuffCsvRecord buffRecord;

	[NonSerialized]
	private float m_returnPoolTime;

	[NonSerialized]
	private float m_returnPoolTimeAccumTime;

	[NonSerialized]
	private Transform buffTF;

	public bool isSpriteReplaced => isUsingSprite;

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
		UseSpriteReplaceSpine(bUseSprite: false);
	}

	public void InitData(zbCombatData _combatData, int _monsterUUID, int _buffId)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)buffSprite == (Object)null)
		{
			buffSprite = ((Component)((Component)this).gameObject.transform.Find("spriteBuff")).GetComponent<SpriteRenderer>();
			buffSpriteGO = ((Component)buffSprite).gameObject;
		}
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
			skeletonAnimationGO = ((Component)skeletonAnimation).gameObject;
		}
		buffTF = ((Component)this).transform;
		Reset();
		combatData = _combatData;
		((Component)this).transform.SetParent(combatData.nodeRootBuffEffect);
		monsterUUID = _monsterUUID;
		buffRecord = zbDataConfig.Instance.skillBuffCsvTable.GetRecordByKey(_buffId);
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			((Component)this).transform.localScale = ((Component)monsterById).transform.localScale * (float)monsterById.monsterAvatarRecord.boxWidth / 70f;
			SetThisPosition(monsterById);
		}
		else
		{
			Free();
		}
		m_returnPoolTime = float.MaxValue;
		m_returnPoolTimeAccumTime = 0f;
		zbUtil.MoveToLastSibling(((Component)this).gameObject);
		if (zbCombatSystem.Instance.combatFPS.IsSpriteReplaceMode() && !monsterById.IsBoss())
		{
			UseSpriteReplaceSpine(bUseSprite: true);
			return;
		}
		string spinePath = buffRecord.spinePath + "_SkeletonData.asset";
		zbCombatSpineAssetUtil.RefreshSpine(skeletonAnimation, spinePath, "animation", loop: true);
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
			{
				skeletonAnimation.UpdateTiming = (UpdateTiming)0;
			}
			else
			{
				skeletonAnimation.UpdateTiming = (UpdateTiming)1;
			}
		}
		if (TryConvertGpuEffect(spinePath))
		{
			gpuPlayer.PlayClip("animation", loop: true);
			gpuPlayer.Advance(0f);
		}
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
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (combatData == null)
		{
			return;
		}
		if (!isUsingSprite)
		{
			if (zbCombatSystem.Instance.combatFPS.IsSpriteReplaceMode())
			{
				UseSpriteReplaceSpine(bUseSprite: true);
			}
			else if ((Object)(object)skeletonAnimation != (Object)null && (int)skeletonAnimation.UpdateTiming == 0)
			{
				if (Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
				{
					skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
					if (m_spineGpuTakenOver && gpuPlayer != null)
					{
						gpuPlayer.Advance(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
					}
				}
			}
			else if (m_spineGpuTakenOver && gpuPlayer != null && (Object)(object)skeletonAnimation != (Object)null)
			{
				gpuPlayer.Advance(Time.deltaTime);
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
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		buffTF.position = targetMonster.monsterTF.position;
		if (buffRecord.spinePoint == 1)
		{
			if (targetMonster.combatData.dataIndex == 1)
			{
				buffTF.position = targetMonster.monsterTF.position + Vector3.up * (float)targetMonster.monsterAvatarRecord.boxHeight * targetMonster.bodyScale / 100f;
			}
			else
			{
				buffTF.position = targetMonster.monsterTF.position - Vector3.up * (float)targetMonster.monsterAvatarRecord.boxHeight * targetMonster.bodyScale / 100f;
			}
		}
		else if (buffRecord.spinePoint == 2)
		{
			buffTF.position = targetMonster.monsterTF.position + Vector3.back;
			if (targetMonster.combatData.dataIndex == 1)
			{
				buffTF.rotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else
			{
				buffTF.rotation = Quaternion.Euler(0f, 0f, 180f);
			}
		}
		else if (buffRecord.spinePoint == 3)
		{
			buffTF.position = targetMonster.monsterTF.position + Vector3.forward;
			if (targetMonster.combatData.dataIndex == 1)
			{
				buffTF.rotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else
			{
				buffTF.rotation = Quaternion.Euler(0f, 0f, 180f);
			}
		}
		else if (buffRecord.spinePoint == 4)
		{
			if (targetMonster.combatData.dataIndex == 1)
			{
				buffTF.position = targetMonster.GetCurPosition() + Vector3.back;
			}
			else
			{
				buffTF.position = targetMonster.GetCurPosition() + Vector3.back - Vector3.up * 2f * ((float)targetMonster.monsterAvatarRecord.boxHeight * targetMonster.bodyScale / 300f + (float)targetMonster.monsterAvatarRecord.boxOffset * targetMonster.bodyScale / 100f);
			}
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
		combatData.buffEffectPoolMgr.ReturnBuffEffect(this);
	}

	public virtual void Free()
	{
		ReturnToPool();
	}

	public void UseSpriteReplaceSpine(bool bUseSprite)
	{
		isUsingSprite = bUseSprite;
		if ((Object)(object)buffSprite != (Object)null)
		{
			zbUtil.SetActive(buffSpriteGO, bUseSprite);
		}
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			zbUtil.SetActive(skeletonAnimationGO, !bUseSprite);
		}
		if (bUseSprite && (Object)(object)buffSprite != (Object)null)
		{
			Sprite val = zbAssetLoader.Instance.LoadCombatSpriteAsset($"ui/image/icon/buff/{buffRecord.buffIndex}.png");
			if ((Object)(object)val != (Object)null)
			{
				buffSprite.sprite = val;
			}
			else
			{
				zbUtil.SetActive(buffSpriteGO, active: false);
			}
		}
	}
}
