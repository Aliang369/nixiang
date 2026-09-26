using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class zbAwardWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private SpineVisualElement m_spineBg;

	private VisualElement m_bg;

	private VisualElement m_awards;

	private zbItem[] m_items = new zbItem[5];

	private ScrollView m_scrollView;

	private Label m_lbTitle;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<zbItem> m_createdItems = new List<zbItem>();

	private int columnCount = 5;

	private int rowCount;

	private bool m_lessFive = true;

	private Coroutine fadeCoroutine;

	private Coroutine addCoroutine;

	private bool m_isFastExit;

	private bool m_isFastForward;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_spineBg = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_bg", (string)null);
		zbUtil.RefreshSpineUI(m_spineBg, "animation/ui/ui_huode/ui_huode_SkeletonData.asset");
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_awards = UQueryExtensions.Q<VisualElement>(m_RootElement, "awards", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_scrollView.horizontalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		((VisualElement)m_scrollView).contentContainer.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)0);
	}

	public void RefreshCustom(AwardDetail[] awardlist, Action<bool> callback)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (awardlist.Length == 0)
		{
			Hide();
			callback?.Invoke(obj: true);
			return;
		}
		m_isFastExit = false;
		m_isFastForward = false;
		m_callback = callback;
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		if (awardlist.Length <= 5)
		{
			m_awards.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			RefreshAwards(awardlist);
			m_lessFive = true;
		}
		else
		{
			m_awards.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshScrollView(awardlist);
			m_lessFive = false;
			m_scrollView.scrollOffset = Vector2.zero;
			((VisualElement)m_scrollView).schedule.Execute((Action)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				m_scrollView.scrollOffset = Vector2.zero;
			}).StartingIn(1L);
		}
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (fadeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}
		fadeCoroutine = ((MonoBehaviour)this).StartCoroutine(FadeInAnimation());
	}

	private void RefreshAwards(AwardDetail[] awardlist)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (AwardDetail awardDetail in awardlist)
		{
			if (num < m_items.Length)
			{
				zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
			}
			num++;
		}
		VisualElement[] items = (VisualElement[])(object)m_items;
		zbUtil.CenterNodeItems(items);
	}

	private void RefreshScrollView(AwardDetail[] awardlist)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (addCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(addCoroutine);
			addCoroutine = null;
		}
		((VisualElement)m_scrollView).Clear();
		m_listItemDatas.Clear();
		m_createdItems.Clear();
		foreach (AwardDetail awardDetail in awardlist)
		{
			m_listItemDatas.Add(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
		}
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int j = 0; j < rowCount; j++)
		{
			VisualElement val = new VisualElement();
			val.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
			val.style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)0);
			val.style.marginBottom = StyleLength.op_Implicit(8f);
			((VisualElement)m_scrollView).Add(val);
		}
		addCoroutine = ((MonoBehaviour)this).StartCoroutine(AddAwardsGradually());
	}

	private IEnumerator AddAwardsGradually()
	{
		int count = m_listItemDatas.Count;
		for (int i = 0; i < Mathf.Min(5, count); i++)
		{
			AddAwardAtIndex(i);
		}
		yield return (object)new WaitForSecondsRealtime(0.1f);
		for (int j = 5; j < count; j++)
		{
			int num = j / columnCount;
			AddAwardAtIndex(j);
			if (num >= 3 && num < ((VisualElement)m_scrollView).childCount)
			{
				VisualElement targetRow = ((VisualElement)m_scrollView)[num];
				((VisualElement)m_scrollView).schedule.Execute((Action)delegate
				{
					m_scrollView.ScrollTo(targetRow);
				}).StartingIn(1L);
			}
			if (m_isFastForward)
			{
				for (int num2 = j + 1; num2 < count; num2++)
				{
					AddAwardAtIndex(num2);
				}
				if (((VisualElement)m_scrollView).childCount > 0)
				{
					m_scrollView.ScrollTo(((VisualElement)m_scrollView)[((VisualElement)m_scrollView).childCount - 1]);
				}
				break;
			}
			yield return (object)new WaitForSecondsRealtime(0.1f);
		}
		m_isFastForward = false;
		addCoroutine = null;
	}

	private void ScrollToLastRow()
	{
		if (((VisualElement)m_scrollView).childCount != 0)
		{
			((VisualElement)m_scrollView).schedule.Execute((Action)delegate
			{
				VisualElement val = ((VisualElement)m_scrollView)[((VisualElement)m_scrollView).childCount - 1];
				m_scrollView.ScrollTo(val);
			}).StartingIn(1L);
		}
	}

	private void AddAwardAtIndex(int index)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		int num = index / columnCount;
		if (num < ((VisualElement)m_scrollView).childCount)
		{
			VisualElement obj = ((VisualElement)m_scrollView)[num];
			zbItem item = new zbItem();
			item.OnEnable(this);
			((VisualElement)item).style.opacity = StyleFloat.op_Implicit(0f);
			item.Refresh(m_listItemDatas[index]);
			obj.Add((VisualElement)(object)item);
			m_createdItems.Add(item);
			((VisualElement)m_scrollView).schedule.Execute((Action)delegate
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				ITransitionAnimations animation = ((VisualElement)item).experimental.animation;
				StyleValues val = default(StyleValues);
				((StyleValues)(ref val)).opacity = 1f;
				animation.Start(val, 150);
			}).StartingIn(0L);
		}
	}

	private IEnumerator FadeInAnimation()
	{
		yield return null;
		yield return null;
		if (m_lessFive)
		{
			m_spineBg.AnimationState.SetAnimation(0, "in2", false);
			m_spineBg.AnimationState.AddAnimation(0, "loop2", true, 0f);
		}
		else
		{
			m_spineBg.AnimationState.SetAnimation(0, "in", false);
			m_spineBg.AnimationState.AddAnimation(0, "loop", true, 0f);
		}
		yield return (object)new WaitForSecondsRealtime(0.1f);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return (object)new WaitForSecondsRealtime(1.2f);
		yield return (object)new WaitForSecondsRealtime(0.2f);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		if (m_isFastExit)
		{
			return;
		}
		if (!m_isFastForward && addCoroutine != null)
		{
			m_isFastForward = true;
			((MonoBehaviour)this).StopCoroutine(addCoroutine);
			addCoroutine = null;
			for (int i = m_createdItems.Count; i < m_listItemDatas.Count; i++)
			{
				AddAwardAtIndex(i);
			}
			ScrollToLastRow();
			if (fadeCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(fadeCoroutine);
				fadeCoroutine = null;
			}
		}
		else
		{
			m_isFastExit = true;
			((MonoBehaviour)this).StartCoroutine(OnTouchTestCoroutine());
		}
	}

	private IEnumerator OnTouchTestCoroutine()
	{
		if (m_spineBg != null && m_spineBg.AnimationState != null)
		{
			if (m_scrollView != null)
			{
				((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			if (m_awards != null)
			{
				m_awards.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			if (m_bg != null)
			{
				m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_spineBg.AnimationState.TimeScale = 2f;
			string text = (m_lessFive ? "out2" : "out");
			TrackEntry val = m_spineBg.AnimationState.SetAnimation(0, text, false);
			if (val != null)
			{
				float num = val.Animation.Duration / m_spineBg.AnimationState.TimeScale;
				yield return (object)new WaitForSecondsRealtime(num);
			}
			yield return null;
			yield return null;
			Hide();
			m_callback?.Invoke(obj: true);
		}
	}
}
