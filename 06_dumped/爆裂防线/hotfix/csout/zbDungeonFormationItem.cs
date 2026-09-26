using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonFormationItem : zbViewBaseItem
{
	private zbDungeonFormationWnd m_parent;

	private VisualElement m_iconHero;

	private Label m_lbName;

	private Button m_btnTouch;

	private Label m_iconIsCheck;

	private zbHeroCsvRecord m_record;

	private HeroDetail m_data;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteRarity;

	private VisualElement m_spriteSkill;

	public zbDungeonFormationItem(zbDungeonFormationWnd parent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/dungeon/tb_formation.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_parent = parent;
		m_iconHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_hero", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_iconIsCheck = UQueryExtensions.Q<Label>((VisualElement)(object)this, "bg_select", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_rarity", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skill", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnToggle, "", (TrickleDown)0);
	}

	public void Refresh(zbHeroCsvRecord data, Dictionary<int, int> dictSelectHero)
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		m_record = data;
		((TextElement)m_lbName).text = m_record.name;
		((VisualElement)m_iconIsCheck).visible = Enumerable.Contains(dictSelectHero.Values.ToArray(), data.type);
		int avatarId = m_record.avatarId;
		HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(data.id);
		if (heroDetail != null)
		{
			avatarId = heroDetail.AvatarId;
		}
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.heroIcon);
			if ((Object)(object)val != (Object)null && (Object)(object)val != (Object)null)
			{
				m_iconHero.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_iconHero.MarkDirtyRepaint();
			}
		}
		EQuality quality = zbResourceHelper.GetResourceQuality(EResourceType.Hero, m_record.id);
		ERarityType rarity = (ERarityType)m_record.rarity;
		m_data = zbSelf.Instance.heroHandler.GetHeroDetail(m_record.id);
		if (m_data != null)
		{
			quality = m_data.Quality;
			rarity = m_data.Rarity;
		}
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(m_record, rarity);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
	}

	private void OnToggle()
	{
		bool visible = ((VisualElement)m_iconIsCheck).visible;
		m_parent.OnToggle(this, !visible);
	}

	public void SetSelectVisible(bool visible)
	{
		((VisualElement)m_iconIsCheck).visible = visible;
	}

	public zbHeroCsvRecord GetHeroRecord()
	{
		return m_record;
	}
}
