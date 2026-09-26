using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaBestRankWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private VisualElement m_ui;

	private SpineVisualElement m_spine;

	private VisualElement m_nodeRank;

	private Label m_lbLastRank;

	private Label m_lbNewRank;

	private VisualElement m_nodeDaily;

	private Label m_lbLastDaily;

	private Label m_lbNewDaily;

	private VisualElement m_nodeBestRank;

	private zbItem m_item;

	private long m_diamondCount;

	private Coroutine m_stateCoroutine;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnTouchTest, "", (TrickleDown)0);
		m_ui = UQueryExtensions.Q<VisualElement>(m_RootElement, "ui", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_nodeRank = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_rank", (string)null);
		m_lbLastRank = UQueryExtensions.Q<Label>(m_nodeRank, "lb_last", (string)null);
		m_lbNewRank = UQueryExtensions.Q<Label>(m_nodeRank, "lb_new", (string)null);
		m_nodeDaily = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_daily", (string)null);
		m_lbLastDaily = UQueryExtensions.Q<Label>(m_nodeDaily, "lb_last", (string)null);
		m_lbNewDaily = UQueryExtensions.Q<Label>(m_nodeDaily, "lb_new", (string)null);
		m_nodeBestRank = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_best", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_nodeBestRank, "item", (string)null);
		m_item.OnEnable(this);
		InitUI();
		AddDark();
	}

	private void InitUI()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		m_ui.style.opacity = StyleFloat.op_Implicit(0f);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_ui.style.transitionProperty = StyleList<StylePropertyName>.op_Implicit(new List<StylePropertyName>
		{
			new StylePropertyName("opacity")
		});
		m_ui.style.transitionDuration = StyleList<TimeValue>.op_Implicit(new List<TimeValue>
		{
			new TimeValue(0.5f)
		});
	}

	public void RefreshCustom(long diamondCount)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		m_diamondCount = diamondCount;
		zbUIMgr.Instance.ShowModel();
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshDetail();
		m_spine.AnimationState.SetAnimation(0, "enter", false);
		m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)this).StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		yield return (object)new WaitForSeconds(1.3f);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
		m_ui.style.opacity = StyleFloat.op_Implicit(1f);
		yield return (object)new WaitForSeconds(0.5f);
		zbUIMgr.Instance.HideModel();
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void RefreshDetail()
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbLastRank).text = zbSelf.Instance.arenaHandler.lastRank.ToString();
		((TextElement)m_lbNewRank).text = zbServerData.sGameData.ArenaInfo.Rank.ToString();
		((TextElement)m_lbLastDaily).text = zbSelf.Instance.arenaHandler.GetDailyArenaCoin(zbSelf.Instance.arenaHandler.lastRank).ToString();
		((TextElement)m_lbNewDaily).text = zbSelf.Instance.arenaHandler.GetDailyArenaCoin(zbServerData.sGameData.ArenaInfo.Rank).ToString();
		if (zbServerData.sGameData.ArenaInfo.BestRank < zbSelf.Instance.arenaHandler.lastBestRank)
		{
			m_nodeBestRank.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_item.Refresh(new zbItemData(EResourceType.Money, 2L, m_diamondCount));
		}
		else
		{
			m_nodeBestRank.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnTouchTest()
	{
		Hide();
		zbUIMgr.Instance.ShowAward(new AwardDetail[1]
		{
			new AwardDetail
			{
				Type = EResourceType.Money,
				Tid = 2L,
				Count = m_diamondCount
			}
		});
	}
}
