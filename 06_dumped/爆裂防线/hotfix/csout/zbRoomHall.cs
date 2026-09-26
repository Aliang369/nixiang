using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbRoomHall : MonoBehaviour
{
	private SkeletonAnimation m_spineBox;

	private SpriteRenderer m_notify;

	private int m_lastCount = -1;

	private void Awake()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_spineBox = ((Component)((Component)this).transform.Find("spineBox")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_spineBox).updateWhenInvisible = (UpdateMode)0;
		m_notify = ((Component)((Component)this).transform.Find("notify")).GetComponent<SpriteRenderer>();
	}

	public void Refresh(HomelandRoomDetail roomData)
	{
		((Component)this).gameObject.SetActive(false);
	}

	private int GetWorldResourceCollectCount()
	{
		if (zbSelf.Instance.worldHandler.CanWorldResIdle())
		{
			if (zbSelf.Instance.worldHandler.worldResIdleSettleSecond > zbServerData.sGameData.WorldInfo.ResIdleInfo.SettleMax)
			{
				return 3;
			}
			if (zbSelf.Instance.worldHandler.worldResIdleSettleSecond > zbServerData.sGameData.WorldInfo.ResIdleInfo.SettleMax / 2)
			{
				return 2;
			}
			if (zbSelf.Instance.worldHandler.worldResIdleSettleSecond > 3600)
			{
				return 1;
			}
		}
		return 0;
	}

	public void Update()
	{
		if (!zbIntervalTimer.Instance.IsOneSecondPassed())
		{
			return;
		}
		((Component)m_notify).gameObject.SetActive(zbSelf.Instance.worldHandler.HasResCollectNotify());
		int worldResourceCollectCount = GetWorldResourceCollectCount();
		if (worldResourceCollectCount != m_lastCount)
		{
			m_lastCount = worldResourceCollectCount;
			bool flag = worldResourceCollectCount > 0;
			((Component)this).gameObject.SetActive(flag);
			if (flag)
			{
				string text = $"event_chekubox{worldResourceCollectCount}";
				zbUtil.RefreshSpine(m_spineBox, "animation/dungeon/event/" + text + "/" + text + "_SkeletonData.asset", "1", loop: true);
			}
		}
	}
}
