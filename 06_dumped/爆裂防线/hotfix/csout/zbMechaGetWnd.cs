using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaGetWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SkeletonAnimation m_spineMecha;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private Button m_btnTouchTest;

	private VisualElement[] m_skills = (VisualElement[])(object)new VisualElement[5];

	private int m_mechaId;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_spineMecha = ((Component)m_GameObjectSceneRoot.transform.Find("avatar")).GetComponent<SkeletonAnimation>();
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_skills[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill" + i, (string)null);
		}
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int mechaId)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(mechaId);
		if (mechaRecord == null)
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
		for (int i = 0; i < 5; i++)
		{
			if (i < mechaRecord.skillList.Length)
			{
				int key = mechaRecord.skillList[i];
				zbMechaSkillCsvRecord recordByKey2 = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(key);
				m_skills[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
			}
		}
		PlayAppearAnimation();
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
			m_spineMecha.AnimationState.SetAnimation(0, "win", false);
			m_spineMecha.AnimationState.AddAnimation(0, "idle", true, 0f);
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
