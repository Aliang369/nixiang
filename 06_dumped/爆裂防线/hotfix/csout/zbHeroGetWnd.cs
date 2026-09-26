using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroGetWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SkeletonAnimation m_spineHero;

	private VisualElement m_spriteBg;

	private VisualElement m_spriteRarity;

	private Label m_lbTitle;

	private VisualElement m_spriteNew;

	private Label m_lbName;

	private VisualElement m_skillIcon;

	private VisualElement m_spriteSkill;

	private Label m_skillName;

	private Label m_skillDesc;

	private Label m_lbFrag;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	private int[] m_heroTids;

	private int m_curIndex;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_spineHero = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene").Find("avatar")).GetComponent<SkeletonAnimation>();
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_rarity", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteNew = UQueryExtensions.Q<VisualElement>(m_RootElement, "new", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_skillIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "skillicon", (string)null);
		m_skillName = UQueryExtensions.Q<Label>(m_RootElement, "skillname", (string)null);
		m_skillDesc = UQueryExtensions.Q<Label>(m_RootElement, "skilldesc", (string)null);
		m_lbFrag = UQueryExtensions.Q<Label>(m_RootElement, "lb_frag", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int[] heroTids, Action<bool> callback = null)
	{
		m_heroTids = heroTids;
		m_callback = callback;
		m_curIndex = 0;
		RefreshDetail(heroTids[m_curIndex]);
	}

	private void RefreshDetail(int heroTid)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		HeroDetail[] lastHeroList = zbSelf.Instance.dataHandler.lastHeroList;
		for (int i = 0; i < lastHeroList.Length; i++)
		{
			if (lastHeroList[i].HeroTid == heroTid)
			{
				flag = false;
				break;
			}
		}
		m_spriteNew.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbFrag).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (flag)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.10262");
		}
		else
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.10264");
		}
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroTid);
		if (recordByKey == null)
		{
			return;
		}
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath((ERarityType)recordByKey.rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		((TextElement)m_lbName).text = recordByKey.name;
		zbHeroRarityCsvRecord recordByKey2 = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey(recordByKey.rarity);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbFrag).text = string.Format(zbUtil.Language("lang.10265"), recordByKey2.frag);
		}
		if ((Object)(object)m_spineHero != (Object)null)
		{
			zbHeroAvatarCsvRecord recordByKey3 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey3 != null)
			{
				string spinePath = recordByKey3.frontBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_spineHero, spinePath, "idle", loop: true);
			}
		}
		zbSkillCsvRecord recordByKey4 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(recordByKey.skillId);
		if (recordByKey4 != null)
		{
			((TextElement)m_skillName).text = recordByKey4.name;
			((TextElement)m_skillDesc).text = recordByKey4.desc;
			m_skillIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey4.icon));
			string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(recordByKey);
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
			PlayAppearAnimation();
		}
	}

	private void PlayAppearAnimation()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		m_spineScene.PlaySequenceAnimation("animation", "loop");
		((Component)m_spineHero).gameObject.SetActive(false);
		m_spriteBg.style.opacity = StyleFloat.op_Implicit(0f);
		((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
	}

	private IEnumerator PlayAnimationSequence()
	{
		yield return (object)new WaitForSeconds(1f / 6f);
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
	}

	public void OnTouchTest(ClickEvent evt)
	{
		m_curIndex++;
		if (m_curIndex >= m_heroTids.Length)
		{
			m_callback?.Invoke(obj: true);
			Hide();
		}
		else
		{
			RefreshDetail(m_heroTids[m_curIndex]);
		}
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
