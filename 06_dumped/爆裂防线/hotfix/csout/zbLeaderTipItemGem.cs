using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderTipItemGem : zbViewBaseItem
{
	private zbLeaderTipWnd m_wndParent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbDesc;

	public zbLeaderTipItemGem(zbLeaderTipWnd wndParent, VisualTreeAsset tAsset)
	{
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int gemTid)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemTid);
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)recordByKey.quality);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteFrame.MarkDirtyRepaint();
		}
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteIcon.MarkDirtyRepaint();
		}
		zbLabelLinkUtil.BuildModLink(zbResourceHelper.GetResorceIconPath(EResourceType.Gem, gemTid).strDesc, m_lbDesc);
	}
}
