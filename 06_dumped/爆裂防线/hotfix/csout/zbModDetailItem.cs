using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModDetailItem : zbViewBaseItem
{
	private Button m_btnTouch;

	private Label m_lbName;

	private Label m_lbLocked;

	private VisualElement m_heroNode;

	private VisualElement m_spFrame;

	private VisualElement m_spHeroIcon;

	private VisualElement m_spRarity;

	private VisualElement m_spSkill;

	private zbModDetailWnd m_wndParent;

	private int m_starCount = -1;

	private int m_heroId;

	public zbModDetailItem(zbModDetailWnd wndParent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = wndParent;
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLocked = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_locked", (string)null);
		m_heroNode = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "heroNode", (string)null);
		m_spFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spHeroIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sp_heroIcon", (string)null);
		m_spRarity = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_rarity", (string)null);
		m_spSkill = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skill", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int heroId, zbModHandbookCsvRecord mod, int starCount = -1)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		m_starCount = starCount;
		m_heroId = heroId;
		m_heroNode.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(starCount == -1));
		((VisualElement)m_lbLocked).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(starCount == -1));
		if (starCount != -1)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroId);
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			m_spHeroIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.heroIcon));
			int num = -1;
			num = starCount switch
			{
				2 => recordByKey.mod1, 
				3 => recordByKey.mod2, 
				4 => recordByKey.mod3, 
				5 => recordByKey.mod4, 
				6 => recordByKey.mod5, 
				7 => recordByKey.mod6, 
				_ => -1, 
			};
			((TextElement)m_lbLocked).text = zbDataConfig.Instance.modCsvTable.GetRecordByKey(num).desc;
			string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)starCount);
			m_spFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
			string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath((ERarityType)recordByKey.rarity);
			m_spRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
			string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(recordByKey);
			m_spSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
			zbQualityCsvRecord recordByKey3 = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(starCount);
			if (recordByKey3 != null)
			{
				string text = string.Format(zbUtil.Language("lang.10833"), recordByKey3.name);
				string text2 = zbUtil.ConvertToHexColor(recordByKey3.color);
				((TextElement)m_lbName).text = recordByKey.name + "<color=" + text2 + ">" + text + "</color>";
			}
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_starCount != -1)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_heroId);
			zbUIMgr.Instance.ShowHeroDetail(recordByKey);
		}
	}
}
