using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGemComposeWnd : zbBaseWnd
{
	private MultiColumnListView m_listView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private Button m_btnTouchTest;

	private Button m_btnCompose;

	private Button m_btnClose;

	private int columnCount = 5;

	private int rowCount;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnCompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_compose", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCompose, (Action)OnCompse, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gem, OnEventUpdateGem);
	}

	public void RefreshCustom(ResourceDetail[] previewList)
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
				makeCell = delegate
				{
					zbItem obj = new zbItem();
					obj.OnEnable(this);
					return (VisualElement)(object)obj;
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_006d: Unknown result type (might be due to invalid IL or missing references)
					//IL_005b: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbItem)?.Refresh(m_listItemDatas[num]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		GenerateData(previewList);
	}

	private void GenerateData(ResourceDetail[] previewList)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		foreach (ResourceDetail resourceDetail in previewList)
		{
			m_listItemDatas.Add(new zbItemData(EResourceType.Gem, resourceDetail.Tid, resourceDetail.Count));
		}
		m_listItemDatas.Sort(delegate(zbItemData a, zbItemData b)
		{
			EQuality resourceQuality = zbResourceHelper.GetResourceQuality(EResourceType.Gem, a.id);
			int num2 = zbResourceHelper.GetResourceQuality(EResourceType.Gem, b.id).CompareTo(resourceQuality);
			if (num2 != 0)
			{
				return num2;
			}
			int type = zbDataConfig.Instance.gemCsvTable.GetRecordByKey((int)a.id).type;
			int type2 = zbDataConfig.Instance.gemCsvTable.GetRecordByKey((int)b.id).type;
			num2 = type.CompareTo(type2);
			return (num2 != 0) ? num2 : a.id.CompareTo(b.id);
		});
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int num = 0; num < rowCount; num++)
		{
			rowIndexes.Add(num);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	private void OnCompse()
	{
		zbWsMsgHandler.Instance.Gem_Compose_Req();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdateGem(object data)
	{
		Hide();
	}
}
