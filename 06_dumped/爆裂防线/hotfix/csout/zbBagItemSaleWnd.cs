using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBagItemSaleWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private zbItem m_item;

	private VisualElement m_spriteMoney;

	private Label m_lbPrice;

	private Label m_lbNum;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Button m_btnSale;

	private Button m_btnClose;

	private long m_numSel;

	private long m_numMin = 1L;

	private long m_numMax = 100L;

	private zbItemData m_data;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_spriteMoney = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_money", (string)null);
		m_lbPrice = UQueryExtensions.Q<Label>(m_RootElement, "lb_price", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_btnSale = UQueryExtensions.Q<Button>(m_RootElement, "btn_sale", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSale, (Action)OnSale, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
		AddDark();
	}

	public void RefreshCustom(zbItemData data)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		long val = 100L;
		int num = 0;
		int num2 = 0;
		if (m_data.type == EResourceType.Item)
		{
			zbItemCsvRecord recordByKey = zbDataConfig.Instance.itemCsvTable.GetRecordByKey((int)m_data.id);
			num = recordByKey.sellMoney;
			num2 = recordByKey.sellPrice;
			if (recordByKey.sellGroupNum > 0)
			{
				val = recordByKey.sellGroupNum;
			}
		}
		else if (m_data.type == EResourceType.Prop)
		{
			zbPropCsvRecord recordByKey2 = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)m_data.id);
			num = recordByKey2.sellMoney;
			num2 = recordByKey2.sellPrice;
			if (recordByKey2.sellGroupNum > 0)
			{
				val = recordByKey2.sellGroupNum;
			}
		}
		m_numMax = Math.Min(m_data.count, val);
		if (m_numSel <= 0)
		{
			m_numSel = m_numMax;
		}
		m_numSel = Math.Min(Math.Max(m_numMin, m_numSel), m_numMax);
		m_item.Refresh(m_data);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, num);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteMoney.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteMoney.MarkDirtyRepaint();
		}
		((TextElement)m_lbPrice).text = num2.ToString();
		RefreshNum();
	}

	private void RefreshNum()
	{
		((TextElement)m_lbNum).text = m_numSel.ToString();
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

	private void OnSale()
	{
		AwardDetail[] saleList = new AwardDetail[1]
		{
			new AwardDetail
			{
				Type = m_data.type,
				Tid = m_data.id,
				Count = m_numSel
			}
		};
		zbWsMsgHandler.Instance.Bag_ItemSale_Req(saleList);
	}

	private void OnClose()
	{
		m_numSel = 0L;
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(m_data.type, (int)m_data.id);
		if (resourceCount > 0)
		{
			RefreshCustom(new zbItemData(m_data.type, m_data.id, resourceCount));
		}
		else
		{
			OnClose();
		}
	}
}
