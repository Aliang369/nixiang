using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderTabEquip
{
	private zbLeaderWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnTouchTest;

	private VisualElement m_bg;

	private Button[] m_btnSorts = (Button[])(object)new Button[4];

	private Button m_btnSort;

	private VisualElement m_spriteSortType;

	private TextElement m_btnSortText;

	private Button m_btnDecompse;

	private VisualElement m_listNo;

	private MultiColumnListView m_listView;

	private VisualElement m_nodeGetWay;

	private zbItem[] m_items = new zbItem[6];

	private List<EquipDetail> m_listItemDatas = new List<EquipDetail>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 6;

	private int rowCount;

	private int m_curIndex;

	private string m_selectId = "";

	private bool m_listViewInitialized;

	public void OnEnable(zbLeaderWnd wndParent, VisualElement root)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "s9_bg", (string)null);
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		for (int i = 0; i < 4; i++)
		{
			m_btnSorts[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_sort" + i, (string)null);
			int curIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSorts[i], (Action<ClickEvent>)delegate
			{
				OnSorts(curIndex);
			}, "", (TrickleDown)0);
		}
		m_btnSort = UQueryExtensions.Q<Button>(m_RootElement, "btn_sort", (string)null);
		m_spriteSortType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnSort, "type", (string)null);
		m_btnSortText = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_btnSort, (string)null, (string)null);
		m_btnDecompse = UQueryExtensions.Q<Button>(m_RootElement, "btn_decompose", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_nodeGetWay = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_getWay", (string)null);
		for (int num = 0; num < 6; num++)
		{
			m_items[num] = UQueryExtensions.Q<zbItem>(m_nodeGetWay, "item" + num, (string)null);
			m_items[num].OnEnable(m_wndParent);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnSortTouch, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSort, (Action)OnSort, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompse, (Action)OnDecompose, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<PointerDownEvent>((VisualElement)(object)m_listView, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		InitListView();
		OnSorts(0);
	}

	public void Refresh()
	{
		RefreshGetWay();
		RefreshListView();
		RefreshSelect(m_selectId);
		((BaseVerticalCollectionView)m_listView).ScrollToItem(0);
	}

	private void InitListView()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (m_listViewInitialized)
		{
			return;
		}
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(120f),
				makeCell = () => (VisualElement)(object)new zbLeaderItemEquip(m_wndParent, this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0085: Unknown result type (might be due to invalid IL or missing references)
					//IL_0068: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						zbLeaderItemEquip obj = element as zbLeaderItemEquip;
						obj?.Refresh(m_listItemDatas[num]);
						obj?.RefreshSelect(m_selectId);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
						element.userData = "data";
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
						element.userData = "empty";
					}
				}
			});
		}
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_listViewInitialized = true;
	}

	public void RefreshListView()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		GenerateData();
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void GenerateData()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		GetListItemDatas();
		m_listItemDatas.Sort(delegate(EquipDetail a, EquipDetail b)
		{
			EQuality resourceQuality = zbResourceHelper.GetResourceQuality(EResourceType.Equip, a.EquipTid);
			int num2 = zbResourceHelper.GetResourceQuality(EResourceType.Equip, b.EquipTid).CompareTo(resourceQuality);
			if (num2 != 0)
			{
				return num2;
			}
			num2 = b.EquipTid.CompareTo(a.EquipTid);
			return (num2 != 0) ? num2 : string.Compare(a.EquipId, b.EquipId);
		});
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int num = 0; num < rowCount; num++)
		{
			rowIndexes.Add(num);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listItemDatas.Count > 0));
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
			zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(equip.EquipTid);
			if (m_curIndex == 0 || (m_curIndex > 0 && recordByKey.type == m_curIndex))
			{
				m_listItemDatas.Add(equip);
			}
		}
	}

	public void RefreshSelect(string id)
	{
		m_selectId = id;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		m_wndParent.RefreshNewIcon();
	}

	public void OnPointerDown(PointerDownEvent evt)
	{
		IEventHandler target = ((EventBase)evt).target;
		VisualElement val = (VisualElement)(object)((target is VisualElement) ? target : null);
		bool flag = false;
		while (val != null)
		{
			if (val.ClassListContains("unity-list-view__item") && val.userData?.ToString() == "empty")
			{
				flag = true;
				break;
			}
			val = val.parent;
		}
		if (!flag)
		{
			RefreshSelect("");
			zbUIMgr.Instance.HideWnd(EWndId.Equip);
		}
	}

	private void OnSorts(int index)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		m_curIndex = index;
		RefreshListView();
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnSort()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (((VisualElement)m_btnTouchTest).style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnSortTouch(ClickEvent evt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnDecompose()
	{
		zbUIMgr.Instance.ShowEquipDecompose();
	}

	private void RefreshGetWay()
	{
		int[] equipIds = GetEquipIds();
		for (int i = 0; i < 6; i++)
		{
			m_items[i].Refresh(new zbItemData(EResourceType.Equip, equipIds[i], 1L), isNameVis: false, isCountVis: false);
		}
	}

	private int[] GetEquipIds()
	{
		List<int> list = new List<int>();
		EQuality eQuality = EQuality.White;
		foreach (EquipSlotDetail equipSlot in zbServerData.sGameData.EquipSlotInfo.EquipSlotList)
		{
			if (equipSlot.Equip.Quality > eQuality)
			{
				eQuality = equipSlot.Equip.Quality;
			}
		}
		if (eQuality >= EQuality.Orange)
		{
			eQuality = EQuality.Red;
		}
		zbEquipCsvTableData records = zbDataConfig.Instance.equipCsvTable.GetRecords();
		foreach (zbEquipCsvRecord value in records.records.Values)
		{
			if (value.quality == (int)eQuality)
			{
				list.Add(value.id);
			}
		}
		foreach (zbEquipCsvRecord value2 in records.records.Values)
		{
			if (value2.quality == (int)(eQuality + 1))
			{
				list.Add(value2.id);
			}
		}
		return list.ToArray();
	}
}
