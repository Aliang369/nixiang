using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderBalanceWnd : zbTabWnd
{
	private VisualElement m_bg;

	private VisualElement m_bgScore;

	private VisualElement m_bgDamage;

	private SpineVisualElement m_spineWin;

	private SpineVisualElement m_spineFail;

	private Label m_lbWin;

	private Label m_lbFail;

	private Button m_btnBack;

	private zbCrossLadderBalanceTabScore m_scorePanel;

	private zbCombatBalanceDamage m_damagePanel;

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private float balanceTime;

	public bool m_isFail;

	protected override void Awake()
	{
		base.Awake();
		m_scorePanel = new zbCrossLadderBalanceTabScore();
		m_damagePanel = new zbCombatBalanceDamage();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_bgScore = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_score", (string)null);
		m_bgDamage = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_damage", (string)null);
		m_spineWin = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_win", (string)null);
		m_spineFail = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_fail", (string)null);
		m_lbWin = UQueryExtensions.Q<Label>(m_RootElement, "lb_win", (string)null);
		m_lbFail = UQueryExtensions.Q<Label>(m_RootElement, "lb_fail", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		m_scorePanel.OnEnable(this, m_tabLayers[0]);
		m_damagePanel.OnEnable(this, m_tabLayers[1]);
		InitTab(m_tabButtons, m_tabLayers, null);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		zbUIMgr.Instance.DestroyWnd(EWndId.ModWheel);
		zbUIMgr.Instance.DestroyWnd(EWndId.SelectMod);
		zbUIMgr.Instance.DestroyWnd(EWndId.ModSelectAction);
		balanceTime = 0f;
		m_isFail = zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail;
		((VisualElement)m_spineWin).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_spineFail).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbWin).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbFail).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_scorePanel.Refresh();
		m_damagePanel.Refresh();
		if (!m_isFail)
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_winBg.png"));
			m_bgScore.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_winTxt.png"));
			m_bgDamage.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_winTxt.png"));
			((MonoBehaviour)this).StartCoroutine(PlayWinEffect());
		}
		else
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_failBg.png"));
			m_bgScore.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_failTxt.png"));
			m_bgDamage.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_failTxt.png"));
			((MonoBehaviour)this).StartCoroutine(PlayFailEffect());
		}
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
	}

	private IEnumerator PlayWinEffect()
	{
		yield return null;
		yield return null;
		((VisualElement)m_spineWin).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
		TrackEntry val = m_spineWin.AnimationState.SetAnimation(0, "in", false);
		if (val != null)
		{
			yield return (object)new WaitForSecondsRealtime(val.Animation.Duration);
		}
		((VisualElement)m_lbWin).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineWin.AnimationState.SetAnimation(0, "loop", true);
		yield return null;
	}

	private IEnumerator PlayFailEffect()
	{
		yield return null;
		yield return null;
		((VisualElement)m_spineFail).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
		TrackEntry val = m_spineFail.AnimationState.SetAnimation(0, "in", false);
		if (val != null)
		{
			yield return (object)new WaitForSecondsRealtime(val.Animation.Duration);
		}
		((VisualElement)m_lbFail).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineFail.AnimationState.SetAnimation(0, "loop", true);
		yield return null;
	}

	public override void DoSelectTab()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			m_scorePanel.Refresh();
			break;
		case 1:
			m_damagePanel.Refresh();
			break;
		}
		for (int i = 0; i < 2; i++)
		{
			if (m_currTab == i)
			{
				((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_tab1.png"));
			}
			else
			{
				((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_balance_tab0.png"));
			}
			((VisualElement)m_tabButtons[i]).style.opacity = StyleFloat.op_Implicit(1f);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
		zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
		zbSelf.Instance.crossLadderHandler.isUpgradeAck = false;
		zbUIMgr.Instance.ShowCrossLadder();
		if (zbCombatSystem.Instance.battleResultInfo.BattleType == EBattleFuncType.CrossLadderMatch && zbCombatSystem.Instance.battleAwardList.Count > 0)
		{
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)zbCombatSystem.Instance.battleAwardList).ToArray());
		}
	}

	protected override void UpdateAtShow()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		balanceTime += Time.unscaledDeltaTime;
		if ((double)balanceTime >= 0.5)
		{
			((VisualElement)m_btnBack).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}
}
