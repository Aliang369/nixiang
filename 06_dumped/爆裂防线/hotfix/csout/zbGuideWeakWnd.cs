using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideWeakWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private int turnIndex;

	private int talkId;

	private Button m_btnTouchTest;

	private Action onFinished;

	private VisualElement m_guideComm;

	private VisualElement m_guideLabel;

	private VisualElement watchNode;

	private VisualElement m_nodeFinger;

	private Button m_btnFinger;

	private SpineVisualElement m_spineFinger;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_guideComm = UQueryExtensions.Q<VisualElement>(m_RootElement, "guideComm", (string)null);
		m_guideLabel = UQueryExtensions.Q<VisualElement>(m_RootElement, "guideLabel", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "desc", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_spineFinger = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_finger", (string)null);
		m_nodeFinger = UQueryExtensions.Q<VisualElement>(m_RootElement, "_finger", (string)null);
		m_btnFinger = UQueryExtensions.Q<Button>(m_RootElement, "sp_finger", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFinger, (Action<ClickEvent>)OnTouchFinger, "", (TrickleDown)0);
	}

	public void RefreshCustom(int _talkId, Action _onFinished, float labelTop, VisualElement _watchNode)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		turnIndex = 0;
		talkId = _talkId;
		onFinished = _onFinished;
		watchNode = _watchNode;
		m_nodeFinger.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((MonoBehaviour)this).StopAllCoroutines();
		m_guideComm.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_guideLabel.style.top = StyleLength.op_Implicit(labelTop);
		if (watchNode != null)
		{
			DelDark();
		}
		else
		{
			DelDark();
		}
		UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null).pickingMode = (PickingMode)0;
		RefreshTalk();
		((MonoBehaviour)this).StartCoroutine(ActionCoroutine());
	}

	public IEnumerator ActionCoroutine()
	{
		yield return null;
		RefreshDetail();
	}

	public void RefreshDetail()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (watchNode != null)
		{
			m_nodeFinger.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbUtil.SetNodePosition(m_nodeFinger, watchNode);
			PlayAnimation2();
		}
	}

	public void PlayAnimation2()
	{
		if (!(m_spineFinger.AnimationState.GetCurrent(0).Animation.Name == "animation2"))
		{
			TrackEntry val = m_spineFinger.AnimationState.SetAnimation(0, "animation2", false);
			if (val != null)
			{
				m_spineFinger.AnimationState.AddAnimation(0, "animation", true, val.AnimationEnd);
			}
			else
			{
				m_spineFinger.AnimationState.AddAnimation(0, "animation", true, 1.6666666f);
			}
		}
	}

	public void RefreshTalk()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		turnIndex++;
		zbGuideTalkCsvRecord[] records = zbDataConfig.Instance.guideTalkCsvTable.GetRecords().records;
		foreach (zbGuideTalkCsvRecord zbGuideTalkCsvRecord2 in records)
		{
			if (talkId == zbGuideTalkCsvRecord2.talkId && turnIndex == zbGuideTalkCsvRecord2.turn)
			{
				((TextElement)m_lbTitle).text = zbGuideTalkCsvRecord2.desc;
				return;
			}
		}
		if (m_nodeFinger.style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			Hide();
			onFinished?.Invoke();
		}
		else
		{
			PlayAnimation2();
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		RefreshTalk();
	}

	public void OnTouchFinger(ClickEvent evt)
	{
		Hide();
		onFinished?.Invoke();
	}
}
