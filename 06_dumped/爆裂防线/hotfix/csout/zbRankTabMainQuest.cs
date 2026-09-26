using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRankTabMainQuest
{
	private zbRankWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_scrollView;

	private VisualElement m_noListView;

	private VisualElement m_mine;

	private VisualElement m_mineNoRankInfo;

	private Label m_mineLbProgress;

	private Label m_mineLbTime;

	private Label m_mineLbRank;

	private RankDetail[] m_rankList;

	private RankData m_selfInfo;

	private RankListAck m_rankListAck;

	public void OnEnable(zbRankWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_mineNoRankInfo = UQueryExtensions.Q<VisualElement>(m_mine, "noRankInfo", (string)null);
		m_mineLbProgress = UQueryExtensions.Q<Label>(m_mine, "lb_progress", (string)null);
		m_mineLbTime = UQueryExtensions.Q<Label>(m_mine, "lb_time", (string)null);
		m_mineLbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
	}

	public void Refresh()
	{
		if (m_wndParent.m_callback != null)
		{
			m_rankListAck = zbSelf.Instance.rankHandler.m_newMainRankListAck;
		}
		else
		{
			m_rankListAck = zbSelf.Instance.rankHandler.m_mainQuestRankListAck;
		}
		if (zbSelf.Instance.rankHandler.isMainQuestRankUp())
		{
			RefreshList(zbSelf.Instance.rankHandler.m_oldMainRankListAck);
			((MonoBehaviour)m_wndParent).StartCoroutine(PlayRankUpAnimation(zbSelf.Instance.rankHandler.m_oldMainRankListAck, m_rankListAck));
		}
		else
		{
			RefreshList(m_rankListAck);
		}
	}

	private void RefreshList(RankListAck rankListAck)
	{
		if (rankListAck?.RankList == null)
		{
			if (!m_wndParent.firstIn)
			{
				zbWsMsgHandler.Instance.Rank_List_Req(ERankType.MainQuest, openRank: true);
			}
			m_wndParent.RefreshTops(Array.Empty<RoleBaseInfo>());
			m_rankList = Array.Empty<RankDetail>();
			m_selfInfo = null;
			UpdateListView();
			RefreshSelfInfo();
			return;
		}
		zbUIMgr.Instance.HideModel();
		List<RoleBaseInfo> list = new List<RoleBaseInfo>(3);
		int num = Mathf.Min(3, rankListAck.RankList.Count);
		for (int i = 0; i < num; i++)
		{
			list.Add(rankListAck.RankList[i].RoleBaseInfo);
		}
		m_wndParent.RefreshTops(list.ToArray());
		m_rankList = ((IEnumerable<RankDetail>)rankListAck.RankList).ToArray();
		m_selfInfo = rankListAck.SelfRank;
		UpdateListView();
		RefreshSelfInfo();
		foreach (RankDetail rank in rankListAck.RankList)
		{
			if (rank.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId())
			{
				m_wndParent.m_copyRankItem.Refresh(rank);
				break;
			}
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
			zbRankItem zbRankItem2 = new zbRankItem();
			zbRankItem2.OnEnable(m_wndParent, ERankType.MainQuest);
			zbRankItem2.Init(rankDetail);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbRankItem2);
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

	private IEnumerator PlayRankUpAnimation(RankListAck oldList, RankListAck newList)
	{
		if (oldList?.RankList == null || newList?.RankList == null)
		{
			if (newList != null)
			{
				RefreshList(newList);
			}
			yield break;
		}
		zbUIMgr.Instance.ShowModel();
		yield return null;
		List<RankDetail> list = ((IEnumerable<RankDetail>)oldList.RankList).ToList();
		List<RankDetail> list2 = ((IEnumerable<RankDetail>)newList.RankList).ToList();
		int oldRankIndex = list.FindIndex((RankDetail d) => d.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId());
		int newRankIndex = list2.FindIndex((RankDetail d) => d.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId());
		if (oldRankIndex < 0 || newRankIndex < 0 || newRankIndex >= oldRankIndex)
		{
			RefreshList(newList);
			yield break;
		}
		int index = Math.Min(oldList.RankList.Count - 1, oldRankIndex + 1);
		if (oldRankIndex <= 4)
		{
			m_scrollView.scrollOffset = Vector2.zero;
		}
		else
		{
			zbUtil.ScrollVerticalToIndex(m_scrollView, 5, oldList.RankList.Count, 120f, index);
		}
		yield return (object)new WaitForSeconds(0.4f);
		zbRankItem zbRankItem2 = ((VisualElement)m_scrollView).ElementAt(oldRankIndex) as zbRankItem;
		zbRankItem zbRankItem3 = ((VisualElement)m_scrollView).ElementAt(newRankIndex) as zbRankItem;
		if (zbRankItem2 == null || zbRankItem3 == null)
		{
			zbUtil.ScrollVerticalToIndex(m_scrollView, 5, oldList.RankList.Count, 120f, oldRankIndex);
			yield return null;
			zbRankItem2 = ((VisualElement)m_scrollView).ElementAt(oldRankIndex) as zbRankItem;
			zbRankItem3 = ((VisualElement)m_scrollView).ElementAt(newRankIndex) as zbRankItem;
			if (zbRankItem2 == null || zbRankItem3 == null)
			{
				RefreshList(newList);
				yield break;
			}
		}
		((VisualElement)m_wndParent.m_copyRankItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_wndParent.m_copyRankItem).style.width = StyleLength.op_Implicit(((VisualElement)zbRankItem2).resolvedStyle.width);
		((VisualElement)m_wndParent.m_copyRankItem).style.height = StyleLength.op_Implicit(((VisualElement)zbRankItem2).resolvedStyle.height);
		Rect worldBound = ((VisualElement)zbRankItem2).worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		worldBound = ((VisualElement)zbRankItem3).worldBound;
		Vector2 position2 = ((Rect)(ref worldBound)).position;
		Vector2 localStart = VisualElementExtensions.WorldToLocal(((VisualElement)m_wndParent.m_copyRankItem).parent, position);
		Vector2 localTarget = VisualElementExtensions.WorldToLocal(((VisualElement)m_wndParent.m_copyRankItem).parent, position2);
		((VisualElement)m_wndParent.m_copyRankItem).style.left = new StyleLength(new Length(localStart.x, (LengthUnit)0));
		((VisualElement)m_wndParent.m_copyRankItem).style.top = new StyleLength(new Length(localStart.y, (LengthUnit)0));
		yield return m_wndParent.m_copyRankItem.PlayAnimation("up");
		m_wndParent.m_copyRankItem.m_spine.AnimationState.SetAnimation(0, "loop", true);
		int num = oldRankIndex - newRankIndex;
		float num2 = Math.Min((float)num * 0.2f, 1f);
		if (num <= 4 || oldRankIndex <= 5)
		{
			yield return AnimateCopyItem(localStart, localTarget, num2);
		}
		else
		{
			int num3 = Mathf.Min(newList.RankList.Count - 1, newRankIndex + 3);
			float targetOffsetY = Mathf.Max(0, (num3 - 2) * 120);
			yield return AnimateScrollAndCopy(m_scrollView, targetOffsetY, 0.6f, localStart, new Vector2(21f, 550f), num2);
			yield return null;
		}
		RefreshList(newList);
		yield return m_wndParent.m_copyRankItem.PlayAnimation("down");
		((VisualElement)m_wndParent.m_copyRankItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private IEnumerator AnimateCopyItem(Vector2 start, Vector2 target, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float elapsed = 0f;
		Vector2 floatOffset = new Vector2(0f, -50f);
		while (elapsed < duration / 2f)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / (duration / 2f));
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			((VisualElement)m_wndParent.m_copyRankItem).style.left = new StyleLength(new Length(Mathf.Lerp(start.x, start.x + floatOffset.x, num2), (LengthUnit)0));
			((VisualElement)m_wndParent.m_copyRankItem).style.top = new StyleLength(new Length(Mathf.Lerp(start.y, start.y + floatOffset.y, num2), (LengthUnit)0));
			yield return null;
		}
		elapsed = 0f;
		Vector2 mid = start + floatOffset;
		while (elapsed < duration / 2f)
		{
			elapsed += Time.deltaTime;
			float num3 = Mathf.Clamp01(elapsed / (duration / 2f));
			float num4 = Mathf.SmoothStep(0f, 1f, num3);
			((VisualElement)m_wndParent.m_copyRankItem).style.left = new StyleLength(new Length(Mathf.Lerp(mid.x, target.x, num4), (LengthUnit)0));
			((VisualElement)m_wndParent.m_copyRankItem).style.top = new StyleLength(new Length(Mathf.Lerp(mid.y, target.y, num4), (LengthUnit)0));
			yield return null;
		}
		((VisualElement)m_wndParent.m_copyRankItem).style.left = new StyleLength(new Length(target.x, (LengthUnit)0));
		((VisualElement)m_wndParent.m_copyRankItem).style.top = new StyleLength(new Length(target.y, (LengthUnit)0));
	}

	private IEnumerator AnimateScrollAndCopy(ScrollView scrollView, float targetOffsetY, float scrollDuration, Vector2 start, Vector2 target, float copyDuration)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float totalTime = Mathf.Max(scrollDuration, copyDuration);
		float startOffsetY = scrollView.scrollOffset.y;
		float elapsed = 0f;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0f, -50f);
		Vector2 mid = start + val;
		while (elapsed < totalTime)
		{
			elapsed += Time.deltaTime;
			Mathf.Clamp01(elapsed / totalTime);
			float num = Mathf.Clamp01(elapsed / scrollDuration);
			num = Mathf.SmoothStep(0f, 1f, num);
			float num2 = Mathf.Lerp(startOffsetY, targetOffsetY, num);
			scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, num2);
			float num3 = Mathf.Clamp01(elapsed / copyDuration);
			float num4 = Mathf.SmoothStep(0f, 1f, num3 * 2f % 1f);
			if (num3 < 0.5f)
			{
				((VisualElement)m_wndParent.m_copyRankItem).style.left = new StyleLength(new Length(Mathf.Lerp(start.x, mid.x, num4), (LengthUnit)0));
				((VisualElement)m_wndParent.m_copyRankItem).style.top = new StyleLength(new Length(Mathf.Lerp(start.y, mid.y, num4), (LengthUnit)0));
			}
			else
			{
				((VisualElement)m_wndParent.m_copyRankItem).style.left = new StyleLength(new Length(Mathf.Lerp(mid.x, target.x, num4), (LengthUnit)0));
				((VisualElement)m_wndParent.m_copyRankItem).style.top = new StyleLength(new Length(Mathf.Lerp(mid.y, target.y, num4), (LengthUnit)0));
			}
			yield return null;
		}
		scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, targetOffsetY);
		((VisualElement)m_wndParent.m_copyRankItem).style.left = new StyleLength(new Length(target.x, (LengthUnit)0));
		((VisualElement)m_wndParent.m_copyRankItem).style.top = new StyleLength(new Length(target.y, (LengthUnit)0));
	}

	public void BeforeClose()
	{
		((VisualElement)m_scrollView).schedule.Execute((Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			m_scrollView.scrollOffset = Vector2.zero;
		}).StartingIn(1L);
		zbSelf.Instance.rankHandler.UpdateMainQuestRankListAck(m_rankListAck, isForce: true);
		zbSelf.Instance.rankHandler.m_mainQuestRankListAck = null;
	}
}
