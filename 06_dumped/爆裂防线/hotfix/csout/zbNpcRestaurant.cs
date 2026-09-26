using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbNpcRestaurant : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	public float moveSpeed = 2f;

	public float walkSpeed = 0.4f;

	public int curHeroAvatarId;

	private float baseZorder;

	private Vector2 m_vector;

	private Coroutine m_walkCoroutine;

	protected void Awake()
	{
		m_skeletonAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		baseZorder = Rand.Range(-0.0005f, 0.0005f);
	}

	protected void OnEnable()
	{
		if (m_walkCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_walkCoroutine);
		}
		m_walkCoroutine = ((MonoBehaviour)this).StartCoroutine(StateLoop());
	}

	protected void Update()
	{
		UpdateZorder();
	}

	private void UpdateZorder()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Component)this).transform.localPosition.y * 0.001f;
		float num2 = baseZorder + num;
		((Component)m_skeletonAnimation).transform.localPosition = new Vector3(((Component)m_skeletonAnimation).transform.localPosition.x, ((Component)m_skeletonAnimation).transform.localPosition.y, num2);
	}

	private void SetFacing(bool left)
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
		return m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
	}

	public SkeletonAnimation GetAnimation()
	{
		return m_skeletonAnimation;
	}

	public void Refresh(int heroAvatarId)
	{
		curHeroAvatarId = heroAvatarId;
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(heroAvatarId);
		if (recordByKey != null)
		{
			zbUtil.RefreshSpine(m_skeletonAnimation, recordByKey.frontBone + "_SkeletonData.asset", "idle", loop: true);
		}
		((Component)this).gameObject.SetActive(true);
	}

	private IEnumerator StateLoop()
	{
		while (true)
		{
			yield return (object)new WaitForSeconds(Rand.Range(3f, 6f));
			Vector2 targetPos = Vector2.op_Implicit(GetRandomWalkablePos());
			SetAnimation("walk", loop: true);
			while (Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), targetPos) > 0.1f)
			{
				SetFacing(((Component)this).transform.position.x > targetPos.x);
				((Component)this).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)this).transform.position), targetPos, walkSpeed * Time.deltaTime));
				yield return null;
			}
			SetAnimation("idle", loop: true);
			yield return (object)new WaitForSeconds(0.5f);
		}
	}

	private Vector3 GetRandomWalkablePos()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float num = 4f;
		float num2 = 0.4f;
		float min = (-2.3f + num) * 0.5f + num2;
		float max = num - num2;
		return new Vector3(Rand.Range(min, max), ((Component)this).transform.position.y, ((Component)this).transform.position.z);
	}
}
