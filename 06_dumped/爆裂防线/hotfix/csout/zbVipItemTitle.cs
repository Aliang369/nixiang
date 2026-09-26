using Proto.Enums;
using UnityEngine.UIElements;

public class zbVipItemTitle : zbViewBaseItem
{
	private VisualElement m_spriteBg;

	private Label m_lbTitle;

	public zbVipItemTitle(EShopType shopType)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_shopTitle.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		EFuncType funcTypeByShopType = zbSelf.Instance.shopHandler.GetFuncTypeByShopType(shopType);
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)funcTypeByShopType);
		if (recordByKey != null)
		{
			((TextElement)m_lbTitle).text = recordByKey.name;
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
