using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbMoneyExchangeWnd : zbBaseWnd
{
	private zbConsumeTargetItem m_consumeTarget;

	private zbItem m_item;

	private Label m_lbCount;

	private Label m_lbNum;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Button m_btnExchange;

	private Label m_lbCost;

	private Button m_btnClose;

	private long m_numSel = 1L;

	private long m_numMin = 1L;

	private long m_numMax = 100L;

	private zbMoneyExchangeCsvRecord m_record;

	private int m_costMoneyType;

	private long m_costAll;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_consumeTarget = UQueryExtensions.Q<zbConsumeTargetItem>(m_RootElement, "consumeTarget", (string)null);
		m_consumeTarget.OnEnable(this);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_btnExchange = UQueryExtensions.Q<Button>(m_RootElement, "btn_exchange", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action)OnExchange, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_ExchangeMoney, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, delegate
		{
			RefreshOperate();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			RefreshOperate();
		});
	}

	public void RefreshCustom(zbMoneyExchangeCsvRecord record)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		RefreshOperate();
		m_record = record;
		m_costMoneyType = record.costId;
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, record.costId);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val != (Object)null)
		{
			((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			((VisualElement)m_lbCost).MarkDirtyRepaint();
		}
		CreateMoneyPanel(new EMoneyType[1] { (EMoneyType)record.costId });
		int dailyLimit = GetDailyLimit();
		int exchangeCount = GetExchangeCount();
		m_numMax = dailyLimit - exchangeCount;
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10058") + ":" + m_numMax + "/" + ((m_record.id == 4) ? zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.TiliBuy, zbSelf.Instance.GetVipLevel()) : record.dailyLimit);
		m_item.Refresh(new zbItemData(EResourceType.Money, record.id, record.count));
		m_numSel = 1L;
		RefreshNum();
	}

	private void RefreshNum()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbNum).text = m_numSel.ToString();
		m_costAll = m_record.cost * m_numSel;
		((TextElement)m_lbCost).text = m_costAll.ToString();
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(m_costMoneyType, m_costAll))
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.white);
		}
	}

	private void OnMin()
	{
		if (m_numSel > m_numMin)
		{
			m_numSel = (long)Mathf.Max((float)m_numMin, (float)(m_numSel - 10));
			if (m_numSel > 10 && m_numSel % 10 != 0L)
			{
				m_numSel = Mathf.CeilToInt((float)m_numSel / 10f) * 10;
			}
			RefreshNum();
		}
	}

	private void OnSubtract()
	{
		if (m_numSel > m_numMin)
		{
			m_numSel--;
			RefreshNum();
		}
	}

	private void OnAdd()
	{
		if (m_numSel < m_numMax)
		{
			m_numSel++;
			RefreshNum();
		}
	}

	private void OnMax()
	{
		if (m_numSel < m_numMax)
		{
			m_numSel = (long)Mathf.Min((float)m_numMax, (float)(m_numSel + 10));
			if (m_numSel > 10 && m_numSel % 10 != 0L && m_numSel / 10 != m_numMax / 10)
			{
				m_numSel = m_numSel / 10 * 10;
			}
			RefreshNum();
		}
	}

	private void OnExchange()
	{
		if (m_numSel > m_numMax)
		{
			if (m_record.id == 4)
			{
				zbUIMgr.Instance.ShowVipTequanWnd();
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10283"));
			}
		}
		else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(m_costMoneyType, m_costAll))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, m_costMoneyType, m_costAll);
		}
		else
		{
			zbWsMsgHandler.Instance.Aux_ExchangeMoney_Req((EMoneyType)m_record.id, (int)m_numSel);
		}
	}

	private int GetDailyLimit()
	{
		if (m_record.id == 4)
		{
			return zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.TiliBuy, zbSelf.Instance.GetVipLevel());
		}
		return m_record.dailyLimit;
	}

	private int GetExchangeCount()
	{
		return zbSelf.Instance.moneyHandler.GetMoneyExchangeDetail((EMoneyType)m_record.id)?.Count ?? 0;
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		OperateConsumTargetData consumeTargetDetail = zbSelf.Instance.shopHandler.GetConsumeTargetDetail();
		if (consumeTargetDetail != null)
		{
			RefreshCustom(m_record);
			if (consumeTargetDetail.UUID == "")
			{
				zbWsMsgHandler.Instance.Journey_Data_Req();
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_Data_Req();
			}
		}
		else
		{
			Hide();
		}
	}

	private void RefreshOperate()
	{
		m_consumeTarget.Refresh();
	}
}
