using System;
using System.Collections;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbFootholdStarSuccessWnd : zbBaseWnd
{
	private SkeletonAnimation m_spineDikuai;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbCurValue;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	private VisualElement m_nodeSkill;

	private VisualElement m_spriteSkill;

	private Button m_btnList;

	private Label m_lbDesc;

	private int footholdId;

	protected override void OnEnable()
	{
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spine_dikuai")).gameObject;
		m_spineDikuai = gameObject.GetComponent<SkeletonAnimation>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "star" + i, (string)null);
		}
		m_lbCurValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_curValue", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_nodeSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_skill", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_nodeSkill, "bg_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeSkill, "lb_desc", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_nodeSkill, "btn_list", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnList, "", (TrickleDown)0);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int id)
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		footholdId = id;
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(id);
		FootholdDetail footholdDetail = zbSelf.Instance.guardHandler.GetFootholdDetail(id);
		if (recordByKey != null && footholdDetail != null)
		{
			zbUtil.RefreshSpine(m_spineDikuai, recordByKey.mainScene + "_SkeletonData.asset", "in");
			m_spineDikuai.AnimationState.AddAnimation(0, "loop", true, 0f);
			((TextElement)m_lbName).text = recordByKey.name;
			zbUIEffect.SetStars(m_stars, footholdDetail.Star);
			zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(id, footholdDetail.Star - 1);
			zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(id, footholdDetail.Star);
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			zbLabelLinkUtil.BuildModLink(zbSelf.Instance.guardHandler.GetFootholdEffectDesc(recordByKey, footholdStarUpgradeRecord.star), m_lbDesc);
			zbUtil.AutoFontSize((TextElement)(object)m_lbDesc, 22f, 14f, 555f, 92f);
			((TextElement)m_lbCurValue).text = zbUtil.Language("lang.10862") + "+" + $"{footholdStarUpgradeRecord.dmg:0.##%}";
			m_spriteBg.style.opacity = StyleFloat.op_Implicit(0f);
			((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
			zbFootholdEffectCsvRecord footholdEffectRecord = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(id, footholdDetail.Star - 1);
			zbFootholdEffectCsvRecord footholdEffectRecord2 = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(id, footholdDetail.Star);
			m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((footholdEffectRecord.id == footholdEffectRecord2.id) ? 1 : 0));
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
	}

	private void OnList()
	{
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(footholdId);
		zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(recordByKey.id, EPreviewType.Foothold);
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
