using TMPro;
using UnityEngine;

public class StoryReferenceDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI storyText;

	public void Setup(Item item)
	{
		base.gameObject.SetActive(value: true);
		storyText.text = item.GetDef().itemStory;
	}

	public void Unset()
	{
		base.gameObject.SetActive(value: false);
	}
}
