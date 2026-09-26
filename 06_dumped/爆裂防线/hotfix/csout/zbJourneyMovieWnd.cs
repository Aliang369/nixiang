using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyMovieWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private Label m_lbTalk;

	private Label m_lbEnd;

	private Button m_btnTouch;

	private int m_journeyType;

	private List<(float time, string str)> m_talkList = new List<(float, string)>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_lbTalk = UQueryExtensions.Q<Label>(m_RootElement, "lb_talk", (string)null);
		m_lbEnd = UQueryExtensions.Q<Label>(m_RootElement, "lb_end", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		zbUIMgr.Instance.m_scrollBubble.Resume();
	}

	public void RefreshCustom(int journeyType)
	{
		zbUIMgr.Instance.m_scrollBubble.Stop();
		zbUIMgr.Instance.m_scrollBubble.Pause();
		m_journeyType = journeyType;
		zbSelf.Instance.newTagHandler.AddJourneyMovieSeen(m_journeyType);
		m_talkList.Clear();
		if (m_journeyType == 1)
		{
			m_spineScene.SetSkeletonDataAsset("animation/ui/huodong1/huodong1_SkeletonData.asset");
			m_talkList.Add((4f, "lang.11347"));
			m_talkList.Add((4f, "lang.11348"));
			m_talkList.Add((4f, "lang.11349"));
			m_talkList.Add((4.6666665f, "lang.11350"));
			m_talkList.Add((4f, "lang.11351"));
		}
		else if (m_journeyType == 2)
		{
			m_spineScene.SetSkeletonDataAsset("animation/ui/huodong2/huodong2_SkeletonData.asset");
			m_talkList.Add((4.5f, "lang.11352"));
			m_talkList.Add((2.7666667f, "lang.11353"));
			m_talkList.Add((1.2f, "lang.11354"));
		}
		else if (m_journeyType == 3)
		{
			m_spineScene.SetSkeletonDataAsset("animation/ui/huodong3/huodong3_SkeletonData.asset");
			m_talkList.Add((4.3333335f, "lang.11355"));
			m_talkList.Add((6f, "lang.11356"));
			m_talkList.Add((7.1666665f, "lang.11357"));
			m_talkList.Add((6.8333335f, "lang.11358"));
		}
		((MonoBehaviour)this).StartCoroutine(ChuchangSequence());
	}

	private IEnumerator ChuchangSequence()
	{
		((VisualElement)m_lbEnd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spineScene.PlaySequenceAnimation("in", "loop");
		if (m_talkList != null && m_talkList.Count > 0)
		{
			foreach (var talk in m_talkList)
			{
				((TextElement)m_lbTalk).text = zbUtil.Language(talk.str);
				yield return (object)new WaitForSeconds(talk.time);
			}
		}
		((VisualElement)m_lbEnd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void OnTouch()
	{
		if (m_journeyType == 1)
		{
			zbUIMgr.Instance.ShowJourneyOneMain();
		}
		else if (m_journeyType == 2)
		{
			zbUIMgr.Instance.ShowJourneyTwoMain();
		}
		else if (m_journeyType == 3)
		{
			zbUIMgr.Instance.ShowJourneyThreeMain();
		}
		Hide();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if ((Object)(object)m_spineScene != (Object)null)
		{
			m_spineScene.ClearSpine();
		}
		zbUIMgr.Instance.m_scrollBubble.Resume();
	}
}
