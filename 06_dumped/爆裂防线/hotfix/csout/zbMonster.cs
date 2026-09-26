using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonster : MonoBehaviour
{
	public enum ESpineUpdateTier
	{
		Full,
		Stepped,
		Culled
	}

	private struct AttrCache
	{
		public int frame;

		public float value;
	}

	[NonSerialized]
	public int attackCount;

	[NonSerialized]
	private Vector3 startPointBuff17;

	private Vector3 endPointBuff17;

	private float durationBuff17;

	private float elapsedBuff17;

	private bool movingBuff17;

	public bool moveDownBuff17;

	[NonSerialized]
	public stSub20005 sub20005;

	[NonSerialized]
	public stMonsterSkillIndex10000 monsterIndex10000;

	[NonSerialized]
	public stMonsterSkillIndex10001 monsterIndex10001;

	public List<stMonsterSkillIndex10001_2> monsterIndex10001_2;

	[NonSerialized]
	public List<stMonsterSkillIndex10021> monsterIndex10021;

	[NonSerialized]
	public bool monsterIndex10051;

	[NonSerialized]
	public float monsterIndex10051Time;

	[NonSerialized]
	public int monsterIndex10051Flag;

	[NonSerialized]
	public float monsterIndex10051Read;

	[NonSerialized]
	public List<stMonsterSkillIndex10081> monsterIndex10081;

	[NonSerialized]
	public bool monsterIndex10101;

	[NonSerialized]
	public List<stMonsterSkillIndex10111> monsterIndex10111;

	[NonSerialized]
	public bool monsterIndex10112;

	[NonSerialized]
	public bool monsterIndex10112Action;

	[NonSerialized]
	public zbMonsterSkillBase monsterIndex10112SkillBase;

	[NonSerialized]
	public List<stMonsterSkillIndex10121> monsterIndex10121;

	[NonSerialized]
	public stMonsterSkillIndex10122 monsterIndex10122;

	[NonSerialized]
	public bool monsterSkillIndex10122Hiding;

	public List<stMonsterSkillIndex10131> monsterIndex10131;

	[NonSerialized]
	public stMonsterSkillIndex10161 monsterIndex10161;

	[NonSerialized]
	public bool monsterIndex10171;

	[NonSerialized]
	public bool monsterIndex10172;

	[NonSerialized]
	public List<stMonsterSkillIndex10201> monsterIndex10201;

	[NonSerialized]
	public stMonsterSkillIndex10211 monsterIndex10211;

	[NonSerialized]
	public stMonsterSkillIndex10222 monsterIndex10222;

	[NonSerialized]
	public stMonsterSkillIndex10223 monsterIndex10223;

	[NonSerialized]
	public List<stMonsterSkillIndex10261> monsterIndex10261;

	[NonSerialized]
	public List<stMonsterSkillIndex10262> monsterIndex10262;

	[NonSerialized]
	public List<stMonsterSkillIndex10281> monsterIndex10281;

	[NonSerialized]
	public int monsterIndex10302;

	[NonSerialized]
	public List<stMonsterSkillIndex10311Extra> monsterIndex10311;

	[NonSerialized]
	public List<stMonsterSkillIndex10321> monsterIndex10321;

	[NonSerialized]
	public stMonsterSkillIndex10322 monsterIndex10322;

	[NonSerialized]
	public List<stMonsterSkillIndex10332> monsterIndex10332;

	[NonSerialized]
	public stMonsterSkillIndex10352 monsterIndex10352;

	[NonSerialized]
	public stMonsterSkillIndex10353 monsterIndex10353;

	[NonSerialized]
	public bool monsterIndex10353Stop;

	public stMonsterSkillIndex10362 monsterIndex10362;

	public stMonsterSkillIndex10373 monsterIndex10373;

	public stMonsterSkillIndex10382 monsterIndex10382;

	[NonSerialized]
	public List<int> immBuffList;

	[NonSerialized]
	public stMonsterSkillIndex10441 monsterIndex10441;

	[NonSerialized]
	public stMonsterSkillIndex10462 monsterIndex10462;

	[NonSerialized]
	public bool monsterIndex20011;

	[NonSerialized]
	public int immAllBuff;

	[NonSerialized]
	public stMonsterSkillIndex20091 monsterIndex20091;

	[NonSerialized]
	public stMonsterSkillIndex20102 monsterIndex20102;

	[NonSerialized]
	public stMonsterSkillIndex20132 monsterIndex20132;

	[NonSerialized]
	public stMonsterSkillIndex20135 monsterIndex20135;

	[NonSerialized]
	public stMonsterSkillIndex20161 monsterIndex20161;

	[NonSerialized]
	public List<stMonsterSkillIndex20162> monsterIndex20162;

	[NonSerialized]
	public stMonsterSkillIndex20163 monsterIndex20163;

	[NonSerialized]
	public stMonsterSkillIndex20192 monsterIndex20192;

	[NonSerialized]
	public stMonsterSkillIndex30012 monsterIndex30012;

	[NonSerialized]
	public bool monsterIndex30012Hide;

	[NonSerialized]
	public stMonsterSkillIndex30021 monsterIndex30021;

	[NonSerialized]
	public stMonsterSkillIndex30042 monsterIndex30042;

	[NonSerialized]
	public stMonsterSkillIndex30062 monsterIndex30062;

	[NonSerialized]
	public float monsterIndex30072DamageSta;

	[NonSerialized]
	public int monsterSkillIndex3011;

	[NonSerialized]
	public bool monsterSkillIndex3011Immune;

	[NonSerialized]
	public bool monsterSkillIndex3011Penetration;

	[NonSerialized]
	public float monsterSkillIndex3011LastHp;

	[NonSerialized]
	public stMonsterSkillIndex30111 monsterSkillIndex30111;

	[NonSerialized]
	public stMonsterSkillIndex30112 monsterSkillIndex30112;

	public zbMonsterSkillIndex30112 monster30112Class;

	[NonSerialized]
	public stMonsterSkillIndex30113 monsterSkillIndex30113;

	public zbMonsterSkillIndex30113 monster30113Class;

	public float monsterSkillIndex30113BreakTime;

	[NonSerialized]
	public int monsterSkillIndex3012;

	public zbMonsterSkillIndex30121 monster30121Class;

	[NonSerialized]
	public stMonsterSkillIndex30121 monsterSkillIndex30121;

	public zbMonsterSkillIndex30122 monster30122Class;

	[NonSerialized]
	public stMonsterSkillIndex30122 monsterSkillIndex30122;

	[NonSerialized]
	public stMonsterSkillIndex30123 monsterSkillIndex30123;

	public zbMonsterSkillIndex30123 monster30123Class;

	[NonSerialized]
	public bool monsterIndex30123Stop;

	[NonSerialized]
	public int monsterSkillIndex3013;

	public zbMonsterSkillIndex30131 monster30131Class;

	[NonSerialized]
	public stMonsterSkillIndex30131 monsterSkillIndex30131;

	public zbMonsterSkillIndex30132 monster30132Class;

	[NonSerialized]
	public stMonsterSkillIndex30132 monsterSkillIndex30132;

	public zbMonsterSkillIndex30133 monster30133Class;

	[NonSerialized]
	public stMonsterSkillIndex30133 monsterSkillIndex30133;

	[NonSerialized]
	public int monsterSkillIndex3014;

	public zbMonsterSkillIndex30141 monster30141Class;

	[NonSerialized]
	public stMonsterSkillIndex30141 monsterSkillIndex30141;

	public List<stMonsterAddSpeed> monsterAddSpeedList;

	public zbMonsterSkillIndex30142 monster30142Class;

	[NonSerialized]
	public stMonsterSkillIndex30142 monsterSkillIndex30142;

	public zbMonsterSkillIndex30143 monster30143Class;

	[NonSerialized]
	public stMonsterSkillIndex30143 monsterSkillIndex30143;

	[NonSerialized]
	public int monsterSkillIndex3015;

	[NonSerialized]
	public int monsterSkillIndex3015HitCount;

	[NonSerialized]
	public stMonsterSkillIndex30151 monsterSkillIndex30151;

	[NonSerialized]
	public stMonsterSkillIndex30152 monsterSkillIndex30152;

	public zbMonsterSkillIndex30152 monster30152Class;

	[NonSerialized]
	public stMonsterSkillIndex30153 monsterSkillIndex30153;

	public zbMonsterSkillIndex30153 monster30153Class;

	public float monsterSkillIndex30153BreakTime;

	[NonSerialized]
	public int monsterSkillIndex3016;

	[NonSerialized]
	public int monsterSkillIndex3016HitCount;

	[NonSerialized]
	public stMonsterSkillIndex30161 monsterSkillIndex30161;

	public int monsterBlockCount;

	[NonSerialized]
	public stMonsterSkillIndex30162 monsterSkillIndex30162;

	public zbMonsterSkillIndex30162 monster30162Class;

	[NonSerialized]
	public stMonsterSkillIndex30163 monsterSkillIndex30163;

	public zbMonsterSkillIndex30163 monster30163Class;

	public float monsterSkillIndex30163BreakTime;

	public stMonsterSkillIndex40111 monsterIndex40111;

	[NonSerialized]
	public stMonsterSkillIndex40151 monsterIndex40151;

	[NonSerialized]
	public stMonsterSkillIndex40402 monsterIndex40402;

	[NonSerialized]
	public stMonsterSkillIndex40421 monsterIndex40421;

	[NonSerialized]
	public stMonsterSkillIndex40462 monsterIndex40462;

	public zbCombatData combatData;

	public zbMonsterHudProgress hud;

	public zbMonsterAnimation anim;

	public zbMonsterStateMachine fsm;

	[NonSerialized]
	public SkeletonRenderer m_skeletonRenderer;

	public zbMonsterSprite monsterSprite;

	[NonSerialized]
	public zbMonsterCsvRecord monsterRecord;

	public bool isGoDestroyed;

	public bool isDead;

	[NonSerialized]
	public zbMonsterAvatarCsvRecord monsterAvatarRecord;

	[NonSerialized]
	public bool isUsingSprite;

	private readonly int m_animUpdateStagger;

	private static readonly bool kSpineTierEnabled = true;

	private ESpineUpdateTier m_spineTier;

	private int m_spineStepInterval;

	public bool m_spineBatchTakenOver;

	public bool m_spineGpuTakenOver;

	private static readonly string[] kSpineTierSwitchKeys = new string[3] { "spine切至全速次数", "spine切至隔帧次数", "spine切至停推次数" };

	public zbSafeLong curHp;

	public zbSafeLong maxHp;

	public zbSafeLong shieldHp;

	[NonSerialized]
	public long maxShieldHp;

	private AttrCache[] m_attrCache;

	[NonSerialized]
	public int minCx;

	[NonSerialized]
	public int maxCx;

	[NonSerialized]
	public int minCy;

	[NonSerialized]
	public int maxCy;

	[NonSerialized]
	public int activeIndex;

	private int m_frameCounter;

	[NonSerialized]
	private zbSafeInt damageAttack;

	[NonSerialized]
	private zbSafeFloat attackSpeed;

	[NonSerialized]
	private zbSafeFloat attackRange;

	[NonSerialized]
	private zbSafeFloat moveSpeed;

	[NonSerialized]
	private float bodyScaleBase;

	[NonSerialized]
	private zbSafeInt mass;

	[NonSerialized]
	private zbSafeFloat damageTaken;

	private ESortLayerType m_sortType;

	[NonSerialized]
	private float slowDownPercent;

	[NonSerialized]
	private float localScalePercent;

	[NonSerialized]
	private float massPercent;

	[NonSerialized]
	private float hitRatePercent;

	[NonSerialized]
	public zbSafeFloat beDamageUpPercent;

	[NonSerialized]
	public List<zbBuffBase> buffDieTodo;

	[NonSerialized]
	public List<zbExtraDirection> extraDirectionList;

	[NonSerialized]
	public List<zbExtraProperty> extraPropertiesList;

	public Dictionary<EAttrMonsterType, float> MonsterExtraProperties;

	[NonSerialized]
	public int curExp;

	private bool giveExp;

	[NonSerialized]
	public bool canMoveOrAttack;

	[NonSerialized]
	public bool hidingNum;

	[NonSerialized]
	public int turntable;

	public static readonly int sInvalidId = -1;

	private int m_UUID;

	public zbMonsterSkillMgr skillMgr;

	private GameObject subObject;

	private SpriteRenderer yingziBg;

	private GameObject yingziBgGO;

	private GameObject shuiwenGO;

	private BoxCollider2D boxCollider;

	[NonSerialized]
	public bool boxColliderEnable;

	[NonSerialized]
	public Bounds monsterBoxBounds;

	[NonSerialized]
	public bool unTriggerDieSkill;

	[NonSerialized]
	public bool isRestoreData;

	[NonSerialized]
	public float initPositionX;

	[NonSerialized]
	public float initPositionY;

	[NonSerialized]
	private float m_returnPoolTime;

	[NonSerialized]
	private float m_returnPoolTimeAccumTime;

	[NonSerialized]
	public float m_lastTimeScale;

	[NonSerialized]
	private GameObject cureAnimationObject;

	private float cureAnimationFlag;

	private int lastUpdateFrameActive;

	[NonSerialized]
	public float m_KnockbackTowUpPercent;

	[NonSerialized]
	public float addPercentMass;

	[NonSerialized]
	public Transform monsterTF;

	private float getSceneAlertRange_GetAlertPositionY;

	[NonSerialized]
	public Vector3 m_framePos;

	private bool m_buffStopCache;

	private int m_buffStopCacheFrame;

	private uint m_buffStopCacheVersion;

	[NonSerialized]
	public float cachPositionX;

	[NonSerialized]
	public float cachPositionY;

	private float constBoxHeight;

	[NonSerialized]
	public float bornTIme;

	private float m_buffSkillInterval;

	private float m_buffSkillIntervalPre;

	private Vector3 bubblePosition;

	public bool needRestoreData;

	public float waterHeight;

	public float fadeRange;

	public bool isEliteBoss;

	public bool bMonsterNeedWaterEffect;

	[NonSerialized]
	public bool monsterSkillIndexBianShen;

	public float monsterSkillIndex3011BreakHitDam { get; set; }

	public bool monsterSkillIndex3011BreakFlag { get; set; }

	public zbMonsterSkillBase monsterSkillIndexClass { get; set; }

	public zbMonsterSkillIndex30111 monster30111Class { get; set; }

	public float monster3014DazeKeep { get; set; }

	public float monster3014DazeDam { get; set; }

	public float monsterSkillIndex3015BreakHitDam { get; set; }

	public bool monsterSkillIndex3015BreakFlag { get; set; }

	public zbMonsterSkillIndex30151 monster30151Class { get; set; }

	public float monsterSkillIndex3016BreakHitDam { get; set; }

	public bool monsterSkillIndex3016BreakFlag { get; set; }

	public zbMonsterSkillIndex30161 monster30161Class { get; set; }

	public float bodyScale { get; private set; }

	public float lastBodyScale { get; private set; }

	private static int sAutoId { get; set; } = 0;

	public long beHitTotalCount { get; set; }

	public bool beBreakBoss { get; set; }

	public int breakCount { get; set; }

	public int breakTotal { get; set; }

	public ESpineUpdateTier GetCurrentSpineTier()
	{
		return m_spineTier;
	}

	public void InvalidateSpineTier()
	{
		m_spineTier = (ESpineUpdateTier)(-1);
		m_spineStepInterval = 1;
	}

	private void UpdateSpineTier(float worldY)
	{
		if (!kSpineTierEnabled)
		{
			ApplySpineTier(ESpineUpdateTier.Full, 1);
		}
		else if (!isUsingSprite && anim != null && anim.spineAnimation != null && !((Object)(object)anim.spineAnimation.skeletonAnimation == (Object)null))
		{
			zbCombatData zbCombatData2 = combatData;
			int stepInterval = 1;
			ESpineUpdateTier newTier;
			if (worldY < zbCombatData2.spineCullMinY || worldY > zbCombatData2.spineCullMaxY)
			{
				newTier = ESpineUpdateTier.Culled;
			}
			else if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
			{
				newTier = ESpineUpdateTier.Stepped;
				stepInterval = zbCombatFPS.MANUAL_UPDATE_FRAME;
			}
			else if (worldY < zbCombatData2.spineFarBandY && !isDead && !isEliteBoss)
			{
				newTier = ESpineUpdateTier.Stepped;
				stepInterval = 2;
			}
			else
			{
				newTier = ESpineUpdateTier.Full;
			}
			ApplySpineTier(newTier, stepInterval);
		}
	}

	private void ApplySpineTier(ESpineUpdateTier newTier, int stepInterval)
	{
		if (m_spineGpuTakenOver)
		{
			m_spineTier = newTier;
			m_spineStepInterval = stepInterval;
		}
		else
		{
			if (!m_spineBatchTakenOver && newTier == m_spineTier && (newTier != ESpineUpdateTier.Stepped || stepInterval == m_spineStepInterval))
			{
				return;
			}
			bool flag = newTier != m_spineTier || (newTier == ESpineUpdateTier.Stepped && stepInterval != m_spineStepInterval);
			m_spineTier = newTier;
			m_spineStepInterval = stepInterval;
			SkeletonAnimation skeletonAnimation = anim.spineAnimation.skeletonAnimation;
			if (!((Object)(object)skeletonAnimation == (Object)null))
			{
				switch (newTier)
				{
				case ESpineUpdateTier.Culled:
					((SkeletonRenderer)skeletonAnimation).UpdateMode = (UpdateMode)0;
					break;
				case ESpineUpdateTier.Stepped:
					((SkeletonRenderer)skeletonAnimation).UpdateMode = (UpdateMode)3;
					skeletonAnimation.UpdateTiming = (UpdateTiming)0;
					break;
				default:
					((SkeletonRenderer)skeletonAnimation).UpdateMode = (UpdateMode)3;
					skeletonAnimation.UpdateTiming = (UpdateTiming)1;
					break;
				}
				if (flag)
				{
					zbPerfLog.Inc(kSpineTierSwitchKeys[(int)newTier], 1L);
				}
			}
		}
	}

	public int GetUUID()
	{
		return m_UUID;
	}

	public int GetTid()
	{
		return monsterRecord.id;
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
		combatData.monsters.RemoveMonster(this);
		combatData.monsterPoolMgr.ReturnMonster(this);
	}

	private IEnumerator ReturnCoroutine(float delayTime)
	{
		if (delayTime > 0f)
		{
			yield return (object)new WaitForSeconds(delayTime);
		}
		combatData.monsters.RemoveMonster(this);
		combatData.monsterPoolMgr.ReturnMonster(this);
	}

	private void Awake()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		monsterTF = ((Component)this).transform;
		subObject = ((Component)((Component)this).transform.Find("subObject")).gameObject;
		yingziBg = ((Component)((Component)this).transform.Find("subObject").Find("yingzi")).GetComponent<SpriteRenderer>();
		yingziBgGO = ((Component)yingziBg).gameObject;
		shuiwenGO = ((Component)((Component)this).transform.Find("subObject").Find("shuiwen")).gameObject;
		shuiwenGO.SetActive(false);
		boxCollider = ((Component)((Component)this).transform).GetComponent<BoxCollider2D>();
		monsterBoxBounds = ((Collider2D)boxCollider).bounds;
		cureAnimationObject = ((Component)((Component)this).transform.Find("subObject").Find("cureSpine")).gameObject;
	}

	private void Reset()
	{
		m_UUID = ++sAutoId;
		if (skillMgr != null)
		{
			skillMgr.Free();
		}
		combatData = null;
		hud = null;
		needRestoreData = false;
		isDead = false;
		breakCount = 0;
		breakTotal = 1;
		beBreakBoss = false;
		monsterIndex10353Stop = false;
		monsterIndex30123Stop = false;
		bornTIme = Time.time;
		cureAnimationFlag = 0f;
		if ((Object)(object)cureAnimationObject != (Object)null)
		{
			cureAnimationObject.SetActive(false);
		}
		isRestoreData = false;
		unTriggerDieSkill = false;
		sub20005.sub20005 = false;
		monsterSkillIndexBianShen = false;
		if (anim != null)
		{
			anim.spineAnimation.ResetAnimations();
			SetBoxColliderEnabel(bEnabale: true);
		}
		MonsterExtraProperties = Enum.GetValues(typeof(EAttrMonsterType)).Cast<EAttrMonsterType>().ToDictionary((EAttrMonsterType e) => e, (EAttrMonsterType e) => 0f);
		m_buffStopCacheFrame = -1;
		m_buffStopCache = false;
		extraDirectionList.Clear();
		extraPropertiesList.Clear();
		skillMgr = null;
		fsm = null;
		monsterRecord = null;
		monsterAvatarRecord = null;
		m_KnockbackTowUpPercent = 0f;
		SetMaxHp(10L);
		SetCurHp(10L);
		SetShieldHp(0L);
		damageAttack.Value = 0;
		attackSpeed.Value = 0f;
		attackRange.Value = 0f;
		addPercentMass = 1f;
		m_lastTimeScale = 1f;
		moveSpeed.Value = 0f;
		bodyScale = 1f;
		bodyScaleBase = 1f;
		lastBodyScale = 0f;
		mass.Value = 0;
		damageTaken.Value = 1f;
		slowDownPercent = 1f;
		beDamageUpPercent.Value = 0f;
		beHitTotalCount = 0L;
		hitRatePercent = 1f;
		massPercent = 1f;
		localScalePercent = 1f;
		buffDieTodo.Clear();
		canMoveOrAttack = true;
		hidingNum = false;
		constBoxHeight = 0f;
		m_returnPoolTime = float.MaxValue;
		m_returnPoolTimeAccumTime = 0f;
		UseSpriteReplaceSpine(bUseSprite: false);
		InitAttrCache();
	}

	private void InitAttrCache()
	{
		for (int i = 0; i < m_attrCache.Length; i++)
		{
			m_attrCache[i].frame = -1;
			m_attrCache[i].value = 0f;
		}
	}

	public void Init(zbCombatData _combatData, int monsterTid, int _turntable, bool _giveExp, bool _isRestoreData)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		giveExp = _giveExp;
		combatData = _combatData;
		combatData.monsters.AddMonster(this);
		isRestoreData = _isRestoreData;
		turntable = _turntable;
		monsterRecord = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		monsterAvatarRecord = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(monsterRecord.avatarId);
		bMonsterNeedWaterEffect = zbCombatSystem.Instance.bMonsterNeedWaterEffect && monsterRecord.moveType == 1;
		isEliteBoss = IsEliteBoss();
		curExp = ((giveExp && !isEliteBoss) ? 1 : 0);
		lastUpdateFrameActive = 3;
		((Component)this).transform.localScale = Vector3.zero;
		if ((Object)(object)yingziBg != (Object)null)
		{
			Sprite val = zbAssetLoader.Instance.LoadCombatSpriteAsset(monsterAvatarRecord.shadow);
			if ((Object)(object)val != (Object)null)
			{
				yingziBg.sprite = val;
			}
		}
		yingziBgGO.transform.localScale = Vector3.one * monsterAvatarRecord.shadowScale;
		shuiwenGO.transform.localScale = Vector3.one * monsterAvatarRecord.shadowScale;
		zbSortMgr.Instance.ApplyShadowReband(yingziBgGO.transform);
		yingziBgGO.SetActive(!bMonsterNeedWaterEffect);
		shuiwenGO.SetActive(bMonsterNeedWaterEffect);
		if (isEliteBoss)
		{
			hud = combatData.combatHud.ShowHud(this);
		}
		CaclProperty();
		constBoxHeight = (float)monsterAvatarRecord.boxHeight * bodyScale / 300f + (float)monsterAvatarRecord.boxOffset * bodyScale / 100f;
		if (combatData.dataIndex == 2 && monsterAvatarRecord.pvpTurnOver != 1)
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			subObject.transform.localPosition = new Vector3(subObject.transform.localPosition.x, (0f - constBoxHeight) * 2f, subObject.transform.localPosition.z);
		}
		if (anim == null)
		{
			anim = new zbMonsterAnimation();
			anim.Init(this);
			m_skeletonRenderer = ((Component)this).GetComponentInChildren<SkeletonRenderer>();
		}
		else
		{
			anim.spineColorChange.Reset();
		}
		m_spineTier = (ESpineUpdateTier)(-1);
		m_spineStepInterval = 1;
		if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
		{
			SetSkeletonManualUpdate(manual: true);
		}
		if (monsterSprite == null)
		{
			monsterSprite = new zbMonsterSprite();
			monsterSprite.Init(this);
		}
		else
		{
			monsterSprite.ResetColor();
		}
		fsm = new zbMonsterStateMachine();
		skillMgr = new zbMonsterSkillMgr();
		fsm.Init(this);
		skillMgr.Init(this, giveExp);
		fsm.SetState(EMonsterState.Show);
		initPositionX = monsterTF.position.x;
		if (combatData.GetSceneAlertRange() < 10f && (!zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode() || Rand.Range(0, 3) == 1))
		{
			combatData.monsterEyeEffectPoolMgr.GetMonsterEyeEffect().InitData(combatData, GetUUID());
		}
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		getSceneAlertRange_GetAlertPositionY = combatData.GetSceneAlertRange() + combatData.defense.GetAlertPositionY();
		monsterBoxBounds = ((Collider2D)boxCollider).bounds;
		combatData.monsters.monsterGrid.UpdateMonster(this);
		cachPositionX = monsterTF.position.x;
		cachPositionY = monsterTF.position.y;
		zbSpineGpuAnimRuntime.RestoreDormantMonster(this);
		if (!zbSpineGpuAnimRuntime.TryConvertMonster(this))
		{
			zbSpineBatchRenderer.TrySubscribeMonster(this);
		}
	}

	private void OnDisable()
	{
		if (m_spineGpuTakenOver)
		{
			m_spineGpuTakenOver = false;
			zbSpineGpuAnimRuntime.ReleaseMonster(this);
		}
		if (m_spineBatchTakenOver)
		{
			m_spineBatchTakenOver = false;
			zbSpineBatchRenderer.UnsubscribeMonster(this);
		}
	}

	public void SetBoxColliderEnabel(bool bEnabale)
	{
		if (Object.op_Implicit((Object)(object)boxCollider))
		{
			((Behaviour)boxCollider).enabled = bEnabale;
			boxColliderEnable = bEnabale;
		}
	}

	public void SetBoxColliderSize(Vector2 size, Vector2 offset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		((Collider2D)boxCollider).offset = offset;
		boxCollider.size = size;
		monsterBoxBounds = ((Collider2D)boxCollider).bounds;
	}

	private bool IsEliteBoss()
	{
		return monsterRecord.type != 1;
	}

	public bool IsBoss()
	{
		if (monsterRecord != null)
		{
			return monsterRecord.type == 3;
		}
		return false;
	}

	public void PvpAddAttr(double prent)
	{
		SetMaxHp((long)((double)(long)maxHp * prent));
		SetCurHp(maxHp);
		damageAttack.Value = (int)((double)(int)damageAttack * prent);
	}

	private void CaclProperty()
	{
		foreach (BattleMonsterDetail monster in zbCombatSystem.Instance.battleInfo.MonsterInfo.MonsterList)
		{
			if (monster.MonsterId != monsterRecord.id)
			{
				continue;
			}
			{
				foreach (BattleMonsterAttrDetail attr in monster.AttrList)
				{
					switch (attr.Type)
					{
					case EAttrMonsterType.Hp:
						SetMaxHp((long)attr.Value);
						SetCurHp(maxHp);
						break;
					case EAttrMonsterType.Damage:
						damageAttack.Value = (int)attr.Value;
						break;
					case EAttrMonsterType.AttackSpeed:
						attackSpeed.Value = attr.Value;
						break;
					case EAttrMonsterType.AttackRange:
						attackRange.Value = attr.Value;
						break;
					case EAttrMonsterType.MoveSpeed:
						moveSpeed.Value = attr.Value;
						break;
					case EAttrMonsterType.BodyScale:
						bodyScaleBase = attr.Value;
						bodyScale = bodyScaleBase;
						break;
					case EAttrMonsterType.Mass:
						mass.Value = (int)attr.Value;
						break;
					case EAttrMonsterType.DamageTaken:
						damageTaken.Value = attr.Value;
						break;
					}
				}
				break;
			}
		}
	}

	public float GetAttrValueByType(EAttrMonsterType attrMonsterType)
	{
		int frameCount = Time.frameCount;
		if (m_attrCache[(int)attrMonsterType].frame == frameCount)
		{
			return m_attrCache[(int)attrMonsterType].value;
		}
		float num = 0f;
		switch (attrMonsterType)
		{
		case EAttrMonsterType.Hp:
			num = (long)curHp;
			break;
		case EAttrMonsterType.Damage:
			num = (int)damageAttack;
			break;
		case EAttrMonsterType.AttackSpeed:
			num = attackSpeed;
			break;
		case EAttrMonsterType.AttackRange:
			num = attackRange;
			break;
		case EAttrMonsterType.MoveSpeed:
			num = moveSpeed;
			break;
		case EAttrMonsterType.BodyScale:
			num = bodyScaleBase * localScalePercent;
			break;
		case EAttrMonsterType.Mass:
			num = (float)(int)mass * massPercent;
			break;
		case EAttrMonsterType.DamageTaken:
			num = damageTaken;
			break;
		}
		if (monsterIndex10021.Count > 0 || monsterAddSpeedList.Count > 0 || monsterIndex10121.Count > 0 || monsterIndex20162.Count > 0 || monsterIndex10111.Count > 0 || monsterIndex10131.Count > 0 || extraPropertiesList.Count > 0 || monsterIndex10353.durationTimeKeep > 0f || monsterIndex10211.monsterIndex10211 || sub20005.sub20005 || monsterIndex20102.skillIndexFlag || monsterSkillIndex3011 == 3 || monsterSkillIndex3013 == 3)
		{
			int count = monsterIndex10021.Count;
			for (int i = 0; i < count; i++)
			{
				stMonsterSkillIndex10021 stMonsterSkillIndex40463 = monsterIndex10021[i];
				if (stMonsterSkillIndex40463.durationTimeKeep > 0f && stMonsterSkillIndex40463.attrMonsterType == (int)attrMonsterType)
				{
					num *= 1f + stMonsterSkillIndex40463.attrPercent;
				}
			}
			for (int num2 = monsterAddSpeedList.Count - 1; num2 >= 0; num2--)
			{
				stMonsterAddSpeed stMonsterAddSpeed2 = monsterAddSpeedList[num2];
				if (stMonsterAddSpeed2.durationTimeKeep >= stMonsterAddSpeed2.durationTime)
				{
					monsterAddSpeedList.RemoveAt(num2);
				}
				else if (stMonsterAddSpeed2.durationTimeKeep > 0f && stMonsterAddSpeed2.attrMonsterType == (int)attrMonsterType)
				{
					num *= 1f + stMonsterAddSpeed2.attrPercent;
				}
			}
			if (monsterIndex10353.durationTimeKeep > 0f && EAttrMonsterType.MoveSpeed == attrMonsterType)
			{
				num *= 1f + monsterIndex10353.attrPercent;
			}
			int count2 = monsterIndex10121.Count;
			for (int j = 0; j < count2; j++)
			{
				stMonsterSkillIndex10121 stMonsterSkillIndex40464 = monsterIndex10121[j];
				if (stMonsterSkillIndex40464.durationTimeKeep > 0f && stMonsterSkillIndex40464.attrMonsterType == (int)attrMonsterType)
				{
					num *= 1f + stMonsterSkillIndex40464.attrPercent;
				}
			}
			if (monsterIndex10211.monsterIndex10211 && monsterIndex10211.durationTimeKeep > 0f && monsterIndex10211.attrMonsterType == (int)attrMonsterType)
			{
				num *= 1f + monsterIndex10211.attrPercent;
			}
			int count3 = monsterIndex20162.Count;
			for (int k = 0; k < count3; k++)
			{
				stMonsterSkillIndex20162 stMonsterSkillIndex40465 = monsterIndex20162[k];
				if (stMonsterSkillIndex40465.hpWarning >= (float)(long)curHp / (float)(long)maxHp && stMonsterSkillIndex40465.attrMonsterType == (int)attrMonsterType)
				{
					num *= 1f + stMonsterSkillIndex40465.attrPercent;
				}
			}
			if (sub20005.sub20005)
			{
				if (sub20005.attrMonsterType1 == (int)attrMonsterType)
				{
					num *= 1f + sub20005.attrPercent1;
				}
				if (sub20005.attrMonsterType2 == (int)attrMonsterType)
				{
					num *= 1f + sub20005.attrPercent2;
				}
			}
			for (int num3 = monsterIndex10111.Count - 1; num3 >= 0; num3--)
			{
				stMonsterSkillIndex10111 stMonsterSkillIndex40466 = monsterIndex10111[num3];
				if (stMonsterSkillIndex40466.triggerFlag && stMonsterSkillIndex40466.durationTime <= 0f)
				{
					monsterIndex10111.RemoveAt(num3);
				}
				else if (stMonsterSkillIndex40466.triggerFlag && stMonsterSkillIndex40466.durationTime > 0f && stMonsterSkillIndex40466.attrMonsterType == (int)attrMonsterType)
				{
					num *= 1f + stMonsterSkillIndex40466.attrPercent;
				}
			}
			if (monsterIndex20102.skillIndexFlag)
			{
				if (monsterIndex20102.distance <= cachPositionY - combatData.defense.GetAlertPositionY())
				{
					if (attrMonsterType == (EAttrMonsterType)monsterIndex20102.attrMonsterType1)
					{
						num *= 1f + monsterIndex20102.attrPercent1;
					}
				}
				else if (attrMonsterType == (EAttrMonsterType)monsterIndex20102.attrMonsterType2)
				{
					num *= 1f + monsterIndex20102.attrPercent2;
				}
			}
			int count4 = monsterIndex10131.Count;
			for (int l = 0; l < count4; l++)
			{
				stMonsterSkillIndex10131 stMonsterSkillIndex40467 = monsterIndex10131[l];
				if (attrMonsterType == (EAttrMonsterType)stMonsterSkillIndex40467.attrMonsterType)
				{
					num *= 1f + stMonsterSkillIndex40467.attrPercent;
				}
			}
			int count5 = extraPropertiesList.Count;
			for (int m = 0; m < count5; m++)
			{
				zbExtraProperty zbExtraProperty2 = extraPropertiesList[m];
				if (attrMonsterType == (EAttrMonsterType)zbExtraProperty2.attrMonsterType && zbExtraProperty2.durationTime > 0f)
				{
					num *= 1f + zbExtraProperty2.attrPercent;
				}
			}
			if (monsterSkillIndex3011 == 3)
			{
				if (attrMonsterType == EAttrMonsterType.MoveSpeed)
				{
					num *= 1f + monsterSkillIndex30113.selfMoveSpeed;
				}
				if (attrMonsterType == EAttrMonsterType.AttackSpeed)
				{
					num *= 1f + monsterSkillIndex30113.attackSpeedPre;
				}
			}
			if (monsterSkillIndex3013 == 3)
			{
				if (attrMonsterType == EAttrMonsterType.MoveSpeed)
				{
					num *= 1f + monsterSkillIndex30133.addMoveSpeed;
				}
				if (attrMonsterType == EAttrMonsterType.Damage)
				{
					num *= 1f + monsterSkillIndex30133.addAttackDamage;
				}
			}
		}
		if (MonsterExtraProperties.TryGetValue(attrMonsterType, out var value))
		{
			num *= 1f + value;
		}
		if (attrMonsterType == EAttrMonsterType.MoveSpeed || attrMonsterType == EAttrMonsterType.AttackRange)
		{
			num /= 100f;
		}
		if (attrMonsterType == EAttrMonsterType.AttackSpeed)
		{
			num = 1f / num;
		}
		if (EAttrMonsterType.MoveSpeed == attrMonsterType && IsMoveSpeedZero())
		{
			num = 0f;
		}
		m_attrCache[(int)attrMonsterType] = new AttrCache
		{
			frame = frameCount,
			value = num
		};
		return num;
	}

	public void UseSpriteReplaceSpine(bool bUseSprite)
	{
		if (monsterAvatarRecord == null || !(monsterAvatarRecord.bigIcon != ""))
		{
			return;
		}
		if (bUseSprite)
		{
			yingziBgGO.SetActive(false);
			if (monsterSprite != null)
			{
				monsterSprite.SetMonsterSprite();
			}
			if (anim != null && anim.spineAnimation != null)
			{
				anim.spineAnimation.SetActive(bValue: false);
			}
			shuiwenGO.SetActive(false);
		}
		else
		{
			yingziBgGO.SetActive(!bMonsterNeedWaterEffect);
			shuiwenGO.SetActive(bMonsterNeedWaterEffect);
			if (monsterSprite != null)
			{
				monsterSprite.SetMonsterSpriteActive(bActive: false);
			}
			if (anim != null && anim.spineAnimation != null)
			{
				anim.spineAnimation.SetActive(bValue: true);
			}
		}
	}

	public void SetSkeletonManualUpdate(bool manual)
	{
		if (anim != null && anim.spineAnimation != null && (Object)(object)anim.spineAnimation.skeletonAnimation != (Object)null)
		{
			anim.spineAnimation.skeletonAnimation.UpdateTiming = (UpdateTiming)(!manual);
		}
	}

	protected void Update()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		zbCombatSystem instance = zbCombatSystem.Instance;
		monster3014DazeKeep += Time.deltaTime;
		monster3014DazeDam += Time.deltaTime;
		if (instance.combatFPS.IsLowPerformanceMode())
		{
			m_frameCounter++;
			if (m_frameCounter >= 3)
			{
				m_frameCounter = 0;
				zbSortMgr.Instance.UpdateSortOrder(m_sortType, ((Component)this).gameObject, this);
			}
		}
		else
		{
			m_frameCounter++;
			if (m_frameCounter >= 2)
			{
				m_frameCounter = 0;
				zbSortMgr.Instance.UpdateSortOrder(m_sortType, ((Component)this).gameObject, this);
			}
		}
		Vector3 val = (m_framePos = monsterTF.position);
		if (val.x != cachPositionX || val.y != cachPositionY)
		{
			monsterBoxBounds = ((Collider2D)boxCollider).bounds;
			combatData.monsters.monsterGrid.UpdateMonster(this);
		}
		cachPositionX = val.x;
		cachPositionY = val.y;
		UpdateSpineTier(val.y);
		fsm.Update();
		skillMgr.Update();
		anim.Update();
		if (isUsingSprite)
		{
			if (monsterSprite != null)
			{
				monsterSprite.Update();
			}
		}
		else
		{
			zbSpineGpuAnimPlayer gpuPlayer = anim.spineAnimation.gpuPlayer;
			if (gpuPlayer != null && m_spineGpuTakenOver && !gpuPlayer.isFallback)
			{
				gpuPlayer.Advance(Time.deltaTime);
			}
			else
			{
				SkeletonAnimation skeletonAnimation = anim.spineAnimation.skeletonAnimation;
				if ((Object)(object)skeletonAnimation != (Object)null && (int)skeletonAnimation.UpdateTiming == 0 && m_spineStepInterval > 1 && (Time.frameCount + m_animUpdateStagger) % m_spineStepInterval == 0)
				{
					skeletonAnimation.Update(Time.deltaTime * (float)m_spineStepInterval);
				}
			}
		}
		m_returnPoolTimeAccumTime += Time.deltaTime;
		if (m_returnPoolTimeAccumTime >= m_returnPoolTime)
		{
			ReturnToPoolImmediate();
		}
		if (cureAnimationFlag > 0f)
		{
			cureAnimationFlag -= Time.deltaTime;
			if (cureAnimationFlag <= 0f)
			{
				cureAnimationObject.SetActive(false);
			}
		}
		for (int num = extraPropertiesList.Count - 1; num >= 0; num--)
		{
			zbExtraProperty obj = extraPropertiesList[num];
			obj.durationTime -= Time.deltaTime;
			obj.durationTime -= Time.deltaTime;
			if (obj.durationTime <= 0f)
			{
				extraPropertiesList.RemoveAt(num);
			}
		}
		lastUpdateFrameActive++;
		if (lastUpdateFrameActive >= 3)
		{
			bodyScale = GetAttrValueByType(EAttrMonsterType.BodyScale);
			if (Math.Abs(lastBodyScale - bodyScale) > 0.1f)
			{
				monsterTF.localScale = Vector3.one * bodyScale;
			}
			lastBodyScale = bodyScale;
			monsterBoxBounds = ((Collider2D)boxCollider).bounds;
			lastUpdateFrameActive = 0;
		}
		if (instance.m_isCelebrating && cachPositionY <= combatData.defense.GetAlertPositionY() + 2f)
		{
			SetCurHp(0L);
		}
		if ((long)curHp > 0 && monsterRecord.moveType != 2)
		{
			m_buffSkillIntervalPre += Time.deltaTime;
			if (m_buffSkillIntervalPre >= m_buffSkillInterval)
			{
				m_buffSkillIntervalPre -= m_buffSkillInterval;
				zbBuffBase zbBuffBase2 = null;
				if (combatData.skillPoolMgr.HasActiveSubSkill(10105))
				{
					zbBuffBase2 = instance.buffSystem.IsMonsterHaveSubSkill(10105, GetUUID());
				}
				if (zbBuffBase2 == null)
				{
					zbSkillPool zbSkillPool2 = (combatData.skillPoolMgr.HasActiveSubSkill(10105) ? combatData.skillPoolMgr.FindSkillPool(10105) : null);
					if (zbSkillPool2 != null)
					{
						foreach (zbSkillBase activeSkill in zbSkillPool2.m_activeSkills)
						{
							if (!activeSkill.isSubSkill || activeSkill.skillSubRecord.id != 10105)
							{
								continue;
							}
							zbSub10105 zbSub41402 = (zbSub10105)activeSkill;
							if (!zbSub41402.boundsCircle2D.Overlaps(monsterBoxBounds))
							{
								continue;
							}
							float[][] skillBuff = zbSub41402.skillSubRecord.skillBuff;
							foreach (float[] array in skillBuff)
							{
								if (zbUtil.Roll0to1(array[0]))
								{
									instance.buffSystem.CreateBuff(zbSub41402, zbSub41402.skillAttrList, GetUUID(), (int)array[1], array[2]);
								}
							}
							break;
						}
					}
				}
				zbBuffBase zbBuffBase3 = null;
				if (combatData.skillPoolMgr.HasActiveSubSkill(11003))
				{
					zbBuffBase3 = instance.buffSystem.IsMonsterHaveSubSkill(11003, GetUUID());
				}
				if (zbBuffBase3 == null)
				{
					zbSkillPool zbSkillPool3 = (combatData.skillPoolMgr.HasActiveSubSkill(11003) ? combatData.skillPoolMgr.FindSkillPool(11003) : null);
					if (zbSkillPool3 != null)
					{
						foreach (zbSkillBase activeSkill2 in zbSkillPool3.m_activeSkills)
						{
							if (!activeSkill2.isSubSkill || activeSkill2.skillSubRecord.id != 11003)
							{
								continue;
							}
							zbSub11003 zbSub41403 = (zbSub11003)activeSkill2;
							if (!zbSub41403.boundsCircle2D.Overlaps(monsterBoxBounds))
							{
								continue;
							}
							float[][] skillBuff = zbSub41403.skillSubRecord.skillBuff;
							foreach (float[] array2 in skillBuff)
							{
								if (zbUtil.Roll0to1(array2[0]))
								{
									instance.buffSystem.CreateBuff(zbSub41403, zbSub41403.skillAttrList, GetUUID(), (int)array2[1], array2[2]);
								}
							}
							break;
						}
					}
				}
			}
		}
		CalCanSelected();
		SetHiddenVisible();
		CalFloatUp();
	}

	private void CalCanSelected()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		if ((long)curHp <= 0)
		{
			return;
		}
		m_buffSkillIntervalPre += Time.deltaTime;
		if (m_buffSkillIntervalPre >= m_buffSkillInterval)
		{
			m_buffSkillIntervalPre -= m_buffSkillInterval;
			zbSkillPool zbSkillPool2 = null;
			if (combatData.skillPoolMgr.HasActiveSubSkill(20001))
			{
				zbSkillPool2 = combatData.skillPoolMgr.FindSkillPool(20001);
			}
			if (zbSkillPool2 != null)
			{
				foreach (zbSkillBase activeSkill in zbSkillPool2.m_activeSkills)
				{
					if (activeSkill.isSubSkill && activeSkill.skillSubRecord.id == 20001 && ((zbSub20001)activeSkill).boundsCircle2D.Overlaps(monsterBoxBounds))
					{
						combatData.monsters.SetHidden(this);
						hidingNum = true;
						return;
					}
				}
			}
			zbSkillPool zbSkillPool3 = null;
			if (combatData.skillPoolMgr.HasActiveSubSkill(20002))
			{
				zbSkillPool3 = combatData.skillPoolMgr.FindSkillPool(20002);
			}
			if (zbSkillPool3 != null)
			{
				foreach (zbSkillBase activeSkill2 in zbSkillPool3.m_activeSkills)
				{
					if (activeSkill2.isSubSkill && activeSkill2.skillSubRecord.id == 20002 && ((zbSub20002)activeSkill2).boundsCircle2D.Overlaps(monsterBoxBounds))
					{
						combatData.monsters.SetHidden(this);
						hidingNum = true;
						return;
					}
				}
			}
		}
		hidingNum = false;
		combatData.monsters.SetVisible(this);
	}

	private void CalFloatUp()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (moveDownBuff17)
		{
			elapsedBuff17 += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedBuff17 / 0.5f);
			((Component)anim.spineAnimation.skeletonAnimation).transform.localPosition = Vector3.Lerp(endPointBuff17, startPointBuff17, num);
			if (num >= 1f)
			{
				moveDownBuff17 = false;
				((Component)anim.spineAnimation.skeletonAnimation).transform.localPosition = startPointBuff17;
			}
		}
	}

	private ECombatBubble GetHpBubbleType(long value, bool violent)
	{
		if (value > 0)
		{
			return ECombatBubble.MonsterAddHp;
		}
		if (violent)
		{
			return ECombatBubble.MonsterNormalViolentDecHp;
		}
		return ECombatBubble.MonsterNormalDecHp;
	}

	public bool CheckInstantKillRoll(int skillType)
	{
		for (int i = 0; i < monsterIndex10262.Count; i++)
		{
			stMonsterSkillIndex10262 stMonsterSkillIndex40463 = monsterIndex10262[i];
			if (stMonsterSkillIndex40463.skillType == skillType && zbUtil.Roll0to1(stMonsterSkillIndex40463.roll0to1))
			{
				return true;
			}
		}
		return false;
	}

	public long ChangeHp(zbAttackerBase hero, zbCombatDefense defense, long value, bool violent = false, bool instantKill = false)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if (isGoDestroyed)
		{
			return 0L;
		}
		long num = value;
		if (value == 0L)
		{
			return 0L;
		}
		if ((long)curHp <= 0 && value > 0)
		{
			return 0L;
		}
		((Vector3)(ref bubblePosition)).Set(cachPositionX, cachPositionY + (float)monsterAvatarRecord.boxHeight * bodyScale * 1f / 100f, monsterTF.position.z);
		if (value < 0 && monsterIndex10302 > 0)
		{
			combatData.combatBubble.ShowBubble(bubblePosition, ECombatBubble.Immune, value);
			monsterIndex10302--;
			return 0L;
		}
		if (instantKill)
		{
			combatData.combatBubble.ShowBubble(bubblePosition, ECombatBubble.InstantKill, 0L);
		}
		else if (value < 0 || curHp < maxHp)
		{
			ECombatBubble hpBubbleType = GetHpBubbleType(value, violent);
			combatData.combatBubble.ShowBubble(bubblePosition, hpBubbleType, value);
		}
		if (!instantKill)
		{
			value = UseShield(value);
		}
		long damage = Math.Min(curHp, Math.Abs(value));
		if (value < 0 && zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.WorldBoss && IsBoss())
		{
			damage = -value;
			if ((float)((long)curHp + value) <= (float)(long)maxHp * 0.1f)
			{
				SetCurHp(maxHp);
			}
			else
			{
				SetCurHp((long)curHp + value);
			}
			combatData.masterTeam.combatStat.AddBossDamageStat(damage);
		}
		else
		{
			SetCurHp((long)curHp + value);
		}
		if ((Object)(object)hero != (Object)null && hero.GetHeroTid() > 0)
		{
			hero.team.combatStat.AddDamageStat(hero.GetHeroTid(), null, damage);
		}
		else if ((Object)(object)defense != (Object)null)
		{
			combatData.masterTeam.combatStat.AddDamageStat(0, defense, damage);
		}
		if (num < 0)
		{
			if (isUsingSprite)
			{
				monsterSprite.LerpChangeColor();
			}
			else
			{
				anim.spineColorChange.LerpChangeColor();
			}
			monsterIndex30072DamageSta -= value;
			long num2 = beHitTotalCount + 1;
			beHitTotalCount = num2;
		}
		if (value < 0)
		{
			if (monsterIndex10222.skillIndexTake)
			{
				monsterIndex10222.skillBase.CallBackLostHp(value);
			}
			if (monsterIndex30012.skillIndexTake)
			{
				monsterIndex30012.skillBase.CallBackLostHp(value);
			}
			if (monsterIndex30042.skillIndexTake)
			{
				monsterIndex30042.skillBase.CallBackLostHp(value);
			}
			if (monster30152Class != null)
			{
				monster30152Class.CallBackLostHp(value);
			}
		}
		else if (value > 0)
		{
			cureAnimationObject.SetActive(true);
			cureAnimationFlag = 1f;
		}
		return value;
	}

	public long UseShield(long preDamage)
	{
		if ((long)shieldHp <= 0 || preDamage >= 0)
		{
			return preDamage;
		}
		long num = -preDamage;
		if ((long)shieldHp > num)
		{
			SetShieldHp((long)shieldHp - num);
			return 0L;
		}
		long num2 = num - (long)shieldHp;
		SetShieldHp(0L);
		combatData.TriggerAction5021();
		return -num2;
	}

	public void AddShieldHp(long value)
	{
		if (value > 0)
		{
			if ((long)shieldHp + value > maxShieldHp)
			{
				maxShieldHp = (long)shieldHp + value;
			}
			SetShieldHp((long)shieldHp + value);
		}
	}

	public void SetShieldHp(long value)
	{
		shieldHp.Value = Math.Clamp(value, 0L, maxShieldHp);
	}

	public void SetCurHp(long value)
	{
		long num = curHp;
		long num2 = Math.Clamp(value, 0L, maxHp);
		curHp.Value = num2;
		isDead = num2 <= 0;
		if (num > 0 && (long)curHp <= 0)
		{
			fsm.SetState(EMonsterState.Die);
			combatData.combatExp.monsterKilledCount.Value += (giveExp ? 1 : 0);
			combatData.combatExp.monsterKilledExp.Value += curExp;
			if ((int)combatData.combatExp.monsterKilledCount == 6 && zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest && zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1 && zbCombatSystem.Instance.combatSpeed.GetSpeed() == ECombatSpeed.X1)
			{
				zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
				zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbCombatWnd2.m_btnSpeed);
			}
			if (isEliteBoss)
			{
				float num3 = combatData.masterTeam.ExtraProperties[ETeamExtra.GemMod1046];
				if (num3 > 0f)
				{
					zbCombatDefense defense = combatData.defense;
					defense.ChangeHp(cachPositionX, (int)((float)(long)defense.maxHp * num3));
				}
				if (combatData.helperTeam != null)
				{
					num3 = combatData.helperTeam.ExtraProperties[ETeamExtra.GemMod1046];
					if (num3 > 0f)
					{
						zbCombatDefense defense2 = combatData.defense;
						defense2.ChangeHp(cachPositionX, (int)((float)(long)defense2.maxHp * num3));
					}
				}
			}
			float num4 = combatData.masterTeam.ExtraProperties[ETeamExtra.GemMod1056];
			if (num4 > 0f)
			{
				combatData.defense.ChangeHp(cachPositionX, (int)num4);
			}
			if (giveExp)
			{
				combatData.TriggerAction5041Passive(this);
			}
			if (combatData.helperTeam != null)
			{
				num4 = combatData.helperTeam.ExtraProperties[ETeamExtra.GemMod1056];
				if (num4 > 0f)
				{
					combatData.defense.ChangeHp(cachPositionX, (int)num4);
				}
			}
		}
		else if (num <= 0)
		{
			_ = (long)curHp;
			_ = 0;
		}
	}

	public void SetMaxHp(long value)
	{
		maxHp.Value = value;
	}

	public float GetBubbleBodyYCenter()
	{
		return 0.8f * bodyScale;
	}

	public bool IsEnterAttackRange(float extraDistance = 0f)
	{
		return Mathf.Abs(cachPositionY - (combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY()) - extraDistance) <= GetAttrValueByType(EAttrMonsterType.AttackRange);
	}

	public void SetHiddenVisible()
	{
		if (isCanSelected())
		{
			combatData.monsters.SetVisible(this);
		}
		else
		{
			combatData.monsters.SetHidden(this);
		}
	}

	public bool isCanSelected()
	{
		if (isDead)
		{
			return false;
		}
		if (monsterIndex30012Hide)
		{
			return false;
		}
		if (monsterSkillIndex10122Hiding)
		{
			return false;
		}
		if (hidingNum)
		{
			return false;
		}
		if (getSceneAlertRange_GetAlertPositionY > cachPositionY)
		{
			return cachPositionY < 7f + combatData.nodeRootPositonY;
		}
		return false;
	}

	public Vector3 GetCurPosition()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(cachPositionX, cachPositionY + constBoxHeight, 0f);
	}

	public (float x, float y) GetCurPositionXY()
	{
		return (x: cachPositionX, y: cachPositionY + constBoxHeight);
	}

	public bool CalBuffStopBool()
	{
		if (monsterIndex30123Stop || monsterIndex10353Stop)
		{
			return true;
		}
		zbBuffSystem buffSystem = zbCombatSystem.Instance.buffSystem;
		int frameCount = Time.frameCount;
		if (m_buffStopCacheFrame == frameCount && m_buffStopCacheVersion == buffSystem.buffChangeVersion)
		{
			return m_buffStopCache;
		}
		m_buffStopCacheFrame = frameCount;
		m_buffStopCacheVersion = buffSystem.buffChangeVersion;
		m_buffStopCache = buffSystem.IsMonsterHaveBuffs(GetUUID(), 2, 3, 4);
		return m_buffStopCache;
	}

	public void AddSlowDownPercent(float percent = 0f)
	{
		slowDownPercent *= 1f - Math.Clamp(percent, 0f, 0.999f);
	}

	public void DecSlowDownPercent(float percent = 0f)
	{
		slowDownPercent /= 1f - Math.Clamp(percent, 0f, 0.999f);
	}

	public void AddLocalScalePercent(float percent = 0f)
	{
		localScalePercent *= 1f + percent;
	}

	public void DecLocalScalePercent(float percent = 0f)
	{
		localScalePercent /= 1f + percent;
	}

	public void AddMassPercent(float percent = 0f)
	{
		massPercent *= 1f + percent;
	}

	public void DecMassPercent(float percent = 0f)
	{
		massPercent /= 1f + percent;
	}

	public float GetSlowDownPercent()
	{
		return slowDownPercent;
	}

	public void AddHitRatePercent(float percent = 0f)
	{
		hitRatePercent += percent;
	}

	public void DecHitRatePercent(float percent = 0f)
	{
		hitRatePercent -= percent;
	}

	public float GetHitRatePercent()
	{
		return hitRatePercent;
	}

	public void SetSkeletonTimeScale(float moveSpeedY, bool unTimeScale = false)
	{
		if (isUsingSprite)
		{
			return;
		}
		if (IsMoveSpeedZero() || unTimeScale)
		{
			anim.spineAnimation.SetTimeScale(1f);
			m_lastTimeScale = 1f;
			return;
		}
		if (!canMoveOrAttack)
		{
			anim.spineAnimation.SetTimeScale(1f);
			m_lastTimeScale = 1f;
			return;
		}
		if (CalBuffStopBool())
		{
			if ((double)Mathf.Abs(m_lastTimeScale) > 0.1)
			{
				anim.spineAnimation.SetTimeScale(0f);
				m_lastTimeScale = 0f;
			}
			return;
		}
		float num = Math.Abs(moveSpeedY) * 100f / (float)monsterRecord.artSpeed / bodyScale;
		if ((double)Mathf.Abs(m_lastTimeScale - num) > 0.1)
		{
			anim.spineAnimation.SetTimeScale(num);
			m_lastTimeScale = num;
		}
	}

	public void TriggerDie()
	{
		if (hud != null)
		{
			combatData.combatHud.ReturnHud(hud);
		}
	}

	public float GetMass()
	{
		return addPercentMass * GetAttrValueByType(EAttrMonsterType.Mass);
	}

	public bool IsMoveSpeedZero()
	{
		if (cachPositionY - combatData.defense.GetAlertPositionY() <= monsterIndex30021.distance && monsterIndex30021.monsterIndex30021Flag)
		{
			return true;
		}
		if (cachPositionY <= monsterSkillIndex30121.stopDistance && monsterSkillIndex3012 > 0)
		{
			return true;
		}
		if (cachPositionY <= monsterSkillIndex30141.stopDistance && monsterSkillIndex3014 == 1)
		{
			return true;
		}
		if (cachPositionY <= monsterSkillIndex30142.stopDistance && monsterSkillIndex3014 == 2)
		{
			return true;
		}
		return false;
	}

	private void OnDestroy()
	{
		isGoDestroyed = true;
		if (anim != null)
		{
			anim.OnDestroy();
		}
	}

	public void AddExtraDirectionList(zbExtraDirection extraDirection)
	{
		extraDirectionList.Add(extraDirection);
	}

	public zbMonster()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		startPointBuff17 = new Vector3(0f, 0f, 0f);
		endPointBuff17 = new Vector3(0f, 0.5f, 0f);
		durationBuff17 = 1f;
		sub20005 = new stSub20005();
		monsterIndex10000 = new stMonsterSkillIndex10000();
		monsterIndex10001 = new stMonsterSkillIndex10001();
		monsterIndex10001_2 = new List<stMonsterSkillIndex10001_2>();
		monsterIndex10021 = new List<stMonsterSkillIndex10021>();
		monsterIndex10051Flag = -1;
		monsterIndex10051Read = 1f;
		monsterIndex10081 = new List<stMonsterSkillIndex10081>();
		monsterIndex10111 = new List<stMonsterSkillIndex10111>();
		monsterIndex10121 = new List<stMonsterSkillIndex10121>();
		monsterIndex10122 = new stMonsterSkillIndex10122();
		monsterIndex10131 = new List<stMonsterSkillIndex10131>();
		monsterIndex10161 = new stMonsterSkillIndex10161();
		monsterIndex10201 = new List<stMonsterSkillIndex10201>();
		monsterIndex10211 = new stMonsterSkillIndex10211();
		monsterIndex10222 = new stMonsterSkillIndex10222();
		monsterIndex10223 = new stMonsterSkillIndex10223();
		monsterIndex10261 = new List<stMonsterSkillIndex10261>();
		monsterIndex10262 = new List<stMonsterSkillIndex10262>();
		monsterIndex10281 = new List<stMonsterSkillIndex10281>();
		monsterIndex10311 = new List<stMonsterSkillIndex10311Extra>();
		monsterIndex10321 = new List<stMonsterSkillIndex10321>();
		monsterIndex10322 = new stMonsterSkillIndex10322();
		monsterIndex10332 = new List<stMonsterSkillIndex10332>();
		monsterIndex10352 = new stMonsterSkillIndex10352();
		monsterIndex10353 = new stMonsterSkillIndex10353();
		monsterIndex10362 = new stMonsterSkillIndex10362();
		monsterIndex10373 = new stMonsterSkillIndex10373();
		monsterIndex10382 = new stMonsterSkillIndex10382();
		immBuffList = new List<int>();
		monsterIndex10441 = new stMonsterSkillIndex10441();
		monsterIndex10462 = new stMonsterSkillIndex10462();
		monsterIndex20091 = new stMonsterSkillIndex20091();
		monsterIndex20102 = new stMonsterSkillIndex20102();
		monsterIndex20132 = new stMonsterSkillIndex20132();
		monsterIndex20135 = new stMonsterSkillIndex20135();
		monsterIndex20161 = new stMonsterSkillIndex20161();
		monsterIndex20162 = new List<stMonsterSkillIndex20162>();
		monsterIndex20163 = new stMonsterSkillIndex20163();
		monsterIndex20192 = new stMonsterSkillIndex20192();
		monsterIndex30012 = new stMonsterSkillIndex30012();
		monsterIndex30021 = new stMonsterSkillIndex30021();
		monsterIndex30042 = new stMonsterSkillIndex30042();
		monsterIndex30062 = new stMonsterSkillIndex30062();
		monsterSkillIndex30111 = new stMonsterSkillIndex30111();
		monsterSkillIndex30112 = new stMonsterSkillIndex30112();
		monsterSkillIndex30113 = new stMonsterSkillIndex30113();
		monsterSkillIndex30121 = new stMonsterSkillIndex30121();
		monsterSkillIndex30122 = new stMonsterSkillIndex30122();
		monsterSkillIndex30123 = new stMonsterSkillIndex30123();
		monsterSkillIndex30131 = new stMonsterSkillIndex30131();
		monsterSkillIndex30132 = new stMonsterSkillIndex30132();
		monsterSkillIndex30133 = new stMonsterSkillIndex30133();
		monsterSkillIndex30141 = new stMonsterSkillIndex30141();
		monsterAddSpeedList = new List<stMonsterAddSpeed>();
		monsterSkillIndex30142 = new stMonsterSkillIndex30142();
		monster3014DazeKeep = 99f;
		monster3014DazeDam = 99f;
		monsterSkillIndex30143 = new stMonsterSkillIndex30143();
		monsterSkillIndex30151 = new stMonsterSkillIndex30151();
		monsterSkillIndex30152 = new stMonsterSkillIndex30152();
		monsterSkillIndex30153 = new stMonsterSkillIndex30153();
		monsterSkillIndex30161 = new stMonsterSkillIndex30161();
		monsterSkillIndex30162 = new stMonsterSkillIndex30162();
		monsterSkillIndex30163 = new stMonsterSkillIndex30163();
		monsterIndex40111 = new stMonsterSkillIndex40111();
		monsterIndex40151 = new stMonsterSkillIndex40151();
		monsterIndex40402 = new stMonsterSkillIndex40402();
		monsterIndex40421 = new stMonsterSkillIndex40421();
		monsterIndex40462 = new stMonsterSkillIndex40462();
		m_animUpdateStagger = Rand.Range(0, zbCombatFPS.MANUAL_UPDATE_FRAME);
		m_spineStepInterval = 1;
		curHp = new zbSafeLong(100L, "zbMonster.curHp");
		maxHp = new zbSafeLong(100L, "zbMonster.maxHp");
		shieldHp = new zbSafeLong(0L, "zbMonster.shieldHp");
		maxShieldHp = 1L;
		m_attrCache = new AttrCache[32];
		minCx = int.MinValue;
		maxCx = int.MinValue;
		minCy = int.MinValue;
		maxCy = int.MinValue;
		activeIndex = -1;
		damageAttack = new zbSafeInt(0, "zbMonster.damageAttack");
		attackSpeed = new zbSafeFloat(0f, "zbMonster.attackSpeed");
		attackRange = new zbSafeFloat(0f, "zbMonster.attackRange");
		moveSpeed = new zbSafeFloat(0f, "zbMonster.moveSpeed");
		mass = new zbSafeInt(0, "zbMonster.mass");
		damageTaken = new zbSafeFloat(0f, "zbMonster.damageTaken");
		m_sortType = ESortLayerType.Monster;
		slowDownPercent = 1f;
		localScalePercent = 1f;
		massPercent = 1f;
		hitRatePercent = 1f;
		beDamageUpPercent = new zbSafeFloat(0f, "zbMonster.beDamageUpPercent");
		buffDieTodo = new List<zbBuffBase>();
		extraDirectionList = new List<zbExtraDirection>();
		extraPropertiesList = new List<zbExtraProperty>();
		MonsterExtraProperties = new Dictionary<EAttrMonsterType, float>();
		curExp = 1;
		canMoveOrAttack = true;
		m_UUID = sInvalidId;
		boxColliderEnable = true;
		m_lastTimeScale = 1f;
		lastUpdateFrameActive = 3;
		addPercentMass = 1f;
		m_buffStopCacheFrame = -1;
		cachPositionX = -15f;
		cachPositionY = -15f;
		m_buffSkillInterval = 0.2f;
		m_buffSkillIntervalPre = 0.2f;
		bubblePosition = Vector3.zero;
		breakTotal = 1;
		waterHeight = 1f;
		fadeRange = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}
