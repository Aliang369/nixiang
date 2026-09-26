using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBagItemDetailWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private VisualElement m_spriteFrame;

	private zbItem m_item;

	private Label m_lbName;

	private Label m_lbHave;

	private Label m_lbType;

	private Label m_lbDesc;

	private VisualElement m_nodeList;

	private MultiColumnListView m_listView;

	private Button m_btnUse;

	private VisualElement m_nodeOpen;

	private Label m_lbKey;

	private Button m_btnOpen;

	private Button m_btnSale;

	private Button m_btnGetWay;

	private VisualElement m_nodeGene;

	private Label m_lbGeneAttr;

	private Label m_lbGeneValue;

	private Label m_lbGeneDesc;

	private Button m_btnExchange;

	private Button m_btnRemove;

	private Button m_btnUpgrade;

	private VisualElement m_notifyExchange;

	private VisualElement m_notifyUpgrade;

	private zbItemData m_data;

	private ESkillType m_skillType;

	private int m_index;

	private zbBoxCsvRecord m_boxRecord;

	private zbGeneCsvRecord m_geneRecord;

	private int m_allWeight;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbHave = UQueryExtensions.Q<Label>(m_RootElement, "lb_have", (string)null);
		m_lbType = UQueryExtensions.Q<Label>(m_RootElement, "lb_type", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_nodeList = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_items", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_btnUse = UQueryExtensions.Q<Button>(m_RootElement, "btn_use", (string)null);
		m_nodeOpen = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_open", (string)null);
		m_lbKey = UQueryExtensions.Q<Label>(m_nodeOpen, "lb_key", (string)null);
		m_btnOpen = UQueryExtensions.Q<Button>(m_nodeOpen, "btn_open", (string)null);
		m_btnSale = UQueryExtensions.Q<Button>(m_RootElement, "btn_sale", (string)null);
		m_btnGetWay = UQueryExtensions.Q<Button>(m_RootElement, "btn_getway", (string)null);
		m_nodeGene = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_gene", (string)null);
		m_lbGeneAttr = UQueryExtensions.Q<Label>(m_nodeGene, "lb_geneAttr", (string)null);
		m_lbGeneValue = UQueryExtensions.Q<Label>(m_nodeGene, "lb_geneValue", (string)null);
		m_lbGeneDesc = UQueryExtensions.Q<Label>(m_nodeGene, "lb_geneDesc", (string)null);
		m_btnExchange = UQueryExtensions.Q<Button>(m_nodeGene, "btn_exchange", (string)null);
		m_btnRemove = UQueryExtensions.Q<Button>(m_nodeGene, "btn_remove", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_nodeGene, "btn_upgrade", (string)null);
		m_notifyExchange = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnExchange, (Vector2?)new Vector2(3f, 5f), 1f, true);
		m_notifyUpgrade = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(3f, 5f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUse, (Action)OnUse, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOpen, (Action)OnOpen, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSale, (Action)OnSale, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetWay, (Action)OnGetWay, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action)OnExchange, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRemove, (Action)OnRemove, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Box, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gem, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_HeroAwaken_GeneRemove_Ack, OnEventHide);
		InitColumns();
	}

	private void InitColumns()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		int maxColumnCount = 5;
		m_listView.columns.Clear();
		for (int i = 0; i < maxColumnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(110f),
				makeCell = delegate
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_001a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					//IL_002f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0036: Unknown result type (might be due to invalid IL or missing references)
					//IL_0040: Unknown result type (might be due to invalid IL or missing references)
					//IL_0047: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					//IL_0058: Unknown result type (might be due to invalid IL or missing references)
					//IL_0084: Unknown result type (might be due to invalid IL or missing references)
					//IL_0089: Unknown result type (might be due to invalid IL or missing references)
					//IL_008e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0098: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a0: Expected O, but got Unknown
					VisualElement val = new VisualElement();
					val.style.width = StyleLength.op_Implicit(110f);
					val.style.height = StyleLength.op_Implicit(92f);
					val.style.alignItems = StyleEnum<Align>.op_Implicit((Align)2);
					val.style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)1);
					val.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
					zbItem zbItem2 = new zbItem();
					zbItem2.OnEnable(this);
					((VisualElement)zbItem2).style.scale = StyleScale.op_Implicit(new Scale(new Vector2(0.7f, 0.7f)));
					val.Add((VisualElement)(object)zbItem2);
					return val;
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0077: Unknown result type (might be due to invalid IL or missing references)
					//IL_0065: Unknown result type (might be due to invalid IL or missing references)
					int num = row * maxColumnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element.ElementAt(0) as zbItem).Refresh(m_listItemDatas[num]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
	}

	public void RefreshCustom(zbItemData data, ESkillType skillType, int index)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e4: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_skillType = skillType;
		m_index = index;
		zbResourceHelper.RefreshResourceBg(m_data.type, m_data.id, m_spriteFrame);
		m_item.Refresh(m_data, isNameVis: false, isCountVis: false, isTouchVis: false);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_data.type, m_data.id);
		((TextElement)m_lbName).text = resorceIconPath.strName;
		((TextElement)m_lbDesc).text = resorceIconPath.strDesc;
		((VisualElement)m_lbType).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/common/transparent.png"));
		((TextElement)m_lbHave).text = zbUtil.Language("lang.10019") + ": " + m_data.count;
		switch (m_data.type)
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
		case EResourceType.Gem:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10023");
			break;
		case EResourceType.Box:
			((TextElement)m_lbType).text = zbUtil.Language("lang.10024");
			break;
		case EResourceType.Gene:
			m_geneRecord = zbDataConfig.Instance.geneCsvTable.GetRecordByKey((int)m_data.id);
			if (m_geneRecord != null)
			{
				((TextElement)m_lbType).text = zbUtil.Language("lang.10015") + ": ";
				((VisualElement)m_lbType).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/hero/hero_geneShape{m_geneRecord.geneShape}.png"));
			}
			else
			{
				((TextElement)m_lbType).text = zbUtil.Language("lang.10891");
			}
			break;
		}
		((VisualElement)m_btnUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_data.type == EResourceType.Box)
		{
			m_boxRecord = zbDataConfig.Instance.boxCsvTable.GetRecordByKey((int)m_data.id);
			m_nodeOpen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.count < m_boxRecord.boxCost) ? 1 : 0));
			m_item.m_spriteFrag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_boxRecord.boxCost <= 1));
			((VisualElement)m_lbKey).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_boxRecord.key <= 0));
			if (m_boxRecord.key > 0)
			{
				stResorceIconInfo resorceIconPath2 = zbResourceHelper.GetResorceIconPath(EResourceType.Item, m_boxRecord.key);
				((VisualElement)m_lbKey).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath2.strIconPath));
				if (zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, m_boxRecord.key, 1))
				{
					((VisualElement)m_lbKey).style.color = StyleColor.op_Implicit(new Color(13f / 51f, 0.23529412f, 19f / 85f, 1f));
				}
				else
				{
					((VisualElement)m_lbKey).style.color = StyleColor.op_Implicit(Color.red);
				}
			}
			if (m_boxRecord.boxShow == 1)
			{
				m_nodeList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				RefreshBox();
			}
			else
			{
				m_nodeList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((BaseVerticalCollectionView)m_listView).itemsSource = null;
			}
		}
		else
		{
			if (m_data.type == EResourceType.Prop)
			{
				zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)m_data.id);
				if (recordByKey != null && recordByKey.type == 16)
				{
					((VisualElement)m_btnUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
			}
			m_nodeOpen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((BaseVerticalCollectionView)m_listView).itemsSource = null;
		}
		((VisualElement)m_btnSale).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!CanSale()));
		((VisualElement)m_btnGetWay).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.type == EResourceType.Gene));
		m_nodeGene.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.type != EResourceType.Gene));
		if (m_data.type == EResourceType.Gene)
		{
			if (m_geneRecord != null)
			{
				((TextElement)m_lbGeneAttr).text = zbAttrHelper.GetTeamAttrName((int)m_geneRecord.attr[0]);
				((TextElement)m_lbGeneValue).text = zbSelf.Instance.geneHandler.GetGeneResonanceAttr(ESkillType.EmptyValue, m_geneRecord.attr[1]).ToString();
			}
			((VisualElement)m_btnExchange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_skillType == ESkillType.EmptyValue));
			((VisualElement)m_btnRemove).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_skillType == ESkillType.EmptyValue));
			if (m_skillType == ESkillType.EmptyValue)
			{
				((VisualElement)m_btnUpgrade).style.left = StyleLength.op_Implicit(300f);
				m_notifyUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.geneHandler.HasGeneNotify(m_geneRecord.type, m_geneRecord.geneShape, 5)));
			}
			else
			{
				((VisualElement)m_btnUpgrade).style.left = StyleLength.op_Implicit(400f);
				m_notifyUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.geneHandler.HasGeneNotify(m_geneRecord.type, m_geneRecord.geneShape, 4)));
				m_notifyExchange.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.geneHandler.HasGeneExchangeNotify(m_skillType, m_index)));
			}
		}
	}

	private bool CanSale()
	{
		if (m_data.type == EResourceType.Item)
		{
			zbItemCsvRecord recordByKey = zbDataConfig.Instance.itemCsvTable.GetRecordByKey((int)m_data.id);
			if (recordByKey.sellMoney > 0 && recordByKey.sellPrice > 0)
			{
				return true;
			}
		}
		else if (m_data.type == EResourceType.Prop)
		{
			zbPropCsvRecord recordByKey2 = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)m_data.id);
			if (recordByKey2.sellMoney > 0 && recordByKey2.sellPrice > 0)
			{
				return true;
			}
		}
		return false;
	}

	private void RefreshBox()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		GetPidAllWeight();
		((TextElement)m_btnOpen).text = ((m_boxRecord.type == 3) ? zbUtil.Language("lang.10002") : zbUtil.Language("lang.10001"));
		m_listItemDatas.Clear();
		zbRandBoxCsvRecord[] records = zbDataConfig.Instance.randBoxCsvTable.GetRecords().records;
		foreach (zbRandBoxCsvRecord zbRandBoxCsvRecord2 in records)
		{
			if (zbRandBoxCsvRecord2.pid == m_boxRecord.box)
			{
				m_listItemDatas.Add(new zbItemData((EResourceType)zbRandBoxCsvRecord2.type, zbRandBoxCsvRecord2.id, zbRandBoxCsvRecord2.count));
			}
		}
		int num = 5;
		int num2 = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)num);
		List<int> list = new List<int>();
		for (int j = 0; j < num2; j++)
		{
			list.Add(j);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = list;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((BaseVerticalCollectionView)m_listView).ScrollToItemById(0);
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

	private void OnUse()
	{
	}

	private void OnOpen()
	{
		if (m_boxRecord.key > 0 && !zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, m_boxRecord.key, 1))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Item, m_boxRecord.key, 1L);
		}
		else if (m_boxRecord.type == 1 || m_boxRecord.type == 4)
		{
			if (m_data.count <= 1)
			{
				if (m_boxRecord.key > 0)
				{
					zbUIMgr.Instance.ShowBoxKeyOpen(new zbItemData(EResourceType.Item, m_boxRecord.key, 1L), delegate(bool isOK)
					{
						if (isOK)
						{
							zbWsMsgHandler.Instance.Bag_RandBoxOpen_Req((int)m_data.id, 1);
						}
					});
				}
				else
				{
					zbWsMsgHandler.Instance.Bag_RandBoxOpen_Req((int)m_data.id, 1);
				}
			}
			else
			{
				zbUIMgr.Instance.ShowBagRandBoxOpen(m_data);
			}
		}
		else if (m_boxRecord.type == 2)
		{
			zbWsMsgHandler.Instance.Bag_SelectBoxView_Req((int)m_data.id);
		}
		else if (m_boxRecord.type == 3)
		{
			if (m_data.count >= m_boxRecord.boxCost)
			{
				zbUIMgr.Instance.ShowBoxCompose(m_data);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(m_data.type, m_data.id, m_boxRecord.boxCost);
			}
		}
	}

	private void OnSale()
	{
		zbUIMgr.Instance.ShowBagItemSale(m_data);
	}

	private void OnGetWay()
	{
		if (m_data.type == EResourceType.Box && m_boxRecord.type == 3 && m_data.count < m_boxRecord.boxCost)
		{
			zbUIMgr.Instance.ShowTip(m_data.type, m_data.id, m_boxRecord.boxCost);
		}
		else
		{
			zbUIMgr.Instance.ShowTip(m_data.type, m_data.id, 0L);
		}
	}

	private void OnExchange()
	{
		GeneSlotDetail geneSlotDetail = zbSelf.Instance.geneHandler.GetGeneSlotDetail(m_skillType, m_index);
		zbUIMgr.Instance.ShowHeroGeneSelect(geneSlotDetail, m_skillType, m_index);
		Hide();
	}

	private void OnRemove()
	{
		zbWsMsgHandler.Instance.HeroAwaken_GeneRemove_Req(m_skillType, m_index);
	}

	private void OnUpgrade()
	{
		zbUIMgr.Instance.ShowGeneCompose((int)m_data.id, m_skillType, m_index);
		Hide();
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(m_data.type, (int)m_data.id);
		if ((m_data.type == EResourceType.Box && resourceCount >= m_boxRecord.boxCost) || (m_data.type != EResourceType.Box && resourceCount > 0))
		{
			zbItemData data2 = new zbItemData(m_data.type, m_data.id, resourceCount);
			RefreshCustom(data2, m_skillType, m_index);
		}
		else
		{
			Hide();
		}
	}

	private void OnEventHide(object data)
	{
		Hide();
	}
}
