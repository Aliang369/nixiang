using UnityEngine;

public class SimpleHintPanel : UIBase
{
	[SerializeField]
	private GameButton closeButton;

	[SerializeField]
	private GameButton darkenButton;

	private float autoCloseTimer;

	private bool isShowing;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		closeButton.onClick.AddListener(OnCloseButtonClicked);
		darkenButton.onClick.AddListener(OnCloseButtonClicked);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnCloseButtonClicked()
	{
		Hide();
	}
}
