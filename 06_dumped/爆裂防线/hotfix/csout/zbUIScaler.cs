using UnityEngine;
using UnityEngine.UIElements;

public class zbUIScaler
{
	private const float DesignWidth = 720f;

	private const float DesignHeight = 1480f;

	private static float m_scale = 1f;

	public static void AdapationUI(UIDocument doc)
	{
		if (!((Object)(object)doc == (Object)null))
		{
			float num = Screen.width;
			float num2 = Screen.height;
			float num3 = 2.0555556f;
			float num4 = num2 / num;
			float match = 0f;
			if (num4 <= num3)
			{
				match = 1f;
				m_scale = num2 / 1480f;
			}
			else
			{
				m_scale = num / 720f;
			}
			doc.panelSettings.match = match;
		}
	}

	public static float GetUIScale()
	{
		return m_scale;
	}
}
