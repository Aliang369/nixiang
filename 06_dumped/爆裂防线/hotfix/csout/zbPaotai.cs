using Spine;
using Spine.Unity;
using UnityEngine;

public class zbPaotai : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	private Bone m_boneShoot;

	private GameObject m_gunpointFollow;

	private zbPaotaiStateMachine fsm;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		m_skeletonAnimation = ((Component)((Component)this).transform).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		m_gunpointFollow = ((Component)((Component)this).transform.Find("gunpoint")).gameObject;
		fsm = new zbPaotaiStateMachine();
		fsm.Init(this);
		Refresh();
	}

	protected void OnEnable()
	{
		fsm.OnEnable();
	}

	public void Refresh()
	{
		RefreshSpine();
	}

	private void RefreshSpine()
	{
		string spinePath = "animation/home/room_damen_paotai/room_damen_paotai_SkeletonData.asset";
		zbUtil.RefreshSpine(m_skeletonAnimation, spinePath, "animation", loop: true);
		m_boneShoot = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("shoot");
	}

	private void Update()
	{
		fsm.Update();
	}

	public TrackEntry SetAnimation(string animationName, bool loop)
	{
		TrackEntry result = m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
		m_skeletonAnimation.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		return result;
	}

	public void LookAt(Vector3 targetWorldPos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(targetWorldPos);
		SkeletonExtensions.SetLocalPosition(m_boneShoot, val);
	}

	public Vector3 GetGunpointPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return m_gunpointFollow.transform.position;
	}

	public void ForceSweep()
	{
		fsm.SetState(EPaotaiState.ShootSweep);
	}
}
