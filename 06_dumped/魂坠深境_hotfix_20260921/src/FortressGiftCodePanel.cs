using TMPro;
using UnityEngine;

public class FortressGiftCodePanel : UIBase, IStackPanel
{
	[SerializeField]
	private TMP_InputField codeInputField;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI errorText;

	[SerializeField]
	private GameButton requestButton;

	protected override void Init()
	{
		backButton.onClick.AddListener(_OnCloseButtonClicked);
		requestButton.onClick.AddListener(_OnRequestButtonClicked);
		errorText.text = string.Empty;
	}

	private void _OnRequestButtonClicked()
	{
		string text = codeInputField.text;
		if (!string.IsNullOrEmpty(text))
		{
			UIBase.applicationState.giftCodeManager.AcceptGiftCode(text, errorText);
			codeInputField.text = string.Empty;
		}
	}

	private void _OnCloseButtonClicked()
	{
		codeInputField.text = string.Empty;
		errorText.text = string.Empty;
		base.gameObject.SetActive(value: false);
		SetDirty();
	}
}
