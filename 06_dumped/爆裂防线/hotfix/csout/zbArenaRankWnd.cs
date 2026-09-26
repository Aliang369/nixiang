using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaRankWnd : zbBaseWnd
{
	private Button m_btnClose;

	private ScrollView m_scrollView;

	private VisualElement m_noListView;

	private VisualElement m_mine;

	private VisualElement m_mineNoRankInfo;

	private Label m_mineLbRank;

	private VisualElement[] m_spriteFrames = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_spriteIcons = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_btnTouchs = (Button[])(object)new Button[4];

	private RankDetail[] m_rankList;

	private RankData m_selfInfo;

	private Transform[] m_nodeTops = (Transform[])(object)new Transform[3];

	private zbPlayerMechaAvatar[] m_spinePlayerMecha = new zbPlayerMechaAvatar[3];

	private VisualElement[] m_tops = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_noAvatars = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbTopNames = (Label[])(object)new Label[3];

	public Sprite m_rankBg;

	public Sprite m_rankBgMine;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_mineNoRankInfo = UQueryExtensions.Q<VisualElement>(m_mine, "noRankInfo", (string)null);
		m_mineLbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_spriteFrames[i] = UQueryExtensions.Q<VisualElement>(m_mine, "frame" + i, (string)null);
			m_spriteIcons[i] = UQueryExtensions.Q<VisualElement>(m_spriteFrames[i], "icon", (string)null);
			m_btnTouchs[i] = UQueryExtensions.Q<Button>(m_spriteFrames[i], "touch", (string)null);
			int currentIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchs[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnTouchs(evt, currentIndex);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		for (int num = 0; num < 3; num++)
		{
			m_nodeTops[num] = ((Component)m_GameObjectSceneRoot.transform.Find($"top{num + 1}")).GetComponent<Transform>();
			m_spinePlayerMecha[num] = ((Component)m_nodeTops[num]).GetComponent<zbPlayerMechaAvatar>();
			m_tops[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"top{num + 1}", (string)null);
			m_noAvatars[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "no_avatar" + (num + 1), (string)null);
			m_lbTopNames[num] = UQueryExtensions.Q<Label>(m_tops[num], "lb_name", (string)null);
			int tempIndex = num;
			m_uiEvent.RegisterCallback<ClickEvent>(m_tops[num], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnHead(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_rankBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legion/rank_bg.png");
		m_rankBgMine = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/worldBoss/rank_bg1.png");
	}

	public void RefreshCustom(ArenaRankListAck rankListAck)
	{
		m_rankList = ((IEnumerable<RankDetail>)rankListAck.RankList).ToArray();
		m_selfInfo = rankListAck.SelfRank;
		RefreshTops();
		UpdateListView();
		RefreshSelfInfo();
	}

	private void RefreshTops()
	{
		for (int i = 0; i < 3; i++)
		{
			bool flag = m_rankList.Length > i;
			((Component)m_nodeTops[i]).gameObject.SetActive(flag);
			m_noAvatars[i].visible = !flag;
			if (flag)
			{
				m_spinePlayerMecha[i].RefreshCustom(m_rankList[i].RoleBaseInfo.AvatarId, m_rankList[i].RoleBaseInfo.GunId, m_rankList[i].RoleBaseInfo.MechaId);
				m_spinePlayerMecha[i].RefreshRankPosition(i, m_rankList[i].RoleBaseInfo.MechaId);
				((TextElement)m_lbTopNames[i]).text = m_rankList[i].RoleBaseInfo.RoleName;
			}
			else
			{
				((TextElement)m_lbTopNames[i]).text = "";
			}
		}
	}

	private void RefreshSelfInfo()
	{
		RoleBaseInfo roleBaseInfo = zbServerData.sGameData.RoleBaseInfo;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_mine, roleBaseInfo);
		bool flag = m_selfInfo != null;
		m_mineNoRankInfo.visible = !flag;
		if (flag)
		{
			zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, (int)m_selfInfo.Value);
		}
		else
		{
			zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, zbServerData.sGameData.ArenaInfo.Rank);
		}
	}

	private void UpdateListView()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_scrollView).Clear();
		bool flag = m_rankList.Length != 0;
		((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		RankDetail[] rankList = m_rankList;
		foreach (RankDetail rankDetail in rankList)
		{
			zbArenaRankItem zbArenaRankItem2 = new zbArenaRankItem();
			zbArenaRankItem2.Refresh(rankDetail);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbArenaRankItem2);
		}
	}

	private void OnTouchs(ClickEvent evt, int index)
	{
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnHead(ClickEvent evt, int index)
	{
		if (m_rankList.Length > index)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankList[index].RoleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10499"));
		}
	}
}
