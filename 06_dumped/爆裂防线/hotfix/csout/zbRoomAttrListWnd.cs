using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbRoomAttrListWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTouch;

	private Button m_btnClose;

	private ListView m_listView;

	private List<EAttrTeamType> m_attrTypes = new List<EAttrTeamType>();

	public Dictionary<EAttrTeamType, float> m_roomAllAttrSum = new Dictionary<EAttrTeamType, float>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/home/tb_roomAttrListItem.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 55f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClickClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClickClose, "", (TrickleDown)0);
		InitListView();
		AddDark();
	}

	public void RefreshCustom()
	{
		RefreshWnd();
	}

	private void RefreshWnd()
	{
		RebuildRoomAttrSums();
		ReloadListView();
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbRoomAttrListItem(m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbRoomAttrListItem obj = element as zbRoomAttrListItem;
			EAttrTeamType eAttrTeamType = (((BaseVerticalCollectionView)m_listView).itemsSource as List<EAttrTeamType>)[index];
			float value = (m_roomAllAttrSum.TryGetValue(eAttrTeamType, out var value2) ? value2 : 0f);
			obj.Refresh(eAttrTeamType, value);
		};
	}

	private void ReloadListView()
	{
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_attrTypes;
	}

	private void RebuildRoomAttrSums()
	{
		m_roomAllAttrSum.Clear();
		m_attrTypes.Clear();
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
			if (recordByKey == null || room.Level == 0)
			{
				continue;
			}
			float[][] roomAttr = recordByKey.roomAttr;
			foreach (float[] obj in roomAttr)
			{
				EAttrTeamType key = (EAttrTeamType)obj[0];
				float num = obj[1] * (float)room.Level;
				if (m_roomAllAttrSum.ContainsKey(key))
				{
					m_roomAllAttrSum[key] += num;
				}
				else
				{
					m_roomAllAttrSum[key] = num;
				}
			}
		}
		HashSet<EAttrTeamType> hashSet = new HashSet<EAttrTeamType>();
		foreach (zbHomeRoomTypeCsvRecord value in zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecords().records.Values)
		{
			if (value == null || value.roomAttr == null)
			{
				continue;
			}
			float[][] roomAttr = value.roomAttr;
			for (int i = 0; i < roomAttr.Length; i++)
			{
				EAttrTeamType eAttrTeamType = (EAttrTeamType)roomAttr[i][0];
				if (zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)eAttrTeamType) != null)
				{
					hashSet.Add(eAttrTeamType);
				}
			}
		}
		m_attrTypes = hashSet.OrderBy((EAttrTeamType t) => (int)t).ToList();
	}

	private void OnClickClose(ClickEvent evt)
	{
		Hide();
	}
}
