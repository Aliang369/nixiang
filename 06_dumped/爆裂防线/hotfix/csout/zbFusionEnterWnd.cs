using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFusionEnterWnd : zbBaseWnd
{
	private zbFusionTopPanel m_topPanel = new zbFusionTopPanel();

	private VisualElement m_nodeDecoration;

	private VisualElement m_progressDailyActive;

	private Label m_lbDesc;

	private Label m_lbDailyActive;

	private Button m_btnGoto;

	private Button m_btnStart;

	private Button m_btnContinue;

	private Button m_btnRestart;

	private Button m_btnAward;

	private Button m_btnRank;

	private Button m_btnClose;

	private VisualElement m_notifyAward;

	private const float UnlockDailyActive = 80f;

	protected override void OnEnable()
	{
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_topPanel", (string)null);
		m_topPanel.OnEnable(this, root);
		m_nodeDecoration = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_decoration", (string)null);
		m_progressDailyActive = UQueryExtensions.Q<VisualElement>(m_nodeDecoration, "progress", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeDecoration, "lb_desc", (string)null);
		m_lbDailyActive = UQueryExtensions.Q<Label>(m_nodeDecoration, "lb_progress", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_nodeDecoration, "btn_goto", (string)null);
		m_btnStart = UQueryExtensions.Q<Button>(m_RootElement, "btn_start", (string)null);
		m_btnContinue = UQueryExtensions.Q<Button>(m_RootElement, "btn_continue", (string)null);
		m_btnRestart = UQueryExtensions.Q<Button>(m_RootElement, "btn_restart", (string)null);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_award", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_notifyAward = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnAward, (Vector2?)new Vector2(-5f, -3f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStart, (Action)OnStart, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnContinue, (Action)OnContinue, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRestart, (Action)OnRestart, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action)OnAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_FusionAwardList, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, delegate
		{
			Refresh();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DailyTask_Data, RefreshDailyActive);
	}

	public override void Refresh()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		m_topPanel.Refresh();
		int count = zbSelf.Instance.operateHandler.fusionData.Count;
		((VisualElement)m_btnStart).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((count != 0) ? 1 : 0));
		((VisualElement)m_btnContinue).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(count == 0));
		((VisualElement)m_btnRestart).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(count == 0));
		RefreshDailyActive();
		RefreshNotify();
	}

	private void RefreshDailyActive(object data = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp((float)zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active / 80f, 0f, 1f);
		m_progressDailyActive.style.width = StyleLength.op_Implicit(new Length(num * 540f, (LengthUnit)0));
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.11084"), 80f);
		((TextElement)m_lbDailyActive).text = zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active + "/" + 80f;
		string resName = (((float)zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active >= 80f) ? "ui/image/button/btn_280_80_orange.png" : "ui/image/button/btnGray_250x80.png");
		((VisualElement)m_btnStart).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
	}

	private void RefreshNotify()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.operateHandler.HasFusionTargetNotify(isHoliday: true) || zbSelf.Instance.operateHandler.HasFusionTargetNotify();
		m_notifyAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnGoto()
	{
		zbLinkHelper.OpenTask();
	}

	private void OnStart()
	{
		if ((float)zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active < 80f)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11084"), 80f));
			return;
		}
		zbSelf.Instance.operateHandler.fusionData.Clear();
		zbUIMgr.Instance.ShowFusion();
	}

	private void OnContinue()
	{
		zbUIMgr.Instance.ShowFusion();
	}

	private void OnRestart()
	{
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10325"), delegate(bool result)
		{
			if (result)
			{
				OnStart();
			}
		});
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		m_topPanel.RefresLeftTime();
	}

	private void OnAward()
	{
		zbUIMgr.Instance.ShowFusionAward();
	}

	private void OnRank()
	{
		zbWsMsgHandler.Instance.Operate_FusionFriendsRank_Req();
	}

	public void OnUpdateEvent(object obj)
	{
		m_topPanel.Refresh();
		RefreshNotify();
	}
}
