using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerSuitSkillWnd : zbBaseWnd
{
	private zbPlayerAvatar m_spineAvatar;

	private ListView m_listView;

	private Button m_btnClose;

	[NonSerialized]
	public int m_selectSuitId;

	private VisualElement[] m_nodeIconTops = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteIconTop = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteSelect = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbLevelTop = (Label[])(object)new Label[5];

	private ScrollView m_scrollView;

	private VisualElement[] m_nodeIconDowns = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbLevelDown = (Label[])(object)new Label[5];

	private VisualElement[] m_spriteFrameDown = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteIconDown = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbDesc = (Label[])(object)new Label[5];

	private VisualElement m_spriteTop;

	private VisualElement m_spriteDetail;

	private VisualElement m_spriteDown;

	private Button m_btnTouch;

	private int m_selectIndex;

	private List<int> m_sortDataList = new List<int>();

	private Label m_lbLanwei;

	private Label m_lbCanUse;

	private VisualElement[] m_newSkill = (VisualElement[])(object)new VisualElement[5];

	protected override void OnEnable()
	{
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_spriteTop = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_top", (string)null);
		m_spriteDetail = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_detail", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_spriteDetail, "scrollView", (string)null);
		m_spriteDown = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_Down", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_nodeIconTops[i] = UQueryExtensions.Q<VisualElement>(m_spriteTop, "skill_icon" + i, (string)null);
			m_spriteIconTop[i] = UQueryExtensions.Q<VisualElement>(m_nodeIconTops[i], "icon", (string)null);
			m_spriteSelect[i] = UQueryExtensions.Q<VisualElement>(m_nodeIconTops[i], "bg_select", (string)null);
			m_lbLevelTop[i] = UQueryExtensions.Q<Label>(m_nodeIconTops[i], "lb_level", (string)null);
			Button val = UQueryExtensions.Q<Button>(m_nodeIconTops[i], "btn_select", (string)null);
			int day = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)val, (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSelect(evt, day);
			}, "", (TrickleDown)0);
			m_newSkill[i] = zbUtil.CreateNewTag((VisualElement)(object)val);
			m_newSkill[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeIconDowns[i] = UQueryExtensions.Q<VisualElement>(m_spriteDetail, "skill_detail" + i, (string)null);
			m_spriteFrameDown[i] = UQueryExtensions.Q<VisualElement>(m_nodeIconDowns[i], "empty", (string)null);
			m_spriteIconDown[i] = UQueryExtensions.Q<VisualElement>(m_nodeIconDowns[i], "detail_icon", (string)null);
			m_lbLevelDown[i] = UQueryExtensions.Q<Label>(m_nodeIconDowns[i], "lb_level", (string)null);
			m_lbDesc[i] = UQueryExtensions.Q<Label>(m_nodeIconDowns[i], "lb_desc", (string)null);
			Button visualElement = UQueryExtensions.Q<Button>(m_nodeIconDowns[i], "empty", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSelect(evt, day);
			}, "", (TrickleDown)0);
		}
		m_lbLanwei = UQueryExtensions.Q<Label>(m_RootElement, "lb_lanwei", (string)null);
		m_lbCanUse = UQueryExtensions.Q<Label>(m_RootElement, "lb_canuse", (string)null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuitSkill_Refresh, OnEventUpdateSuitList);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Suit_Refresh, OnEventUpdateSuitList);
	}

	private void OnEventUpdateSuitList(object obj)
	{
		RefreshDetail();
	}

	public void RefreshCustom()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		m_spriteDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		for (int i = 0; i < 5; i++)
		{
			m_spriteSelect[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int avatarId = zbSelf.Instance.GetAvatarId();
		int gunId = zbSelf.Instance.GetGunId();
		m_spineAvatar.RefreshCustom(avatarId, gunId);
		RefreshDetail();
	}

	public void RefreshDetail()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		bool hasNew = zbSelf.Instance.suitGunHandler.HasNewSuitPoint();
		int num = 0;
		foreach (int use in zbServerData.sGameData.SuitInfo.UseList)
		{
			RefreshTopItem(num, use, hasNew);
			num++;
		}
		GetAllSkillList();
		if (!zbSelf.Instance.suitGunHandler.IsSuperSuitOpen())
		{
			m_nodeIconTops[3].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeIconTops[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeIconDowns[3].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeIconDowns[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_nodeIconTops[3].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_nodeIconTops[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_nodeIconDowns[3].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_nodeIconDowns[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		foreach (int superUse in zbServerData.sGameData.SuitInfo.SuperUseList)
		{
			RefreshTopItem(num, superUse, hasNew);
			num++;
		}
		ReorderDownIcons();
	}

	private void RefreshTopItem(int index, int suitId, bool hasNew)
	{
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		if (suitId != 0)
		{
			PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(suitId);
			zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.skillIcon);
			m_spriteIconTop[index].style.backgroundImage = StyleBackground.op_Implicit(val);
			m_spriteIconDown[index].style.backgroundImage = StyleBackground.op_Implicit(val);
			((TextElement)m_lbLevelTop[index]).text = "Lv." + playerSuitDetail.Star;
			((TextElement)m_lbLevelDown[index]).text = "Lv." + playerSuitDetail.Star;
			((TextElement)m_lbDesc[index]).text = zbSelf.Instance.suitGunHandler.GetSkillDesc(suitId, playerSuitDetail.Star);
			m_spriteIconTop[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteIconDown[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbLevelTop[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbLevelDown[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_newSkill[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			string resName = ((recordByKey.grade == 2) ? "ui/image/suitGun/skill_empty1.png" : "ui/image/suitGun/skill_empty.png");
			m_spriteFrameDown[index].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
			float num = ((recordByKey.grade == 2) ? 1f : 0.8f);
			m_spriteIconDown[index].style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num, num)));
		}
		else
		{
			m_spriteIconTop[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			string resName2 = ((index >= 3) ? "ui/image/suitGun/skill_empty1.png" : "ui/image/suitGun/skill_empty.png");
			m_spriteFrameDown[index].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName2));
			m_spriteIconDown[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbLevelTop[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbLevelDown[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbDesc[index]).text = zbUtil.Language("lang.10588");
			m_newSkill[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!hasNew));
		}
		m_newSkill[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void ReorderDownIcons()
	{
		List<int> list = new List<int>();
		int num = 3;
		foreach (int superUse in zbServerData.sGameData.SuitInfo.SuperUseList)
		{
			if (superUse > 0)
			{
				list.Add(num);
			}
			num++;
		}
		int num2 = 0;
		foreach (int use in zbServerData.sGameData.SuitInfo.UseList)
		{
			if (use > 0)
			{
				list.Add(num2);
			}
			num2++;
		}
		if (zbSelf.Instance.suitGunHandler.IsSuperSuitOpen())
		{
			int num3 = 3;
			foreach (int superUse2 in zbServerData.sGameData.SuitInfo.SuperUseList)
			{
				if (superUse2 == 0)
				{
					list.Add(num3);
				}
				num3++;
			}
			int num4 = 0;
			foreach (int use2 in zbServerData.sGameData.SuitInfo.UseList)
			{
				if (use2 == 0)
				{
					list.Add(num4);
				}
				num4++;
			}
		}
		foreach (int item in list)
		{
			((VisualElement)m_scrollView).Add(m_nodeIconDowns[item]);
		}
	}

	public void RefreshList()
	{
		((VisualElement)m_listView).Clear();
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		m_listView.makeItem = () => (VisualElement)(object)new zbPlayerSuitSkillItem(this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbPlayerSuitSkillItem).Init(m_sortDataList[index], m_selectIndex);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_sortDataList;
	}

	private void GetAllSkillList()
	{
		bool flag = m_selectIndex >= 3;
		int[] array = (flag ? ((IEnumerable<int>)zbServerData.sGameData.SuitInfo.SuperUseList).ToArray() : ((IEnumerable<int>)zbServerData.sGameData.SuitInfo.UseList).ToArray());
		m_sortDataList.Clear();
		int num = (flag ? array[m_selectIndex - 3] : array[m_selectIndex]);
		if (num != 0)
		{
			m_sortDataList.Add(num);
		}
		foreach (PlayerSuitDetail suit in zbServerData.sGameData.SuitInfo.SuitList)
		{
			if (suit.SuitId == 1 || num == suit.SuitId || Enumerable.Contains(array, suit.SuitId))
			{
				continue;
			}
			if (flag)
			{
				zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suit.SuitId);
				if (recordByKey != null && recordByKey.grade == 2)
				{
					goto IL_0106;
				}
			}
			if (flag)
			{
				continue;
			}
			zbPlayerSuitCsvRecord recordByKey2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suit.SuitId);
			if (recordByKey2 == null || recordByKey2.grade != 1)
			{
				continue;
			}
			goto IL_0106;
			IL_0106:
			m_sortDataList.Add(suit.SuitId);
		}
		RefreshList();
		((TextElement)m_lbCanUse).text = string.Format(zbUtil.Language("lang.10587"), m_sortDataList.Count);
	}

	private void OnSelect(ClickEvent evt, int day)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		m_spriteDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		string resName = ((day >= 3) ? "ui/image/suitGun/skill_selectbg1.png" : "ui/image/suitGun/skill_selectbg.png");
		m_spriteDown.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		zbSelf.Instance.newTagHandler.RemoveAllNewSuits();
		for (int i = 0; i < 5; i++)
		{
			m_spriteSelect[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((day != i) ? 1 : 0));
			m_newSkill[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_selectIndex = day;
		if (m_selectIndex < 3)
		{
			((TextElement)m_lbLanwei).text = string.Format(zbUtil.Language("lang.10586"), m_selectIndex + 1);
		}
		else
		{
			((TextElement)m_lbLanwei).text = string.Format(zbUtil.Language("lang.10881"), m_selectIndex - 2);
		}
		GetAllSkillList();
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Suit_Refresh);
	}

	private void OnClose()
	{
		Hide();
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
			for (int i = 0; i < 5; i++)
			{
				m_spriteSelect[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spriteDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}
}
