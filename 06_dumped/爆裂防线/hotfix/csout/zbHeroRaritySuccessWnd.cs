using System;
using System.Collections;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroRaritySuccessWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SkeletonAnimation m_spineHero;

	private VisualElement m_spriteBg;

	private VisualElement m_spriteRarity0;

	private VisualElement m_spriteRarity1;

	private Label m_lbName;

	private Label m_lbDamage0;

	private Label m_lbDamage1;

	private Button m_btnTouchTest;

	private HeroDetail m_heroDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_spineHero = ((Component)m_GameObjectSceneRoot.transform.Find("avatar")).GetComponent<SkeletonAnimation>();
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_spriteRarity0 = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_rarity0", (string)null);
		m_spriteRarity1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_rarity1", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDamage0 = UQueryExtensions.Q<Label>(m_RootElement, "lb_damage0", (string)null);
		m_lbDamage1 = UQueryExtensions.Q<Label>(m_RootElement, "lb_damage1", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(HeroDetail heroDetail)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		m_heroDetail = heroDetail;
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroDetail.HeroTid);
		if (recordByKey != null)
		{
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				string spinePath = recordByKey2.frontBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_spineHero, spinePath, "idle", loop: true);
			}
			((TextElement)m_lbName).text = recordByKey.name;
			string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(heroDetail.Rarity - 1);
			m_spriteRarity0.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
			string heroRarityIconPath2 = zbResourceHelper.GetHeroRarityIconPath(heroDetail.Rarity);
			m_spriteRarity1.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath2));
			float damage = zbSelf.Instance.heroHandler.GetDamage(heroDetail.Quality, heroDetail.Rarity - 1, heroDetail.QualityStar);
			((TextElement)m_lbDamage0).text = $"{damage * 100f:0.##}%";
			float damage2 = zbSelf.Instance.heroHandler.GetDamage(heroDetail.Quality, heroDetail.Rarity, heroDetail.QualityStar);
			((TextElement)m_lbDamage1).text = $"{damage2 * 100f:0.##}%";
			PlayAppearAnimation();
		}
	}

	private void PlayAppearAnimation()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		m_spineScene.PlaySequenceAnimation("enter", "loop");
		((Component)m_spineHero).gameObject.SetActive(false);
		m_spriteBg.style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
	}

	private IEnumerator PlayAnimationSequence()
	{
		yield return (object)new WaitForSeconds(2f / 3f);
		((Component)m_spineHero).gameObject.SetActive(true);
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
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
