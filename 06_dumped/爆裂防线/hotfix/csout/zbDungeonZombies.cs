using System.Collections;
using Spine.Unity;
using UnityEngine;

public class zbDungeonZombies : MonoBehaviour
{
	private Transform m_root;

	private SkeletonAnimation m_skeletonAnimation;

	private string currentAnimation = "";

	private Transform m_target;

	public float moveSpeed = 1f;

	private Coroutine m_moveCoroutine;

	private float shoutTimer;

	private float nextShoutTime;

	private void Awake()
	{
		m_root = ((Component)this).transform.Find("root");
		m_skeletonAnimation = ((Component)m_root.Find("SkeletonAnimation")).GetComponent<SkeletonAnimation>();
		ResetShoutTimer();
	}

	public void RefreshSpine(int npcAvatarId)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		zbNpcAvatarCsvRecord recordByKey = zbDataConfig.Instance.npcAvatarCsvTable.GetRecordByKey(npcAvatarId);
		if (recordByKey != null)
		{
			zbStyleInfoHelper.RefreshNpcAvatar(m_skeletonAnimation, recordByKey);
			((Component)m_skeletonAnimation).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
		}
	}

	public void SetTarget(Transform newTarget)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		m_target = newTarget;
		if (m_moveCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_moveCoroutine);
		}
		if ((Object)(object)m_target != (Object)null)
		{
			Vector2 val = Vector2.op_Implicit(m_target.position - ((Component)this).transform.position);
			Vector3 localScale = ((Component)this).transform.localScale;
			if (val.x > 0f)
			{
				localScale.x = 0f - Mathf.Abs(localScale.x);
			}
			else if (val.x < 0f)
			{
				localScale.x = Mathf.Abs(localScale.x);
			}
			((Component)this).transform.localScale = localScale;
			m_moveCoroutine = ((MonoBehaviour)this).StartCoroutine(MoveToTarget());
		}
		else
		{
			PlayAnimation("idle", loop: true);
		}
	}

	private IEnumerator MoveToTarget()
	{
		PlayAnimation("run_zombie", loop: true);
		Vector2 val = default(Vector2);
		Vector2 val2 = default(Vector2);
		while ((Object)(object)m_target != (Object)null)
		{
			Vector3 position = ((Component)this).transform.position;
			((Vector2)(ref val))._002Ector(position.x, position.y);
			((Vector2)(ref val2))._002Ector(m_target.position.x, m_target.position.y);
			if (Vector2.Distance(val, val2) <= 0.2f)
			{
				break;
			}
			Vector2 val3 = val2 - val;
			Vector2 val4 = ((Vector2)(ref val3)).normalized * moveSpeed * Time.unscaledDeltaTime;
			val += val4;
			((Component)this).transform.position = new Vector3(val.x, val.y, position.z);
			yield return null;
		}
		PlayAnimation("idle", loop: true);
	}

	private void Update()
	{
		shoutTimer += Time.unscaledDeltaTime;
		if (shoutTimer >= nextShoutTime)
		{
			if (Rand.value < 0.3f)
			{
				zbAudioMgr.Instance.PlayAudio("audio/combat/monster_bellow.ogg");
			}
			ResetShoutTimer();
		}
	}

	private void ResetShoutTimer()
	{
		shoutTimer = 0f;
		nextShoutTime = Rand.Range(3f, 6f);
	}

	private void PlayAnimation(string animationName, bool loop)
	{
		if (!(currentAnimation == animationName) && zbUtil.CheckAnimationNameExist(m_skeletonAnimation, animationName))
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
			currentAnimation = animationName;
		}
	}
}
