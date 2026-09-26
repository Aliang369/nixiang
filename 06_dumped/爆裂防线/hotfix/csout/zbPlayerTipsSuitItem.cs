using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerTipsSuitItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private Button m_btnTouch;

	private zbPlayerTipsDetailWnd m_parent;

	public zbPlayerSuitCsvRecord m_record;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private PlayerSuitDetail m_data;

	public zbPlayerTipsSuitItem(zbPlayerTipsDetailWnd parent)
	{
		m_parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_tipsSuit.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "iconstar" + i, (string)null);
		}
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void RefreshBase()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.heroIcon);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteHero.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteHero.MarkDirtyRepaint();
			}
		}
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)m_record.quality);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(heroFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrame.MarkDirtyRepaint();
		}
	}

	public void Refresh(PlayerSuitDetail suitDetail)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		m_data = suitDetail;
		m_record = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitDetail.SuitId);
		RefreshBase();
		for (int i = 0; i < 5; i++)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/suitGun/star_0.png");
			m_spriteStars[i].style.backgroundImage = StyleBackground.op_Implicit(val);
		}
		zbUIEffect.SetStars(m_spriteStars, suitDetail.Star);
		m_spriteFrame.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(1f, 1f, 1f, 1f));
		m_spriteHero.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(1f, 1f, 1f, 1f));
	}

	public void OnTouch()
	{
		if (m_record.propId == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11338"));
		}
		else
		{
			zbUIMgr.Instance.ShowPreview(EPreviewType.Suit, m_record.propId, m_data.Star);
		}
	}
}
