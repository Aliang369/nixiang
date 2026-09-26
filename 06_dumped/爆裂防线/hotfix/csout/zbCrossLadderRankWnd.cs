using System;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderRankWnd : zbBaseWnd
{
	private Button m_btnClose;

	private ScrollView m_scrollView;

	private VisualElement m_noListView;

	private VisualElement m_mine;

	private Label m_mineLbRank;

	private zbLadderItem m_itemLadder;

	private CrossLadderFriendDetail[] m_rankList;

	private CrossLadderFriendDetail m_selfInfo;

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
		m_itemLadder = UQueryExtensions.Q<zbLadderItem>(m_RootElement, "ladder", (string)null);
		m_itemLadder.OnEnable(this);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_mineLbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 3; i++)
		{
			m_nodeTops[i] = ((Component)m_GameObjectSceneRoot.transform.Find($"top{i + 1}")).GetComponent<Transform>();
			m_spinePlayerMecha[i] = ((Component)m_nodeTops[i]).GetComponent<zbPlayerMechaAvatar>();
			m_tops[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"top{i + 1}", (string)null);
			m_noAvatars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "no_avatar" + (i + 1), (string)null);
			m_lbTopNames[i] = UQueryExtensions.Q<Label>(m_tops[i], "lb_name", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>(m_tops[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnHead(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_rankBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legion/rank_bg.png");
		m_rankBgMine = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/worldBoss/rank_bg1.png");
	}

	public void RefreshCustom()
	{
		m_rankList = zbSelf.Instance.crossLadderHandler.crossLadderFriendRankList;
		m_selfInfo = m_rankList.FirstOrDefault((CrossLadderFriendDetail v) => v.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId());
		int num = 0;
		CrossLadderFriendDetail[] crossLadderFriendRankList = zbSelf.Instance.crossLadderHandler.crossLadderFriendRankList;
		foreach (CrossLadderFriendDetail obj in crossLadderFriendRankList)
		{
			num++;
			if (obj.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId())
			{
				break;
			}
		}
		RefreshTops();
		UpdateListView();
		RefreshSelfInfo(num);
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

	private void RefreshSelfInfo(int index)
	{
		RoleBaseInfo roleBaseInfo = zbServerData.sGameData.RoleBaseInfo;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_mine, roleBaseInfo);
		zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, index);
		m_itemLadder.Refresh(m_selfInfo.LadderId);
	}

	private void UpdateListView()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_scrollView).Clear();
		bool flag = m_rankList.Length != 0;
		((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		int num = 0;
		CrossLadderFriendDetail[] rankList = m_rankList;
		foreach (CrossLadderFriendDetail rankDetail in rankList)
		{
			zbCrossLadderRankItem zbCrossLadderRankItem2 = new zbCrossLadderRankItem();
			zbCrossLadderRankItem2.OnEnable(this);
			zbCrossLadderRankItem2.Refresh(rankDetail, num + 1);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbCrossLadderRankItem2);
			num++;
		}
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
