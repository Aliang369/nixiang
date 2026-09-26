using Proto.Types;
using UnityEngine.UIElements;

public class zbEndlessMineRow : zbViewBaseItem
{
	public zbEndlessMineCell[] m_cells = new zbEndlessMineCell[6];

	public zbEndlessMineRow()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/endlessMine/tb_endlessMineRow.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable()
	{
		for (int i = 0; i < 6; i++)
		{
			m_cells[i] = UQueryExtensions.Q<zbEndlessMineCell>((VisualElement)(object)this, "node" + i, (string)null);
			m_cells[i].OnEnable((VisualElement)(object)m_cells[i]);
			m_cells[i].Refresh(null);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(EndlessMineMapRowDetail data)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_cells.Length; i++)
		{
			m_cells[i].SetNodePosConvert(((VisualElement)this).resolvedStyle.translate.y);
			m_cells[i].Refresh(null);
		}
		if (data == null)
		{
			return;
		}
		int num = 0;
		foreach (EndlessMineMapCellDetail cell in data.CellList)
		{
			if (num < m_cells.Length)
			{
				m_cells[num].Refresh(cell, data.Row);
			}
			num++;
		}
	}
}
