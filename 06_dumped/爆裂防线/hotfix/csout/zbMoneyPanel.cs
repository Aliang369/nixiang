using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMoneyPanel
{
	private struct OtherMoneyCache
	{
		public bool valid;

		public string iconPath;

		public long count;

		public Action<ClickEvent> callback;
	}

	private OtherMoneyCache[] m_otherCaches = new OtherMoneyCache[2];

	private zbBaseWnd m_parentWnd;

	private EMoneyType[] m_moneyList;

	private VisualElement m_bg;

	public VisualElement[] m_moneys = (VisualElement[])(object)new VisualElement[2];

	public VisualElement[] m_spriteIcons = (VisualElement[])(object)new VisualElement[2];

	public Label[] m_lbCounts = (Label[])(object)new Label[2];

	public Button[] m_btnTouchs = (Button[])(object)new Button[2];

	public VisualElement[] m_spriteOffs = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_rootElement;

	private Action<ClickEvent>[] m_touchCallbacks = new Action<ClickEvent>[2];

	public zbMoneyPanel(zbBaseWnd parentWnd)
	{
		m_parentWnd = parentWnd;
		VisualElement val = (VisualElement)(object)zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/money.uxml").Instantiate();
		m_rootElement = UQueryExtensions.Q<VisualElement>(val, "moneypnl_root", (string)null);
		parentWnd.GetRootElement().Add(m_rootElement);
		OnEnable();
		zbFontReplacer.Instance.ReplaceFontInUI(m_rootElement);
	}

	public void OnEnable()
	{
		m_bg = UQueryExtensions.Q<VisualElement>(m_rootElement, "bg", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_moneys[i] = UQueryExtensions.Q<VisualElement>(m_rootElement, "money" + i, (string)null);
			m_spriteIcons[i] = UQueryExtensions.Q<VisualElement>(m_moneys[i], "spriteIcon", (string)null);
			m_lbCounts[i] = UQueryExtensions.Q<Label>(m_moneys[i], "lbCount", (string)null);
			m_btnTouchs[i] = UQueryExtensions.Q<Button>(m_moneys[i], "btnTouch", (string)null);
			m_spriteOffs[i] = UQueryExtensions.Q<VisualElement>(m_moneys[i], "off", (string)null);
			int currentIndex = i;
			m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchs[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnTouchs(evt, currentIndex);
			}, "", (TrickleDown)0);
		}
		m_parentWnd.m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdateMoney);
	}

	public void Refresh(EMoneyType[] moneylist, bool isBgVis = false)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		m_touchCallbacks[0] = null;
		m_touchCallbacks[1] = null;
		m_moneyList = moneylist;
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isBgVis));
		for (int i = 0; i < 2; i++)
		{
			if (i < m_moneyList.Length)
			{
				m_moneys[i].visible = true;
				RefreshItem(i);
			}
			else
			{
				m_moneys[i].visible = false;
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (j >= m_moneyList.Length && m_otherCaches[j].valid)
			{
				ApplyOther(j);
			}
		}
	}

	public void RefreshOther(int index, string iconPath, long count, Action<ClickEvent> callback)
	{
		if (m_moneys[index] != null)
		{
			m_otherCaches[index] = new OtherMoneyCache
			{
				valid = true,
				iconPath = iconPath,
				count = count,
				callback = callback
			};
			ApplyOther(index);
		}
	}

	private void ApplyOther(int index)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		OtherMoneyCache otherMoneyCache = m_otherCaches[index];
		if (otherMoneyCache.valid)
		{
			m_touchCallbacks[index] = otherMoneyCache.callback;
			m_moneys[index].visible = true;
			m_spriteIcons[index].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(otherMoneyCache.iconPath));
			((TextElement)m_lbCounts[index]).text = zbUtil.ConvertToTBMK(otherMoneyCache.count);
			m_spriteOffs[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshItem(int index)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		zbMoneyCsvRecord recordByKey = zbDataConfig.Instance.moneyCsvTable.GetRecordByKey((int)m_moneyList[index]);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
		m_spriteIcons[index].style.backgroundImage = new StyleBackground(val);
		long countByTid = zbSelf.Instance.moneyHandler.GetCountByTid((int)m_moneyList[index]);
		if (m_moneyList[index] == EMoneyType.Tili)
		{
			((TextElement)m_lbCounts[index]).text = zbUtil.ConvertToTBMK(countByTid) + $"/{120}";
		}
		else
		{
			((TextElement)m_lbCounts[index]).text = zbUtil.ConvertToTBMK(countByTid);
		}
		if (m_moneyList[index] == EMoneyType.BarNormal || m_moneyList[index] == EMoneyType.BarSpecial)
		{
			bool flag = zbSelf.Instance.moneyHandler.IsMoneyExchangeHalf(m_moneyList[index]);
			m_spriteOffs[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
		else
		{
			m_spriteOffs[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnTouchs(ClickEvent evt, int index)
	{
		if (m_touchCallbacks[index] != null)
		{
			m_touchCallbacks[index](evt);
		}
		else if (m_moneyList[index] == EMoneyType.Diamond)
		{
			zbUIMgr.Instance.ShowVip(2);
		}
		else if (!zbUIMgr.Instance.IsShow(EWndId.TipCommon1))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, (long)m_moneyList[index], 0L);
		}
	}

	private void OnEventUpdateMoney(object data)
	{
		Refresh(m_moneyList);
	}

	public void SetDisplayStyle(DisplayStyle displayStyle)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_rootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit(displayStyle);
	}
}
