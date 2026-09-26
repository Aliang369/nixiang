using Spine.Unity;
using UnityEngine;

public class zbElevatorTop : MonoBehaviour
{
	private SkeletonAnimation m_spineDoor;

	private void Awake()
	{
		m_spineDoor = ((Component)((Component)this).transform).GetComponent<SkeletonAnimation>();
		m_spineDoor.AnimationState.SetAnimation(0, "close_idle", true);
	}

	public void OpenDoor()
	{
		if ((Object)(object)m_spineDoor != (Object)null && (Object)(object)((Component)m_spineDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineDoor, "open"))
		{
			m_spineDoor.AnimationState.SetAnimation(0, "open", false);
		}
	}

	public void CloseDoor()
	{
		if ((Object)(object)m_spineDoor != (Object)null && (Object)(object)((Component)m_spineDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineDoor, "close"))
		{
			m_spineDoor.AnimationState.SetAnimation(0, "close", false);
		}
	}
}
