using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateDisplayModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private CanvasGroup envCanvasGroup;

	[SerializeField]
	private Image nameImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Image finishImage;

	public override void UpdateDisplay()
	{
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		bool flag = base.pathLocationDisplay.location.variantType != PathLocation.LocationVariantType.None;
		switch (base.pathLocationDisplay.currentState)
		{
		case PathLocationDisplay.State.Current_Unfinished:
		case PathLocationDisplay.State.Current_Finished:
		case PathLocationDisplay.State.Finished:
			SetDisplay(dungeonMapDisplayConfig.finished);
			finishImage.gameObject.SetActive(value: true);
			break;
		case PathLocationDisplay.State.Selectable:
		case PathLocationDisplay.State.Focused:
			SetDisplay(flag ? ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.customPivot : dungeonMapDisplayConfig.selectable);
			finishImage.gameObject.SetActive(value: false);
			break;
		case PathLocationDisplay.State.Idle:
			SetDisplay(flag ? ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.customPivot : dungeonMapDisplayConfig.canReach);
			finishImage.gameObject.SetActive(value: false);
			break;
		default:
			SetDisplay(dungeonMapDisplayConfig.cannotReach);
			finishImage.gameObject.SetActive(value: false);
			break;
		}
	}

	private void SetDisplay(DungeonMapDisplay.Config.StateConfig stateConfig)
	{
		icon.color = stateConfig.iconColor;
		envCanvasGroup.alpha = icon.color.a;
		nameImage.color = stateConfig.nameImageColor;
		nameText.color = stateConfig.nameTextColor;
	}
}
