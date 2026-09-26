using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbLoadingWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private VisualElement m_progress;

	private VisualElement m_progressCursor;

	private float m_maxWidth = 708f;

	private float m_cursorOffset = 53f;

	private float m_visualProgress;

	private bool m_bInit;

	private bool m_bLast;

	private float m_autoGuideMainLevelWaitTime;

	private bool m_loadCompletedEventExeced;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_progressCursor = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress_cursor", (string)null);
		int num = Rand.Range(1, 6);
		m_spineScene.SetSceneBg($"ui/image/login/loading/loading_{num}.jpg");
		m_visualProgress = 0f;
		m_bInit = false;
		m_bLast = false;
		m_autoGuideMainLevelWaitTime = 0f;
		m_loadCompletedEventExeced = false;
		UpdateProgressBar(0f);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Quest_Level1, OnEventQuestLevel1);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (!m_bInit)
		{
			m_bInit = true;
			return;
		}
		m_visualProgress += Time.unscaledDeltaTime * 400f;
		if (m_visualProgress >= 100f && !m_bLast)
		{
			m_visualProgress = 100f;
			UpdateProgressBar(m_visualProgress);
			m_bLast = true;
			return;
		}
		if (m_visualProgress >= 100f)
		{
			m_visualProgress = 100f;
			if (!m_loadCompletedEventExeced)
			{
				m_loadCompletedEventExeced = true;
				OnLoadingComplete();
			}
			m_autoGuideMainLevelWaitTime += Time.unscaledDeltaTime;
			if (m_autoGuideMainLevelWaitTime >= 5f)
			{
				zbUIMgr.Instance.ShowMainLevel();
				Hide();
			}
		}
		UpdateProgressBar(m_visualProgress);
	}

	private void UpdateProgressBar(float progress)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(progress / 100f) * m_maxWidth;
		float num2 = num - m_cursorOffset;
		m_progress.style.width = new StyleLength(num);
		m_progressCursor.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_progressCursor.style.left = new StyleLength(num2);
		if ((Object)(object)m_spineScene?.skeletonAnimation != (Object)null)
		{
			((Component)m_spineScene.skeletonAnimation).transform.localPosition = new Vector3(num2 * 0.01f - 2.7f, -6.1f, 0f);
		}
	}

	private void OnEventQuestLevel1(object obj)
	{
		Hide();
	}

	private void OnLoadingComplete()
	{
		zbSelf.Instance.OnLoadingComplete();
		zbProgressEventHelper.Instance.SetProgressTrack(603);
		zbSelf.Instance.operateHandler.GetOperateCount();
		zbSelf.Instance.holidayHandler.GetHolidayCount();
		if (!zbGameSave.Instance.GetBool("FirstLevel2Result" + zbSelf.Instance.GetRoleId()) && zbServerData.sGameData.MainQuestInfo.MainLevel >= 2)
		{
			zbGameSave.Instance.SetBool("FirstLevel2Result" + zbSelf.Instance.GetRoleId(), value: true);
		}
		if (zbServerData.sGameData.MainQuestInfo.MainLevel == 0 && zbSelf.Instance.guideHandler.IsFirstEnterGame())
		{
			zbSelf.Instance.guideHandler.SetFirstEnterGame();
			zbWsMsgHandler.Instance.MainQuest_FightStart_Req(1);
		}
		else
		{
			zbUIMgr.Instance.ShowMainLevel();
			if (!zbChannel_Login.IsOpenTSCheck())
			{
				zbSubjectMgr.Instance.ShowLoginSubject();
			}
			Hide();
		}
		zbChannel_Report.ReportDataByChannel(EReportType.E_ENTER_GAME, "");
	}
}
