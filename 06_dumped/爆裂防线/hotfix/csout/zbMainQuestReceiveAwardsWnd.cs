using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestReceiveAwardsWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnConfirm;

	private Label m_lbName;

	private VisualElement m_defense;

	private VisualElement m_progress;

	private VisualElement m_bg;

	private Button[] m_btns = (Button[])(object)new Button[3];

	private SpineVisualElement[] m_spines = (SpineVisualElement[])(object)new SpineVisualElement[3];

	private bool m_isMain;

	private int m_curLevel;

	private MultiColumnListView m_listView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private List<AwardDetail> m_listAwardDetails = new List<AwardDetail>();

	private int columnCount = 6;

	private int rowCount;

	private MainQuestScoreAwardAck m_mainQuestScoreAwardAck;

	private EliteQuestScoreAwardAck m_eliteQuestScoreAwardAck;

	private MainQuestDetail m_roomDetail;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_defense = UQueryExtensions.Q<VisualElement>(m_RootElement, "defense", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn_box{num}", (string)null);
			m_spines[i] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btns[i], $"box{num}", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Quest_Receive, OnUpdateEventQuestRecevie);
		CreateListView();
	}

	public void RefreshCustom(bool isMain, int curLevel, Action<bool> callback)
	{
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		m_isMain = isMain;
		m_curLevel = curLevel;
		m_callback = callback;
		m_mainQuestScoreAwardAck = null;
		m_eliteQuestScoreAwardAck = null;
		if (m_isMain)
		{
			((TextElement)m_lbName).text = zbResourceHelper.GetMainLevelName(m_curLevel);
		}
		else
		{
			((TextElement)m_lbName).text = zbResourceHelper.GetEliteLevelName(m_curLevel);
		}
		m_roomDetail = null;
		if (!m_isMain)
		{
			_ = m_curLevel;
			_ = zbServerData.sGameData.EliteQuestInfo.NextLevel;
		}
		else
		{
			_ = m_curLevel;
			_ = zbServerData.sGameData.MainQuestInfo.NextLevel;
		}
		m_roomDetail = (m_isMain ? zbSelf.Instance.mainQuestHandler.GetMainQuestDetail(m_curLevel) : zbSelf.Instance.eliteQuestHandler.GetEliteQuestDetail(m_curLevel));
		if (!m_isMain)
		{
			zbSelf.Instance.eliteQuestHandler.GetQuestProgress(m_curLevel);
		}
		else
		{
			zbSelf.Instance.mainQuestHandler.GetQuestProgress(m_curLevel);
		}
		for (int i = 0; i < 3; i++)
		{
			m_spines[i].AnimationState.SetAnimation(0, "1", false);
		}
		m_progress.style.width = StyleLength.op_Implicit(new Length(m_roomDetail.DefensePct, (LengthUnit)1));
		((TextElement)UQueryExtensions.Q<Label>(m_defense, "lb_successDesc", (string)null)).text = $"{Mathf.Floor(m_roomDetail.DefensePct)}%";
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_listItemDatas.Clear();
		((MonoBehaviour)this).StartCoroutine(ReciveReward());
	}

	private void OnUpdateEventQuestRecevie(object obj)
	{
		if (obj is MainQuestScoreAwardAck mainQuestScoreAwardAck)
		{
			m_mainQuestScoreAwardAck = mainQuestScoreAwardAck;
			m_listAwardDetails = ((IEnumerable<AwardDetail>)mainQuestScoreAwardAck.AwardList).ToList();
		}
		if (obj is EliteQuestScoreAwardAck eliteQuestScoreAwardAck)
		{
			m_eliteQuestScoreAwardAck = eliteQuestScoreAwardAck;
			m_listAwardDetails = ((IEnumerable<AwardDetail>)eliteQuestScoreAwardAck.AwardList).ToList();
		}
		((MonoBehaviour)this).StartCoroutine(delayShowAward());
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Quest_Refresh);
	}

	private IEnumerator delayShowAward()
	{
		yield return (object)new WaitForSeconds(0.3f);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (zbUIMgr.Instance.IsShow(EWndId.MainQuestReceiveAward) && m_listAwardDetails.Count > 0)
		{
			SetAnim();
			RefreshAward();
		}
	}

	private void CreateListView()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(105f),
				makeCell = delegate
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_001a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					//IL_002f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0036: Unknown result type (might be due to invalid IL or missing references)
					//IL_0040: Unknown result type (might be due to invalid IL or missing references)
					//IL_0047: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					//IL_0058: Unknown result type (might be due to invalid IL or missing references)
					//IL_007f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0084: Unknown result type (might be due to invalid IL or missing references)
					//IL_0089: Unknown result type (might be due to invalid IL or missing references)
					//IL_0093: Unknown result type (might be due to invalid IL or missing references)
					//IL_009b: Expected O, but got Unknown
					VisualElement val = new VisualElement();
					val.style.width = StyleLength.op_Implicit(105f);
					val.style.height = StyleLength.op_Implicit(105f);
					val.style.alignItems = StyleEnum<Align>.op_Implicit((Align)2);
					val.style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)1);
					val.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
					zbItem zbItem2 = new zbItem();
					zbItem2.OnEnable(this);
					((VisualElement)zbItem2).style.scale = StyleScale.op_Implicit(new Scale(new Vector2(0.8f, 0.8f)));
					val.Add((VisualElement)(object)zbItem2);
					return val;
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0031: Unknown result type (might be due to invalid IL or missing references)
					//IL_0087: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					element.style.marginTop = StyleLength.op_Implicit(0f);
					if (num < m_listItemDatas.Count)
					{
						(element.ElementAt(0) as zbItem)?.Refresh(m_listItemDatas[num]);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
	}

	private void RefreshAward()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listAwardDetails.Count <= 0));
		foreach (AwardDetail listAwardDetail in m_listAwardDetails)
		{
			m_listItemDatas.Add(new zbItemData(listAwardDetail.Type, listAwardDetail.Tid, listAwardDetail.Count));
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int i = 0; i < rowCount; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.top = StyleLength.op_Implicit(20f);
	}

	private IEnumerator ReciveReward()
	{
		if (m_isMain)
		{
			zbWsMsgHandler.Instance.MainQuest_ScoreAward_Req(m_curLevel);
		}
		else
		{
			zbWsMsgHandler.Instance.EliteQuest_ScoreAward_Req(m_curLevel);
		}
		yield return (object)new WaitForSeconds(2f);
		if (m_listAwardDetails.Count == 0)
		{
			OnClose();
		}
	}

	private void SetAnim()
	{
		int score = (int)m_roomDetail.Score;
		int scoreAward = (int)m_roomDetail.ScoreAward;
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			if (num <= score && num > scoreAward)
			{
				m_spines[i].AnimationState.SetAnimation(0, "3", false);
			}
			else if (num <= scoreAward)
			{
				m_spines[i].AnimationState.SetAnimation(0, "4", false);
			}
		}
	}

	private void OnClose()
	{
		if (m_mainQuestScoreAwardAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(m_mainQuestScoreAwardAck.DataChange, ((IEnumerable<AwardDetail>)m_mainQuestScoreAwardAck.AwardList).ToArray());
			m_mainQuestScoreAwardAck = null;
		}
		else if (m_eliteQuestScoreAwardAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(m_eliteQuestScoreAwardAck.DataChange, ((IEnumerable<AwardDetail>)m_eliteQuestScoreAwardAck.AwardList).ToArray());
			m_eliteQuestScoreAwardAck = null;
		}
		m_listAwardDetails = new List<AwardDetail>();
		m_listItemDatas = new List<zbItemData>();
		m_roomDetail = null;
		Hide();
		m_callback?.Invoke(obj: true);
	}
}
