using Spine;
using Spine.Unity;
using UnityEngine;

public class zbZombieDieState(zbZombieStateMachine fsm, EZombieState eState) : zbZombieState(fsm, eState)
{
	private float m_overTime;

	private float m_accumTime;

	private float m_fadeDuration = 0.5f;

	private float m_fadeStartTime;

	private Color m_originColor;

	public override void OnEnter()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		OnEnable();
		m_overTime = 0f;
		m_accumTime = 0f;
		TrackEntry val = m_fsm.zombie.SetAnimation("die", loop: false);
		if (val != null)
		{
			m_overTime = val.Animation.Duration;
		}
		m_fadeStartTime = m_overTime - m_fadeDuration;
		m_originColor = SkeletonExtensions.GetColor(((SkeletonRenderer)m_fsm.zombie.m_skeletonAnimation).skeleton);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_accumTime += Time.deltaTime;
		if (m_accumTime >= m_fadeStartTime)
		{
			float num = Mathf.Clamp01((m_accumTime - m_fadeStartTime) / m_fadeDuration);
			float a = Mathf.Lerp(1f, 0f, num);
			((SkeletonRenderer)m_fsm.zombie.m_skeletonAnimation).skeleton.A = a;
		}
		if (m_accumTime >= m_overTime)
		{
			m_fsm.zombie.ReturnToPoolImmediate();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		((SkeletonRenderer)m_fsm.zombie.m_skeletonAnimation).skeleton.A = m_originColor.a;
	}
}
