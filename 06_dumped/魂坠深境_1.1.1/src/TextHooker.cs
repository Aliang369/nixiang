using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextHooker : MonoBehaviour
{
	[SerializeField]
	private Text text;

	[SerializeField]
	private TextMeshProUGUI textMeshProUGUI;

	private void Update()
	{
		text.enabled = false;
		textMeshProUGUI.text = text.text;
	}

	private void OnValidate()
	{
		if (LocalSettings.autoUpdate && (text == null || textMeshProUGUI == null))
		{
			Debug.LogError(base.gameObject.name);
		}
	}
}
