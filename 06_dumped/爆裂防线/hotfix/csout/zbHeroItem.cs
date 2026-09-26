using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private VisualElement m_spriteRarity;

	private VisualElement m_spriteSkill;

	private Label m_lbLevel;

	private Label m_lbName;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_new;

	private zbHeroWnd m_wndParent;

	private zbHeroCsvRecord m_record;

	private HeroDetail m_data;

	public zbHeroItem(zbHeroWnd wndParent)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_hero.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_rarity", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skill", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-3f, -2f), 1f, true);
		m_new = zbUtil.CreateNewTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-5f, -7f), 0.9f);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbHeroCsvRecord record)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_data = zbSelf.Instance.heroHandler.GetHeroDetail(m_record.id);
		((TextElement)m_lbLevel).text = "Lv." + zbSelf.Instance.heroHandler.GetHeroLevel(m_record.id);
		((TextElement)m_lbName).text = m_record.name;
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(m_data.Quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(m_data.AvatarId);
		if (recordByKey != null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.heroIcon));
		}
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(m_data.Rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(m_record, m_data.Rarity);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		bool flag = zbSelf.Instance.heroHandler.HasHeroUpgradeNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroAwakenNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id) || zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)m_record.type, m_record.id) || zbSelf.Instance.heroHandler.HasHeroFollowNotify((ESkillType)m_record.type) || zbSelf.Instance.heroHandler.HasHeroRarityNotify(m_record.id);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_new.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag || !zbSelf.Instance.newTagHandler.IsNewHero(m_record.id)) ? 1 : 0));
		RefreshSelect(m_record.id == m_wndParent.m_selectHeroTid);
	}

	public void RefreshSelect(bool flag)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void OnTouch()
	{
		m_wndParent.m_needScroll = false;
		m_wndParent.PlayDoorAnimation(delegate
		{
			m_wndParent.m_selectHeroTid = m_record.id;
			m_wndParent.RefreshCustom(m_record, m_wndParent.m_currTab, isFirstIn: false);
		}, isLeftRight: false);
	}
}
