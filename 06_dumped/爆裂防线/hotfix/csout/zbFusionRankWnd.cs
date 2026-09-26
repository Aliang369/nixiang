using System;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFusionRankWnd : zbBaseWnd
{
	private Button m_btnClose;

	private ScrollView m_scrollView;

	private VisualElement m_noListView;

	private VisualElement m_mine;

	private VisualElement m_mineNoRankInfo;

	private Label m_mineLbProgress;

	private Label m_mineLbTime;

	private Label m_mineLbRank;

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
		m_mineLbProgress = UQueryExtensions.Q<Label>(m_mine, "lb_progress", (string)null);
		m_mineLbTime = UQueryExtensions.Q<Label>(m_mine, "lb_time", (string)null);
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

	public void RefreshCustom(RankDetail[] rankList)
	{
		m_rankList = rankList;
		m_selfInfo = rankList.FirstOrDefault((RankDetail v) => v.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId())?.RankData;
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
		((VisualElement)m_mineLbProgress).visible = flag;
		((VisualElement)m_mineLbTime).visible = flag;
		if (flag)
		{
			zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, m_selfInfo.Rank);
			((TextElement)m_mineLbProgress).text = zbResourceHelper.GetMainLevelName((int)m_selfInfo.Value);
			((TextElement)m_mineLbTime).text = zbUtil.FormatUnixTimeToUtcString(m_selfInfo.RankTm);
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
			zbFusionRankItem zbFusionRankItem2 = new zbFusionRankItem();
			zbFusionRankItem2.OnEnable(this);
			zbFusionRankItem2.Refresh(rankDetail);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbFusionRankItem2);
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
