using System;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSkillBase : MonoBehaviour
{
	[NonSerialized]
	public zbCombatData combatData;

	[NonSerialized]
	private string m_prefabPath;

	[NonSerialized]
	public zbAttackerBase hero;

	[NonSerialized]
	private int m_UUID;

	[NonSerialized]
	public int mainMonsterUUID;

	[NonSerialized]
	public int targetMonsterUUID;

	protected Vector3 m_direction;

	protected ESortLayerType m_sortType;

	[NonSerialized]
	protected float lifetime;

	[NonSerialized]
	private float m_returnPoolTime;

	[NonSerialized]
	private float m_returnPoolTimeAccumTime;

	[NonSerialized]
	public zbSkillCsvRecord skillRecord;

	[NonSerialized]
	public zbSkillSubCsvRecord skillSubRecord;

	[NonSerialized]
	public zbMechaSkillCsvRecord mechaSkillRecord;

	[NonSerialized]
	public zbWallSkillCsvRecord wallSkillRecord;

	[NonSerialized]
	public zbFootholdEffectCsvRecord footholdEffectRecord;

	[NonSerialized]
	public Dictionary<EAttrSkillType, float> skillAttrList;

	[NonSerialized]
	public int hitNumber;

	[NonSerialized]
	public int ricochetCount;

	private static readonly float sceneBoxWidth = 8.4f;

	private static readonly float sceneBoxHeight = 18.2f;

	[NonSerialized]
	private static int sAutoSkillUUID;

	[NonSerialized]
	public zbSpineGpuAnimPlayer gpuPlayer;

	[NonSerialized]
	public bool m_spineGpuTakenOver;

	[NonSerialized]
	private bool m_gpuConvertPending;

	[NonSerialized]
	private string m_gpuSpinePath;

	[NonSerialized]
	public SkeletonAnimation m_skeletonAnimation;

	[NonSerialized]
	public int currPenetration;

	private int speedChange;

	[NonSerialized]
	public zbSkillBase parentSkillBase;

	[NonSerialized]
	public bool triggerEndMod;

	private readonly List<stMainIntervalToSubSkill> _tempIntervalList;

	[NonSerialized]
	public List<zbMonster> monsterRandomList;

	private float mainIntervalToSubSkillTime;

	private float mainIntervalToSubSkillTimePre;

	[NonSerialized]
	public float needDurationPre;

	[NonSerialized]
	public List<zbMonster> overlapResultList;

	[NonSerialized]
	public readonly HashSet<int> ignoreMonsters;

	[NonSerialized]
	public bool canOutWindow;

	[NonSerialized]
	public bool ignoreAnimation;

	protected GameObject m_spriteMonster;

	[NonSerialized]
	public Transform skillTF;

	[NonSerialized]
	public GameObject skillGO;

	[NonSerialized]
	public bool isMainSkill;

	[NonSerialized]
	public bool isSubSkill;

	[NonSerialized]
	public bool isWallSkill;

	[NonSerialized]
	public bool isMechaSkill;

	[NonSerialized]
	public bool isFootholdSkill;

	[NonSerialized]
	public bool isBallistic;

	[NonSerialized]
	public bool isSubSkillTagExplode;

	private Vector3 dataVecter3;

	protected virtual bool IsGpuBulletExcluded => false;

	public zbSkillParentType skillParentType { get; set; }

	public zbCombatMonsterGrid m_combatMonsterGrid { get; private set; }

	protected void MarkGpuEffectConvert(string spinePath)
	{
		if (!string.IsNullOrEmpty(spinePath))
		{
			m_gpuSpinePath = spinePath;
			m_gpuConvertPending = true;
		}
	}

	protected TrackEntry PlayBulletAnim(string clip, bool loop)
	{
		TrackEntry result = m_skeletonAnimation.AnimationState.SetAnimation(0, clip, loop);
		if (m_spineGpuTakenOver && gpuPlayer != null)
		{
			gpuPlayer.PlayClip(clip, loop);
		}
		return result;
	}

	protected void QueueBulletAnim(string clip, bool loop)
	{
		m_skeletonAnimation.AnimationState.AddAnimation(0, clip, loop, 0f);
		if (m_spineGpuTakenOver && gpuPlayer != null)
		{
			gpuPlayer.QueueClip(clip, loop);
		}
	}

	public bool IsMainSkill()
	{
		return IsSkillParentType(zbSkillParentType.HeroMain);
	}

	public bool IsMechaSkill()
	{
		return IsSkillParentType(zbSkillParentType.MechaSkill);
	}

	public bool IsSubSkill()
	{
		return IsSkillParentType(zbSkillParentType.HeroSub);
	}

	public bool IsWallSkill()
	{
		return IsSkillParentType(zbSkillParentType.WallSkill);
	}

	public bool IsFootholdSkill()
	{
		return IsSkillParentType(zbSkillParentType.FootholdSkill);
	}

	public bool IsSkillParentType(zbSkillParentType _skillParentType)
	{
		return skillParentType == _skillParentType;
	}

	public void ReturnToPool(float delayTime = 0f)
	{
		m_returnPoolTime = delayTime;
		m_returnPoolTimeAccumTime = 0f;
		if (delayTime == 0f)
		{
			ReturnToPoolImmediate();
		}
	}

	private void ReturnToPoolImmediate()
	{
		combatData.skillPoolMgr.ReturnSkill(m_prefabPath, this);
	}

	protected void Reset()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		combatData = null;
		m_combatMonsterGrid = null;
		m_prefabPath = "";
		hero = null;
		m_UUID = -1;
		mainMonsterUUID = -1;
		m_direction = Vector3.up;
		m_sortType = ESortLayerType.Projectile;
		lifetime = 5f;
		hitNumber = 0;
		triggerEndMod = true;
		m_gpuConvertPending = false;
		m_gpuSpinePath = null;
		skillRecord = null;
		skillSubRecord = null;
		skillAttrList.Clear();
		monsterRandomList.Clear();
		overlapResultList.Clear();
		m_returnPoolTime = float.MaxValue;
		m_returnPoolTimeAccumTime = 0f;
		isMainSkill = false;
		isSubSkill = false;
		isWallSkill = false;
		isFootholdSkill = false;
		isMechaSkill = false;
		isSubSkillTagExplode = false;
	}

	protected virtual void OnInited()
	{
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_skeletonAnimation == (Object)null)
		{
			m_skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		}
		if ((Object)(object)m_spriteMonster == (Object)null)
		{
			Transform obj = ((Component)this).transform.Find("spriteMonster");
			m_spriteMonster = ((obj != null) ? ((Component)obj).gameObject : null);
		}
		SpriteReplaceMode();
		isMainSkill = IsMainSkill();
		isSubSkill = IsSubSkill();
		isWallSkill = IsWallSkill();
		isMechaSkill = IsMechaSkill();
		isFootholdSkill = IsFootholdSkill();
		isSubSkillTagExplode = false;
		m_combatMonsterGrid = combatData.monsters.monsterGrid;
		if (isMainSkill)
		{
			isBallistic = IsSkillRecordTagsContainsTag(hero.skillRecord.tag, 1);
			if ((Object)(object)m_skeletonAnimation != (Object)null)
			{
				string text = "animation/effect/hero/" + hero.skillRecord.bulletSpineName + "/" + hero.skillRecord.bulletSpineName + "_SkeletonData.asset";
				if (!ignoreAnimation)
				{
					zbCombatSpineAssetUtil.RefreshSpine(m_skeletonAnimation, text);
					((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
					m_gpuSpinePath = text;
					m_gpuConvertPending = true;
				}
			}
			lifetime = 5f;
			speedChange = skillRecord.speedChange;
			if ((Object)(object)hero.goRiseEffect != (Object)null)
			{
				hero.goRiseEffect.SetActive(true);
			}
			_tempIntervalList.Clear();
			foreach (stMainIntervalToSubSkill item in hero.team.mainIntervalToSubSkill)
			{
				if (item.skillType == hero.GetSkillType())
				{
					_tempIntervalList.Add(item);
				}
			}
			if (_tempIntervalList.Count > 0)
			{
				mainIntervalToSubSkillTime = _tempIntervalList[0].damageInterval;
				mainIntervalToSubSkillTimePre = _tempIntervalList[0].damageInterval;
			}
		}
		else if (isSubSkill)
		{
			isBallistic = IsSkillRecordTagsContainsTag(skillSubRecord.tag, 1);
			isSubSkillTagExplode = IsSkillRecordTagsContainsTag(skillSubRecord.tag, 4);
			if ((Object)(object)m_skeletonAnimation != (Object)null)
			{
				((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
			}
			if (skillSubRecord.bottomLayer == 1)
			{
				m_sortType = ESortLayerType.BurnLayer;
			}
			if (skillSubRecord.id == 20001 || skillSubRecord.id == 20001)
			{
				m_sortType = ESortLayerType.Caocong;
			}
			lifetime = skillSubRecord.duration;
			speedChange = skillSubRecord.speedChange;
		}
		else if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		}
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		ignoreMonsters.Clear();
	}

	protected virtual void Awake()
	{
		skillTF = ((Component)this).transform;
		skillGO = ((Component)this).gameObject;
		skillAttrList = new Dictionary<EAttrSkillType, float>();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		zbSpineGpuAnimRuntime.ReleaseEffect(gpuPlayer);
		m_spineGpuTakenOver = false;
		m_gpuConvertPending = false;
		if ((Object)(object)hero == (Object)null || combatData == null || !((Component)combatData.nodeRoot).gameObject.activeInHierarchy || zbCombatSystem.Instance.combatStateMachine.GetStateId() == ECombatState.Balance)
		{
			return;
		}
		bool hasFetchedMonsters = false;
		if (isMainSkill)
		{
			if ((Object)(object)hero.goRiseEffect != (Object)null && skillRecord.type != 1)
			{
				hero.goRiseEffect.SetActive(false);
			}
			if (!triggerEndMod)
			{
				return;
			}
			if (hero.GetSkillType() != 8 && hero.team.mainEndToSubSkillBySkill.TryGetValue(hero.GetSkillType(), out var value))
			{
				EnsureFetchMonsters();
				foreach (stMainEndToSubSkill item in value)
				{
					for (int i = 0; i < item.subCount; i++)
					{
						zbSkillSystem.Instance.CreateSubSkill(item.subSkillId, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, mainMonsterUUID, (monsterRandomList.Count > i) ? monsterRandomList[i].GetUUID() : 0);
					}
				}
			}
			{
				foreach (stSkillEndToSubSkill item2 in hero.team.skillEndToSubSkill)
				{
					if (item2.srcSkillId == hero.skillRecord.id)
					{
						EnsureFetchMonsters();
						for (int j = 0; j < item2.subCount; j++)
						{
							zbSkillSystem.Instance.CreateSubSkill(item2.subSkillId, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, mainMonsterUUID, (monsterRandomList.Count > j) ? monsterRandomList[j].GetUUID() : 0);
						}
					}
				}
				return;
			}
		}
		if (!isSubSkill)
		{
			return;
		}
		foreach (stSubSkillEndToSubSkill item3 in hero.team.subSkillEndToSubSkill)
		{
			if (item3.srcSubSkillId == skillSubRecord.id)
			{
				EnsureFetchMonsters();
				for (int k = 0; k < item3.subCount; k++)
				{
					zbSkillSystem.Instance.CreateSubSkill(item3.subSkillId, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, mainMonsterUUID, (monsterRandomList.Count > k) ? monsterRandomList[k].GetUUID() : 0);
				}
			}
		}
		void EnsureFetchMonsters()
		{
			if (!hasFetchedMonsters)
			{
				combatData.monsters.GetRandomMonsterList(10, monsterRandomList, mainMonsterUUID);
				hasFetchedMonsters = true;
			}
		}
	}

	protected virtual void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		if (!canOutWindow && !zbUtil.IsPointInBox(skillTF.position, new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY, 0f), sceneBoxWidth, sceneBoxHeight))
		{
			ReturnToPoolImmediate();
			return;
		}
		m_returnPoolTimeAccumTime += Time.deltaTime;
		if (m_returnPoolTimeAccumTime >= m_returnPoolTime)
		{
			ReturnToPoolImmediate();
			return;
		}
		if (m_gpuConvertPending)
		{
			m_gpuConvertPending = false;
			ConvertGpuBullet();
		}
		if (m_spineGpuTakenOver && gpuPlayer != null && (Object)(object)m_skeletonAnimation != (Object)null)
		{
			if ((int)m_skeletonAnimation.UpdateTiming == 0)
			{
				if (Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
				{
					gpuPlayer.Advance(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
				}
			}
			else
			{
				gpuPlayer.Advance(Time.deltaTime);
			}
		}
		bool hasFetchedMonsters;
		if (isMainSkill)
		{
			hasFetchedMonsters = false;
			for (int i = 0; i < hero.team.modIndex1067.Count; i++)
			{
				stModIndex1067 stModIndex2146 = hero.team.modIndex1067[i];
				if (stModIndex2146.srcSkillId != hero.skillRecord.id)
				{
					continue;
				}
				stModIndex2146.intervalKeep += Time.deltaTime;
				if (stModIndex2146.intervalKeep >= stModIndex2146.interval)
				{
					stModIndex2146.intervalKeep = 0f;
					EnsureFetchMonsters();
					for (int j = 0; j < stModIndex2146.subCount; j++)
					{
						zbSkillSystem.Instance.CreateSubSkill(stModIndex2146.subSkillId, hero, this, skillAttrList, skillTF.position, Vector2.up, mainMonsterUUID, (monsterRandomList.Count > i) ? monsterRandomList[i].GetUUID() : 0);
					}
				}
			}
			if (_tempIntervalList.Count > 0)
			{
				mainIntervalToSubSkillTimePre += Time.deltaTime;
				if (mainIntervalToSubSkillTimePre >= mainIntervalToSubSkillTime)
				{
					mainIntervalToSubSkillTimePre = 0f;
					EnsureFetchMonsters();
					foreach (stMainIntervalToSubSkill tempInterval in _tempIntervalList)
					{
						for (int k = 0; k < tempInterval.subCount; k++)
						{
							zbSkillSystem.Instance.CreateSubSkill(tempInterval.subSkillId, hero, this, skillAttrList, skillTF.position, Vector2.up, mainMonsterUUID, (monsterRandomList.Count > k) ? monsterRandomList[k].GetUUID() : 0);
						}
					}
				}
			}
		}
		if (speedChange == 1)
		{
			skillTF.Translate(Vector3.down * zbCombatSystem.Instance.scrollSpeed * Time.deltaTime, (Space)0);
		}
		void EnsureFetchMonsters()
		{
			if (!hasFetchedMonsters)
			{
				combatData.monsters.GetRandomMonsterList(10, monsterRandomList, mainMonsterUUID);
				hasFetchedMonsters = true;
			}
		}
	}

	private void ConvertGpuBullet()
	{
		m_spineGpuTakenOver = false;
		if ((Object)(object)m_skeletonAnimation == (Object)null || string.IsNullOrEmpty(m_gpuSpinePath) || combatData == null)
		{
			return;
		}
		zbSpineGpuAnimRuntime.RestoreDormantEffect(m_skeletonAnimation);
		if (IsGpuBulletExcluded)
		{
			return;
		}
		if (gpuPlayer == null)
		{
			gpuPlayer = new zbSpineGpuAnimPlayer();
		}
		m_spineGpuTakenOver = zbSpineGpuAnimRuntime.TryConvertEffect(gpuPlayer, m_skeletonAnimation, m_gpuSpinePath, combatData);
		if (!m_spineGpuTakenOver)
		{
			return;
		}
		TrackEntry val = ((m_skeletonAnimation.AnimationState != null) ? m_skeletonAnimation.AnimationState.GetCurrent(0) : null);
		if (val != null && val.Animation != null && val.Animation.Name != "<empty>" && gpuPlayer.PlayClip(val.Animation.Name, val.Loop))
		{
			TrackEntry next = val.Next;
			while (next != null && next.Animation != null)
			{
				if (next.Animation.Name != "<empty>")
				{
					gpuPlayer.QueueClip(next.Animation.Name, next.Loop);
				}
				next = next.Next;
			}
			gpuPlayer.Advance(0f);
		}
		else
		{
			m_spineGpuTakenOver = false;
			gpuPlayer.Unbind();
			zbSpineGpuAnimRuntime.RestoreDormantEffect(m_skeletonAnimation);
		}
	}

	public int GetUUID()
	{
		return m_UUID;
	}

	public virtual void Init(string _prefabPath, zbAttackerBase _owner, zbSkillBase skillBase, Dictionary<EAttrSkillType, float> _skillAttrList, Vector3 _curPosition, Vector2 dir, int _mainMonsterUUID = 0, int _subBulletId = 0, int _targetMonsterUUID = 0, int mechaSkillId = 0)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		m_UUID = ++sAutoSkillUUID;
		parentSkillBase = skillBase;
		combatData = _owner.combatData;
		m_prefabPath = _prefabPath;
		hero = _owner;
		mainMonsterUUID = _mainMonsterUUID;
		targetMonsterUUID = _targetMonsterUUID;
		((Component)this).transform.SetParent(combatData.nodeRootHeroSkill);
		((Component)this).transform.position = _curPosition;
		SetDirection(dir);
		if (_subBulletId != 0)
		{
			skillParentType = zbSkillParentType.HeroSub;
			skillSubRecord = zbDataConfig.Instance.skillSubCsvTable.GetRecordByKey(_subBulletId);
			skillAttrList[EAttrSkillType.Duration] = skillSubRecord.duration;
			float[][] baseVal = skillSubRecord.baseVal;
			foreach (float[] array in baseVal)
			{
				skillAttrList[(EAttrSkillType)array[0]] = array[1];
			}
			int[] fatherVal = skillSubRecord.fatherVal;
			int num = fatherVal.Length;
			for (int j = 0; j < num; j++)
			{
				EAttrSkillType key = (EAttrSkillType)fatherVal[j];
				if (_skillAttrList != null && _skillAttrList.TryGetValue(key, out var value))
				{
					skillAttrList[key] = value;
				}
				else if (!skillAttrList.ContainsKey(key))
				{
					skillAttrList[key] = 0f;
				}
			}
			if (skillSubRecord.atkValRate > 0f)
			{
				skillAttrList[EAttrSkillType.Damage] = (float)(int)hero.team.teamAttack * skillSubRecord.atkValRate;
				if (IsSkillRecordTagsContainsTag(skillSubRecord.tag, 4))
				{
					skillAttrList[EAttrSkillType.ExplosionDamage] = skillAttrList[EAttrSkillType.Damage];
				}
			}
		}
		else if (mechaSkillId != 0)
		{
			skillParentType = zbSkillParentType.MechaSkill;
			skillAttrList.Clear();
			if (_skillAttrList != null)
			{
				foreach (KeyValuePair<EAttrSkillType, float> _skillAttr in _skillAttrList)
				{
					skillAttrList[_skillAttr.Key] = _skillAttr.Value;
				}
			}
		}
		else
		{
			skillAttrList.Clear();
			if (_skillAttrList != null)
			{
				foreach (KeyValuePair<EAttrSkillType, float> _skillAttr2 in _skillAttrList)
				{
					skillAttrList[_skillAttr2.Key] = _skillAttr2.Value;
				}
			}
			skillRecord = hero.skillRecord;
		}
		OnInited();
		if (isSubSkill && skillSubRecord != null)
		{
			combatData.skillPoolMgr.RegisterActiveSubSkill(skillSubRecord.id);
		}
	}

	public float GetAttrValueByType(EAttrSkillType attrSkillType)
	{
		if (!skillAttrList.TryGetValue(attrSkillType, out var value))
		{
			_ = skillSubRecord;
			return 0f;
		}
		float num = zbSkillSystem.Instance.SafeCheckAttrNeedDiv100(attrSkillType, value);
		if (skillSubRecord != null)
		{
			if (attrSkillType == EAttrSkillType.Duration)
			{
				num += hero.team.subSkillAddDuration[skillSubRecord.id];
			}
			if (attrSkillType == EAttrSkillType.Penetration)
			{
				num += (float)hero.team.subSkillAddPenetration[skillSubRecord.id];
			}
			if (attrSkillType == EAttrSkillType.DamageRange)
			{
				num *= 1f + hero.team.subSkillAddDamageRange[skillSubRecord.id];
			}
			if (IsDamageType(attrSkillType))
			{
				num *= skillSubRecord.fatherValRate;
			}
		}
		else if (hero.GetSkillType() == 8 && attrSkillType == EAttrSkillType.Damage)
		{
			num *= 1f + hero.team.ExtraProperties[ETeamExtra.WurenjiMod2081] * (float)ricochetCount;
		}
		return num;
	}

	public virtual float GetExtraAddRatio(zbMonster monster)
	{
		return 1f;
	}

	public void SetDirection(Vector2 direction)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		m_direction = Vector2.op_Implicit(((Vector2)(ref direction)).normalized);
		((Component)this).transform.up = m_direction;
	}

	public Vector3 getDirection()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return m_direction;
	}

	public int getAttackType()
	{
		if (isMainSkill)
		{
			return skillRecord.attackType;
		}
		if (isSubSkill)
		{
			return skillSubRecord.attackType;
		}
		return 0;
	}

	public void SetOwner(zbAttackerBase owner)
	{
		hero = owner;
	}

	public bool IsDamageType(EAttrSkillType _attrSkillType)
	{
		switch (_attrSkillType)
		{
		case EAttrSkillType.Damage:
		case EAttrSkillType.PeripheralDamage:
		case EAttrSkillType.CentralDamage:
		case EAttrSkillType.ExplosionDamage:
		case EAttrSkillType.SplashDamage:
		case EAttrSkillType.DianqiuDamage:
		case EAttrSkillType.BurnDamage:
		case EAttrSkillType.IceDamage:
			return true;
		default:
			return false;
		}
	}

	public bool IsSkillTag(int tag)
	{
		int[] tags = ((!isSubSkill) ? skillRecord?.tag : skillSubRecord?.tag);
		return IsSkillRecordTagsContainsTag(tags, tag);
	}

	public bool IsSkillRecordTagsContainsTag(int[] tags, int targetTag)
	{
		if (tags == null)
		{
			return false;
		}
		int num = tags.Length;
		for (int i = 0; i < num; i++)
		{
			if (tags[i] == targetTag)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void SetFirst(bool _isFirst)
	{
	}

	public void SpriteReplaceMode()
	{
		if (!((Object)(object)m_spriteMonster == (Object)null) && !((Object)(object)m_skeletonAnimation == (Object)null))
		{
			zbCombatFPS combatFPS = zbCombatSystem.Instance.combatFPS;
			if (combatFPS.IsSpriteReplaceMode())
			{
				m_spriteMonster.gameObject.SetActive(true);
				((Component)m_skeletonAnimation).gameObject.SetActive(false);
			}
			else if (combatFPS.IsLowPerformanceMode())
			{
				m_spriteMonster.gameObject.SetActive(false);
				((Component)m_skeletonAnimation).gameObject.SetActive(true);
			}
		}
	}

	public virtual stDamageResult CalcMonsterDamage(EAttrSkillType _attrSkillType, zbMonster monster, float damage = 0f, bool param = false)
	{
		EAttrSkillType attrSkillType = _attrSkillType;
		if (isSubSkill && isSubSkillTagExplode)
		{
			attrSkillType = EAttrSkillType.ExplosionDamage;
		}
		return zbCombatSystem.Instance.combatFormula.CalcMonsterDamage(this, attrSkillType, monster, damage, 0, isAgain: false, param);
	}

	public virtual void UpdateMonsterCollision(zbBoundsCircle2D boundsCircle2D, HashSet<int> ignoreList, EAttrSkillType _attrSkillType, bool stopAtFirst = false)
	{
		if (m_combatMonsterGrid != null)
		{
			if (stopAtFirst)
			{
				m_combatMonsterGrid.QueryFirst(in boundsCircle2D, ignoreList, overlapResultList);
			}
			else
			{
				m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreList, overlapResultList);
			}
			HandleCollisionResult(_attrSkillType, stopAtFirst);
		}
	}

	public virtual void UpdateMonsterCollision(zbBounds2D boundsCircle2D, HashSet<int> ignoreList, EAttrSkillType _attrSkillType, bool stopAtFirst = false)
	{
		if (m_combatMonsterGrid != null)
		{
			if (stopAtFirst)
			{
				m_combatMonsterGrid.QueryFirst(in boundsCircle2D, ignoreList, overlapResultList);
			}
			else
			{
				m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreList, overlapResultList);
			}
			HandleCollisionResult(_attrSkillType, stopAtFirst);
		}
	}

	public virtual void UpdateMonsterCollision(zbOBB2D boundsCircle2D, HashSet<int> ignoreList, EAttrSkillType _attrSkillType, bool stopAtFirst = false)
	{
		if (m_combatMonsterGrid != null)
		{
			if (stopAtFirst)
			{
				m_combatMonsterGrid.QueryFirst(in boundsCircle2D, ignoreList, overlapResultList);
			}
			else
			{
				m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreList, overlapResultList);
			}
			HandleCollisionResult(_attrSkillType, stopAtFirst);
		}
	}

	private void HandleCollisionResult(EAttrSkillType _attrSkillType, bool stopAtFirst)
	{
		int count = overlapResultList.Count;
		if (count <= 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			zbMonster monster = overlapResultList[i];
			CalcMonsterDamage(_attrSkillType, monster);
			if (stopAtFirst)
			{
				ReturnToPool();
				break;
			}
		}
	}

	public zbSkillBase()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		m_UUID = -1;
		m_direction = Vector3.up;
		m_sortType = ESortLayerType.Projectile;
		lifetime = 5f;
		triggerEndMod = true;
		_tempIntervalList = new List<stMainIntervalToSubSkill>();
		monsterRandomList = new List<zbMonster>();
		overlapResultList = new List<zbMonster>();
		ignoreMonsters = new HashSet<int>();
		skillParentType = zbSkillParentType.HeroMain;
		((MonoBehaviour)this)._002Ector();
	}
}
