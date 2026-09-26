using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerTipsGunItem : VisualElement
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private Button m_btnTouch;

	private VisualElement m_nodeStar;

	private zbPlayerTipsDetailWnd m_parent;

	public zbPlayerGunCsvRecord m_record;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private PlayerGunDetail m_data;

	public zbPlayerTipsGunItem(zbPlayerTipsDetailWnd parent)
	{
		m_parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_tipsGun.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_bottom", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "iconstar" + i, (string)null);
		}
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void Refresh(PlayerGunDetail gunDetail)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		m_data = gunDetail;
		m_record = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gunDetail.GunId);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(m_record.icon);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteHero.MarkDirtyRepaint();
		}
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)m_record.quality);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrame.MarkDirtyRepaint();
		}
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_record.propId == 0));
		for (int i = 0; i < 5; i++)
		{
			Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/suitGun/star_0.png");
			m_spriteStars[i].style.backgroundImage = StyleBackground.op_Implicit(val3);
		}
		zbUIEffect.SetStars(m_spriteStars, gunDetail.Star);
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
			zbUIMgr.Instance.ShowPreview(EPreviewType.Gun, m_record.propId, m_data.Star);
		}
	}
}
