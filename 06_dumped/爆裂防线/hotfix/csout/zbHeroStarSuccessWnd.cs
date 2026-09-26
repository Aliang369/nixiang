using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroStarSuccessWnd : zbBaseWnd
{
	private SpineVisualElement m_spineBg;

	private VisualElement m_bg;

	private VisualElement[] m_nodeHeros = (VisualElement[])(object)new VisualElement[2];

	private VisualElement[] m_spriteFrames = (VisualElement[])(object)new VisualElement[2];

	private VisualElement[] m_spriteHeros = (VisualElement[])(object)new VisualElement[2];

	private Label[] m_lbNames = (Label[])(object)new Label[2];

	private VisualElement m_preStars;

	private VisualElement m_curStars;

	private Label m_lbSkillDesc;

	private VisualElement m_starInfo;

	private Label m_lbStarInfo;

	private Label m_lbTitle;

	private Button m_btnTouchTest;

	private VisualElement m_skillDescInfo;

	private HeroDetail m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineBg = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_bg", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_nodeHeros[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "hero" + i, (string)null);
			m_spriteFrames[i] = UQueryExtensions.Q<VisualElement>(m_nodeHeros[i], "bg_frame", (string)null);
			m_spriteHeros[i] = UQueryExtensions.Q<VisualElement>(m_nodeHeros[i], "bg_icon", (string)null);
			m_lbNames[i] = UQueryExtensions.Q<Label>(m_nodeHeros[i], "lb_name", (string)null);
		}
		m_preStars = UQueryExtensions.Q<VisualElement>(m_nodeHeros[0], "stars", (string)null);
		m_curStars = UQueryExtensions.Q<VisualElement>(m_nodeHeros[1], "stars", (string)null);
		m_lbSkillDesc = UQueryExtensions.Q<Label>(m_RootElement, "skill_desc", (string)null);
		m_starInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "starInfo", (string)null);
		m_lbStarInfo = UQueryExtensions.Q<Label>(m_starInfo, "lb_starInfo", (string)null);
		m_skillDescInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "skillDescInfo", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(int heroTid, int nextMod, bool isUpgrade, int preStar, int curStar)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spineBg.AnimationState.SetAnimation(0, "in", false);
		m_spineBg.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_success.ogg");
		((MonoBehaviour)this).StartCoroutine(DelayShowBg());
		m_data = zbSelf.Instance.heroHandler.GetHeroDetail(heroTid);
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(m_data.AvatarId);
		if (recordByKey != null)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.heroIcon);
			m_spriteHeros[0].style.backgroundImage = StyleBackground.op_Implicit(val);
			m_spriteHeros[1].style.backgroundImage = StyleBackground.op_Implicit(val);
		}
		((TextElement)m_lbTitle).text = (isUpgrade ? "突破成功" : "升星成功");
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)Math.Max((int)(isUpgrade ? (m_data.Quality - 1) : m_data.Quality), 1));
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(heroFrameTex);
		m_spriteFrames[0].style.backgroundImage = StyleBackground.op_Implicit(val2);
		string heroFrameTex2 = zbResourceHelper.GetHeroFrameTex(m_data.Quality);
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(heroFrameTex2);
		m_spriteFrames[1].style.backgroundImage = StyleBackground.op_Implicit(val3);
		zbHeroCsvRecord recordByKey2 = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroTid);
		zbDataConfig.Instance.skillCsvTable.GetRecordByKey(recordByKey2.skillId);
		zbModCsvRecord recordByKey3 = zbDataConfig.Instance.modCsvTable.GetRecordByKey(nextMod);
		if (recordByKey3 != null)
		{
			((TextElement)m_lbSkillDesc).text = recordByKey3.desc;
		}
		if (recordByKey2 != null)
		{
			((TextElement)m_lbNames[0]).text = recordByKey2.name;
			((TextElement)m_lbNames[1]).text = recordByKey2.name;
		}
		m_starInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(isUpgrade ? 1 : 0));
		m_skillDescInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isUpgrade));
		m_curStars.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(isUpgrade ? 1 : 0));
		m_preStars.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(isUpgrade ? 1 : 0));
		if (!isUpgrade)
		{
			float damage = zbSelf.Instance.heroHandler.GetDamage(m_data.Quality, m_data.Rarity, m_data.QualityStar);
			EQuality quality = ((m_data.QualityStar == 0) ? (m_data.Quality - 1) : m_data.Quality);
			int qualityStar = ((m_data.QualityStar == 0) ? 4 : (m_data.QualityStar - 1));
			float damage2 = zbSelf.Instance.heroHandler.GetDamage(quality, m_data.Rarity, qualityStar);
			((TextElement)m_lbStarInfo).text = "升星加成:伤害系数+" + Mathf.Round((damage - damage2) * 100f) + "%";
			Sprite val4 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/hero/icon_star" + preStar + ".png");
			m_preStars.style.backgroundImage = new StyleBackground(val4);
			Sprite val5 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/hero/icon_star" + curStar + ".png");
			m_curStars.style.backgroundImage = new StyleBackground(val5);
		}
	}

	private IEnumerator DelayShowBg()
	{
		yield return (object)new WaitForSeconds(0.4f);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spineBg.AnimationState.SetAnimation(0, "out", false).Complete += new TrackEntryDelegate(OnOutAnimationComplete);
	}

	private void OnOutAnimationComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Animation.Name == "out")
		{
			Hide();
		}
	}
}
