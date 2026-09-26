using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasureTitleItem : zbViewBaseItem
{
	private Label m_lbTitle;

	private int m_quality;

	private static readonly string[] m_titles = new string[3] { "Keter", "Euclid", "Safe" };

	public zbTreasureTitleItem(int quality)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_quality = quality;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/treasure/tb_treasureTitle.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		((VisualElement)m_lbTitle).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/treasure/treasure_title{quality}.png"));
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh()
	{
		int num = 0;
		foreach (KeyValuePair<int, zbTreasureCsvRecord[]> dictionaryRecord in zbSelf.Instance.treasureHandler.dictionaryRecords)
		{
			if (dictionaryRecord.Key == m_quality)
			{
				int num2 = 0;
				zbTreasureCsvRecord[] value = dictionaryRecord.Value;
				foreach (zbTreasureCsvRecord zbTreasureCsvRecord2 in value)
				{
					if (zbSelf.Instance.treasureHandler.GetTreasureLevel(zbTreasureCsvRecord2.id) > 0)
					{
						num2++;
					}
				}
				((TextElement)m_lbTitle).text = m_titles[num] + "(" + num2 + "/" + dictionaryRecord.Value.Length + ")";
				break;
			}
			num++;
		}
	}
}
