using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarScoreWnd : zbBaseWnd
{
	public VisualTreeAsset m_TreeAsset;

	private List<LimitTimeHeroScoreDetail> m_listItemDatas = new List<LimitTimeHeroScoreDetail>();

	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Label m_lbLeftScore;

	private Label m_lbDesc;

	private ListView m_listView;

	public LimitTimeHeroDetail m_timeHeroDetail;

	private Dictionary<int, int> m_batchCountMap = new Dictionary<int, int>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbLeftScore = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftScore", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateData);
	}

	public void RefreshCustom(LimitTimeHeroDetail timeHeroDetail)
	{
		m_timeHeroDetail = zbSelf.Instance.operateHandler.GetTimeHeroInfoByUUID(timeHeroDetail.BaseInfo.UUID);
		if (m_timeHeroDetail == null)
		{
			return;
		}
		((TextElement)m_lbLeftScore).text = zbUtil.Language("lang.10900") + ": " + m_timeHeroDetail.BaseData.LeftScore;
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10300"), m_timeHeroDetail.BaseData.OnceScore);
		m_listItemDatas.Clear();
		((VisualElement)m_listView).Clear();
		if ((Object)(object)m_TreeAsset == (Object)null)
		{
			return;
		}
		foreach (LimitTimeHeroScoreDetail score in m_timeHeroDetail.ScoreList)
		{
			m_listItemDatas.Add(score);
		}
		m_TreeAsset.Instantiate();
		m_listView.makeItem = () => (VisualElement)(object)new zbBarScoreItem(this, m_TreeAsset);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_listItemDatas[index] != null)
			{
				(element as zbBarScoreItem).Refresh(m_listItemDatas[index], m_timeHeroDetail.BaseData.LeftScore, m_timeHeroDetail.BaseInfo.UUID);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnUpdateData(object data)
	{
		RefreshCustom(m_timeHeroDetail);
	}

	public int GetBatchCount(int index)
	{
		if (m_batchCountMap.TryGetValue(index, out var value))
		{
			return value;
		}
		return 1;
	}

	public void SetBatchCount(int index, int count)
	{
		m_batchCountMap[index] = count;
	}
}
