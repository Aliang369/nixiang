using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTabHeadFrame
{
	private readonly zbHeadWnd m_parent;

	private readonly VisualElement m_rootElement;

	private readonly VisualElement m_headIcon;

	private readonly VisualElement m_headFrame;

	private Label m_lbHeadFrameName;

	private Label m_lbHeadFrameDes;

	private Label m_UnlockDes;

	private readonly MultiColumnListView m_listView;

	private readonly Button m_btnChange;

	private readonly Button m_btnActive;

	private zbTabHeadFrame m_tabHeadFrame;

	private readonly List<zbHeadItem> headListDetail = new List<zbHeadItem>();

	private readonly List<zbPlayerHeadFrameCsvRecord> m_listReleasedRecords = new List<zbPlayerHeadFrameCsvRecord>();

	private int headId = -1;

	private int headFrameId = -1;

	private int reqId = -1;

	public zbTabHeadFrame(VisualElement root, zbHeadWnd parent)
	{
		m_parent = parent;
		m_rootElement = root;
		m_lbHeadFrameName = UQueryExtensions.Q<Label>(m_rootElement, "Label_HeadFrameName", (string)null);
		m_lbHeadFrameDes = UQueryExtensions.Q<Label>(m_rootElement, "Label_HeadFrameDes", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_rootElement, "listView", (string)null);
		m_btnChange = UQueryExtensions.Q<Button>(m_rootElement, "btn_change", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_rootElement, "btn_active", (string)null);
		m_headIcon = UQueryExtensions.Q<VisualElement>(m_rootElement, "headIcon", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_rootElement, "headFrame", (string)null);
		m_UnlockDes = UQueryExtensions.Q<Label>(m_rootElement, "Label_UnlockDes", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChange, (Action<ClickEvent>)OnChange, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Head_Data, OnHeadFrameRefresh);
	}

	private void OnHeadFrameRefresh(object obj)
	{
		if (m_tabHeadFrame != null && headFrameId != -1)
		{
			int id = headFrameId;
			Refresh(m_tabHeadFrame);
			OnHeadFrameSel(id);
		}
	}

	public void Refresh(zbTabHeadFrame _tabHead)
	{
		m_tabHeadFrame = _tabHead;
		headId = zbSelf.Instance.GetHeadId();
		headFrameId = zbSelf.Instance.GetHeadFrameId();
		RefreshDetail();
		RefreshSelectItem(headFrameId);
		RefreshList();
	}

	private void RefreshList()
	{
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		m_listReleasedRecords.Clear();
		foreach (zbPlayerHeadFrameCsvRecord value in zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecords().records.Values)
		{
			if (zbVersionMgr.IsReleased(value.version))
			{
				m_listReleasedRecords.Add(value);
			}
		}
		if ((HasFrameXXX(11) || zbSelf.Instance.headHandler.IsHeadFrameUnlocker(11)) && !m_listReleasedRecords.Exists((zbPlayerHeadFrameCsvRecord r) => r != null && r.id == 11))
		{
			zbPlayerHeadFrameCsvRecord recordByKey = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(11);
			if (recordByKey != null)
			{
				m_listReleasedRecords.Add(recordByKey);
			}
		}
		if ((HasFrameXXX(13) || zbSelf.Instance.headHandler.IsHeadFrameUnlocker(13)) && !m_listReleasedRecords.Exists((zbPlayerHeadFrameCsvRecord r) => r != null && r.id == 13))
		{
			zbPlayerHeadFrameCsvRecord recordByKey2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(13);
			if (recordByKey2 != null)
			{
				m_listReleasedRecords.Add(recordByKey2);
			}
		}
		if (m_listView.columns.Count == 0)
		{
			for (int num = 0; num < 5; num++)
			{
				int columnIndex = num;
				m_listView.columns.Add(new Column
				{
					width = Length.op_Implicit(120f),
					makeCell = () => (VisualElement)(object)new zbHeadItem(),
					bindCell = delegate(VisualElement element, int row)
					{
						//IL_009e: Unknown result type (might be due to invalid IL or missing references)
						//IL_008c: Unknown result type (might be due to invalid IL or missing references)
						int num2 = row * 5 + columnIndex;
						if (num2 >= 0 && num2 < m_listReleasedRecords.Count)
						{
							zbPlayerHeadFrameCsvRecord headFrameRecord = m_listReleasedRecords[num2];
							zbHeadItem zbHeadItem2 = element as zbHeadItem;
							zbHeadItem2.OnEnable(m_parent, m_tabHeadFrame);
							zbHeadItem2.RefreshHeadFrame(headFrameRecord);
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

	public bool HasFrameXXX(int headFrameId)
	{
		zbPlayerHeadFrameCsvRecord recordByKey = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(headFrameId);
		if (recordByKey != null)
		{
			return zbSelf.Instance.headHandler.HasHeadFrame(recordByKey.propId);
		}
		return false;
	}

	public void OnHeadFrameSel(int id)
	{
		headFrameId = id;
		RefreshDetail();
		RefreshSelectItem(headFrameId);
	}

	private void RefreshDetail()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		if (headId != -1 && headFrameId != -1)
		{
			string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(headId);
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex);
			if ((Object)(object)val != (Object)null)
			{
				m_headIcon.style.backgroundImage = new StyleBackground(val);
			}
			string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(headFrameId);
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex);
			if ((Object)(object)val2 != (Object)null)
			{
				m_headFrame.style.backgroundImage = new StyleBackground(val2);
			}
			zbPlayerHeadFrameCsvRecord recordByKey = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(headFrameId);
			int num = zbServerData.sGameData.RoleBaseInfo.HeadFrameId;
			bool flag = zbSelf.Instance.headHandler.HasHeadFrame(recordByKey.propId);
			bool flag2 = zbSelf.Instance.headHandler.IsHeadFrameUnlocker(headFrameId);
			((VisualElement)m_btnActive).visible = flag && !flag2;
			((VisualElement)m_btnChange).visible = flag2;
			((VisualElement)m_btnChange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((headFrameId == num) ? 1 : 0));
			((VisualElement)m_UnlockDes).visible = (!flag2 && !flag) || headFrameId == num;
			((TextElement)m_UnlockDes).text = ((headFrameId != num) ? recordByKey.desc : zbUtil.Language("lang.10162"));
		}
	}

	public void RefreshSelectItem(int id)
	{
		foreach (zbHeadItem item in headListDetail)
		{
			item.SetSelected(item.m_headFrameData.id == id);
		}
		((TextElement)m_lbHeadFrameName).text = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(id).name;
		((TextElement)m_lbHeadFrameDes).text = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(id).desc;
	}

	private void OnActive()
	{
		if (zbSelf.Instance.headHandler.IsHeadFrameUnlocker(headFrameId))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10131"));
			return;
		}
		zbPlayerHeadFrameCsvRecord recordByKey = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(headFrameId);
		if (recordByKey != null)
		{
			reqId = headFrameId;
			zbWsMsgHandler.Instance.Bag_HeadFrameActive_Req(recordByKey.propId);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10163"));
		}
	}

	private void OnChange(ClickEvent evt)
	{
		if (headFrameId == zbServerData.sGameData.RoleBaseInfo.HeadFrameId)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10102"));
			return;
		}
		if (!zbSelf.Instance.headHandler.IsHeadFrameUnlocker(headFrameId))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10103"));
			return;
		}
		reqId = headFrameId;
		zbWsMsgHandler.Instance.Reset_HeadFrameId_Req(headFrameId);
	}
}
