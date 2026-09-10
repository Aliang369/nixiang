using UnityEngine;
using UnityEngine.UI;

public class SpeedButton : UIBase
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private Image image;

	[SerializeField]
	private Sprite[] sprites;

	protected override void Init()
	{
		button.onClick.AddListener(_OnButtonClicked);
		UIBase.applicationState.eventManager.speedChanged.AddListener(SetDirty);
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.speedChanged.RemoveListener(SetDirty);
	}

	protected override void UpdateDisplay()
	{
		switch (Bootstrap.instance.globalSaveConfigData.battleTimeScaleTypeSetting)
		{
		case TimeScaleManager.TimeScaleType.X2:
			image.sprite = sprites[1];
			break;
		case TimeScaleManager.TimeScaleType.X4:
			image.sprite = sprites[2];
			break;
		default:
			image.sprite = sprites[0];
			break;
		}
	}

	private void _OnButtonClicked()
	{
		switch (Bootstrap.instance.globalSaveConfigData.battleTimeScaleTypeSetting)
		{
		case TimeScaleManager.TimeScaleType.X2:
			Bootstrap.instance.globalSaveConfigData.uiTimeScaleTypeSetting = TimeScaleManager.TimeScaleType.X4;
			Bootstrap.instance.globalSaveConfigData.battleTimeScaleTypeSetting = TimeScaleManager.TimeScaleType.X4;
			break;
		case TimeScaleManager.TimeScaleType.X4:
			Bootstrap.instance.globalSaveConfigData.uiTimeScaleTypeSetting = TimeScaleManager.TimeScaleType.X1;
			Bootstrap.instance.globalSaveConfigData.battleTimeScaleTypeSetting = TimeScaleManager.TimeScaleType.X1;
			break;
		default:
			Bootstrap.instance.globalSaveConfigData.uiTimeScaleTypeSetting = TimeScaleManager.TimeScaleType.X2;
			Bootstrap.instance.globalSaveConfigData.battleTimeScaleTypeSetting = TimeScaleManager.TimeScaleType.X2;
			break;
		}
		Bootstrap.instance.globalSaveConfigData.Save(UIBase.applicationState);
		ThinkingAnalyticsExtends.TrackSettingChange("加速模式", Bootstrap.instance.globalSaveConfigData.battleTimeScaleTypeSetting.ToString(), UIBase.applicationState);
		SetDirty();
	}
}
