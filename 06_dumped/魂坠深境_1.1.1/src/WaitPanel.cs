using Common.UI;
using TMPro;
using UnityEngine;

public class WaitPanel : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI loadingContentTextPrefab;

	private PrefabPool<TextMeshProUGUI> loadingContentTextPool;

	public CanvasGroup CanvasGroup => canvasGroup;

	public void Init()
	{
		loadingContentTextPool = new PrefabPool<TextMeshProUGUI>(loadingContentTextPrefab);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetAlpha(float alpha)
	{
		canvasGroup.alpha = alpha;
	}

	public void SyncLoadingContents(string[] contents)
	{
		loadingContentTextPool.DespawnAll();
		foreach (string text in contents)
		{
			loadingContentTextPool.Spawn().text = text;
		}
	}
}
