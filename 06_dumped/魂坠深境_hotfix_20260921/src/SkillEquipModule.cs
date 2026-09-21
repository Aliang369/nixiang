using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipModule : SkillInfoDisplay.DisplayModule
{
	public class Info
	{
		public bool useEquipSprite;

		public string buttonText;

		public Action onClick;
	}

	[SerializeField]
	private Sprite equipSprite;

	[SerializeField]
	private Sprite unequipSprite;

	[SerializeField]
	private GameObject equipArea;

	[SerializeField]
	private GameButton button;

	[SerializeField]
	private Image buttonImage;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[NonSerialized]
	private Info info;

	public RectTransform ActiveButtonRectTransform
	{
		get
		{
			if (info == null || !(button != null) || !button.gameObject.activeInHierarchy)
			{
				return null;
			}
			return button.GetComponent<RectTransform>();
		}
	}

	public override void Init()
	{
		button.onClick.AddListener(delegate
		{
			info?.onClick?.Invoke();
		});
	}

	public override void Setup()
	{
		info = parent.GetAppendData<Info>();
		if (info == null)
		{
			equipArea.gameObject.SetActive(value: false);
			return;
		}
		equipArea.gameObject.SetActive(value: true);
		buttonImage.sprite = (info.useEquipSprite ? equipSprite : unequipSprite);
		buttonText.text = info.buttonText;
	}

	public override void Unset()
	{
		info = null;
	}
}
