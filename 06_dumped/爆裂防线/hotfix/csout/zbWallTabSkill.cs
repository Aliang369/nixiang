using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWallTabSkill
{
	private zbWallWnd m_wndParent;

	private VisualElement m_RootElement;

	private ListView m_listView;

	public int m_selectWallId;

	private VisualElement[] m_spriteIconTop = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_spriteSelect = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbLevelTop = (Label[])(object)new Label[3];

	private Label[] m_lbLevelDown = (Label[])(object)new Label[3];

	private VisualElement[] m_spriteIconDown = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbDesc = (Label[])(object)new Label[3];

	private Button[] m_btnsPreview = (Button[])(object)new Button[3];

	private VisualElement m_spriteTop;

	private VisualElement m_spriteDetail;

	private VisualElement m_spriteDown;

	private Button m_btnTouch;

	private int m_selectIndex;

	private List<int> m_sortDataList = new List<int>();

	private Label m_lbLanwei;

	private Label m_lbCanUse;

	private VisualElement[] m_newSkill = (VisualElement[])(object)new VisualElement[3];

	private bool m_isFirstIn = true;

	public void OnEnable(zbWallWnd wndParent, VisualElement root)
	{
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_spriteTop = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_top", (string)null);
		m_spriteDetail = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_detail", (string)null);
		m_spriteDown = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_Down", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_spriteIconTop[i] = UQueryExtensions.Q<VisualElement>(m_spriteTop, "icon" + i, (string)null);
			m_spriteSelect[i] = UQueryExtensions.Q<VisualElement>(m_spriteTop, "bg_select" + i, (string)null);
			m_lbLevelTop[i] = UQueryExtensions.Q<Label>(m_spriteTop, "lb_level" + i, (string)null);
			Button val = UQueryExtensions.Q<Button>(m_spriteTop, "btn_select" + i, (string)null);
			int day = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)val, (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSelect(evt, day);
			}, "", (TrickleDown)0);
			m_newSkill[i] = zbUtil.CreateNewTag((VisualElement)(object)val);
			m_newSkill[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteIconDown[i] = UQueryExtensions.Q<VisualElement>(m_spriteDetail, "detail_icon" + i, (string)null);
			m_lbLevelDown[i] = UQueryExtensions.Q<Label>(m_spriteDetail, "lb_level" + i, (string)null);
			m_lbDesc[i] = UQueryExtensions.Q<Label>(m_spriteDetail, "lb_desc" + i, (string)null);
			Button visualElement = UQueryExtensions.Q<Button>(m_spriteDetail, "empty" + i, (string)null);
			m_btnsPreview[i] = UQueryExtensions.Q<Button>(m_spriteDetail, "btn_preview" + i, (string)null);
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsPreview[i], (Action)delegate
			{
				OnPreview(day);
			}, "", (TrickleDown)0);
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSelect(evt, day);
			}, "", (TrickleDown)0);
		}
		m_lbLanwei = UQueryExtensions.Q<Label>(m_RootElement, "lb_lanwei", (string)null);
		m_lbCanUse = UQueryExtensions.Q<Label>(m_RootElement, "lb_canuse", (string)null);
	}

	public void Refresh()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent.RefreshDetail(zbServerData.sGameData.WallInfo.WallId);
		if (m_isFirstIn)
		{
			m_spriteDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			for (int i = 0; i < 3; i++)
			{
				m_spriteSelect[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_isFirstIn = false;
		}
		RefreshDetail();
	}

	public void RefreshDetail()
	{
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.wallHandler.HasNewPoint();
		int num = 0;
		foreach (int use in zbServerData.sGameData.WallInfo.UseList)
		{
			if (use != 0)
			{
				WallDetail wallDetail = zbSelf.Instance.wallHandler.GetWallDetail(use);
				zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(use);
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.skillIcon);
				m_spriteIconTop[num].style.backgroundImage = StyleBackground.op_Implicit(val);
				m_spriteIconDown[num].style.backgroundImage = StyleBackground.op_Implicit(val);
				((TextElement)m_lbLevelTop[num]).text = "Lv." + wallDetail.Star;
				((TextElement)m_lbLevelDown[num]).text = "Lv." + wallDetail.Star;
				((TextElement)m_lbDesc[num]).text = zbSelf.Instance.wallHandler.GetSkillDesc(use, wallDetail.Star);
				m_spriteIconTop[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spriteIconDown[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbLevelTop[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbLevelDown[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spriteIconTop[num].style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)1);
				m_newSkill[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_btnsPreview[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				m_spriteIconTop[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_spriteIconDown[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbLevelTop[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbLevelDown[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_btnsPreview[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((TextElement)m_lbDesc[num]).text = zbUtil.Language("lang.10588");
				m_newSkill[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			}
			num++;
		}
		GetAllSkillList();
	}

	public void RefreshList()
	{
		((VisualElement)m_listView).Clear();
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		m_listView.makeItem = () => (VisualElement)(object)new zbWallTabSkillItem(m_wndParent);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbWallTabSkillItem).Init(m_sortDataList[index], m_selectIndex);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_sortDataList;
	}

	private void GetAllSkillList()
	{
		m_sortDataList.Clear();
		int num = zbServerData.sGameData.WallInfo.UseList[m_selectIndex];
		if (num != 0)
		{
			m_sortDataList.Add(num);
		}
		foreach (WallDetail wall in zbServerData.sGameData.WallInfo.WallList)
		{
			if (wall.WallId != 1 && num != wall.WallId && !zbServerData.sGameData.WallInfo.UseList.Contains(wall.WallId))
			{
				m_sortDataList.Add(wall.WallId);
			}
		}
		RefreshList();
		((TextElement)m_lbCanUse).text = string.Format(zbUtil.Language("lang.10587"), m_sortDataList.Count);
	}

	private void OnSelect(ClickEvent evt, int day)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		m_spriteDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		zbSelf.Instance.newTagHandler.RemoveAllNewWalls();
		for (int i = 0; i < 3; i++)
		{
			m_spriteSelect[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((day != i) ? 1 : 0));
			m_newSkill[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_selectIndex = day;
		((TextElement)m_lbLanwei).text = string.Format(zbUtil.Language("lang.10586"), m_selectIndex + 1);
		GetAllSkillList();
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WallInfo);
	}

	private void OnPreview(int index)
	{
		zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(zbServerData.sGameData.WallInfo.UseList[index], EPreviewType.Wall);
	}

	private void OnTouch()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteDown.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			for (int i = 0; i < 3; i++)
			{
				m_spriteSelect[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}
}
