using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct SwitchButton
{
	[SerializeField]
	public GameButton button;

	[SerializeField]
	private Image unselectImage;

	[SerializeField]
	private Image selectedImage;

	public void Select()
	{
		unselectImage.gameObject.SetActive(value: false);
		selectedImage.gameObject.SetActive(value: true);
		button.targetGraphic = selectedImage;
	}

	public void Unselect()
	{
		unselectImage.gameObject.SetActive(value: true);
		selectedImage.gameObject.SetActive(value: false);
		button.targetGraphic = unselectImage;
	}
}
