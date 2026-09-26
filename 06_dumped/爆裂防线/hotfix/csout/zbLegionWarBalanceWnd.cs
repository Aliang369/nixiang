using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarBalanceWnd : zbBaseWnd
{
	private VisualElement m_bg;

	private SpineVisualElement m_winEffect;

	private Label m_lbResult;

	private Label m_lbLoseResult;

	private Label m_lbDesc;

	private VisualElement m_bgScore;

	private Label m_lbScore;

	private VisualElement m_bgScoreStreak;

	private Label m_lbKillStreak;

	private Label m_lbScoreStreak;

	private Button m_btnConfirm;

	private Sprite m_winBg;

	private Sprite m_failBg;

	private int[] m_killSeriesScoreList;

	private int m_showScoreStreak;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_winEffect = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "winEffect", (string)null);
		m_lbResult = UQueryExtensions.Q<Label>(m_RootElement, "lb_result", (string)null);
		m_lbLoseResult = UQueryExtensions.Q<Label>(m_RootElement, "lb_loseResult", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_bgScore = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_score", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>(m_RootElement, "lb_score", (string)null);
		m_bgScoreStreak = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_scoreStreak", (string)null);
		m_lbKillStreak = UQueryExtensions.Q<Label>(m_RootElement, "lb_killStreak", (string)null);
		m_lbScoreStreak = UQueryExtensions.Q<Label>(m_RootElement, "lb_scoreStreak", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_winBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWar_bg_win.png");
		m_failBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWar_bg_fail.png");
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		JArray source = (JArray)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["attack"][(object)"killSeriesScore"];
		m_killSeriesScoreList = ((IEnumerable<JToken>)source).Select((JToken x) => Extensions.Value<int>((IEnumerable<JToken>)x)).ToArray();
		m_showScoreStreak = m_killSeriesScoreList.Select((int value, int i) => new { value, i }).FirstOrDefault(x => x.value > 0)?.i ?? (-1);
	}

	public override void Refresh()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		zbUIMgr.Instance.DestroyWnd(EWndId.ModWheel);
		zbUIMgr.Instance.DestroyWnd(EWndId.SelectMod);
		zbUIMgr.Instance.DestroyWnd(EWndId.ModSelectAction);
		bool flag = zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail;
		m_bg.style.backgroundImage = new StyleBackground(flag ? m_failBg : m_winBg);
		((VisualElement)m_winEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (!flag)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
			((MonoBehaviour)this).StartCoroutine(PlayWinEffect());
		}
		((VisualElement)m_lbLoseResult).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbResult).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (flag)
		{
			m_bgScoreStreak.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_bgScore.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/legionWar_balance_fail.jpg"));
			((TextElement)m_lbScore).text = "+" + (object)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["attack"][(object)"failScore"];
			((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.11242"), 1);
		}
		else
		{
			m_bgScoreStreak.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.legionWarHandler.legionWarSelfInfo.KillStreak < m_showScoreStreak + 1) ? 1 : 0));
			((TextElement)m_lbKillStreak).text = string.Format(zbUtil.Language("lang.11249"), zbSelf.Instance.legionWarHandler.legionWarSelfInfo.KillStreak);
			int num = Mathf.Min(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.KillStreak - 1, m_killSeriesScoreList.Length - 1);
			((TextElement)m_lbScoreStreak).text = "+" + m_killSeriesScoreList[num];
			m_bgScore.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_bestRank_cell1.jpg"));
			((TextElement)m_lbScore).text = "+" + (object)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["attack"][(object)"winScore"];
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.11241");
		}
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
	}

	private IEnumerator PlayWinEffect()
	{
		yield return null;
		yield return null;
		((VisualElement)m_winEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
		TrackEntry val = m_winEffect.AnimationState.SetAnimation(0, "in", false);
		if (val != null)
		{
			yield return (object)new WaitForSecondsRealtime(val.Animation.Duration);
		}
		m_winEffect.AnimationState.SetAnimation(0, "loop", true);
		((VisualElement)m_lbResult).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
		zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
		zbUIMgr.Instance.ShowLegionWarFight();
	}
}
