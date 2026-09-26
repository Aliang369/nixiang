using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestViewWnd : zbBaseWnd
{
	public class ArrayComparer : IEqualityComparer<int[]>
	{
		public bool Equals(int[] x, int[] y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			if (x.Length < 2 || y.Length < 2)
			{
				return false;
			}
			if (x[0] == y[0])
			{
				return x[1] == y[1];
			}
			return false;
		}

		public int GetHashCode(int[] obj)
		{
			if (obj == null || obj.Length < 2)
			{
				return 0;
			}
			return (17 * 31 + obj[0].GetHashCode()) * 31 + obj[1].GetHashCode();
		}
	}

	private Button m_btnClose;

	private Button m_btnAllClose;

	private Label m_lbLevelName;

	private VisualElement m_panel0;

	private Button m_tab0;

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private SpineVisualElement[] m_spineMonster = (SpineVisualElement[])(object)new SpineVisualElement[3];

	private Label[] m_LbMonsterName = (Label[])(object)new Label[3];

	private VisualElement[] m_seals = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_btnTouch = (Button[])(object)new Button[3];

	private MultiColumnListView m_listView;

	private Button m_btnGoto;

	private List<int> rowIndexes = new List<int>();

	private int[] monsterIds;

	private int[][] m_record;

	private int columnCount = 5;

	private int rowCount;

	private int m_level;

	private bool m_isMain;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnAllClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_allClose", (string)null);
		m_lbLevelName = UQueryExtensions.Q<Label>(m_RootElement, "lb_levelName", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goTo", (string)null);
		m_panel0 = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel0", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_panel0, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		for (int i = 0; i < 3; i++)
		{
			int index = i;
			m_spineMonster[i] = UQueryExtensions.Q<SpineVisualElement>(m_panel0, "spine_monster" + i, (string)null);
			m_LbMonsterName[i] = UQueryExtensions.Q<Label>(m_panel0, "lb_monsterName" + i, (string)null);
			m_btnTouch[i] = UQueryExtensions.Q<Button>(m_panel0, "btn_touch" + i, (string)null);
			m_seals[i] = UQueryExtensions.Q<VisualElement>(m_panel0, "seal" + i, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch[i], (Action)delegate
			{
				OnShowMonsterDetail(index);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoTo, "", (TrickleDown)0);
		AddDark();
	}

	private void OnGoTo()
	{
		if (m_isMain)
		{
			zbUIMgr.Instance.ShowMainLevel(0, m_level);
		}
		else
		{
			zbUIMgr.Instance.ShowEliteQuest(m_level);
		}
	}

	public void RefreshCustom(int[][] _record, int[] _monsterids, int level, bool isMain)
	{
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		m_record = _record;
		m_isMain = isMain;
		m_level = level;
		((TextElement)m_lbLevelName).text = zbResourceHelper.GetMainLevelName(level);
		bool visible = zbUIMgr.Instance.IsShow(EWndId.QuestList) && (isMain ? (zbServerData.sGameData.MainQuestInfo.MainLevel >= level - 1) : (zbServerData.sGameData.EliteQuestInfo.EliteLevel >= level));
		((VisualElement)m_btnGoto).visible = visible;
		monsterIds = _monsterids;
		for (int i = 0; i < 3; i++)
		{
			bool flag = i < monsterIds.Length;
			m_seals[i].visible = !flag;
			((VisualElement)m_LbMonsterName[i]).visible = flag;
			((VisualElement)m_spineMonster[i]).visible = flag;
			((VisualElement)m_btnTouch[i]).visible = flag;
			if (flag)
			{
				zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterIds[i]);
				((TextElement)m_LbMonsterName[i]).text = recordByKey.name.ToString();
				zbMonsterAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
				string shadow = recordByKey2.shadow;
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(shadow);
				VisualElement obj = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_LbMonsterName[i], "yingzi", (string)null);
				obj.style.scale = new StyleScale(new Vector2(recordByKey.bodyScale, recordByKey.bodyScale));
				IStyle style = UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style;
				Rect rect = val.rect;
				style.width = StyleLength.op_Implicit(((Rect)(ref rect)).width);
				IStyle style2 = UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style;
				rect = val.rect;
				style2.height = StyleLength.op_Implicit(((Rect)(ref rect)).height);
				UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style.backgroundImage = new StyleBackground(val);
				IStyle style3 = obj.style;
				rect = val.rect;
				float height = ((Rect)(ref rect)).height;
				rect = val.rect;
				style3.top = StyleLength.op_Implicit((height > ((Rect)(ref rect)).width) ? 105f : 163f);
				string spinePath = recordByKey2.fightBone + "_SkeletonData.asset";
				zbUtil.RefreshSpineUI(m_spineMonster[i], spinePath, "move", loop: true);
			}
		}
		RefreshAwards();
	}

	public void RefreshAwards()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		if (m_record == null)
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
				makeCell = () => (VisualElement)(object)new zbItem(),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
					//IL_0081: Unknown result type (might be due to invalid IL or missing references)
					//IL_0092: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
					//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
					int num2 = row * columnCount + columnIndex;
					if (num2 < m_record.Count())
					{
						int[] array = m_record[num2];
						zbItem obj = element as zbItem;
						obj.OnEnable(this);
						obj.Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
						((VisualElement)UQueryExtensions.Q<Button>((VisualElement)(object)obj, "touch", (string)null)).style.height = StyleLength.op_Implicit(120f);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
						((VisualElement)obj).style.marginTop = StyleLength.op_Implicit(5f);
						((VisualElement)obj).style.marginBottom = StyleLength.op_Implicit(5f);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_record.Count() / (float)columnCount);
		for (int num = 0; num < rowCount; num++)
		{
			rowIndexes.Add(num);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.height = StyleLength.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	private void OnShowMonsterDetail(int _index)
	{
		zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterIds[_index]);
		zbUIMgr.Instance.ShowPreviewMonsterDetail(recordByKey);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
