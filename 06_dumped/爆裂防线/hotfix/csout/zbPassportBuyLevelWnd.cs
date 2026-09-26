using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbPassportBuyLevelWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnBuy;

	private Label m_lbNum;

	private Label m_lbDesc;

	private Label m_lbDesc2;

	private PassportLevelInfo m_levelInfo;

	private int m_num = 1;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbDesc2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc2", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
	}

	private void OnBuy()
	{
		if (CheckCanBuy())
		{
			zbWsMsgHandler.Instance.Passport_BuyLevel_Req(m_levelInfo.BuyLevelList[0].Index, m_num);
		}
		OnClose();
	}

	public override void Refresh()
	{
		base.Refresh();
		m_levelInfo = zbServerData.sGameData.RechargeActiveInfo.PassportInfo.LevelInfo;
		m_num = 1;
		RefreshContent();
	}

	private void OnSubtract()
	{
		ChangeNum(-1);
	}

	private void OnMin()
	{
		ChangeNum(-10);
	}

	private void OnAdd()
	{
		ChangeNum(1);
	}

	private void OnMax()
	{
		ChangeNum(10);
	}

	private bool CheckCanBuy()
	{
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_num * m_levelInfo.BuyLevelList[0].Diamond))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10257"));
			return false;
		}
		return true;
	}

	private void RefreshContent()
	{
		((TextElement)m_lbNum).text = m_num.ToString();
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuy, "lb_cost", (string)null)).text = (m_num * m_levelInfo.BuyLevelList[0].Diamond).ToString();
		((TextElement)m_lbDesc2).text = string.Format(zbUtil.Language("lang.10487"), m_levelInfo.BuyLevelList[0].LimitNum - m_levelInfo.BuyLevelList[0].BuyCount);
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10344"), m_levelInfo.Level + m_num);
	}

	private void ChangeNum(int count)
	{
		int val = m_levelInfo.BuyLevelList[0].LimitNum - m_levelInfo.BuyLevelList[0].BuyCount;
		int val2 = m_levelInfo.BuyLevelList[0].LimitNum - m_levelInfo.Level;
		int num = Math.Min(val, val2);
		m_num += count;
		if (m_num < 1)
		{
			m_num = 1;
		}
		else if (m_num > num)
		{
			m_num = num;
		}
		RefreshContent();
	}

	private void OnClose()
	{
		Hide();
	}
}
