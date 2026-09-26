using System;
using System.Collections;
using System.Linq;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaStarSuccessWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SkeletonAnimation m_spineMecha;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_nodeAttrs = (VisualElement[])(object)new VisualElement[1];

	private Label[] m_lbCurValues = (Label[])(object)new Label[1];

	private Label[] m_lbNextValues = (Label[])(object)new Label[1];

	private VisualElement[] m_spriteArrows = (VisualElement[])(object)new VisualElement[1];

	private VisualElement m_nodeSkill;

	private VisualElement m_spriteSkill;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	protected override void OnEnable()
	{
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_spineMecha = ((Component)m_GameObjectSceneRoot.transform.Find("avatar")).GetComponent<SkeletonAnimation>();
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "star" + i, (string)null);
		}
		for (int j = 0; j < 1; j++)
		{
			m_nodeAttrs[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + j, (string)null);
			m_lbCurValues[j] = UQueryExtensions.Q<Label>(m_nodeAttrs[j], "lb_curValue", (string)null);
			m_lbNextValues[j] = UQueryExtensions.Q<Label>(m_nodeAttrs[j], "lb_nextValue", (string)null);
			m_spriteArrows[j] = UQueryExtensions.Q<VisualElement>(m_nodeAttrs[j], "arrow", (string)null);
		}
		m_nodeSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int mechaId)
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(mechaId);
		MechaDetail mechaDetail = zbSelf.Instance.mechaHandler.GetMechaDetail(mechaId);
		if (mechaRecord == null || mechaDetail == null)
		{
			return;
		}
		((TextElement)m_lbName).text = mechaRecord.name;
		if ((Object)(object)m_spineMecha != (Object)null)
		{
			zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(mechaRecord.avatarId);
			if (recordByKey != null)
			{
				string spinePath = recordByKey.fightBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_spineMecha, spinePath);
			}
		}
		zbUIEffect.SetStars(m_stars, mechaDetail.Star);
		zbMechaStarUpgradeCsvRecord recordByKey2 = zbDataConfig.Instance.mechaStarUpgradeCsvTable.GetRecordByKey(mechaDetail.Star - 1);
		zbMechaStarUpgradeCsvRecord recordByKey3 = zbDataConfig.Instance.mechaStarUpgradeCsvTable.GetRecordByKey(mechaDetail.Star);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbCurValues[0]).text = (mechaDetail.Attack - recordByKey3.attack + recordByKey2.attack).ToString();
		}
		if (recordByKey3 != null)
		{
			((TextElement)m_lbNextValues[0]).text = mechaDetail.Attack.ToString();
		}
		int[] array = mechaRecord.skillList.ToArray();
		int key = array[Mathf.Clamp(mechaDetail.Star - 1, 0, array.Length - 1)];
		zbMechaSkillCsvRecord recordByKey4 = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(key);
		if (recordByKey4 != null)
		{
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey4.icon));
			PlayAppearAnimation();
		}
	}

	private void PlayAppearAnimation()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		m_spineScene.PlaySequenceAnimation("enter", "loop");
		((Component)m_spineMecha).gameObject.SetActive(false);
		m_spriteBg.style.opacity = StyleFloat.op_Implicit(0f);
		((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
	}

	private IEnumerator PlayAnimationSequence()
	{
		yield return (object)new WaitForSeconds(2f / 3f);
		((Component)m_spineMecha).gameObject.SetActive(true);
		PlayMechaAnimation();
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		yield return (object)new WaitForSeconds(1f / 3f);
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

	public void PlayMechaAnimation()
	{
		if ((Object)(object)m_spineMecha != (Object)null)
		{
			m_spineMecha.AnimationState.ClearTracks();
			m_spineMecha.AnimationState.SetAnimation(0, "idle", true);
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
