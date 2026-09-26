using UnityEngine.UIElements;

public class zbWallSubjectRelicCell : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbWallSubjectRelicCell>
	{
	}

	private zbWallSubjectRelicWnd m_wndParent;

	private zbWallSubjectRelicItem[] m_items = new zbWallSubjectRelicItem[5];

	public zbWallSubjectRelicCell()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/wallSubject/tb_wallSubjectRelicCell.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbWallSubjectRelicWnd wndParent)
	{
		m_wndParent = wndParent;
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbWallSubjectRelicItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int[] relicList)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (int id in relicList)
		{
			if (num < m_items.Length)
			{
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(id);
			}
			num++;
		}
	}
}
