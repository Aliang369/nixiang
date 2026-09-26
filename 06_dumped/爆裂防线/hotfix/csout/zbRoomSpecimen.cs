using Proto.Types;
using Spine.Unity;
using UnityEngine;
using zbType;

public class zbRoomSpecimen : MonoBehaviour
{
	private SkeletonAnimation m_spineComputer;

	private SpriteRenderer m_notify;

	private void Awake()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_spineComputer = ((Component)((Component)this).transform.Find("spineComputer")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_spineComputer).updateWhenInvisible = (UpdateMode)0;
		m_notify = ((Component)((Component)this).transform.Find("notify")).GetComponent<SpriteRenderer>();
	}

	public void Refresh(HomelandRoomDetail roomData)
	{
		((Component)this).gameObject.SetActive(roomData != null && roomData.Level > 0);
	}

	public void Update()
	{
		((Component)m_notify).gameObject.SetActive(zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Monster));
	}
}
