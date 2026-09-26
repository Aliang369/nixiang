using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroRaritySelectItem : zbViewBaseItem
{
	private zbHeroRaritySelectWnd m_wndParent;

	private zbItem m_item = new zbItem();

	private VisualElement m_spriteSkillType;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private Button m_btnTouch;

	private VisualElement m_spriteOk;

	private HeroDetail m_data;

	public zbHeroRaritySelectItem(zbHeroRaritySelectWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_heroRaritySelect.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_skillType", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sprite_ok", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(HeroDetail data)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_item.Refresh(new zbItemData(EResourceType.Hero, data.HeroTid, 1L), isNameVis: false, isCountVis: false);
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)(m_data.Rarity + 2));
		m_item.m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resourceFrameTex));
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_data.HeroTid);
		if (recordByKey != null)
		{
			string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(recordByKey, m_data.Rarity);
			m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		}
		zbHeroRarityCsvRecord recordByKey2 = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey((int)(m_data.Rarity + 1));
		float num = Mathf.Clamp((float)m_data.RarityUpgrade / (float)recordByKey2.upgradeFrag, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num * 132f, (LengthUnit)0));
		((TextElement)m_lbProgress).text = m_data.RarityUpgrade + "/" + recordByKey2.upgradeFrag;
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_selectHeroTid != data.HeroTid) ? 1 : 0));
	}

	private void OnTouch(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			m_wndParent.RefreshSelect(-1);
		}
		else
		{
			m_wndParent.RefreshSelect(m_data.HeroTid);
		}
	}
}
