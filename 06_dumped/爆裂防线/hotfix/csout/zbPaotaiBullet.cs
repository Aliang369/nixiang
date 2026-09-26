using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class zbPaotaiBullet : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	private GameObject m_objBound;

	private zbBounds2D bounds2D;

	private static float boxRadius = 0.06f;

	private static readonly float m_lifeTime = 5f;

	private float m_accumTime;

	private static readonly float m_bulletSpped = 22f;

	protected Vector3 m_direction;

	protected ESortMainLayerType m_sortType;

	private bool m_canPenetrate;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		m_skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		m_objBound = ((Component)((Component)this).transform.Find("bound")).gameObject;
	}

	public void Init(Vector2 startPos, Vector2 targetPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = Vector2.op_Implicit(startPos);
		Vector2 val = targetPos - startPos;
		m_direction = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		((Component)this).transform.up = m_direction;
		m_canPenetrate = false;
		m_accumTime = 0f;
		UpdateZorder();
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Translate(m_direction * m_bulletSpped * Time.deltaTime, (Space)0);
		UpdateBound();
		UpdateMonsterCollision();
		UpdateZorder();
	}

	private void UpdateBound()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = m_objBound.transform;
		bounds2D.center.x = transform.position.x;
		bounds2D.center.y = transform.position.y;
		float num = boxRadius;
		if (m_canPenetrate)
		{
			num *= 2f;
		}
		bounds2D.halfSize.x = num * transform.localScale.x;
		bounds2D.halfSize.y = num * transform.localScale.y;
		if (transform.position.y < -13.5f)
		{
			zbHomeMgr.Instance.paotaiMgr.bulletPool.Recycle(this);
		}
		m_accumTime += Time.deltaTime;
		if (m_accumTime > m_lifeTime)
		{
			zbHomeMgr.Instance.paotaiMgr.bulletPool.Recycle(this);
		}
	}

	private void UpdateZorder()
	{
		zbSortMainMgr.Instance.UpdateSortOrder(m_sortType, ((Component)this).gameObject);
	}

	private void UpdateMonsterCollision()
	{
		foreach (KeyValuePair<int, zbZombie> zombie in zbHomeMgr.Instance.zombieMgr.zombieList)
		{
			zbZombie value = zombie.Value;
			if (bounds2D.Overlaps(value.bounds2D))
			{
				TakeDamage(value);
				if (!m_canPenetrate)
				{
					break;
				}
			}
		}
	}

	private void TakeDamage(zbZombie zombie)
	{
		zombie.OnHit(m_canPenetrate);
		if (!m_canPenetrate)
		{
			zbHomeMgr.Instance.paotaiMgr.bulletPool.Recycle(this);
		}
	}

	public void SetPenetrate(bool canPenetrate)
	{
		m_canPenetrate = canPenetrate;
	}

	public zbPaotaiBullet()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_direction = Vector3.up;
		m_sortType = ESortMainLayerType.Projectile;
		((MonoBehaviour)this)._002Ector();
	}
}
