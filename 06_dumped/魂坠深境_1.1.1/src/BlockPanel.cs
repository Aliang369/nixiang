using System;
using TMPro;
using UnityEngine;

public class BlockPanel : MonoBehaviour
{
	[SerializeField]
	private GameButton button1;

	[SerializeField]
	private GameButton button2;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI subTitle;

	[SerializeField]
	private TextMeshProUGUI action1Title;

	[SerializeField]
	private TextMeshProUGUI action2Title;

	[NonSerialized]
	private Action action1;

	[NonSerialized]
	private Action action2;

	public void Init()
	{
		button1.onClick.AddListener(delegate
		{
			action1?.Invoke();
		});
		button2.onClick.AddListener(delegate
		{
			action2?.Invoke();
		});
		base.gameObject.SetActive(value: false);
	}

	public void Show(string titleText, string subtitleText = null, Action action1 = null, Action action2 = null, string action1TitleText = null, string action2TitleText = null)
	{
		base.gameObject.SetActive(value: true);
		title.text = titleText;
		subTitle.text = subtitleText;
		this.action1 = action1;
		this.action2 = action2;
		button1.gameObject.SetActive(value: false);
		button2.gameObject.SetActive(value: false);
		if (!action1TitleText.IsNullOrEmpty())
		{
			action1Title.text = action1TitleText;
			button1.gameObject.SetActive(value: true);
		}
		if (!action2TitleText.IsNullOrEmpty())
		{
			action2Title.text = action2TitleText;
			button2.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		title.text = null;
		subTitle.text = null;
		action1 = null;
		action2 = null;
	}
}
