using System;
using Newtonsoft.Json.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbZombie : MonoBehaviour
{
	[NonSerialized]
	public zbZombieStateMachine fsm;

	[NonSerialized]
	public SkeletonAnimation m_skeletonAnimation;

	private zbSpineColorChange m_spineColorChange;

	private Color m_originColor;

	private bool m_firstInit;

	[NonSerialized]
	public ESortMainLayerType m_sortType = ESortMainLayerType.Worker;

	public static readonly int sInvalidId = -1;

	private static int sAutoId = 0;

	private int m_UUID = sInvalidId;

	[NonSerialized]
	public zbBounds2D bounds2D;

	private static float boxRadius = 0.25f;

	private int m_hp;

	private int m_maxHp;

	private float m_zorderYOffset;

	public int GetUUID()
	{
		return m_UUID;
	}

	public void Init(bool isSweep)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		m_UUID = ++sAutoId;
		m_hp = Rand.Range(5, 10);
		m_maxHp = m_hp;
		m_zorderYOffset = Rand.Range(-0.08f, 0.08f);
		RefreshAvatar();
		zbHomeMgr.Instance.zombieMgr.AddZombie(this);
		Vector3 zombieSpawnPosition = zbRoomDefine.Instance.ZombieSpawnPosition;
		((Component)this).transform.position = zombieSpawnPosition + new Vector3(0f, m_zorderYOffset, 0f);
		SetFacing(left: false);
		if (isSweep)
		{
			fsm.SetState(EZombieState.Run);
		}
		else
		{
			fsm.SetState(EZombieState.Move);
		}
		UpdateBound();
	}

	protected void Awake()
	{
		m_skeletonAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_spineColorChange = new zbSpineColorChange();
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		fsm = new zbZombieStateMachine();
		fsm.Init(this);
		UpdateBound();
	}

	protected void OnEnable()
	{
		fsm.OnEnable();
	}

	protected void Update()
	{
		fsm.Update();
		m_spineColorChange.DeltaUpdate();
		UpdateBound();
		UpdateZorder();
	}

	private void RefreshAvatar()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		JArray val = (JArray)zbDataConfig.Instance.idleBaseJsonTable.GetRecord()["npcAvatar"];
		int key = (int)val[Rand.Range(0, ((JContainer)val).Count)];
		zbNpcAvatarCsvRecord recordByKey = zbDataConfig.Instance.npcAvatarCsvTable.GetRecordByKey(key);
		if (recordByKey != null)
		{
			zbStyleInfoHelper.RefreshNpcAvatar(m_skeletonAnimation, recordByKey);
		}
		if (!m_firstInit)
		{
			m_originColor = SkeletonExtensions.GetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton);
			m_firstInit = true;
		}
		else
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, m_originColor);
		}
		m_spineColorChange.Init((SkeletonRenderer)(object)m_skeletonAnimation);
		((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
	}

	private void UpdateBound()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		bounds2D.halfSize.x = boxRadius * transform.localScale.x;
		bounds2D.halfSize.y = boxRadius * transform.localScale.y;
		bounds2D.center.x = transform.position.x;
		bounds2D.center.y = transform.position.y + bounds2D.halfSize.y + 0.05f;
	}

	private void UpdateZorder()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		position.y += m_zorderYOffset;
		zbSortMainMgr.Instance.UpdateSortOrder(m_sortType, ((Component)this).gameObject, position);
	}

	public Rect GetWorldRect2D()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		return new Rect(bounds2D.center.x - bounds2D.halfSize.x, bounds2D.center.y - bounds2D.halfSize.y, bounds2D.halfSize.x * 2f, bounds2D.halfSize.y * 2f);
	}

	public Vector3 GetBoundCenter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		position.x = bounds2D.center.x;
		position.y = bounds2D.center.y;
		return position;
	}

	public void SetFacing(bool left)
	{
		if (left)
		{
			((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX = 1f;
		}
		else
		{
			((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX = -1f;
		}
	}

	public TrackEntry SetAnimation(string animationName, bool loop)
	{
		TrackEntry result = m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
		m_skeletonAnimation.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		return result;
	}

	public void ReturnToPoolImmediate()
	{
		fsm.SetState(EZombieState.Exit);
		zbHomeMgr.Instance.zombieMgr.RemoveZombie(this);
		zbHomeMgr.Instance.zombieMgr.Recycle(this);
	}

	public void OnHit(bool killed)
	{
		m_spineColorChange.LerpChangeColor();
		if (killed)
		{
			m_hp = 0;
		}
		else
		{
			m_hp--;
		}
		if (m_hp <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		fsm.SetState(EZombieState.Die);
	}

	public bool IsDie()
	{
		return m_hp <= 0;
	}
}
