using System;
using UnityEngine.UIElements;

public class zbMainLevelPerfectPassTipWnd : zbBaseWnd
{
	private Button m_btnCheck;

	private Button m_btnClose;

	private Button m_btnConfirm;

	private Button m_btnCancel;

	private Label m_iconCheck;

	private int m_toLevel;

	private bool m_isElite;

	private long m_roleId;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnCheck = UQueryExtensions.Q<Button>(m_RootElement, "btn_check", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_iconCheck = UQueryExtensions.Q<Label>(m_RootElement, "lb_isCheck", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheck, (Action)OnToggle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnToNew, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnFight, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(int toLevel, bool isElite, long roleId)
	{
		m_toLevel = toLevel;
		m_isElite = isElite;
		m_roleId = roleId;
		((VisualElement)m_iconCheck).visible = !zbSelf.Instance.mainQuestHandler.shouldShowMainQuestTip;
	}

	private void OnToggle()
	{
		zbSelf.Instance.mainQuestHandler.shouldShowMainQuestTip = !zbSelf.Instance.mainQuestHandler.shouldShowMainQuestTip;
		((VisualElement)m_iconCheck).visible = !zbSelf.Instance.mainQuestHandler.shouldShowMainQuestTip;
	}

	private void OnFight()
	{
		if (!m_isElite)
		{
			zbWsMsgHandler.Instance.MainQuest_FightStart_Req(m_toLevel);
		}
		else if (m_roleId == 0L)
		{
			Hide();
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10662"), OnFightEliteQuest);
		}
		else
		{
			Hide();
			zbWsMsgHandler.Instance.EliteQuest_FightStart_Req(m_toLevel, m_roleId, zbSelf.Instance.eliteQuestHandler.fightCount);
		}
	}

	private void OnFightEliteQuest(bool isFight)
	{
		if (isFight && m_roleId == 0L)
		{
			zbWsMsgHandler.Instance.EliteQuest_FightStart_Req(m_toLevel, 0L, zbSelf.Instance.eliteQuestHandler.fightCount);
		}
	}

	private void OnToNew()
	{
		Hide();
		if (!m_isElite)
		{
			zbUIMgr.Instance.ShowMainLevel(0, zbServerData.sGameData.MainQuestInfo.NextLevel);
		}
		else
		{
			zbUIMgr.Instance.ShowEliteQuest(zbServerData.sGameData.EliteQuestInfo.NextLevel);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
