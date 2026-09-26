using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonLogWnd : zbTabWnd
{
	private VisualTreeAsset m_itemTA;

	private VisualElement[] m_panels = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_btns = (Button[])(object)new Button[2];

	private Button m_btnConfirm;

	private Button m_btnTouch;

	private ListView m_logListView;

	private MultiColumnListView m_awardListView;

	private List<zbItemData> m_awardList = new List<zbItemData>();

	private int columnCount = 6;

	private int rowCount;

	private List<int> rowIndexes = new List<int>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_logItem.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_panels[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_" + i, (string)null);
		}
		m_logListView = UQueryExtensions.Q<ListView>(m_RootElement, "logListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_logListView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_logListView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_awardListView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "awardListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_awardListView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		InitTab(m_btns, m_panels, null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
	}

	private void OnClose()
	{
		Hide();
	}

	public void RefreshCustom(AwardDetail[] awardList, DungeonLogInfo dungeonLogInfo)
	{
		AddDark();
		CreateListView(awardList);
		UpdateLogListView(((IEnumerable<DungeonLogDetail>)dungeonLogInfo.LogList).ToArray());
	}

	private void UpdateLogListView(DungeonLogDetail[] items)
	{
		((VisualElement)m_logListView).Clear();
		((BaseVerticalCollectionView)m_logListView).itemsSource = null;
		m_logListView.makeItem = () => (VisualElement)(object)new zbDungeonLogItem(m_itemTA, this);
		m_logListView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbDungeonLogItem).Init(items[index], index);
		};
		((BaseVerticalCollectionView)m_logListView).itemsSource = items;
	}

	public override void DoSelectTab()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 2; i++)
		{
			bool flag = m_currTab == i;
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/combat/balance_tabse.png");
			((VisualElement)m_btns[i]).style.backgroundImage = (StyleBackground)(flag ? new StyleBackground(val) : StyleBackground.op_Implicit((Texture2D)null));
		}
	}

	private void CreateListView(AwardDetail[] awardList)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		m_awardListView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_awardListView.columns.Add(new Column
			{
				width = Length.op_Implicit(98f),
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
					//IL_007f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0084: Unknown result type (might be due to invalid IL or missing references)
					//IL_0089: Unknown result type (might be due to invalid IL or missing references)
					//IL_0093: Unknown result type (might be due to invalid IL or missing references)
					//IL_009b: Expected O, but got Unknown
					VisualElement val = new VisualElement();
					val.style.width = StyleLength.op_Implicit(98f);
					val.style.height = StyleLength.op_Implicit(98f);
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
					//IL_0073: Unknown result type (might be due to invalid IL or missing references)
					//IL_0061: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_awardList.Count())
					{
						zbItem obj = element.ElementAt(0) as zbItem;
						obj?.Refresh(m_awardList[num]);
						((VisualElement)obj).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		RefreshAward(awardList);
	}

	private void RefreshAward(AwardDetail[] awardList)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		m_awardList.Clear();
		foreach (AwardDetail awardDetail in awardList)
		{
			m_awardList.Add(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_awardList.Count / (float)columnCount);
		for (int j = 0; j < rowCount; j++)
		{
			rowIndexes.Add(j);
		}
		((BaseVerticalCollectionView)m_awardListView).itemsSource = null;
		((BaseVerticalCollectionView)m_awardListView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_awardListView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_awardListView, "unity-multi-column-view__header-container", (string)null).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_awardListView, "unity-multi-column-view__header-container", (string)null).style.top = StyleLength.op_Implicit(20f);
	}
}
