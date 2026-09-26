using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbShopConfirmWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private zbItem m_item;

	private Label m_lbName;

	private Label m_lbHave;

	private Label m_lbDesc;

	private VisualElement m_iconOff;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Label m_lbNum;

	private Button m_btnBuy;

	private Label m_lbCost;

	private Button m_btnClose;

	private int m_numSel = 1;

	private int m_numMin = 1;

	private int m_numMax = 100;

	private int m_numTen = 10;

	private EShopType m_shopType;

	private ShopSlotDetail m_data;

	private stResorceIconInfo m_itemInfo;

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
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbHave = UQueryExtensions.Q<Label>(m_RootElement, "lb_have", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_iconOff = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_off", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Shop, OnEventUpdateShop);
	}

	public void RefreshCustom(EShopType shopType, ShopSlotDetail data)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		m_shopType = shopType;
		m_data = data;
		m_numSel = 1;
		m_numMax = ((m_data.Limit <= 0) ? 100 : (m_data.Limit - m_data.Count));
		int offByItemId = zbSelf.Instance.shopHandler.GetOffByItemId(data.Index, shopType);
		bool flag = offByItemId > 0;
		m_iconOff.visible = flag;
		if (flag)
		{
			m_iconOff.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/operate/off/off{offByItemId}.png"));
		}
		m_item.Refresh(new zbItemData(m_data.SlotItem.Type, m_data.SlotItem.Tid, m_data.SlotItem.Count));
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, (long)m_data.MoneyType);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val != (Object)null)
		{
			((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			((VisualElement)m_lbCost).MarkDirtyRepaint();
		}
		m_itemInfo = zbResourceHelper.GetResorceIconPath(m_data.SlotItem.Type, m_data.SlotItem.Tid);
		((TextElement)m_lbName).text = m_itemInfo.strName;
		((TextElement)m_lbDesc).text = m_itemInfo.strDesc;
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(m_data.SlotItem.Type, (int)m_data.SlotItem.Tid);
		((TextElement)m_lbHave).text = zbUtil.Language("lang.10019") + ": " + resourceCount;
		RefreshNum();
	}

	private void RefreshNum()
	{
		((TextElement)m_lbName).text = m_itemInfo.strName + "X" + m_numSel * m_data.SlotItem.Count;
		if (m_data.Limit <= 0)
		{
			((TextElement)m_lbNum).text = m_numSel.ToString();
		}
		else
		{
			((TextElement)m_lbNum).text = m_numSel + "/" + m_numMax;
		}
		((TextElement)m_lbCost).text = (m_data.Price * m_numSel).ToString();
	}

	private void OnMin()
	{
		int num = m_numSel - m_numTen;
		if (num < m_numMin)
		{
			m_numSel = m_numMin;
		}
		else
		{
			m_numSel = num;
		}
		RefreshNum();
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
		int num = m_numSel + m_numTen;
		if (num > m_numMax)
		{
			m_numSel = m_numMax;
		}
		else
		{
			m_numSel = num;
		}
		RefreshNum();
	}

	private void OnBuy()
	{
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney((int)m_data.MoneyType, m_data.Price * m_numSel))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, (long)m_data.MoneyType, m_data.Price * m_numSel);
		}
		else
		{
			zbWsMsgHandler.Instance.Shop_Buy_Req(m_shopType, m_data.Index, m_numSel);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdateShop(object obj)
	{
		Hide();
	}
}
