using Generated.ChillyRoomSdkClient;
using TMPro;

namespace ContainerOfSoul.Display.Fortress;

public class FortressSettingPanel : UIBase
{
	public GameButton resumeButton;

	public GameButton logoutButton;

	public GameButton privacyButton;

	public TextMeshProUGUI accountInfoText;

	public TextMeshProUGUI realNameInfoText;

	protected override void Init()
	{
		base.Init();
		resumeButton.onClick.AddListener(_OnResumeButtonClicked);
		logoutButton.onClick.AddListener(_OnLogoutButtonClicked);
		privacyButton.onClick.AddListener(_OnPrivacyButtonClicked);
		ForceUpdate();
	}

	protected override void UpdateDisplay()
	{
		accountInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_current_account_arg0.FormatWith(Bootstrap.instance.CurrentAccountID);
		realNameInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_real_name_applied;
	}

	private void _OnResumeButtonClicked()
	{
		base.gameObject.SetActive(value: false);
	}

	private void _OnLogoutButtonClicked()
	{
		AccountUtil.Logout();
	}

	private void _OnPrivacyButtonClicked()
	{
		ChillyRoomService.GetSdk().Agreement.ShowPrivacyPage();
	}
}
