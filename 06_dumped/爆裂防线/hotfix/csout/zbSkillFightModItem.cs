using UnityEngine;
using UnityEngine.UIElements;

public class zbSkillFightModItem : zbViewBaseItem
{
	private Label m_lbName;

	private Label m_icon;

	private Label m_lbDesc;

	public VisualElement iconMod;

	private VisualElement sp_bg;

	public zbSkillFightModItem(VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_icon = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_icon", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		iconMod = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_icon, "sp_modicon", (string)null);
		sp_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
	}

	public void Init(zbModCsvRecord data, int skillId)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = data.name;
		zbLabelLinkUtil.BuildModLink(data.desc, m_lbDesc);
		((TextElement)m_lbDesc).text = ((TextElement)m_lbDesc).text.Replace("<color=#64FF26>", "<color=#3CB317>");
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/mod_bg" + data.quality + ".png");
		sp_bg.style.backgroundImage = new StyleBackground(val);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(data.icon);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		iconMod.style.backgroundImage = new StyleBackground(val2);
		zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(skillId);
		if (recordByKey != null)
		{
			Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
			((VisualElement)m_icon).style.backgroundImage = new StyleBackground(val3);
		}
	}
}
