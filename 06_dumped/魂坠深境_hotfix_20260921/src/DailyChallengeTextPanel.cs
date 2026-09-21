using TMPro;
using UnityEngine;

public sealed class DailyChallengeTextPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI body;

	public void Setup(string titleText, string bodyText, bool active)
	{
		base.gameObject.SetActive(active);
		if (active)
		{
			title.text = titleText;
			body.text = bodyText;
		}
	}
}
