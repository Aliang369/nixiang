using TMPro;
using UnityEngine;

public class SwitchImageSwitchTag : SwitchTag
{
	[SerializeField]
	private GameObject selectedImage;

	[SerializeField]
	private GameObject unselectedImage;

	[SerializeField]
	private TextMeshProUGUI SelectedText;

	[SerializeField]
	private TextMeshProUGUI UnselectedText;

	protected override void Init()
	{
		base.Init();
		selectedImage.SetActive(value: false);
		unselectedImage.SetActive(value: true);
	}

	protected override void OnSelect()
	{
		selectedImage.SetActive(value: true);
		unselectedImage.SetActive(value: false);
	}

	protected override void OnUnselect()
	{
		selectedImage.SetActive(value: false);
		unselectedImage.SetActive(value: true);
	}

	public override void SetText(string text)
	{
		SelectedText.text = text;
		UnselectedText.text = text;
	}
}
