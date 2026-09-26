using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSkillUpgradePreviewItem : zbViewBaseItem
{
	private zbMechaSkillUpgradePreviewWnd m_Parent;

	private VisualElement m_title;

	private VisualElement m_content;

	private Label m_lbTitle;

	private Label m_lbContent;

	private VisualElement m_icon;

	public zbMechaSkillUpgradePreviewItem(zbMechaSkillUpgradePreviewWnd parent)
	{
		m_Parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mecha/tb_mechaSkillUpgradePreview.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_title = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "title", (string)null);
		m_content = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "content", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		m_lbContent = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_content", (string)null);
		m_icon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int skillId)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		zbMechaSkillCsvRecord recordByKey = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(skillId);
		((TextElement)m_lbTitle).text = zbUtil.Language(recordByKey.name);
		m_icon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		bool flag = zbSelf.Instance.mechaHandler.GetMechaSkillDetailBySkillId(skillId) == null;
		m_title.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_title_0.jpg" : "ui/image/suitGun/suit_skill_title_1.jpg"));
		m_content.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_content_0.jpg" : "ui/image/suitGun/suit_skill_content_1.jpg"));
		zbLabelLinkUtil.BuildModLink(zbSelf.Instance.mechaHandler.GetCurMechaSkillDesc(skillId), m_lbContent);
		((TextElement)m_lbContent).text = ((TextElement)m_lbContent).text.Replace("<color=#64FF26>", "<color=#3CB317>");
	}
}
