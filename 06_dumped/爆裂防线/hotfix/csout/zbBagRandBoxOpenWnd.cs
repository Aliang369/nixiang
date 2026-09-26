using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBagRandBoxOpenWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private zbItem m_item;

	private Label m_lbHave;

	private Label m_lbNum;

	private Label m_lbName;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Label m_lbBatchLimit;

	private Button m_btnOpen;

	private Button m_btnKeyOpen;

	private Label m_lbKey;

	private Button m_btnClose;

	private VisualElement m_nodeGift;

	private zbBagRandBoxOpenGiftPanel m_giftPanel = new zbBagRandBoxOpenGiftPanel();

	private long m_numSel = 1L;

	private long m_numMin = 1L;

	private long m_numMax = 100L;

	private long m_numStep = 10L;

	private zbItemData m_data;

	private int m_keyId;

	private long m_keyCount;

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
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_lbBatchLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_batchLimit", (string)null);
		m_btnOpen = UQueryExtensions.Q<Button>(m_RootElement, "btn_open", (string)null);
		m_btnKeyOpen = UQueryExtensions.Q<Button>(m_RootElement, "btn_keyOpen", (string)null);
		m_lbKey = UQueryExtensions.Q<Label>(m_RootElement, "lb_key", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_nodeGift = UQueryExtensions.Q<VisualElement>(m_RootElement, "gift", (string)null);
		m_giftPanel.OnEnable(this, m_nodeGift);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOpen, (Action)OnOpen, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnKeyOpen, (Action)OnOpen, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Box, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_ExchangeMoney, OnEventUpdate);
	}

	public void RefreshCustom(zbItemData data, bool isFirstIn = true)
	{
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		m_keyId = 0;
		CreateMoneyPanel(new EMoneyType[0]);
		m_data = data;
		zbBoxCsvRecord recordByKey = zbDataConfig.Instance.boxCsvTable.GetRecordByKey((int)m_data.id);
		if (isFirstIn)
		{
			if (recordByKey != null)
			{
				m_numSel = Math.Min(m_data.count, recordByKey.batchLimit);
			}
			else
			{
				m_numSel = 1L;
			}
		}
		else
		{
			m_numSel = Math.Min(Math.Max(1L, m_numSel), m_data.count);
		}
		((TextElement)m_lbHave).text = zbUtil.Language("lang.10019") + ":" + m_data.count;
		m_item.Refresh(m_data);
		if (recordByKey != null)
		{
			((TextElement)m_lbBatchLimit).text = string.Format(zbUtil.Language("lang.11036"), recordByKey.batchLimit);
			m_numMax = Math.Min(m_data.count, recordByKey.batchLimit);
			m_numStep = recordByKey.batchStep;
			((TextElement)m_lbName).text = zbUtil.Language(recordByKey.name);
			((VisualElement)m_btnOpen).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(recordByKey.key > 0));
			((VisualElement)m_btnKeyOpen).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(recordByKey.key <= 0));
			if (recordByKey.key > 0)
			{
				m_keyId = recordByKey.key;
				m_keyCount = zbSelf.Instance.itemHandler.GetCountByItemTid(recordByKey.key);
				m_numSel = Math.Min(Math.Max(1L, m_keyCount), m_numSel);
				stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Item, recordByKey.key);
				((VisualElement)m_lbKey).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
				m_moneyPanel.RefreshOther(1, resorceIconPath.strIconPath, m_keyCount, delegate
				{
					zbUIMgr.Instance.ShowTip(EResourceType.Item, m_keyId, 0L);
				});
			}
		}
		else
		{
			((TextElement)m_lbBatchLimit).text = "";
		}
		RefreshNum();
	}

	private void RefreshNum()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbNum).text = m_numSel.ToString();
		((TextElement)m_lbKey).text = "X" + m_numSel;
		if (m_keyId <= 0)
		{
			m_nodeGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		if (zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, m_keyId, (int)m_numSel))
		{
			((VisualElement)m_lbKey).style.color = StyleColor.op_Implicit(Color.white);
			m_nodeGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		((VisualElement)m_lbKey).style.color = StyleColor.op_Implicit(Color.red);
		zbItemCsvRecord recordByKey = zbDataConfig.Instance.itemCsvTable.GetRecordByKey(m_keyId);
		if (recordByKey != null && recordByKey.quickBuy.Length == 2 && zbSelf.Instance.moneyHandler.GetCountByTid(3) > 0)
		{
			m_nodeGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_giftPanel.Refresh(m_keyId, m_numSel - m_keyCount);
		}
		else
		{
			m_nodeGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnMin()
	{
		if (m_numSel > m_numMin)
		{
			m_numSel = (long)Mathf.Max((float)m_numMin, (float)(m_numSel - m_numStep));
			if (m_numSel > m_numStep && m_numSel % m_numStep != 0L)
			{
				m_numSel = Mathf.CeilToInt((float)(m_numSel / m_numStep)) * m_numStep;
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
			m_numSel = (long)Mathf.Min((float)m_numMax, (float)(m_numSel + m_numStep));
			if (m_numSel > m_numStep && m_numSel % m_numStep != 0L && m_numSel / m_numStep != m_numMax / m_numStep)
			{
				m_numSel = m_numSel / m_numStep * m_numStep;
			}
			RefreshNum();
		}
	}

	private void OnOpen()
	{
		if (m_keyId > 0)
		{
			if (m_keyCount < m_numSel)
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Item, m_keyId, m_numSel - m_keyCount);
				return;
			}
			zbUIMgr.Instance.ShowBoxKeyOpen(new zbItemData(EResourceType.Item, m_keyId, m_numSel), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Bag_RandBoxOpen_Req((int)m_data.id, (int)m_numSel);
				}
			});
		}
		else
		{
			zbWsMsgHandler.Instance.Bag_RandBoxOpen_Req((int)m_data.id, (int)m_numSel);
		}
	}

	private void OnClose()
	{
		m_numSel = 100L;
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Box, (int)m_data.id);
		if (resourceCount > 0)
		{
			RefreshCustom(new zbItemData(EResourceType.Box, m_data.id, resourceCount), isFirstIn: false);
		}
		else
		{
			OnClose();
		}
	}
}
