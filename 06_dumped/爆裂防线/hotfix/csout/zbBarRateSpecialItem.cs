using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarRateSpecialItem : zbViewBaseItem
{
	private zbBarRateSpecialWnd m_wndParent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private Label m_lbRate;

	private Label m_lbName;

	private Button m_btnTouch;

	private zbHeroCsvRecord m_heroRecord;

	public zbBarRateSpecialItem(zbBarRateSpecialWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_barRateSpecial.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_lbRate = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rate", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbRateData rateData)
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbRate).text = (rateData.rate * 100f).ToString("F2") + "%";
		m_heroRecord = zbDataConfig.Instance.heroCsvTable.GetRecordByKey((int)rateData.itemData.id);
		if (m_heroRecord == null)
		{
			return;
		}
		((TextElement)m_lbName).text = m_heroRecord.name;
		int avatarId = m_heroRecord.avatarId;
		HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(m_heroRecord.id);
		if (heroDetail != null)
		{
			avatarId = heroDetail.AvatarId;
		}
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.heroIcon);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteHero.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteHero.MarkDirtyRepaint();
			}
		}
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(zbResourceHelper.GetResourceQuality(EResourceType.Hero, m_heroRecord.id));
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(heroFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrame.MarkDirtyRepaint();
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHeroDetail(m_heroRecord);
	}
}
