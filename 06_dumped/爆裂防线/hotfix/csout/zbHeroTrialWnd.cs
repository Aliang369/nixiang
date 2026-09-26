using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbHeroTrialWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private zbItem[] m_awards = new zbItem[4];

	private Label m_lbTime;

	private Button m_btnConfirm;

	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"award{i}", (string)null);
			m_awards[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnFight, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
		if (zbServerData.sGameData.HeroTrialInfo != null)
		{
			InitTrial();
		}
	}

	private void InitTrial()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		HeroTrialInfo heroTrialInfo = zbServerData.sGameData.HeroTrialInfo;
		zbHeroTrialCsvRecord recordByKey = zbDataConfig.Instance.heroTrialCsvTable.GetRecordByKey((int)heroTrialInfo.CurrentType);
		((TextElement)m_lbTitle).text = zbUtil.Language(recordByKey.name);
		((TextElement)m_lbDesc).text = zbUtil.Language(recordByKey.desc);
		((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS((int)(zbServerData.sGameData.HeroTrialInfo.ShowEndTm - zbSelf.Instance.timeHandler.GetServerTime()));
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < recordByKey.awardShow.Length;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)recordByKey.awardShow[i][0], recordByKey.awardShow[i][1], recordByKey.awardShow[i][2]);
				m_awards[i].Refresh(data);
			}
		}
	}

	private void OnFight()
	{
		if (IsInDuration())
		{
			zbWsMsgHandler.Instance.HeroTrial_FightStart_Req(zbServerData.sGameData.HeroTrialInfo.CurrentType, zbServerData.sGameData.HeroTrialInfo.CurrentLevel);
		}
		else
		{
			Hide();
		}
	}

	private bool IsInDuration()
	{
		HeroTrialInfo heroTrialInfo = zbServerData.sGameData.HeroTrialInfo;
		if (heroTrialInfo == null)
		{
			return false;
		}
		if (heroTrialInfo.DailyCount > 0 && zbSelf.Instance.timeHandler.GetServerTime() < heroTrialInfo.ShowEndTm)
		{
			return true;
		}
		return false;
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (IsInDuration())
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS((int)(zbServerData.sGameData.HeroTrialInfo.ShowEndTm - zbSelf.Instance.timeHandler.GetServerTime()));
		}
		else
		{
			Hide();
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
