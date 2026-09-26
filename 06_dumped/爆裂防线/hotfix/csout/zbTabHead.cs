using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTabHead
{
	private zbHeadWnd m_parent;

	private VisualElement m_rootElement;

	private VisualElement m_headIcon;

	private VisualElement m_headFrame;

	private Label m_lbHeadName;

	private Label m_lbHeadDes;

	private Label m_lbUnlockDes;

	private MultiColumnListView m_listView;

	private Button m_btnUse;

	private zbTabHead m_tabHead;

	private List<zbHeadItem> headListDetail = new List<zbHeadItem>();

	private List<int> headList = new List<int>();

	private List<zbPlayerHeadCsvRecord> m_listReleasedRecords = new List<zbPlayerHeadCsvRecord>();

	private int headId = -1;

	private int headFrameId = -1;

	public zbTabHead(VisualElement _root, zbHeadWnd _parent)
	{
		m_parent = _parent;
		m_rootElement = _root;
		m_lbHeadName = UQueryExtensions.Q<Label>(m_rootElement, "Label_HeadName", (string)null);
		m_lbHeadDes = UQueryExtensions.Q<Label>(m_rootElement, "Label_HeadDes", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_rootElement, "listView", (string)null);
		m_btnUse = UQueryExtensions.Q<Button>(m_rootElement, "Button_Use", (string)null);
		m_headIcon = UQueryExtensions.Q<VisualElement>(m_rootElement, "headIcon", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_rootElement, "headFrame", (string)null);
		m_lbUnlockDes = UQueryExtensions.Q<Label>(m_rootElement, "Label_UnlockDes", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUse, (Action<ClickEvent>)OnChange, "", (TrickleDown)0);
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Head_Data, RefreshHeadList);
	}

	private void RefreshHeadList(object obj)
	{
		Refresh(m_tabHead);
	}

	public void Refresh(zbTabHead _tabHead)
	{
		m_tabHead = _tabHead;
		headId = zbSelf.Instance.GetHeadId();
		headFrameId = zbSelf.Instance.GetHeadFrameId();
		UpdateHeadAndHeadFrameDisplay();
		RefreshHeadList();
		RefreshSelectItem(headId);
	}

	private void RefreshHeadList()
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		m_listReleasedRecords.Clear();
		foreach (zbPlayerHeadCsvRecord value in zbDataConfig.Instance.playerHeadCsvTable.GetRecords().records.Values)
		{
			if (zbVersionMgr.IsReleased(value.version))
			{
				m_listReleasedRecords.Add(value);
			}
		}
		if (m_listView.columns.Count == 0)
		{
			for (int i = 0; i < 5; i++)
			{
				int columnIndex = i;
				m_listView.columns.Add(new Column
				{
					width = Length.op_Implicit(120f),
					makeCell = () => (VisualElement)(object)new zbHeadItem(),
					bindCell = delegate(VisualElement element, int row)
					{
						//IL_009e: Unknown result type (might be due to invalid IL or missing references)
						//IL_008c: Unknown result type (might be due to invalid IL or missing references)
						int num = row * 5 + columnIndex;
						if (num >= 0 && num < m_listReleasedRecords.Count)
						{
							zbPlayerHeadCsvRecord headRecord = m_listReleasedRecords[num];
							zbHeadItem zbHeadItem2 = element as zbHeadItem;
							zbHeadItem2.OnEnable(m_parent, m_tabHead);
							zbHeadItem2.RefreshHead(headRecord);
							if (!headListDetail.Contains(zbHeadItem2))
							{
								headListDetail.Add(zbHeadItem2);
							}
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
		((BaseVerticalCollectionView)m_listView).itemsSource = new List<int>(Enumerable.Range(0, Mathf.CeilToInt((float)m_listReleasedRecords.Count / 5f)));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.height = StyleLength.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	public void RefreshSelectItem(int headId)
	{
		zbPlayerHeadCsvRecord recordByKey = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(headId);
		foreach (zbHeadItem item in headListDetail)
		{
			item.SetSelected(item.m_headData.id == headId);
		}
		bool flag = headId == zbServerData.sGameData.RoleBaseInfo.HeadId;
		bool flag2 = zbSelf.Instance.headHandler.IsHeadUnlocker(headId);
		((TextElement)m_lbHeadName).text = recordByKey.name;
		((TextElement)m_lbHeadDes).text = recordByKey.desc;
		((VisualElement)m_btnUse).visible = !flag & flag2;
		((VisualElement)m_lbUnlockDes).visible = !flag2 | flag;
		((TextElement)m_lbUnlockDes).text = ((!flag) ? recordByKey.desc : zbUtil.Language("lang.10162"));
	}

	public void OnHeadSel(int head)
	{
		headId = head;
		UpdateHeadAndHeadFrameDisplay();
		RefreshSelectItem(head);
	}

	private void OnChange(ClickEvent evt)
	{
		if (headId == zbServerData.sGameData.RoleBaseInfo.HeadId)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10101"));
		}
		else
		{
			zbWsMsgHandler.Instance.Reset_HeadId_Req(headId);
		}
	}

	private void UpdateHeadAndHeadFrameDisplay()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(headId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex);
		m_headIcon.style.backgroundImage = new StyleBackground(val);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(headFrameId);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex);
		m_headFrame.style.backgroundImage = new StyleBackground(val2);
	}

	public List<int> GetHeadIconList()
	{
		List<HeroDetail> list = new List<HeroDetail>();
		foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
		{
			list.Add(hero);
		}
		list.Sort((HeroDetail v1, HeroDetail v2) => v2.Quality.CompareTo(v1.Quality));
		List<int> list2 = new List<int>();
		foreach (HeroDetail item in list)
		{
			list2.Add(item.HeroTid);
		}
		return list2;
	}
}
