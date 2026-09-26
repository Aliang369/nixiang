using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerSuitUpgradePreviewWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private ScrollView m_scrollView;

	public int m_id;

	public EPreviewType m_type;

	public int isCurrent;

	public List<int> skillIdList = new List<int>();

	public List<int> firstIndexes = new List<int>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.horizontalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(int id, EPreviewType type, int index)
	{
		m_id = id;
		m_type = type;
		isCurrent = index;
		UpdateListView();
	}

	private void UpdateListView()
	{
		skillIdList.Clear();
		int num = 5;
		HashSet<int> hashSet = new HashSet<int>();
		if (m_type == EPreviewType.Foothold)
		{
			for (int i = 0; i < 10; i++)
			{
				zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(m_id);
				zbFootholdEffectCsvRecord footholdEffectRecord = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(m_id, i + 1);
				if (hashSet.Add(footholdEffectRecord.id))
				{
					skillIdList.Add(footholdEffectRecord.id);
					firstIndexes.Add(i);
				}
			}
		}
		skillIdList = skillIdList.Distinct().ToList();
		((VisualElement)m_scrollView).Clear();
		for (int j = 0; j < num; j++)
		{
			zbPlayerSuitUpgradePreviewItem zbPlayerSuitUpgradePreviewItem2 = new zbPlayerSuitUpgradePreviewItem(this);
			zbPlayerSuitUpgradePreviewItem2.Init(j);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbPlayerSuitUpgradePreviewItem2);
		}
	}

	private void OnClose()
	{
		Hide();
		if (zbCombatSystem.Instance != null && zbGameStateMachine.Instance.IsFighting())
		{
			zbCombatSystem.Instance.Resume();
			zbAudioMgr.Instance.ResumeAllAudio();
		}
	}
}
