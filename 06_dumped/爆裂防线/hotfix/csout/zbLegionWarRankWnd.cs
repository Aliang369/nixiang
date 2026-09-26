using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarRankWnd : zbTabWnd
{
	private zbSpineScene m_spineScene;

	private Transform[] m_nodeTops = (Transform[])(object)new Transform[3];

	private zbPlayerMechaAvatar[] m_spinePlayerMecha = new zbPlayerMechaAvatar[3];

	private VisualElement[] m_tops = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_noAvatars = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbTopNames = (Label[])(object)new Label[3];

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	public Button[] m_tabButtons = (Button[])(object)new Button[2];

	private Button m_btnAward;

	private Button m_btnClose;

	private VisualElement m_spriteTabSel;

	private zbLegionWarRankTabPlayer m_modulePlayer;

	private zbLegionWarRankTabLegion m_moduleLegion;

	private zbLegionWarZonePanel m_legionWarZone = new zbLegionWarZonePanel();

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + i, (string)null);
		}
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		for (int j = 0; j < 3; j++)
		{
			m_nodeTops[j] = ((Component)m_GameObjectSceneRoot.transform.Find($"top{j + 1}")).GetComponent<Transform>();
			m_spinePlayerMecha[j] = ((Component)m_nodeTops[j]).GetComponent<zbPlayerMechaAvatar>();
			m_tops[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"top{j + 1}", (string)null);
			m_noAvatars[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "no_avatar" + (j + 1), (string)null);
			m_lbTopNames[j] = UQueryExtensions.Q<Label>(m_tops[j], "lb_name", (string)null);
			int tempIndex = j;
			m_uiEvent.RegisterCallback<ClickEvent>(m_tops[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnHead(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "legionZonePullDown", (string)null);
		m_legionWarZone.OnEnable(this, root);
		((VisualElement)m_legionWarZone.btn_clickItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_award", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action)OnAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, OnSelect);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarRankInfo, OnEventUpdate);
	}

	public void RefreshCustom()
	{
	}

	private void OnSelect(int index)
	{
		m_currTab = index;
	}

	public override void DoSelectTab()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		SpriteRenderer componentInChildren = ((Component)m_spineScene).GetComponentInChildren<SpriteRenderer>();
		Vector3 position = ((Component)componentInChildren).transform.position;
		switch (m_currTab)
		{
		case 0:
			if (m_modulePlayer == null)
			{
				m_modulePlayer = new zbLegionWarRankTabPlayer();
				m_modulePlayer.OnEnable(this, m_tabLayers[m_currTab]);
			}
			zbWsMsgHandler.Instance.LegionWar_PlayerRankList_Req();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			m_spineScene.SetSceneBg("ui/image/legionWar/legionWar_rank_bg.png");
			position.y = 0f;
			((Component)componentInChildren).transform.position = position;
			m_spineScene.SetSkeletonDataAsset("animation/ui/paihangbang_bg5/paihangbang_bg5_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		case 1:
			if (m_moduleLegion == null)
			{
				m_moduleLegion = new zbLegionWarRankTabLegion();
				m_moduleLegion.OnEnable(this, m_tabLayers[m_currTab]);
			}
			zbWsMsgHandler.Instance.LegionWar_LegionRankList_Req();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			m_spineScene.SetSceneBg("ui/image/legionWar/legionWarRankLegion_bg.png");
			position.y = 6.16f;
			((Component)componentInChildren).transform.position = position;
			m_spineScene.SetSkeletonDataAsset("animation/ui/juntuanpaihang_bg/juntuanpaihang_bg_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		}
		VisibleTops();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
	}

	private void OnAward()
	{
		long num = 0L;
		long num2 = 0L;
		if (zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck != null)
		{
			LegionWarPlayerRankDetail legionWarPlayerRankDetail = ((IEnumerable<LegionWarPlayerRankDetail>)zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck.RankList).FirstOrDefault((LegionWarPlayerRankDetail a) => a.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId());
			if (legionWarPlayerRankDetail != null)
			{
				num = legionWarPlayerRankDetail.RankData.Rank;
			}
		}
		if (zbSelf.Instance.legionWarHandler.legionWarLegionRankListAck != null)
		{
			LegionWarLegionRankDetail legionWarLegionRankDetail = ((IEnumerable<LegionWarLegionRankDetail>)zbSelf.Instance.legionWarHandler.legionWarLegionRankListAck.RankList).FirstOrDefault((LegionWarLegionRankDetail a) => a.LegionFlagInfo.LegionId == zbServerData.sGameData.LegionSelfInfo.LegionId);
			if (legionWarLegionRankDetail != null)
			{
				num2 = legionWarLegionRankDetail.RankData.Rank;
			}
		}
		zbUIMgr.Instance.ShowLegionWarRankAward((int)num, (int)num2, m_currTab);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		if (m_currTab == 0)
		{
			m_modulePlayer.RefreshCustom();
		}
		else if (m_currTab == 1)
		{
			m_moduleLegion.RefreshCustom();
		}
		RefreshTops();
	}

	private void VisibleTops()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_currTab == 0;
		for (int i = 0; i < 3; i++)
		{
			((Component)m_nodeTops[i]).gameObject.SetActive(flag ? true : false);
			m_tops[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			m_noAvatars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	public void RefreshTops()
	{
		m_legionWarZone.Refresh();
		if (m_currTab != 0)
		{
			return;
		}
		RepeatedField<LegionWarPlayerRankDetail> rankList = zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck.RankList;
		for (int i = 0; i < 3; i++)
		{
			bool flag = rankList.Count > i;
			((Component)m_nodeTops[i]).gameObject.SetActive(flag);
			m_noAvatars[i].visible = !flag;
			if (flag)
			{
				m_spinePlayerMecha[i].RefreshCustom(rankList[i].RoleBaseInfo.AvatarId, rankList[i].RoleBaseInfo.GunId, rankList[i].RoleBaseInfo.MechaId);
				m_spinePlayerMecha[i].RefreshRankPosition(i, rankList[i].RoleBaseInfo.MechaId);
				((TextElement)m_lbTopNames[i]).text = rankList[i].RoleBaseInfo.RoleName;
			}
			else
			{
				((TextElement)m_lbTopNames[i]).text = "";
			}
		}
	}

	private void OnHead(ClickEvent evt, int index)
	{
		if (zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck.RankList.Count > index)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck.RankList[index].RoleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10499"));
		}
	}
}
