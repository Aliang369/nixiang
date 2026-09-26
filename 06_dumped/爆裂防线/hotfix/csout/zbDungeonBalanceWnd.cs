using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonBalanceWnd : zbBaseWnd
{
	private Label m_lbTitle0;

	private Label m_lbTitle1;

	private Label m_lbAddress;

	private Label m_lbAddressVal;

	private Label m_lbKillCount;

	private Label m_lbKillCountVal;

	private Label m_lbExplore;

	private Label m_lbExploreVal;

	private Label m_lbStatus;

	private VisualElement m_iconAddress;

	private Label m_lbDesc;

	private VisualElement m_noListView;

	private MultiColumnListView m_listView;

	private List<DungeonResultAwardDetail> m_listAwardDetail = new List<DungeonResultAwardDetail>();

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 6;

	private int rowCount;

	private Button m_btnBack;

	private zbDungeonHandler m_dungeonHandler;

	private zbDungeonCsvRecord m_dungeonRecord;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbAddressVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_addressVal", (string)null);
		m_lbKillCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_killCountVal", (string)null);
		m_lbExploreVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_exploreVal", (string)null);
		m_lbStatus = UQueryExtensions.Q<Label>(m_RootElement, "lb_status", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_iconAddress = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_address", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnBack, "", (TrickleDown)0);
	}

	private void OnBack()
	{
		Close();
	}

	private void Close()
	{
		Hide();
		if (m_dungeonRecord != null && m_dungeonRecord.type == 2)
		{
			zbUIMgr.Instance.ShowWorld();
		}
		CheckIsFirstPassResource();
	}

	public void RefreshCustom(zbDungeonHandler dungeonBaseHandler)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		m_dungeonHandler = dungeonBaseHandler;
		m_dungeonRecord = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(m_dungeonHandler.dungeonResultInfo.DungeonId);
		m_listAwardDetail = ((IEnumerable<DungeonResultAwardDetail>)m_dungeonHandler.dungeonResultInfo.AwardList).ToList();
		((TextElement)m_lbAddressVal).text = zbUtil.Language(m_dungeonRecord.name);
		((TextElement)m_lbKillCountVal).text = m_dungeonHandler.dungeonResultInfo.KillCount.ToString();
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(m_dungeonRecord.icon);
		m_iconAddress.style.backgroundImage = new StyleBackground(val);
		((TextElement)m_lbExploreVal).text = m_dungeonHandler.dungeonResultInfo.TriggerCount * 100 / m_dungeonHandler.dungeonResultInfo.EventCount + "%";
		bool flag = m_dungeonHandler.dungeonResultInfo.PassState == EPassState.Passed;
		bool num = m_dungeonHandler.dungeonResultInfo.PassState != EPassState.Failed && m_dungeonHandler.dungeonResultInfo.PassState != EPassState.Passed;
		((VisualElement)m_lbDesc).visible = !flag;
		if (num | flag)
		{
			((TextElement)m_lbStatus).text = zbUtil.Language("lang.10346");
			((VisualElement)m_lbStatus).style.color = new StyleColor(new Color(0f, 0.4392157f, 0.03137255f, 1f));
		}
		else if (!flag)
		{
			((TextElement)m_lbStatus).text = zbUtil.Language("lang.10347");
			((VisualElement)m_lbStatus).style.color = StyleColor.op_Implicit(Color.red);
		}
		RefreshAward();
		((VisualElement)m_lbDesc).visible = CheckHasLossItem();
		if (zbCombatSystem.Instance != null)
		{
			zbCombatSystem.Instance.combatRecorder.ClearRecord();
		}
	}

	private void CheckIsFirstPassResource()
	{
		bool flag = m_dungeonHandler.dungeonResultInfo.PassState == EPassState.Passed;
		if (!flag)
		{
			return;
		}
		zbWorldBuildingCsvRecord buildRecord = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(m_dungeonHandler.dungeonResultInfo.DungeonId);
		if (buildRecord.type != 3)
		{
			return;
		}
		WorldMapGridDetail[] mapGridResList = zbSelf.Instance.worldHandler.GetMapGridResList();
		if (mapGridResList == null)
		{
			return;
		}
		WorldMapGridDetail worldMapGridDetail = mapGridResList.FirstOrDefault((WorldMapGridDetail r) => r.BuildingId == buildRecord.id && r.State == EWorldMapGridState.Explored);
		if (worldMapGridDetail != null)
		{
			WorldBuildingResDetail worldBuildingResDetail = zbSelf.Instance.worldHandler.GetWorldBuildingResDetail(worldMapGridDetail.GridId);
			if (worldBuildingResDetail != null && ((worldBuildingResDetail.ResLevel == 0 && worldMapGridDetail != null && worldMapGridDetail.BuildingId == buildRecord.id) & flag))
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10844"));
				zbUIMgr.Instance.ShowWorldResourceCreate(zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(worldMapGridDetail.GridId));
			}
		}
	}

	private bool CheckHasLossItem()
	{
		foreach (DungeonResultAwardDetail item in m_listAwardDetail)
		{
			if (item.State != EBoolState.True)
			{
				return true;
			}
		}
		return false;
	}

	private void CreateListView()
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
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0031: Unknown result type (might be due to invalid IL or missing references)
					//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					element.style.marginTop = StyleLength.op_Implicit(0f);
					if (num < m_listItemDatas.Count)
					{
						zbItem obj = element.ElementAt(0) as zbItem;
						obj?.Refresh(m_listItemDatas[num]);
						obj.SetLossVisible(m_listAwardDetail[num].State != EBoolState.True);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
	}

	private void RefreshAward()
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		foreach (DungeonResultAwardDetail item in m_listAwardDetail)
		{
			m_listItemDatas.Add(new zbItemData(item.Award.Type, item.Award.Tid, item.Award.Count));
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int i = 0; i < rowCount; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.top = StyleLength.op_Implicit(20f);
		CreateListView();
	}
}
