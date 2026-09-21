using UnityEngine;
using UnityEngine.UI;

public class TestTogglePanel : MonoBehaviour
{
	public static TestTogglePanel instance;

	[SerializeField]
	private Toggle skipGuidToggle;

	[SerializeField]
	private Toggle skipStoryToggle;

	[SerializeField]
	private Toggle startUnlockAllToggle;

	public bool skipGuide => skipGuidToggle.isOn;

	public bool skipStory => skipStoryToggle.isOn;

	public bool startUnlockAll
	{
		get
		{
			return ScriptableObjectSingleton<GameLoadDataManager>.instance.testMenuConfigData.startUnlockAll;
		}
		set
		{
			ScriptableObjectSingleton<GameLoadDataManager>.instance.testMenuConfigData.startUnlockAll = value;
		}
	}

	private void Awake()
	{
		instance = this;
		startUnlockAllToggle.isOn = startUnlockAll;
		startUnlockAllToggle.onValueChanged.AddListener(delegate(bool v)
		{
			startUnlockAll = v;
		});
	}
}
