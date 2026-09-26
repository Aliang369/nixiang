using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderTabGem
{
	private zbLeaderWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnTouchTest;

	private VisualElement m_bg;

	private Button[] m_btnSorts = (Button[])(object)new Button[7];

	private Button m_btnSort;

	private VisualElement m_spriteSortType;

	private TextElement m_btnSortText;

	private Button m_btnCompose;

	private VisualElement m_listNo;

	private MultiColumnListView m_listView;

	private VisualElement m_notifyCompose;

	private List<ResourceDetail> m_listItemDatas = new List<ResourceDetail>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 6;

	private int rowCount;

	private int m_curIndex;

	public int m_selectTid = -1;

	private bool m_columnsInitialized;

	private Dictionary<int, Sprite> m_sortSprites = new Dictionary<int, Sprite>();

	public void OnEnable(zbLeaderWnd wndParent, VisualElement root)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnTouchTest = UQueryExtensions.Q<Button>(root, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(root, "s9_bg", (string)null);
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		for (int i = 0; i < 7; i++)
		{
			m_btnSorts[i] = UQueryExtensions.Q<Button>(root, "btn_sort" + i, (string)null);
			int curIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSorts[i], (Action<ClickEvent>)delegate
			{
				OnSorts(curIndex);
			}, "", (TrickleDown)0);
		}
		m_btnSort = UQueryExtensions.Q<Button>(root, "btn_sort", (string)null);
		m_spriteSortType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnSort, "type", (string)null);
		m_btnSortText = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_btnSort, (string)null, (string)null);
		m_btnCompose = UQueryExtensions.Q<Button>(root, "btn_compose", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(root, "list_no", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(root, "listView", (string)null);
		m_notifyCompose = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCompose);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnSortTouch, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSort, (Action)OnSort, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCompose, (Action)OnCompose, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<PointerDownEvent>((VisualElement)(object)m_listView, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		InitListViewColumns();
		OnSorts(0);
	}

	private void InitListViewColumns()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		if (m_columnsInitialized)
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
				makeCell = () => (VisualElement)(object)new zbLeaderItemGem(m_wndParent, this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0085: Unknown result type (might be due to invalid IL or missing references)
					//IL_0068: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						zbLeaderItemGem obj = element as zbLeaderItemGem;
						obj?.Refresh(m_listItemDatas[num]);
						obj?.RefreshSelect(m_selectTid);
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
		m_columnsInitialized = true;
	}

	public void Refresh()
	{
		RefreshListView();
		RefreshSelect(m_selectTid);
		zbUtil.SetDisplayIfChanged(m_notifyCompose, (DisplayStyle)(!zbSelf.Instance.equipHandler.CanGemCompose()));
	}

	public void RefreshListView()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		GetListItemDatas();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		rowIndexes.Clear();
		for (int i = 0; i < rowCount; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		zbUtil.SetDisplayIfChanged(m_listNo, (DisplayStyle)((m_listItemDatas.Count != 0) ? 1 : 0));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void GetListItemDatas()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
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
			m_spriteSortType.style.backgroundImage = new StyleBackground(GetSortSprite(m_curIndex));
		}
		foreach (ResourceDetail gem in zbServerData.sGameData.GemList)
		{
			zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gem.Tid);
			if ((m_curIndex == 0 || recordByKey.type == m_curIndex) && zbSelf.Instance.equipHandler.getGemCount(gem.Tid) > 0)
			{
				m_listItemDatas.Add(gem);
			}
		}
		m_listItemDatas.Sort(delegate(ResourceDetail a, ResourceDetail b)
		{
			EQuality resourceQuality = zbResourceHelper.GetResourceQuality(EResourceType.Gem, a.Tid);
			int num = zbResourceHelper.GetResourceQuality(EResourceType.Gem, b.Tid).CompareTo(resourceQuality);
			if (num != 0)
			{
				return num;
			}
			int type = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(a.Tid).type;
			int type2 = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(b.Tid).type;
			num = type.CompareTo(type2);
			return (num != 0) ? num : a.Tid.CompareTo(b.Tid);
		});
	}

	private Sprite GetSortSprite(int index)
	{
		if (!m_sortSprites.TryGetValue(index, out var value))
		{
			value = zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/leader/leader_sort{index}.png");
			m_sortSprites[index] = value;
		}
		return value;
	}

	public void RefreshSelect(int tid)
	{
		m_selectTid = tid;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		m_wndParent.RefreshNewIcon();
	}

	public void OnPointerDown(PointerDownEvent evt)
	{
		IEventHandler target = ((EventBase)evt).target;
		for (VisualElement val = (VisualElement)(object)((target is VisualElement) ? target : null); val != null; val = val.parent)
		{
			if (val.ClassListContains("unity-list-view__item") && val.userData?.ToString() == "empty")
			{
				return;
			}
		}
		RefreshSelect(-1);
		zbUIMgr.Instance.HideWnd(EWndId.GemInsert);
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

	private void OnCompose()
	{
		zbWsMsgHandler.Instance.Gem_ComposePreview_Req();
	}
}
