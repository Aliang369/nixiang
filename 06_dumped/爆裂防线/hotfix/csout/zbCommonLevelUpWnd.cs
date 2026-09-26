using Spine;
using Spine.Unity;
using UnityEngine.UIElements;

public class zbCommonLevelUpWnd : zbBaseWnd
{
	private SpineVisualElement m_spine;

	private bool m_animationFinished;

	private TrackEntry m_currentTrack;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_spine == null)
		{
			m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "level_up", (string)null);
		}
	}

	public override void Refresh()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		m_animationFinished = false;
		if (m_currentTrack != null)
		{
			m_currentTrack.Complete -= new TrackEntryDelegate(OnAnimationComplete);
			m_currentTrack = null;
		}
		if (m_spine.AnimationState != null)
		{
			m_currentTrack = m_spine.AnimationState.SetAnimation(0, "animation", false);
			m_currentTrack.Complete += new TrackEntryDelegate(OnAnimationComplete);
		}
	}

	private void OnAnimationComplete(TrackEntry entry)
	{
		if (!m_animationFinished && m_RootElement.visible)
		{
			m_animationFinished = true;
			Hide();
		}
	}
}
