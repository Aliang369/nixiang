using System;
using System.Collections.Generic;
using System.Reflection;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatBalanceAward
{
	private enum EBindMode
	{
		Pause,
		Balance
	}

	private zbBaseWnd m_wndParent;

	private VisualElement m_rootElement;

	private VisualElement m_noDataAwardList;

	private MultiColumnListView m_listView;

	private ScrollView m_scrollView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private int rowCount;

	private bool m_columnsBuilt;

	private EBindMode m_bindMode = EBindMode.Balance;

	private readonly List<zbItem> m_cellPool = new List<zbItem>();

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_rootElement = root;
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_rootElement, "awardlist", (string)null);
		m_noDataAwardList = UQueryExtensions.Q<VisualElement>(m_rootElement, "noDataAwardList", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		m_scrollView.horizontalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 120f;
	}

	public void RefreshPauseAward()
	{
		HarvestCellsToPool();
		m_bindMode = EBindMode.Pause;
		EnsureColumns();
		CaclPauseAward();
	}

	private void GeneratePauseAward()
	{
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		zbMainLevelCsvRecord recordByKey = zbDataConfig.Instance.mainLevelCsvTable.GetRecordByKey(1);
		int num = (int)zbCombatSystem.Instance.combatData.combatExp.curLevel + zbCombatSystem.Instance.battleInfo.BattleBaseInfo.InitLevel;
		int[][] baseAward = recordByKey.baseAward;
		foreach (int[] array in baseAward)
		{
			m_listItemDatas.Add(new zbItemData((EResourceType)array[0], array[1], array[2]));
		}
		if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel != 0)
		{
			baseAward = recordByKey.growthAward;
			foreach (int[] array2 in baseAward)
			{
				float num2 = (float)(array2[2] * num) / (float)zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel;
				if (num2 > 0f)
				{
					m_listItemDatas.Add(new zbItemData((EResourceType)array2[0], array2[1], (long)((float)(array2[2] * num) / num2)));
				}
			}
		}
		for (int j = 0; j < 5; j++)
		{
			int num3 = j + 1;
			PropertyInfo property = recordByKey.GetType().GetProperty("spLvl" + num3);
			if (!(property != null))
			{
				continue;
			}
			int num4 = (int)property.GetValue(recordByKey, null);
			if (num - 1 < num4)
			{
				continue;
			}
			PropertyInfo property2 = recordByKey.GetType().GetProperty("spAward" + num3);
			if (property2 != null)
			{
				baseAward = (int[][])property2.GetValue(recordByKey, null);
				foreach (int[] array3 in baseAward)
				{
					m_listItemDatas.Add(new zbItemData((EResourceType)array3[0], array3[1], array3[2]));
				}
			}
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int k = 0; k < rowCount; k++)
		{
			rowIndexes.Add(k);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.top = StyleLength.op_Implicit(-40f);
	}

	public void CaclPauseAward()
	{
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		int num = zbCombatSystem.Instance.combatData.combatExp.curLevel;
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		EBattleFuncType battleType = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType;
		bool flag = battleType == EBattleFuncType.ElementTrialElec || battleType == EBattleFuncType.ElementTrialEnergy || battleType == EBattleFuncType.ElementTrialFire || battleType == EBattleFuncType.ElementTrialIce || battleType == EBattleFuncType.ElementTrialPhy || battleType == EBattleFuncType.ElementTrialWind;
		switch (battleType)
		{
		case EBattleFuncType.MainQuest:
		case EBattleFuncType.EliteQuest:
		case EBattleFuncType.HeroTrialQiang:
		case EBattleFuncType.HeroTrialPao:
		case EBattleFuncType.HeroTrialJiguang:
		case EBattleFuncType.HeroTrialBingdan:
		case EBattleFuncType.HeroTrialRanshaodan:
		case EBattleFuncType.HeroTrialHongzha:
		case EBattleFuncType.HeroTrialDianci:
		case EBattleFuncType.HeroTrialWurenji:
		case EBattleFuncType.HeroTrialMotuo:
		case EBattleFuncType.HeroTrialBingbao:
		case EBattleFuncType.HeroTrialShexian:
		case EBattleFuncType.HeroTrialDianqiu:
		case EBattleFuncType.HeroTrialLongjuan:
		case EBattleFuncType.HeroTrialFengren:
		{
			zbTableBaseRecord zbTableBaseRecord2 = battleType switch
			{
				EBattleFuncType.MainQuest => zbDataConfig.Instance.mainLevelCsvTable.GetRecordByKey(enemyId), 
				EBattleFuncType.EliteQuest => zbDataConfig.Instance.eliteLevelCsvTable.GetRecordByKey(enemyId), 
				_ => (!flag) ? ((zbTableBaseRecord)zbDataConfig.Instance.heroTrialCsvTable.GetRecordByKey(enemyId)) : ((zbTableBaseRecord)zbDataConfig.Instance.elementTrialCsvTable.GetRecordByKey(enemyId)), 
			};
			int[][] fieldIntArray = zbTableBaseRecord2.GetFieldIntArray("baseAward");
			int[][] fieldIntArray2 = zbTableBaseRecord2.GetFieldIntArray("growthAward");
			int[][] array = fieldIntArray;
			foreach (int[] array2 in array)
			{
				int num2 = (((battleType != EBattleFuncType.MainQuest || zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.MainQuest || (array2[0] == 1 && array2[1] == 5)) && (battleType != EBattleFuncType.EliteQuest || zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.EliteQuest || (array2[0] == 1 && array2[1] == 5))) ? 1 : 2);
				m_listItemDatas.Add(new zbItemData((EResourceType)array2[0], array2[1], array2[2] * num2));
			}
			if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel != 0)
			{
				array = fieldIntArray2;
				foreach (int[] array3 in array)
				{
					if (array3[2] * num / zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel > 0)
					{
						m_listItemDatas.Add(new zbItemData((EResourceType)array3[0], array3[1], array3[2] * num / zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel));
					}
				}
			}
			for (int j = 0; j < 4; j++)
			{
				int num3 = j + 1;
				int fieldInt = zbTableBaseRecord2.GetFieldInt("spLvl" + num3);
				if (num - 1 < fieldInt)
				{
					continue;
				}
				int[][] fieldIntArray3 = zbTableBaseRecord2.GetFieldIntArray("spAward" + num3);
				if (fieldIntArray3 != null)
				{
					array = fieldIntArray3;
					foreach (int[] array4 in array)
					{
						m_listItemDatas.Add(new zbItemData((EResourceType)array4[0], array4[1], array4[2]));
					}
				}
			}
			break;
		}
		default:
			switch (battleType)
			{
			case EBattleFuncType.LegionBoss:
			case EBattleFuncType.WorldBoss:
			case EBattleFuncType.WorldLair:
			case EBattleFuncType.WorldLairHelp:
				switch (battleType)
				{
				case EBattleFuncType.LegionBoss:
					m_listItemDatas = zbSelf.Instance.legionHandler.GetLegionBossPauseAwardByKilled(m_listItemDatas);
					break;
				case EBattleFuncType.WorldBoss:
					m_listItemDatas = zbSelf.Instance.worldBossHandler.GetWorldBossPauseAwardByHurt(m_listItemDatas);
					break;
				case EBattleFuncType.WorldLair:
				case EBattleFuncType.WorldLairHelp:
					m_listItemDatas = zbSelf.Instance.worldLairHandler.GetLairPauseAwardByKilled(m_listItemDatas);
					break;
				}
				break;
			case EBattleFuncType.EndlessHorde:
				m_listItemDatas = zbSelf.Instance.endlessHordeHandler.GetEndlessHordePauseAwardByScroll(m_listItemDatas);
				break;
			}
			break;
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int k = 0; k < rowCount; k++)
		{
			rowIndexes.Add(k);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.top = StyleLength.op_Implicit(-40f);
	}

	public void RefreshBalanceAward()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		int num = ((zbSelf.Instance.journeyOneHandler.GetJourneyOneZhanLingDetail() != null || zbSelf.Instance.operateHandler.GetZhanLingKillDetailByUUID() != null) ? 160 : 0);
		if (zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail)
		{
			m_rootElement.style.height = StyleLength.op_Implicit(907f);
			((VisualElement)m_listView).style.height = StyleLength.op_Implicit(388f);
			m_noDataAwardList.style.height = StyleLength.op_Implicit(388f);
		}
		else
		{
			m_rootElement.style.height = StyleLength.op_Implicit((StyleKeyword)2);
			((VisualElement)m_listView).style.height = StyleLength.op_Implicit(680f - (float)num);
			m_noDataAwardList.style.height = StyleLength.op_Implicit(488f);
		}
		((VisualElement)m_scrollView).contentContainer.MarkDirtyRepaint();
		((VisualElement)m_scrollView).schedule.Execute((Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			m_scrollView.scrollOffset = Vector2.zero;
			if (m_scrollView.horizontalScroller != null)
			{
				m_scrollView.horizontalScroller.value = 0f;
			}
		}).StartingIn(1L);
		HarvestCellsToPool();
		m_bindMode = EBindMode.Balance;
		EnsureColumns();
		GenerateDataBalanceAward();
		((BaseVerticalCollectionView)m_listView).Rebuild();
	}

	public void GenerateDataBalanceAward()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.battleAwardList.Count <= 0));
		m_noDataAwardList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.battleAwardList.Count > 0));
		foreach (AwardDetail battleAward in zbCombatSystem.Instance.battleAwardList)
		{
			m_listItemDatas.Add(new zbItemData(battleAward.Type, battleAward.Tid, battleAward.Count));
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int i = 0; i < rowCount; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void EnsureColumns()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		if (m_columnsBuilt)
		{
			return;
		}
		m_columnsBuilt = true;
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(128f),
				makeCell = MakeCellFromPool,
				bindCell = delegate(VisualElement element, int row)
				{
					BindCell(element, row, columnIndex);
				}
			});
		}
	}

	private zbItem MakeCellFromPool()
	{
		int num = m_cellPool.Count - 1;
		if (num >= 0)
		{
			zbItem result = m_cellPool[num];
			m_cellPool.RemoveAt(num);
			return result;
		}
		zbItem obj = new zbItem();
		obj.OnEnable(m_wndParent);
		return obj;
	}

	private void BindCell(VisualElement element, int row, int columnIndex)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int num = row * columnCount + columnIndex;
		if (num < m_listItemDatas.Count)
		{
			element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbItem obj = element as zbItem;
			obj?.Refresh(m_listItemDatas[num]);
			bool vis = m_bindMode == EBindMode.Balance && zbSelf.Instance.truckHandler.bBestPrize;
			obj?.AddBestPrizeTag(vis);
		}
		else
		{
			element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void HarvestCellsToPool()
	{
		m_cellPool.Clear();
		HarvestCellsRecursive((VisualElement)(object)m_listView);
	}

	private void HarvestCellsRecursive(VisualElement element)
	{
		int childCount = element.childCount;
		for (int i = 0; i < childCount; i++)
		{
			VisualElement val = element[i];
			if (val is zbItem item)
			{
				m_cellPool.Add(item);
			}
			else
			{
				HarvestCellsRecursive(val);
			}
		}
	}

	public void SetBackgroundImage(Sprite bg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_rootElement.style.backgroundImage = new StyleBackground(bg);
	}
}
