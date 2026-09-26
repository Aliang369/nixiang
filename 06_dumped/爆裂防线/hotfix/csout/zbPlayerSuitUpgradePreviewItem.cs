using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerSuitUpgradePreviewItem : zbViewBaseItem
{
	private zbPlayerSuitUpgradePreviewWnd m_Parent;

	private VisualElement m_title;

	private VisualElement m_content;

	private Label m_lbTitle;

	private Label m_lbContent;

	private int m_index;

	public zbPlayerSuitUpgradePreviewItem(zbPlayerSuitUpgradePreviewWnd parent)
	{
		m_Parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/player/playerSuit/tb_playerSuitUpgradePreview.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_title = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "title", (string)null);
		m_content = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "content", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		m_lbContent = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_content", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int index)
	{
		m_index = index;
		((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.10060"), (m_index + 1).ToString());
		if (m_Parent.m_type == EPreviewType.Suit)
		{
			RefreshSuit();
		}
		if (m_Parent.m_type == EPreviewType.Wall)
		{
			RefreshWall();
		}
		if (m_Parent.m_type == EPreviewType.Foothold)
		{
			RefreshFoothold();
		}
	}

	private void RefreshSuit()
	{
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(m_Parent.m_id);
		string key = "";
		switch (m_index)
		{
		case 0:
			key = recordByKey.skillDesc1;
			break;
		case 1:
			key = recordByKey.skillDesc2;
			break;
		case 2:
			key = recordByKey.skillDesc3;
			break;
		case 3:
			key = recordByKey.skillDesc4;
			break;
		case 4:
			key = recordByKey.skillDesc5;
			break;
		}
		((TextElement)m_lbContent).text = zbUtil.ReplaceRichTextColor(zbUtil.Language(key));
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(m_Parent.m_id);
		bool flag = playerSuitDetail != null && m_index + 1 == playerSuitDetail.Star;
		if (m_Parent.isCurrent >= 0)
		{
			flag = m_index == m_Parent.isCurrent;
		}
		((TextElement)m_lbTitle).text = (flag ? (((TextElement)m_lbTitle).text + "(" + zbUtil.Language("lang.10193") + ")") : ((TextElement)m_lbTitle).text);
		m_title.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_title_1.jpg" : "ui/image/suitGun/suit_skill_title_0.jpg"));
		m_content.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_content_1.jpg" : "ui/image/suitGun/suit_skill_content_0.jpg"));
	}

	private void RefreshWall()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		zbDataConfig.Instance.wallCsvTable.GetRecordByKey(m_Parent.m_id);
		((TextElement)m_lbContent).text = zbUtil.ReplaceRichTextColor(zbSelf.Instance.wallHandler.GetSkillDesc(m_Parent.m_id, m_index + 1));
		WallDetail wallDetail = zbSelf.Instance.wallHandler.GetWallDetail(m_Parent.m_id);
		bool flag = wallDetail != null && m_index + 1 == wallDetail.Star;
		if (m_Parent.isCurrent >= 0)
		{
			flag = m_index == m_Parent.isCurrent;
		}
		((TextElement)m_lbTitle).text = (flag ? (((TextElement)m_lbTitle).text + "(" + zbUtil.Language("lang.10193") + ")") : ((TextElement)m_lbTitle).text);
		m_title.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_title_1.jpg" : "ui/image/suitGun/suit_skill_title_0.jpg"));
		m_content.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_content_1.jpg" : "ui/image/suitGun/suit_skill_content_0.jpg"));
	}

	private void RefreshFoothold()
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		zbFootholdEffectCsvRecord recordByKey = zbDataConfig.Instance.footholdEffectCsvTable.GetRecordByKey(m_Parent.skillIdList[m_index]);
		((TextElement)m_lbContent).text = zbUtil.ReplaceRichTextColor(string.Format(recordByKey.desc, recordByKey.param.Cast<object>().ToArray())) + "\n(" + string.Format(zbUtil.Language("lang.10801"), m_Parent.firstIndexes[m_index] + 1) + ")";
		FootholdDetail footholdDetail = zbSelf.Instance.guardHandler.GetFootholdDetail(m_Parent.m_id);
		bool flag = footholdDetail != null && m_index + 1 == footholdDetail.Star;
		if (m_Parent.isCurrent >= 0)
		{
			flag = m_index == m_Parent.isCurrent;
		}
		((TextElement)m_lbTitle).text = (flag ? (((TextElement)m_lbTitle).text + "(" + zbUtil.Language("lang.10193") + ")") : ((TextElement)m_lbTitle).text);
		m_title.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_title_1.jpg" : "ui/image/suitGun/suit_skill_title_0.jpg"));
		m_content.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/suit_skill_content_1.jpg" : "ui/image/suitGun/suit_skill_content_0.jpg"));
	}
}
