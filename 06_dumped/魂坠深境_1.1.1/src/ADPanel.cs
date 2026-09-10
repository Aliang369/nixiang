using TMPro;
using UnityEngine;

public class ADPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI statusText;

	[SerializeField]
	private GameObject loadingRoot;

	[SerializeField]
	private GameButton closeButton;

	public void Init()
	{
		closeButton.onClick.AddListener(Hide);
		base.gameObject.SetActive(value: false);
	}

	public void ShowLoading()
	{
		base.gameObject.SetActive(value: true);
		loadingRoot.SetActive(value: true);
		closeButton.gameObject.SetActive(value: false);
		statusText.text = ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_ad;
	}

	public void ShowFailed()
	{
		base.gameObject.SetActive(value: true);
		loadingRoot.SetActive(value: false);
		closeButton.gameObject.SetActive(value: true);
		statusText.text = ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_ad_play_failed;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
