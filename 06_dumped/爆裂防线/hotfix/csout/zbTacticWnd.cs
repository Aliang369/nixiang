using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTacticWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lblName;

	private MultiColumnListView m_multiColumnListView;

	private ScrollView m_scrollView;

	private zbTacticUpgradeWnd upgradeWnd;

	private zbTacticInfoWnd infoWnd;

	private zbTacticUxmlFilter m_uxmlFilter;

	private Button m_btnUpgrade;

	private Button m_btnInformation;

	private List<zbTacticItemGroup> m_tacticItemGroups = new List<zbTacticItemGroup>();

	private zbTacticCsvRecord[] m_allRecords;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_lblName = UQueryExtensions.Q<Label>(m_RootElement, "lbl_name", (string)null);
		m_multiColumnListView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "cardGrid", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "groupList", (string)null);
		m_uxmlFilter = new zbTacticUxmlFilter(UQueryExtensions.Q<TemplateContainer>(m_RootElement, "uxml_filter", (string)null));
		m_btnInformation = UQueryExtensions.Q<Button>(m_RootElement, "btn_information", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_multiColumnListView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_multiColumnListView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnInformation, (Action<ClickEvent>)OnInformation, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnUpgrade, "", (TrickleDown)0);
		InitScrollView();
		RefreshGrid(ESkillType.EmptyValue);
		m_uxmlFilter.OnEnable();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnUpgrade(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTacticUpgrade();
	}

	private void OnInformation(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTacticInfo();
	}

	private void OnFilter(ClickEvent evt)
	{
		zbUIMgr.Instance.BubbleInfo("[信息]被点击了");
	}

	private void InitScrollView()
	{
		((VisualElement)m_scrollView).Clear();
		for (int i = 0; i <= 14; i++)
		{
			if (1 != i)
			{
				zbTacticItemGroup zbTacticItemGroup2 = new zbTacticItemGroup(this);
				zbTacticItemGroup2.skillType = (ESkillType)i;
				zbTacticItemGroup2.OnEnable();
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbTacticItemGroup2);
				m_tacticItemGroups.Add(zbTacticItemGroup2);
			}
		}
		m_scrollView.ScrollTo((VisualElement)(object)m_tacticItemGroups.First());
	}

	public void RefreshScrollViewItemsToDefault()
	{
		foreach (zbTacticItemGroup tacticItemGroup in m_tacticItemGroups)
		{
			tacticItemGroup.RefreshStatusToDefault();
		}
	}

	private void RefreshGrid(ESkillType skillType)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		m_allRecords = zbSelf.Instance.tacticHandler.GetAllTacticRecords();
		zbTacticCsvRecord[] records;
		if (skillType == ESkillType.EmptyValue)
		{
			records = m_allRecords;
		}
		else
		{
			records = m_allRecords.Where((zbTacticCsvRecord record) => record.type == (int)skillType).ToArray();
		}
		m_multiColumnListView.columns.Clear();
		int totalItems = records.Length;
		int num = 3;
		for (int num2 = 0; num2 < num; num2++)
		{
			int columnIndex = num2;
			m_multiColumnListView.columns.Add(new Column
			{
				width = Length.op_Implicit(240f),
				stretchable = false,
				makeCell = () => (VisualElement)(object)new zbTacticItemCard(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_004a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0038: Unknown result type (might be due to invalid IL or missing references)
					int num3 = row * 3 + columnIndex;
					if (num3 < totalItems)
					{
						(element as zbTacticItemCard).Refresh(records[num3]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		((BaseVerticalCollectionView)m_multiColumnListView).itemsSource = new List<int>(Enumerable.Range(0, Mathf.CeilToInt((float)totalItems / 3f)));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_multiColumnListView, "unity-multi-column-view__header-container", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	public void OnScrollItemSkillSelected(zbTacticItemGroup item)
	{
		((TextElement)m_lblName).text = GetSkillNameByType(item.skillType);
		RefreshGrid(item.skillType);
	}

	public void OnGridItemCardSelected(zbTacticItemCard item)
	{
		zbUIMgr.Instance.ShowTacticCardDetail();
	}

	public string GetSkillNameByType(ESkillType skillType)
	{
		return skillType switch
		{
			ESkillType.Pao => "炮", 
			ESkillType.Jiguang => "激光", 
			ESkillType.Bingdan => "液氮枪", 
			ESkillType.Ranshaodan => "燃烧弹", 
			ESkillType.Hongzha => "轰炸", 
			ESkillType.Dianci => "电磁脉冲", 
			ESkillType.Wurenji => "无人机", 
			ESkillType.Motuo => "摩托", 
			ESkillType.Bingbao => "冰暴", 
			ESkillType.Shexian => "高能射线", 
			ESkillType.Dianqiu => "球状闪电", 
			ESkillType.Longjuan => "龙卷风", 
			ESkillType.Fengren => "风刃", 
			_ => string.Empty, 
		};
	}

	public string GetSkillImagePathForSkillType(ESkillType skillType)
	{
		return skillType switch
		{
			ESkillType.EmptyValue => "ui/image/tactic/all.png", 
			ESkillType.Pao => "ui/image/tactic/skill/2pao.png", 
			ESkillType.Jiguang => "ui/image/tactic/skill/3jiguang.png", 
			ESkillType.Bingdan => "ui/image/tactic/skill/4bingdan.png", 
			ESkillType.Ranshaodan => "ui/image/tactic/skill/5ranshaodan.png", 
			ESkillType.Hongzha => "ui/image/tactic/skill/6hongzha.png", 
			ESkillType.Dianci => "ui/image/tactic/skill/7dianci.png", 
			ESkillType.Wurenji => "ui/image/tactic/skill/8wurenji.png", 
			ESkillType.Motuo => "ui/image/tactic/skill/9motuo.png", 
			ESkillType.Bingbao => "ui/image/tactic/skill/10bingbao.png", 
			ESkillType.Shexian => "ui/image/tactic/skill/11shexian.png", 
			ESkillType.Dianqiu => "ui/image/tactic/skill/12dianqiu.png", 
			ESkillType.Longjuan => "ui/image/tactic/skill/13longjuan.png", 
			ESkillType.Fengren => "ui/image/tactic/skill/14fengren.png", 
			_ => string.Empty, 
		};
	}
}
