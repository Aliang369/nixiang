using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbPeeragePanel
{
	private VisualTreeAsset m_itemTA;

	private Label m_lbIcon;

	private Label m_lbDes;

	private ListView m_listView;

	private Label m_lbPeerage;

	private Label m_lbTopPeerageTitle;

	private Label m_lbTopPeerageName;

	private VisualElement m_root;

	private Button m_btnUpgrade;

	private Button m_btnPeerageInfo;

	private zbPeerageWnd m_wndParent;

	private zbItem[] m_items = new zbItem[3];

	public void OnEnable(zbPeerageWnd wndParent, VisualElement root)
	{
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/peerage/tb_peerageTaskItem.uxml");
		m_root = root;
		m_lbIcon = UQueryExtensions.Q<Label>(root, "lb_icon", (string)null);
		m_lbDes = UQueryExtensions.Q<Label>(root, "lb_des", (string)null);
		m_lbTopPeerageTitle = UQueryExtensions.Q<Label>(root, "lb_topPeerageTitle", (string)null);
		m_lbTopPeerageName = UQueryExtensions.Q<Label>(root, "lb_topPeerageName", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(root, "btn_upgrade", (string)null);
		m_btnPeerageInfo = UQueryExtensions.Q<Button>(root, "btn_peerageInfo", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(root, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 134f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_lbPeerage = UQueryExtensions.Q<Label>(root, "lb_peerage", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_wndParent = wndParent;
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(root, $"item{i}", (string)null);
			m_items[i].OnEnable(wndParent);
		}
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPeerageInfo, (Action)OnShowPeerageInfo, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		Init();
		UpdateListView();
	}

	private void Init()
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		int peerageId = zbServerData.sGameData.PeerageInfo.PeerageId;
		zbPeerageCsvRecord recordByKey = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(peerageId);
		bool flag = zbSelf.Instance.taskHandler.IsTopPeerage();
		((VisualElement)m_lbTopPeerageName).visible = flag;
		((VisualElement)m_lbTopPeerageTitle).visible = flag;
		UQueryExtensions.Q<VisualElement>(m_root, "bg", (string)null).visible = !flag;
		UQueryExtensions.Q<VisualElement>(m_root, "layouts_bg", (string)null).visible = !flag;
		((VisualElement)UQueryExtensions.Q<Label>(m_root, "lb_peerage", (string)null)).visible = !flag;
		UQueryExtensions.Q<VisualElement>(m_root, "header", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)UQueryExtensions.Q<ListView>(m_root, (string)null, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)UQueryExtensions.Q<Button>(m_root, "btn_upgrade", (string)null)).visible = !flag;
		((VisualElement)m_btnPeerageInfo).style.height = StyleLength.op_Implicit((float)(flag ? 1380 : 400));
		if (!flag)
		{
			bool flag2 = ((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).ToList().All((TaskDetail t) => t.State == EActiveState.Done);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			((TextElement)m_lbPeerage).text = recordByKey.name;
			((VisualElement)m_lbPeerage).style.color = StyleColor.op_Implicit(zbResourceHelper.GetColor((EQuality)recordByKey.colorQuality));
			zbPeerageCsvRecord recordByKey2 = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(peerageId + 1);
			for (int num = 0; num < recordByKey2.awardList.Length; num++)
			{
				bool flag3 = num < recordByKey2.awardList.Length;
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
				if (flag3)
				{
					zbItemData data = new zbItemData((EResourceType)recordByKey2.awardList[num][0], recordByKey2.awardList[num][1], recordByKey2.awardList[num][2]);
					m_items[num].Refresh(data);
				}
			}
		}
		else
		{
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbTopPeerageName).text = recordByKey.name;
			((VisualElement)m_lbTopPeerageName).style.color = StyleColor.op_Implicit(zbResourceHelper.GetColor((EQuality)recordByKey.colorQuality));
		}
	}

	private void OnUpgrade()
	{
		zbWsMsgHandler.Instance.Peerage_Upgrade_Req();
	}

	private void OnShowPeerageInfo()
	{
		zbUIMgr.Instance.ShowPeerageInfo();
	}

	public void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		List<TaskDetail> items = ((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).OrderBy((TaskDetail t) => (t.State != EActiveState.Active) ? ((t.State == EActiveState.Pending) ? 1 : 2) : 0).ToList();
		m_listView.makeItem = () => (VisualElement)(object)new zbPeerageTaskItem(m_itemTA, m_wndParent);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbPeerageTaskItem).Init(m_wndParent, items[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
	}
}
