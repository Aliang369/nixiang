using System;
using System.Collections;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderUpgradeWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private SpineVisualElement m_spine;

	private Label m_lbTitle;

	private zbLadderItem m_itemLadder;

	private Label m_lbLadderName;

	private Label m_lbOverPct;

	private Label m_lbFriend;

	private Button m_btnConfirm;

	private bool m_actionAdd;

	private Coroutine m_stateCoroutine;

	private VisualElement m_friend;

	private VisualElement[] m_playerInfo = (VisualElement[])(object)new VisualElement[5];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouchTest, "", (TrickleDown)0);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_itemLadder = UQueryExtensions.Q<zbLadderItem>(m_RootElement, "ladder", (string)null);
		m_lbLadderName = UQueryExtensions.Q<Label>(m_RootElement, "lb_ladderName", (string)null);
		m_lbOverPct = UQueryExtensions.Q<Label>(m_RootElement, "lb_overPct", (string)null);
		m_lbFriend = UQueryExtensions.Q<Label>(m_RootElement, "lb_friend", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnTouchTest, "", (TrickleDown)0);
		m_itemLadder.OnEnable(this);
		m_friend = UQueryExtensions.Q<VisualElement>(m_RootElement, "m_friend", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_playerInfo[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "playerInfo" + (i + 1), (string)null);
		}
		AddDark();
	}

	public void RefreshCustom(float OverPct)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbTitle).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spine.AnimationState.SetAnimation(0, "enter", false);
		m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		m_actionAdd = false;
		m_itemLadder.Refresh(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId, visName: false, isAction: true, zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId - 1);
		zbLadderCsvRecord recordByKey = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId);
		((TextElement)m_lbLadderName).text = recordByKey.name;
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)this).StartCoroutine(FadeEnterLoop());
		((TextElement)m_lbOverPct).text = string.Format(zbUtil.Language("lang.11094"), OverPct * 100f);
		for (int i = 0; i < 5; i++)
		{
			m_playerInfo[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_friend.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.crossLadderHandler.crossLadderFriendRankList == null)
		{
			return;
		}
		int num = 0;
		CrossLadderFriendDetail[] crossLadderFriendRankList = zbSelf.Instance.crossLadderHandler.crossLadderFriendRankList;
		foreach (CrossLadderFriendDetail crossLadderFriendDetail in crossLadderFriendRankList)
		{
			if (zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId == crossLadderFriendDetail.LadderId + 1 && crossLadderFriendDetail.RoleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
			{
				m_friend.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_playerInfo[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				InitPlayerInfo(m_playerInfo[num], crossLadderFriendDetail.RoleBaseInfo, num);
				num++;
			}
			if (num >= 5)
			{
				break;
			}
		}
	}

	private IEnumerator FadeEnterLoop()
	{
		yield return (object)new WaitForSeconds(0.5f);
		((VisualElement)m_lbTitle).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return (object)new WaitForSeconds(17f / 30f);
		m_actionAdd = true;
	}

	public void InitPlayerInfo(VisualElement element, RoleBaseInfo roleBaseInfo, int index)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		Button visualElement = UQueryExtensions.Q<Button>(element, "btn_player", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate
		{
			OnPlayer(roleBaseInfo);
		}, "", (TrickleDown)0);
		zbPlayerHeadCsvRecord recordByKey = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(roleBaseInfo.HeadId);
		if (recordByKey != null)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
			UQueryExtensions.Q<VisualElement>(element, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		}
		zbPlayerHeadFrameCsvRecord recordByKey2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(roleBaseInfo.HeadFrameId);
		if (recordByKey2 != null)
		{
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon);
			UQueryExtensions.Q<VisualElement>(element, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		}
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_level", (string)null)).text = roleBaseInfo.Level.ToString();
	}

	private void OnPlayer(RoleBaseInfo roleBaseInfo)
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)roleBaseInfo.RoleId);
	}

	private void OnTouchTest()
	{
		if (m_actionAdd)
		{
			Hide();
		}
	}
}
