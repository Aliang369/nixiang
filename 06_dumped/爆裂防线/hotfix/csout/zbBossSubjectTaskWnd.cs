using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBossSubjectTaskWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ListView m_listView;

	private VisualElement m_spriteProgress;

	public BossSubjectDetail bossSubjectDetail;

	private SkeletonAnimation skeletonAnimation;

	private Label[] m_lbCounts = (Label[])(object)new Label[5];

	private Label[] m_lbTargets = (Label[])(object)new Label[5];

	private Label m_lbMainTarget;

	private VisualTreeAsset m_itemTA;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/bossSubject/tb_bossSubjectTask.uxml");
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 5; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "target" + i, (string)null);
			m_lbCounts[i] = UQueryExtensions.Q<Label>(val, "lb_count", (string)null);
			m_lbTargets[i] = UQueryExtensions.Q<Label>(val, "lb_target", (string)null);
		}
		m_lbMainTarget = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "mainTarget", (string)null), "lb_count", (string)null);
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spineScene").Find("SkeletonAnimation")).gameObject;
		skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
		OnEnableList();
		m_gameEevent.RegisterEvent(EGameEvent.Event_BossSubject_Ack, OnEventUpdate);
	}

	private void OnEnableList()
	{
		zbSelf.Instance.bossSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "bossSubjectRenwubg");
		m_listView.makeItem = () => (VisualElement)(object)new zbBossSubjectTaskItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbBossSubjectTaskItem).Refresh(((BaseVerticalCollectionView)m_listView).itemsSource[index] as TaskDetail);
		};
	}

	private void RefresLeftTime()
	{
		((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.timeHandler.GetRefreshLeftTime());
	}

	public void RefreshCustom(BossSubjectDetail _bossSubjectDetail)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		bossSubjectDetail = _bossSubjectDetail;
		UpdateListView();
		RefresLeftTime();
		zbBossTaskTargetCsvRecord zbBossTaskTargetCsvRecord2 = zbSelf.Instance.bossSubjectHandler.m_bossTaskTargetRecords[zbSelf.Instance.bossSubjectHandler.m_bossTaskTargetRecords.Count() - 1];
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length((float)bossSubjectDetail.TaskCount / (float)zbBossTaskTargetCsvRecord2.target * 543f, (LengthUnit)0));
		(zbBossTaskTargetCsvRecord, zbBossTaskTargetCsvRecord) bossTaskTargetRecord = zbSelf.Instance.bossSubjectHandler.GetBossTaskTargetRecord(bossSubjectDetail.TaskCount);
		if (bossTaskTargetRecord.Item2 != null)
		{
			if (bossTaskTargetRecord.Item1 == null)
			{
				((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11360"), bossTaskTargetRecord.Item2.target - bossSubjectDetail.TaskCount, bossTaskTargetRecord.Item2.damageAdd);
			}
			else
			{
				((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11360"), bossTaskTargetRecord.Item2.target - bossSubjectDetail.TaskCount, bossTaskTargetRecord.Item2.damageAdd - bossTaskTargetRecord.Item1.damageAdd);
			}
			((TextElement)m_lbMainTarget).text = bossSubjectDetail.TaskCount + "/" + bossTaskTargetRecord.Item2.target;
		}
		else
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11361");
			((TextElement)m_lbMainTarget).text = "MAX";
		}
		int num = 0;
		foreach (zbBossTaskTargetCsvRecord bossTaskTargetRecord2 in zbSelf.Instance.bossSubjectHandler.m_bossTaskTargetRecords)
		{
			((TextElement)m_lbTargets[num]).text = bossTaskTargetRecord2.target.ToString();
			((TextElement)m_lbCounts[num]).text = "+" + bossTaskTargetRecord2.damageAdd * 100f + "%";
			if (bossSubjectDetail.TaskCount >= bossTaskTargetRecord2.target)
			{
				((VisualElement)m_lbCounts[num]).style.color = StyleColor.op_Implicit(new Color(0f, 83f / 85f, 1f, 1f));
			}
			else
			{
				((VisualElement)m_lbCounts[num]).style.color = StyleColor.op_Implicit(Color.white);
			}
			num++;
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefresLeftTime();
	}

	private void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		((BaseVerticalCollectionView)m_listView).itemsSource = ((IEnumerable<TaskDetail>)bossSubjectDetail.TaskList).OrderBy((TaskDetail x) => x.State switch
		{
			EActiveState.Active => 0, 
			EActiveState.Pending => 1, 
			EActiveState.Done => 2, 
			_ => 999, 
		}).ToList();
	}

	private void OnEventUpdate(object data)
	{
		BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail == null)
		{
			OnClose();
		}
		else
		{
			RefreshCustom(bossSubjectDetail);
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
