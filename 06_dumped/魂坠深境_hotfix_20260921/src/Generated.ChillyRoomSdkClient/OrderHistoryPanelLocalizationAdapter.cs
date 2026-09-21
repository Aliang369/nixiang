using CoreKit.Module;
using TMPro;
using UnityEngine;

namespace Generated.ChillyRoomSdkClient;

public static class OrderHistoryPanelLocalizationAdapter
{
	private const string TipsPath = "BottomBar/Tips";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Initialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Canvas.preWillRenderCanvases -= new WillRenderCanvases(LocalizeVisiblePanels);
		Canvas.preWillRenderCanvases += new WillRenderCanvases(LocalizeVisiblePanels);
	}

	private static void LocalizeVisiblePanels()
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		if (instance == null || string.IsNullOrEmpty(instance.term_order_history_tips))
		{
			return;
		}
		OrderHistoryPanel[] array = Object.FindObjectsOfType<OrderHistoryPanel>(true);
		for (int i = 0; i < array.Length; i++)
		{
			TMP_Text tMP_Text = ((Component)(object)array[i]).transform.Find("BottomBar/Tips")?.GetComponent<TMP_Text>();
			if (tMP_Text != null && tMP_Text.text != instance.term_order_history_tips)
			{
				tMP_Text.text = instance.term_order_history_tips;
			}
		}
	}
}
