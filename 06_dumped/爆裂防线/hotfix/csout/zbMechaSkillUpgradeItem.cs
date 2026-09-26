using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSkillUpgradeItem : zbViewBaseItem
{
	private VisualElement m_spriteBg;

	private Label m_lbLevel;

	private Label m_lbDesc;

	public zbMechaSkillUpgradeItem(VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int level, MechaSkillDetail skillDetail, zbMechaSkillCsvRecord skillRecord)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbLevel).text = level.ToString();
		if (skillDetail != null && skillDetail.Level >= level)
		{
			m_spriteBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/worldBoss/rank_bg1.png"));
		}
		else
		{
			m_spriteBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legion/rank_bg.png"));
		}
		if (skillDetail != null && skillRecord != null)
		{
			float damageRate = zbSelf.Instance.mechaHandler.GetDamageRate(skillDetail.SkillId, level);
			((TextElement)m_lbDesc).text = string.Format(skillRecord.upgradeDesc, damageRate);
		}
	}
}
