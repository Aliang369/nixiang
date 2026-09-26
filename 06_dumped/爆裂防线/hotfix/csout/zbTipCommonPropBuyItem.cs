using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipCommonPropBuyItem : zbViewBaseItem
{
	private zbTipCommonWnd m_wndParent;

	private Label m_lbName;

	private Label m_lbCount;

	private zbItem m_item;

	private Button m_btnExchange;

	private Label m_lbCost;

	private int m_pickaxeCount;

	private int m_pickaxeDiamond;

	private int m_drillCount;

	private int m_drillDiamond;

	private int m_bombCount;

	private int m_bombDiamond;

	private int m_snowVeilPropCount;

	private int m_snowVeilPropDiamond;

	private int m_propId;

	private zbPropCsvRecord m_record;

	private SnowVeilDetail m_snowVeilDetail;

	private int m_dailyLimit;

	private int m_buyCount;

	public zbTipCommonPropBuyItem(zbTipCommonWnd wndParent)
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
		InitRecord();
	}

	private void InitRecord()
	{
		JObject record = zbDataConfig.Instance.endlessMineBaseJsonTable.GetRecord();
		m_pickaxeCount = Extensions.Value<int>((IEnumerable<JToken>)record["pickaxe"][(object)"count"]);
		m_pickaxeDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["pickaxe"][(object)"diamond"]);
		m_drillCount = Extensions.Value<int>((IEnumerable<JToken>)record["drill"][(object)"count"]);
		m_drillDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["drill"][(object)"diamond"]);
		m_bombCount = Extensions.Value<int>((IEnumerable<JToken>)record["bomb"][(object)"count"]);
		m_bombDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["bomb"][(object)"diamond"]);
		JObject record2 = zbDataConfig.Instance.snowVeilBaseJsonTable.GetRecord();
		m_snowVeilPropCount = Extensions.Value<int>((IEnumerable<JToken>)record2["prop"][(object)"count"]);
		m_snowVeilPropDiamond = Extensions.Value<int>((IEnumerable<JToken>)record2["prop"][(object)"diamond"]);
	}

	public void Refresh(int propId, SnowVeilDetail snowVeilDetail = null)
	{
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		m_propId = propId;
		m_record = zbDataConfig.Instance.propCsvTable.GetRecordByKey(m_propId);
		m_snowVeilDetail = snowVeilDetail;
		if (m_snowVeilDetail != null)
		{
			m_dailyLimit = m_snowVeilDetail.Prop.DailyLimit;
			m_buyCount = m_snowVeilDetail.Prop.BuyCount;
		}
		else
		{
			EndlessMinePropDetail endlessMinePropDetail = zbSelf.Instance.endlessMineHandler.GetEndlessMinePropDetail(m_propId);
			if (endlessMinePropDetail != null)
			{
				m_dailyLimit = endlessMinePropDetail.DailyCount;
				m_buyCount = endlessMinePropDetail.BuyCount;
			}
		}
		var (num, num2) = GetCountAndDiamond();
		m_item.Refresh(new zbItemData(EResourceType.Prop, m_record.id, num), isNameVis: false, isCountVis: false);
		((TextElement)m_lbName).text = m_record.name + "X" + num;
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10048") + ":" + (m_dailyLimit - m_buyCount) + "/" + m_dailyLimit;
		((TextElement)m_lbCost).text = num2.ToString();
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, num2))
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.white);
		}
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, 2L);
		((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
	}

	private void OnExchange(ClickEvent evt)
	{
		if (m_buyCount >= m_dailyLimit)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10283"));
			return;
		}
		int item = GetCountAndDiamond().Item2;
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 2, item))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, item);
		}
		else if (m_snowVeilDetail != null)
		{
			zbWsMsgHandler.Instance.Operate_SnowVeilPropBuy_Req(m_snowVeilDetail.BaseInfo.UUID);
		}
		else
		{
			zbWsMsgHandler.Instance.EndlessMine_PropBuy_Req(m_propId);
		}
	}

	private (int, int) GetCountAndDiamond()
	{
		if (m_snowVeilDetail != null)
		{
			return (m_snowVeilPropCount, m_snowVeilPropDiamond);
		}
		if (m_record.type == 3)
		{
			return (m_pickaxeCount, m_pickaxeDiamond);
		}
		if (m_record.type == 4)
		{
			return (m_drillCount, m_drillDiamond);
		}
		if (m_record.type == 5)
		{
			return (m_bombCount, m_bombDiamond);
		}
		return (0, 0);
	}
}
