using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbTipCommonExchangeItem : zbViewBaseItem
{
	private zbTipCommonWnd m_wndParent;

	private Label m_lbName;

	private Label m_lbCount;

	private zbItem m_item;

	private Button m_btnExchange;

	private Label m_lbCost;

	private zbMoneyExchangeCsvRecord m_record;

	public zbTipCommonExchangeItem(zbTipCommonWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tip/tb_tipCommon_exchange.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_btnExchange = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_exchange", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_cost", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action<ClickEvent>)OnExchange, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbMoneyExchangeCsvRecord record)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_item.Refresh(new zbItemData(EResourceType.Money, record.id, record.count), isNameVis: false, isCountVis: false, isTouchVis: false);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, record.id);
		((TextElement)m_lbName).text = resorceIconPath.strName + "X" + record.count;
		int dailyLimit = GetDailyLimit();
		int exchangeCount = GetExchangeCount();
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10048") + ":" + (dailyLimit - exchangeCount) + "/" + dailyLimit;
		if (zbSelf.Instance.moneyHandler.IsMoneyExchangeHalf((EMoneyType)m_record.id) && exchangeCount <= 0)
		{
			((TextElement)m_lbCount).text = zbUtil.Language("lang.10049");
		}
		int num = record.cost;
		if (zbSelf.Instance.moneyHandler.IsMoneyExchangeHalf((EMoneyType)m_record.id))
		{
			num = record.cost / 2;
		}
		((TextElement)m_lbCost).text = num.ToString();
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(m_record.costId, num))
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.white);
		}
		stResorceIconInfo resorceIconPath2 = zbResourceHelper.GetResorceIconPath(EResourceType.Money, record.costId);
		((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath2.strIconPath));
	}

	public void OnExchange(ClickEvent evt)
	{
		int dailyLimit = GetDailyLimit();
		if (GetExchangeCount() >= dailyLimit)
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
		else if (zbSelf.Instance.moneyHandler.IsMoneyExchangeHalf((EMoneyType)m_record.id))
		{
			zbWsMsgHandler.Instance.Aux_ExchangeHalf_Req((EMoneyType)m_record.id);
		}
		else if (m_record.batch == 1)
		{
			zbUIMgr.Instance.ShowMoneyExchange(m_record);
		}
		else
		{
			zbWsMsgHandler.Instance.Aux_ExchangeMoney_Req((EMoneyType)m_record.id, 1);
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
}
