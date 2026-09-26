using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonFormationWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbCost;

	private Button m_btnBack;

	private Button m_btnComfirm;

	private MultiColumnListView m_listView;

	private Dictionary<int, int> m_dictSelectHero = new Dictionary<int, int>
	{
		{ 0, 1 },
		{ 1, 0 },
		{ 2, 0 },
		{ 3, 0 },
		{ 4, 0 }
	};

	private List<int> rowIndexes = new List<int>();

	private List<zbHeroPreview> m_previewHeroes = new List<zbHeroPreview>();

	private zbSpineScene m_spineScene;

	private int m_param1;

	private EDifficultyLevel m_param2;

	private zbDungeonHandler m_dungeonHandler;

	private EDungeonType m_type;

	private VisualElement m_eleBottom;

	private int m_costTili;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_eleBottom = UQueryExtensions.Q<VisualElement>(m_RootElement, "main", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnComfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_comfirm", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).mode = (ScrollViewMode)0;
		m_previewHeroes.Clear();
		for (int i = 0; i < 5; i++)
		{
			zbHeroPreview component = zbUtil.FindGameObjectRecursive(((Component)this).transform, "hero" + i).GetComponent<zbHeroPreview>();
			m_previewHeroes.Add(component);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnComfirm, (Action)OnComfirm, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Tili });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Dungeon_Open_Ack, OnEventDungeonOpen);
		m_spineScene = m_GameObjectSceneRoot.GetComponentInChildren<zbSpineScene>();
		m_uiEvent.RegisterCallback<PointerDownEvent>(m_eleBottom, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)1);
	}

	public void RefreshCustom(zbDungeonHandler dungeonBaseHandler, EDungeonType type, int param1, EDifficultyLevel param2)
	{
		m_dungeonHandler = dungeonBaseHandler;
		m_param1 = param1;
		m_param2 = param2;
		m_type = type;
		m_dictSelectHero[0] = 1;
		m_dictSelectHero[1] = 0;
		m_dictSelectHero[2] = 0;
		m_dictSelectHero[3] = 0;
		m_dictSelectHero[4] = 0;
		int num = 1;
		foreach (int use in zbServerData.sGameData.SuitInfo.UseList)
		{
			if (use > 0)
			{
				zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(use);
				if (recordByKey != null)
				{
					m_dictSelectHero[num] = recordByKey.skillType;
					num++;
				}
			}
		}
		int key = 1;
		if (param2 > EDifficultyLevel.EmptyValue)
		{
			WorldMapGridDetail worldMapGridDetailByGridId = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(param1);
			if (worldMapGridDetailByGridId != null)
			{
				if (worldMapGridDetailByGridId.BuildingId > 0)
				{
					zbWorldBuildingCsvRecord recordByKey2 = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(worldMapGridDetailByGridId.BuildingId);
					if (recordByKey2 != null)
					{
						key = recordByKey2.easyDungeonId;
					}
				}
				else
				{
					WorldEventDetail dungeonEventDetail = zbSelf.Instance.worldHandler.GetDungeonEventDetail();
					if (dungeonEventDetail != null && dungeonEventDetail.GridId == param1)
					{
						zbWorldBuildingCsvRecord recordByKey3 = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(dungeonEventDetail.EventId);
						if (recordByKey3 != null)
						{
							key = recordByKey3.easyDungeonId;
						}
					}
				}
			}
		}
		zbDungeonCsvRecord recordByKey4 = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(key);
		if (recordByKey4 != null)
		{
			m_spineScene.Load(recordByKey4.sceneId);
			m_costTili = recordByKey4.costTili;
			((TextElement)m_lbCost).text = "x" + recordByKey4.costTili;
		}
		UpdateListView();
		RefreshHero();
	}

	private void RefreshHero()
	{
		for (int i = 0; i < 5; i++)
		{
			if (m_dictSelectHero.TryGetValue(i, out var value) && value != 0)
			{
				m_previewHeroes[i].SetHeroActive(active: true);
				m_previewHeroes[i].Refresh(value);
			}
			else
			{
				m_previewHeroes[i].SetHeroActive(active: false);
			}
		}
	}

	private void OnClose()
	{
		Hide();
		if (m_type != EDungeonType.HomeFloor)
		{
			zbUIMgr.Instance.ShowWorld();
		}
	}

	private void OnComfirm()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, int> item in m_dictSelectHero)
		{
			if (item.Value > 0)
			{
				list.Add(item.Value);
			}
		}
		if (list.Count < 5)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10315"));
		}
		else if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_costTili))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, m_costTili);
		}
		else
		{
			m_dungeonHandler.OpenDungeon_Req(m_param1, m_param2, list.ToArray());
		}
	}

	private void UpdateListView()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		rowIndexes.Clear();
		List<zbHeroCsvRecord> roomHeros = zbSelf.Instance.heroHandler.GetHeroList().roomList;
		if (roomHeros != null && roomHeros.Count > 1)
		{
			zbHeroCsvRecord item = roomHeros[0];
			List<zbHeroCsvRecord> collection = (from h in roomHeros.Skip(1)
				orderby h.rarity descending
				select h).ToList();
			roomHeros.Clear();
			roomHeros.Add(item);
			roomHeros.AddRange(collection);
		}
		int columnCount = 4;
		int num = Mathf.CeilToInt((float)roomHeros.Count / (float)columnCount);
		for (int num2 = 0; num2 < num; num2++)
		{
			rowIndexes.Add(num2);
		}
		m_listView.columns.Clear();
		for (int num3 = 0; num3 < columnCount; num3++)
		{
			int columnIndex = num3;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(160f),
				makeCell = () => (VisualElement)(object)new zbDungeonFormationItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0073: Unknown result type (might be due to invalid IL or missing references)
					//IL_0061: Unknown result type (might be due to invalid IL or missing references)
					int num4 = row * columnCount + columnIndex;
					if (num4 < roomHeros.Count)
					{
						(element as zbDungeonFormationItem)?.Refresh(roomHeros[num4], m_dictSelectHero);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		((VisualElement)m_listView).visible = rowIndexes.Count > 0;
		VisualElement val = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null);
		if (val != null)
		{
			val.style.opacity = StyleFloat.op_Implicit(0f);
			val.style.top = StyleLength.op_Implicit(-40f);
		}
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	public void OnToggle(zbDungeonFormationItem item, bool select)
	{
		if (item.GetHeroRecord().type != 1)
		{
			if (select)
			{
				AddHero(item);
			}
			else
			{
				RemoveHero(item);
			}
		}
	}

	public void AddHero(zbDungeonFormationItem item)
	{
		if (m_dictSelectHero.Count((KeyValuePair<int, int> kvp) => kvp.Value != 0) >= 5)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10316"));
			return;
		}
		foreach (KeyValuePair<int, int> item2 in m_dictSelectHero)
		{
			if (item2.Value == 0)
			{
				m_dictSelectHero[item2.Key] = item.GetHeroRecord().type;
				item.SetSelectVisible(visible: true);
				RefreshHero();
				break;
			}
		}
	}

	public void RemoveHero(zbDungeonFormationItem item)
	{
		foreach (KeyValuePair<int, int> item2 in m_dictSelectHero)
		{
			if (item2.Value == item.GetHeroRecord().type)
			{
				m_dictSelectHero[item2.Key] = 0;
				item.SetSelectVisible(visible: false);
				RefreshHero();
				break;
			}
		}
	}

	public void OnEventDungeonOpen(object obj)
	{
		Hide();
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		zbHeroPreview zbHeroPreview2 = PickHero(evt);
		if ((Object)(object)zbHeroPreview2 != (Object)null && zbHeroPreview2.record != null)
		{
			SetAlertRandgeVisible(zbHeroPreview2);
		}
	}

	public zbHeroPreview PickHero(PointerDownEvent evt)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.8f;
		float num2 = 1f;
		float num3 = 0.2f;
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		foreach (zbHeroPreview previewHero in m_previewHeroes)
		{
			Vector3 position = ((Component)previewHero).transform.position;
			float y = position.y;
			float num4 = num * 0.5f;
			if (val.x >= position.x - num4 && val.x <= position.x + num4 && val.y >= y - num3 && val.y <= y + num2)
			{
				return previewHero;
			}
		}
		return null;
	}

	public void SetAlertRandgeVisible(zbHeroPreview hero)
	{
		zbUIMgr.Instance.ShowHeroDetail(hero.record);
	}
}
