using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress;

public class FestivalCheckInEntry : UIBase
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private TextMeshProUGUI hintText;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Sprite launchIcon;

	[SerializeField]
	private Sprite carnivalIcon;

	public RectTransform ButtonTransform => button.transform as RectTransform;

	protected override void Init()
	{
		button.onClick.AddListener(_OnClicked);
	}

	public bool ShouldShow()
	{
		bool flag = UIBase.fortressState.uiManager.festivalCheckInPanel.gameObject.activeInHierarchy || UIBase.fortressState.uiManager.carnivalCheckInPanel.gameObject.activeInHierarchy;
		if ((UIBase.applicationState.dailyCheckManager.GetDisplayFestival() != null) | flag)
		{
			return UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableDailyCheckIn);
		}
		return false;
	}

	public void Refresh()
	{
		FestivalDef displayFestival = UIBase.applicationState.dailyCheckManager.GetDisplayFestival();
		if (!(displayFestival == null))
		{
			hintText.text = ((displayFestival.panelType == FestivalDef.PanelType.Carnival) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_festival_arg0_check_in_carnival_hint : ScriptableObjectSingleton<TextDataPreset>.instance.term_festival_arg0_check_in_hint.FormatWith(displayFestival.displayName));
			icon.sprite = ((displayFestival.panelType == FestivalDef.PanelType.Carnival) ? carnivalIcon : launchIcon);
		}
	}

	private void _OnClicked()
	{
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		FestivalDef displayFestival = UIBase.applicationState.dailyCheckManager.GetDisplayFestival();
		UIBase.applicationState.dailyCheckManager.SelectFestivalPanel(displayFestival).ShowUI();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "通用-活动签到");
	}
}
