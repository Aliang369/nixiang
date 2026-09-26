using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFootholdGetWnd : zbBaseWnd
{
	private SkeletonAnimation m_spineDikuai;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement m_spriteSkilll;

	private Label m_lbDesc;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	private int m_id;

	protected override void OnEnable()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spine_dikuai")).gameObject;
		m_spineDikuai = gameObject.GetComponent<SkeletonAnimation>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteSkilll = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int id)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		m_id = id;
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			zbUtil.RefreshSpine(m_spineDikuai, recordByKey.mainScene + "_SkeletonData.asset", "in");
			m_spineDikuai.AnimationState.AddAnimation(0, "loop", true, 0f);
			((TextElement)m_lbName).text = recordByKey.name;
			m_spriteSkilll.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			((TextElement)m_lbDesc).text = zbSelf.Instance.guardHandler.GetFootholdEffectDesc(recordByKey);
			zbUtil.AutoFontSize((TextElement)(object)m_lbDesc, 22f, 14f, 480f, 120f);
			m_spriteBg.style.opacity = StyleFloat.op_Implicit(0f);
			((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
		}
	}

	private IEnumerator PlayAnimationSequence()
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		float duration = 1f / 6f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			m_spriteBg.style.opacity = StyleFloat.op_Implicit(num);
			yield return null;
		}
		m_spriteBg.style.opacity = StyleFloat.op_Implicit(1f);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.10596"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbFootholdWnd zbFootholdWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Foothold) as zbFootholdWnd;
				if ((Object)(object)zbFootholdWnd2 != (Object)null)
				{
					zbWsMsgHandler.Instance.Foothold_Set_Req(zbFootholdWnd2.m_selectId, m_id);
				}
			}
		});
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
