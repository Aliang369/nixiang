using UnityEngine.UIElements;

public class zbWorldItemGuide : zbViewBaseItem
{
	public VisualElement m_spriteBg;

	private float m_cellWidth = 140f;

	private float m_cellHeight = 140f;

	public zbWorldItemGuide(zbWorldWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldGuide.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		zbUtil.SetPickingModeIgnoreRecursive((VisualElement)(object)this);
		Refresh();
	}

	public void Refresh()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		int num = 1 + (zbServerData.sGameData.WorldInfo.WorldMapInfo.MaxCol + 1) / 2;
		int num2 = (zbServerData.sGameData.WorldInfo.WorldMapInfo.MaxRow + 1) / 2 + 2;
		((VisualElement)this).style.left = StyleLength.op_Implicit((float)num * m_cellWidth - m_cellWidth);
		((VisualElement)this).style.top = StyleLength.op_Implicit((float)num2 * m_cellHeight - m_cellHeight);
	}
}
