using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEquipDecomposeWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private VisualElement m_bg;

	private Button[] m_btnSorts = (Button[])(object)new Button[4];

	private Button m_btnSort;

	private VisualElement m_spriteSortType;

	private TextElement m_btnSortText;

	public VisualElement m_listNo;

	private MultiColumnListView m_listView;

	private List<EquipDetail> m_listItemDatas = new List<EquipDetail>();

	private List<int> rowIndexes = new List<int>();

	private zbItem m_item;

	private Button m_btnSelect;

	private Button m_btnDecompse;

	private Button m_btnClose;

	private int columnCount = 6;

	private int rowCount;

	public List<EquipDetail> selectList = new List<EquipDetail>();

	private int m_curIndex;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "s9_bg", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_btnSorts[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_sort" + i, (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSorts[i], (Action<ClickEvent>)delegate
			{
				OnSorts(curIndex);
			}, "", (TrickleDown)0);
		}
		m_btnSort = UQueryExtensions.Q<Button>(m_RootElement, "btn_sort", (string)null);
		m_spriteSortType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnSort, "type", (string)null);
		m_btnSortText = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_btnSort, (string)null, (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_btnSelect = UQueryExtensions.Q<Button>(m_RootElement, "btn_select", (string)null);
		m_btnDecompse = UQueryExtensions.Q<Button>(m_RootElement, "btn_decompose", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnSortTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSort, (Action)OnSort, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelect, (Action<ClickEvent>)OnSelect, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompse, (Action<ClickEvent>)OnDecompse, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.EquipSoul });
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Equip, OnEventUpdateEquip);
		OnSorts(0);
	}

	public override void Refresh()
	{
		selectList.Clear();
		m_item.Refresh(new zbItemData(EResourceType.Money, 12L, 1L));
		RefreshListView();
		RefreshHeroSoulNum();
	}

	private void RefreshHeroSoulNum()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (EquipDetail select in selectList)
		{
			num += GetHeroSoulNum(select.Quality, select.SubLevel);
		}
		((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num <= 0));
		((TextElement)m_item.m_lbCount).text = num.ToString();
	}

	private int GetHeroSoulNum(EQuality quality, int level)
	{
		zbEquipUpgradeSubCsvRecord[] records = zbDataConfig.Instance.equipUpgradeSubCsvTable.GetRecords().records;
		foreach (zbEquipUpgradeSubCsvRecord zbEquipUpgradeSubCsvRecord2 in records)
		{
			if (zbEquipUpgradeSubCsvRecord2.id == (int)quality && zbEquipUpgradeSubCsvRecord2.lv == level)
			{
				return zbEquipUpgradeSubCsvRecord2.deExpTotal;
			}
		}
		return 0;
	}

	public void RefreshListView()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(120f),
				makeCell = () => (VisualElement)(object)new zbEquipDecomposeItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0069: Unknown result type (might be due to invalid IL or missing references)
					//IL_0057: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						zbEquipDecomposeItem obj = element as zbEquipDecomposeItem;
						obj?.Refresh(m_listItemDatas[num]);
						obj.RefreshSelect();
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		GenerateData();
	}

	private void GenerateData()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		GetListItemDatas();
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int i = 0; i < rowCount; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	public void RefreshSelectList()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listItemDatas.Count > 0));
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		RefreshHeroSoulNum();
	}

	private void GetListItemDatas()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (m_curIndex == 0)
		{
			m_btnSortText.text = "All";
			m_spriteSortType.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_btnSortText.text = "";
			m_spriteSortType.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteSortType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/leader/leader_equipslot" + m_curIndex + ".png"));
		}
		foreach (EquipDetail equip in zbServerData.sGameData.EquipList)
		{
			if (equip.Lock == EBoolState.False)
			{
				zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(equip.EquipTid);
				if (m_curIndex == 0 || (m_curIndex > 0 && recordByKey.type == m_curIndex))
				{
					m_listItemDatas.Add(equip);
				}
			}
		}
		m_listItemDatas.Sort((EquipDetail a, EquipDetail b) => zbResourceHelper.GetResourceQuality(EResourceType.Equip, a.EquipTid).CompareTo(zbResourceHelper.GetResourceQuality(EResourceType.Equip, b.EquipTid)));
	}

	private void OnSorts(int index)
	{
		m_curIndex = index;
		Refresh();
		((VisualElement)m_btnTouchTest).visible = false;
		m_bg.visible = false;
	}

	private void OnSort()
	{
		if (((VisualElement)m_btnTouchTest).visible)
		{
			((VisualElement)m_btnTouchTest).visible = false;
			m_bg.visible = false;
		}
		else
		{
			((VisualElement)m_btnTouchTest).visible = true;
			m_bg.visible = true;
		}
	}

	private void OnSortTouch(ClickEvent evt)
	{
		((VisualElement)m_btnTouchTest).visible = false;
		m_bg.visible = false;
	}

	private void OnSelect(ClickEvent evt)
	{
		OnSortTouch(evt);
		zbUIMgr.Instance.ShowEquipDecomposeSelect(m_curIndex, delegate(List<EquipDetail> list)
		{
			selectList.Clear();
			foreach (EquipDetail item in list)
			{
				selectList.Add(item);
			}
			RefreshSelectList();
		});
	}

	private void OnDecompse(ClickEvent evt)
	{
		OnSortTouch(evt);
		if (selectList.Count < 1)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10077"));
			return;
		}
		List<string> idList = new List<string>();
		foreach (EquipDetail select in selectList)
		{
			idList.Add(select.EquipId);
		}
		bool flag = false;
		foreach (string item in idList)
		{
			EquipDetail equipDetail = zbSelf.Instance.equipHandler.GetEquipDetail(item);
			if (equipDetail == null)
			{
				continue;
			}
			zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(equipDetail.EquipTid);
			if (recordByKey != null)
			{
				EquipDetail equipSlotEquip = zbSelf.Instance.equipHandler.GetEquipSlotEquip((EEquipType)recordByKey.type);
				if (equipSlotEquip != null && equipDetail.Quality > equipSlotEquip.Quality)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			zbUIMgr.Instance.ShowVerificationCode(zbUtil.Language("lang.10764"), zbUtil.Language("lang.10566"), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Equip_Decompose_Req(idList.ToArray());
				}
			});
		}
		else
		{
			zbWsMsgHandler.Instance.Equip_Decompose_Req(idList.ToArray());
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdateEquip(object data)
	{
		Refresh();
	}
}
