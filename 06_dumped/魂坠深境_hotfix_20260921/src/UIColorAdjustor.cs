using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIColorAdjustor : MonoBehaviour
{
	private Image image;

	private Text text;

	private TextMeshProUGUI tmp;

	private UnityEngine.UI.Extensions.Gradient uiGradient;

	public Color adjustColor { get; private set; }

	public void Reset()
	{
		SetAdjustColor(Color.white);
	}

	public void SetAdjustColor(Color c)
	{
		if (image == null && text == null && tmp == null)
		{
			image = GetComponent<Image>();
			text = GetComponent<Text>();
			tmp = GetComponent<TextMeshProUGUI>();
			if (image != null || text != null)
			{
				uiGradient = base.gameObject.GetOrAddComponent<UnityEngine.UI.Extensions.Gradient>();
				uiGradient.GradientMode = UnityEngine.UI.Extensions.GradientMode.Local;
			}
		}
		adjustColor = c;
		if (tmp != null)
		{
			tmp.faceColor = adjustColor;
		}
		if (uiGradient != null)
		{
			uiGradient.Vertex1 = adjustColor;
			uiGradient.Vertex2 = adjustColor;
		}
	}

	public static bool CheckGOColorAdjustable(GameObject go)
	{
		if (go.GetComponent<Image>() != null || go.GetComponent<Text>() != null || go.GetComponent<TextMeshProUGUI>() != null)
		{
			return true;
		}
		return false;
	}
}
