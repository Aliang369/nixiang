using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSkillFightModWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Label m_lbTitle;

	private ListView m_listView;

	private Button[] m_btns = (Button[])(object)new Button[5];

	private Button m_btnClose;

	private Button m_btnTouch;

	private VisualElement m_noListView;

	private int m_curIndex;

	private SkillDetail[] m_heroList;

	private List<int> m_modList;

	private Sprite sp_select;

	private Sprite sp_unSelect;

	private bool isResumeFight;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/combat/tb_fightMod.uxml");
		AddDark();
		sp_select = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_138x73_1.png");
		sp_unSelect = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_138x73_2.png");
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "ListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 160f;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "nolistview", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_skill" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btns[i], (Action)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(int skillIndex, SkillDetail[] heroList, List<int> modList, bool _isResumeFight)
	{
		m_curIndex = skillIndex;
		if (heroList.Count() <= skillIndex)
		{
			m_curIndex = skillIndex - 1;
		}
		m_modList = modList;
		isResumeFight = _isResumeFight;
		m_heroList = heroList;
		List<zbModCsvRecord> userSelectModList = GetUserSelectModList(m_curIndex);
		InitBtns();
		UpdateListView(userSelectModList);
	}

	private void OnSelect(int index)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (index != m_curIndex)
		{
			((VisualElement)m_btns[m_curIndex]).style.backgroundImage = new StyleBackground(sp_unSelect);
			((VisualElement)m_btns[index]).style.backgroundImage = new StyleBackground(sp_select);
			m_curIndex = index;
			List<zbModCsvRecord> userSelectModList = GetUserSelectModList(m_curIndex);
			UpdateListView(userSelectModList);
		}
	}

	public List<zbModCsvRecord> GetUserSelectModList(int index)
	{
		List<zbModCsvRecord> list = new List<zbModCsvRecord>();
		if (m_heroList.Length == 0)
		{
			return list;
		}
		if (index >= m_heroList.Length)
		{
			index = 0;
		}
		ESkillType skillType = m_heroList[index].SkillType;
		foreach (int mod in m_modList)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(mod);
			if (recordByKey.skillLvUp == 1 && Enumerable.Contains(recordByKey.skillType, (int)skillType))
			{
				list.Add(recordByKey);
			}
		}
		return list;
	}

	private void UpdateListView(List<zbModCsvRecord> items)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listView).Clear();
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		bool flag = items.Count > 0;
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		m_listView.makeItem = () => (VisualElement)(object)new zbSkillFightModItem(m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbSkillFightModItem).Init(items[index], m_heroList[m_curIndex].SkillId);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
	}

	private void OnClose()
	{
		Hide();
		if (isResumeFight && zbUIMgr.Instance.IsShow(EWndId.Combat))
		{
			zbCombatSystem.Instance.Resume();
		}
	}

	private void InitBtns()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < m_heroList.Length;
			((VisualElement)m_btns[i]).visible = flag;
			if (flag)
			{
				List<zbModCsvRecord> userSelectModList = GetUserSelectModList(i);
				((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btns[i], "icon_skill", (string)null)).text = userSelectModList.Count.ToString();
				((VisualElement)m_btns[i]).style.backgroundImage = ((i == m_curIndex) ? new StyleBackground(sp_select) : new StyleBackground(sp_unSelect));
				zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(m_heroList[i].SkillId);
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
				((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btns[i], "icon_skill", (string)null)).style.backgroundImage = new StyleBackground(val);
			}
		}
	}
}
