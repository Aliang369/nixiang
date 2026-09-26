using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbTipCommonWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private VisualElement m_spriteFrame;

	private zbItem m_item;

	private Label m_lbName;

	private Label m_lbHave;

	private Label m_lbType;

	private Button m_btnPreview;

	private Label m_lbDesc;

	private Label m_nodeAttr;

	private Label m_lbAttrDesc;

	private Label m_lbAttr;

	private Label m_lbValue;

	private ScrollView m_scrollView;

	private Label m_lbGetWay;

	private VisualElement m_dynamicContainer;

	private Label m_lbBox;

	private VisualElement m_dynamicBox;

	private ScrollView m_boxScrollView;

	private VisualElement m_nodeGift;

	private zbTipCommonGiftPanel m_giftPanel = new zbTipCommonGiftPanel();

	private VisualElement m_nodeBuy;

	private zbTipCommonBuyPanel m_buyPanel = new zbTipCommonBuyPanel();

	public EResourceType m_type;

	public long m_id;

	public long m_needCount;

	private zbBoxCsvRecord m_boxRecord;

	private int m_allWeight;

	private zbTipCommonConvertItem m_itemCovert;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
		m_itemCovert = new zbTipCommonConvertItem(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_lbGetWay = UQueryExtensions.Q<Label>((VisualElement)(object)m_scrollView, "lb_getWay", (string)null);
		m_dynamicContainer = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "dynamicContainer", (string)null);
		m_lbBox = UQueryExtensions.Q<Label>((VisualElement)(object)m_scrollView, "lb_box", (string)null);
		m_dynamicBox = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "dynamicBox", (string)null);
		m_boxScrollView = UQueryExtensions.Q<ScrollView>(m_dynamicBox, "boxScrollView", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbHave = UQueryExtensions.Q<Label>(m_RootElement, "lb_have", (string)null);
		m_lbType = UQueryExtensions.Q<Label>(m_RootElement, "lb_type", (string)null);
		m_btnPreview = UQueryExtensions.Q<Button>(m_RootElement, "btn_preview", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_nodeAttr = UQueryExtensions.Q<Label>(m_RootElement, "node_attr", (string)null);
		m_lbAttrDesc = UQueryExtensions.Q<Label>((VisualElement)(object)m_nodeAttr, "lb_attrDesc", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>((VisualElement)(object)m_nodeAttr, "lb_attr", (string)null);
		m_lbValue = UQueryExtensions.Q<Label>((VisualElement)(object)m_nodeAttr, "lb_value", (string)null);
		m_nodeGift = UQueryExtensions.Q<VisualElement>(m_RootElement, "gift", (string)null);
		m_giftPanel.OnEnable(this, m_nodeGift);
		m_nodeBuy = UQueryExtensions.Q<VisualElement>(m_RootElement, "buy", (string)null);
		m_buyPanel.OnEnable(this, m_nodeBuy);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPreview, (Action<ClickEvent>)OnPreview, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Box, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_ExchangeMoney, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
		AddDark();
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Voucher });
	}

	private void OnEventUpdate(object data)
	{
		if (m_needCount > 0 && zbSelf.Instance.dataHandler.GetResourceCount(m_type, (int)m_id) > m_needCount)
		{
			OnTouchTest(null);
		}
		else
		{
			RefreshCustom(m_type, m_id, m_needCount);
		}
	}

	public void RefreshCustom(EResourceType type, long id, long needCount)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		m_dynamicContainer.Clear();
		m_type = type;
		m_id = id;
		m_needCount = needCount;
		((VisualElement)m_btnPreview).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.dataHandler.GetPreviewType(m_type, (int)m_id) <= EPreviewType.EmptyValue));
		zbResourceHelper.RefreshResourceBg(m_type, id, m_spriteFrame);
		m_item.Refresh(new zbItemData(m_type, id, 0L), isNameVis: false, isCountVis: false, isTouchVis: false);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(type, m_id);
		((TextElement)m_lbName).text = resorceIconPath.strName;
		if (m_type == EResourceType.HeroFrag || m_type == EResourceType.Hero)
		{
			long composeCount = zbSelf.Instance.heroHandler.GetComposeCount((int)m_id);
			((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10288"), composeCount, resorceIconPath.strName);
		}
		else
		{
			((TextElement)m_lbDesc).text = resorceIconPath.strDesc + zbSelf.Instance.itemHandler.GetStarMaxDesc((int)id);
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(m_type, (int)m_id);
		if (m_needCount > 0 && resourceCount < m_needCount && CanQuickBuy() && zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.QuickBuy, zbSelf.Instance.GetVipLevel()) > 0)
		{
			((TextElement)m_lbHave).text = zbUtil.Language("lang.10019") + ": " + zbUtil.GetNeedHaveRichString(resourceCount, m_needCount);
			m_nodeGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_giftPanel.Refresh(m_needCount - resourceCount);
			m_moneyPanel.SetDisplayStyle((DisplayStyle)0);
		}
		else
		{
			((TextElement)m_lbHave).text = zbUtil.Language("lang.10019") + ": " + resourceCount;
			m_nodeGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_moneyPanel.SetDisplayStyle((DisplayStyle)1);
		}
		if (m_nodeGift.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			zbGetWayBuyPanelData getWayBuyPanelData = zbSelf.Instance.getWayHandler.GetGetWayBuyPanelData(m_type, (int)m_id);
			if (getWayBuyPanelData != null)
			{
				m_nodeBuy.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_buyPanel.Refresh(getWayBuyPanelData);
				m_moneyPanel.Refresh(new EMoneyType[1] { EMoneyType.Diamond }, isBgVis: true);
				m_moneyPanel.SetDisplayStyle((DisplayStyle)0);
			}
			else
			{
				m_nodeBuy.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_moneyPanel.SetDisplayStyle((DisplayStyle)1);
			}
		}
		else
		{
			m_nodeBuy.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_moneyPanel.SetDisplayStyle((DisplayStyle)1);
		}
		if (m_type == EResourceType.Money)
		{
			RefreshGetWayExchangeItem();
		}
		else if (m_type == EResourceType.Prop)
		{
			RefreshGetWayPropBuyItem();
		}
		RefreshGetWayConvertItem();
		RefreshAttr();
		RefreshGetWayBoxItems();
		RefreshBoxPreview();
		RefreshGetWayItems();
		ChangeTypeByLang();
		((VisualElement)m_lbGetWay).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_dynamicContainer.childCount <= 0));
	}

	private bool CanQuickBuy()
	{
		if (m_type == EResourceType.Money)
		{
			zbMoneyCsvRecord recordByKey = zbDataConfig.Instance.moneyCsvTable.GetRecordByKey((int)m_id);
			if (recordByKey == null)
			{
				zbWsMsgHandler.Instance.Abnormal_ClientReport_Req(EFuncType.MainQuest, m_id, m_id, zbSdkHelper.GetResVersion(), "EResourceType.Money");
				return false;
			}
			if (recordByKey.quickBuy == null || recordByKey.quickBuy.Length != 2)
			{
				return m_id == 3;
			}
			return true;
		}
		if (m_type == EResourceType.Item)
		{
			zbItemCsvRecord recordByKey2 = zbDataConfig.Instance.itemCsvTable.GetRecordByKey((int)m_id);
			if (recordByKey2 == null)
			{
				zbWsMsgHandler.Instance.Abnormal_ClientReport_Req(EFuncType.MainQuest, m_id, m_id, zbSdkHelper.GetResVersion(), "EResourceType.Item");
				return false;
			}
			if (recordByKey2.quickBuy != null)
			{
				return recordByKey2.quickBuy.Length == 2;
			}
			return false;
		}
		return false;
	}

	private void RefreshAttr()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_nodeAttr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_type != EResourceType.Prop)
		{
			return;
		}
		zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)m_id);
		if (recordByKey != null && (recordByKey.type == 8 || recordByKey.type == 9 || recordByKey.type == 12 || recordByKey.type == 14))
		{
			if (recordByKey.type == 8)
			{
				zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord record) => record.propId == m_id);
				zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(zbPlayerSuitCsvRecord2.id, 1);
				((TextElement)m_lbAttr).text = zbUtil.Language("lang.10798");
				((TextElement)m_lbValue).text = "+" + suitStarUpgrade.atk;
			}
			else if (recordByKey.type == 9)
			{
				zbPlayerGunCsvRecord zbPlayerGunCsvRecord2 = zbDataConfig.Instance.playerGunCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerGunCsvRecord record) => record.propId == m_id);
				zbPlayerGunUpgradeCsvRecord gunStarUpgrade = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(zbPlayerGunCsvRecord2.id, 1);
				((TextElement)m_lbAttr).text = zbUtil.Language("lang.10590");
				((TextElement)m_lbValue).text = "+" + gunStarUpgrade.dmg * 100f + "%";
			}
			else if (recordByKey.type == 12)
			{
				zbMechaStarUpgradeCsvRecord recordByKey2 = zbDataConfig.Instance.mechaStarUpgradeCsvTable.GetRecordByKey(1);
				((TextElement)m_lbAttr).text = zbUtil.Language("lang.10798");
				((TextElement)m_lbValue).text = "+" + recordByKey2.attack;
			}
			else if (recordByKey.type == 14)
			{
				zbFootholdCsvRecord zbFootholdCsvRecord2 = zbDataConfig.Instance.footholdCsvTable.GetRecords().records.Values.FirstOrDefault((zbFootholdCsvRecord record) => record.propId == m_id);
				zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(zbFootholdCsvRecord2.id, 1);
				((TextElement)m_lbAttr).text = zbUtil.Language("lang.10862");
				((TextElement)m_lbValue).text = $"+{footholdStarUpgradeRecord.dmg:0.##%}";
			}
			((VisualElement)m_nodeAttr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbAttrDesc).text = zbUtil.Language("lang.11340");
			return;
		}
		foreach (zbTreasureCsvRecord value in zbDataConfig.Instance.treasureCsvTable.GetRecords().records.Values)
		{
			if (value.propId == m_id)
			{
				((VisualElement)m_nodeAttr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbAttrDesc).text = zbUtil.Language("lang.10534");
				((TextElement)m_lbAttr).text = zbAttrHelper.GetTeamAttrName((int)value.attr[0]);
				zbTeamAttrCsvRecord recordByKey3 = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)value.attr[0]);
				((TextElement)m_lbValue).text = zbAttrHelper.FormatAttrValue(value.attr[1], recordByKey3.showPercent == 1, isDisplayAdd: true);
				break;
			}
		}
	}

	private void RefreshGetWayBoxItems()
	{
		List<zbBoxCsvRecord> wayBoxs = zbSelf.Instance.getWayHandler.GetWayBoxs(m_type, m_id, isBoxCross: false);
		for (int i = 0; i < wayBoxs.Count; i++)
		{
			zbTipCommonBoxItem zbTipCommonBoxItem2 = new zbTipCommonBoxItem(this);
			if (zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Box, wayBoxs[i].id) > 0)
			{
				zbTipCommonBoxItem2.Refresh(wayBoxs[i]);
				m_dynamicContainer.Add((VisualElement)(object)zbTipCommonBoxItem2);
			}
		}
	}

	private void RefreshBoxPreview()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		if (m_type != EResourceType.Box)
		{
			((VisualElement)m_lbBox).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_dynamicBox.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_boxRecord = zbDataConfig.Instance.boxCsvTable.GetRecordByKey((int)m_id);
		if (m_boxRecord == null || m_boxRecord.boxShow == 0)
		{
			((VisualElement)m_lbBox).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_dynamicBox.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		GetPidAllWeight();
		((VisualElement)m_lbBox).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_dynamicBox.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_boxScrollView).Clear();
		zbRandBoxCsvRecord[] records = zbDataConfig.Instance.randBoxCsvTable.GetRecords().records;
		foreach (zbRandBoxCsvRecord zbRandBoxCsvRecord2 in records)
		{
			if (zbRandBoxCsvRecord2.pid == m_boxRecord.box)
			{
				VisualElement val = new VisualElement();
				val.style.width = StyleLength.op_Implicit(90f);
				val.style.height = StyleLength.op_Implicit(110f);
				val.style.flexShrink = StyleFloat.op_Implicit(0f);
				val.style.marginRight = StyleLength.op_Implicit(4f);
				zbItem zbItem2 = new zbItem();
				zbItem2.OnEnable(this);
				((VisualElement)zbItem2).style.scale = StyleScale.op_Implicit(new Scale(new Vector2(0.7f, 0.7f)));
				val.Add((VisualElement)(object)zbItem2);
				((VisualElement)m_boxScrollView).Add(val);
				zbItemData data = new zbItemData((EResourceType)zbRandBoxCsvRecord2.type, zbRandBoxCsvRecord2.id, zbRandBoxCsvRecord2.count);
				zbItem2.Refresh(data);
			}
		}
	}

	private string GetItemRate(zbItemData itemData)
	{
		zbRandBoxCsvRecord[] records = zbDataConfig.Instance.randBoxCsvTable.GetRecords().records;
		foreach (zbRandBoxCsvRecord zbRandBoxCsvRecord2 in records)
		{
			if (m_boxRecord.box == zbRandBoxCsvRecord2.pid && zbRandBoxCsvRecord2.type == (int)itemData.type && zbRandBoxCsvRecord2.id == itemData.id && zbRandBoxCsvRecord2.count == itemData.count)
			{
				return ((double)zbRandBoxCsvRecord2.weight / (double)m_allWeight * 100.0).ToString("F2") + "%";
			}
		}
		return "";
	}

	private void GetPidAllWeight()
	{
		m_allWeight = 0;
		zbRandBoxCsvRecord[] records = zbDataConfig.Instance.randBoxCsvTable.GetRecords().records;
		foreach (zbRandBoxCsvRecord zbRandBoxCsvRecord2 in records)
		{
			if (m_boxRecord.box == zbRandBoxCsvRecord2.pid)
			{
				m_allWeight += zbRandBoxCsvRecord2.weight;
			}
		}
	}

	private void RefreshGetWayItems()
	{
		List<int> list = new List<int>();
		foreach (zbBoxCsvRecord wayBox in zbSelf.Instance.getWayHandler.GetWayBoxs(m_type, m_id, isBoxCross: true))
		{
			list.AddRange(zbSelf.Instance.getWayHandler.GetWayTypes(EResourceType.Box, wayBox.id));
		}
		list.AddRange(zbSelf.Instance.getWayHandler.GetWayTypes(m_type, m_id));
		list = list.Distinct().ToList();
		if (!zbSelf.Instance.legendaryHandler.IsInZhanlingGetWay(m_type, m_id))
		{
			list.Remove(48);
		}
		for (int i = 0; i < list.Count; i++)
		{
			zbTipCommonItem zbTipCommonItem2 = new zbTipCommonItem(this);
			zbTipCommonItem2.Refresh(list[i]);
			m_dynamicContainer.Add((VisualElement)(object)zbTipCommonItem2);
		}
	}

	private void RefreshGetWayExchangeItem()
	{
		zbMoneyExchangeCsvRecord recordByKey = zbDataConfig.Instance.moneyExchangeCsvTable.GetRecordByKey((int)m_id);
		if (recordByKey != null)
		{
			zbTipCommonExchangeItem zbTipCommonExchangeItem2 = new zbTipCommonExchangeItem(this);
			zbTipCommonExchangeItem2.Refresh(recordByKey);
			m_dynamicContainer.Add((VisualElement)(object)zbTipCommonExchangeItem2);
			if (!CanQuickBuy())
			{
				m_moneyPanel.Refresh(new EMoneyType[2]
				{
					EMoneyType.Diamond,
					(EMoneyType)m_id
				}, isBgVis: true);
			}
		}
	}

	private void RefreshGetWayPropBuyItem()
	{
		if (zbSelf.Instance.endlessMineHandler.endlessMineInfo != null)
		{
			foreach (EndlessMinePropDetail prop in zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.PropList)
			{
				if (m_id == prop.PropId && prop.DailyCount > 0)
				{
					zbTipCommonPropBuyItem zbTipCommonPropBuyItem2 = new zbTipCommonPropBuyItem(this);
					zbTipCommonPropBuyItem2.Refresh(prop.PropId);
					m_dynamicContainer.Add((VisualElement)(object)zbTipCommonPropBuyItem2);
					((TextElement)m_lbHave).text = zbUtil.Language("lang.10019") + ": " + prop.Count;
					m_moneyPanel.Refresh(new EMoneyType[1] { EMoneyType.Diamond }, isBgVis: true);
				}
			}
		}
		SnowVeilDetail snowVeilDetail = zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail();
		if (snowVeilDetail != null && snowVeilDetail.Prop.PropId == m_id)
		{
			zbTipCommonPropBuyItem zbTipCommonPropBuyItem3 = new zbTipCommonPropBuyItem(this);
			zbTipCommonPropBuyItem3.Refresh(snowVeilDetail.Prop.PropId, snowVeilDetail);
			m_dynamicContainer.Add((VisualElement)(object)zbTipCommonPropBuyItem3);
			m_moneyPanel.Refresh(new EMoneyType[1] { EMoneyType.Diamond }, isBgVis: true);
		}
	}

	private void RefreshGetWayConvertItem()
	{
		zbResourceConvertCsvRecord convertRecord = zbDataConfig.Instance.resourceConvertCsvTable.GetRecords().records.Values.FirstOrDefault((zbResourceConvertCsvRecord record) => record.convertType == (int)m_type && record.convertId == m_id);
		if (convertRecord != null && (convertRecord.resType == 1 || convertRecord.resType == 2))
		{
			m_itemCovert.Refresh(convertRecord);
			if (((VisualElement)m_itemCovert).parent != m_dynamicContainer)
			{
				m_dynamicContainer.Add((VisualElement)(object)m_itemCovert);
			}
			if (convertRecord.resType == 1)
			{
				m_moneyPanel.Refresh(new EMoneyType[1] { (EMoneyType)convertRecord.resId }, isBgVis: true);
				return;
			}
			m_moneyPanel.SetDisplayStyle((DisplayStyle)0);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)convertRecord.resType, convertRecord.resId);
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)convertRecord.resType, convertRecord.resId);
			m_moneyPanel.RefreshOther(0, resorceIconPath.strIconPath, resourceCount, delegate
			{
				zbUIMgr.Instance.ShowTip((EResourceType)convertRecord.resType, convertRecord.resId, 0L);
			});
		}
		else if (((VisualElement)m_itemCovert).parent == m_dynamicContainer)
		{
			m_dynamicContainer.Remove((VisualElement)(object)m_itemCovert);
		}
	}

	private void ChangeTypeByLang()
	{
		switch (m_type)
		{
		case EResourceType.Money:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10020");
			break;
		case EResourceType.Prop:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10021");
			break;
		case EResourceType.Item:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10022");
			break;
		case EResourceType.Equip:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10054");
			break;
		case EResourceType.Hero:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10055");
			break;
		case EResourceType.HeroFrag:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10056");
			break;
		case EResourceType.Gem:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10023");
			break;
		case EResourceType.Box:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10024");
			break;
		case (EResourceType)6:
		case (EResourceType)9:
			break;
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}

	private void OnPreview(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowPreview(zbSelf.Instance.dataHandler.GetPreviewType(m_type, (int)m_id), (int)m_id);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		m_buyPanel?.RefreshTime();
	}
}
