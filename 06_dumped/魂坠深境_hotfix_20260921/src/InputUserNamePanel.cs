using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChillyRoom.Censor.V1;
using ChillyRoom.Services.ContainerOfSoul;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class InputUserNamePanel : UIBase
{
	public TMP_InputField input;

	public GameButton confirmButton;

	public TextMeshProUGUI hint;

	public CanvasGroup canvasGroup;

	private bool isSetting;

	private static readonly string SpecialCharacters = "!\"#$%&'()*+,-./:;<=>?@[]^_`{|}~。！｛｝【】、、《》''；：？￥·";

	protected override void Init()
	{
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
	}

	public void Setup()
	{
		base.gameObject.SetActive(value: true);
		canvasGroup.alpha = 0f;
		DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.5f);
		confirmButton.interactable = true;
		hint.text = "";
		isSetting = false;
	}

	private bool ContainsSpecialCharacter(string text)
	{
		return text.IndexOfAny(SpecialCharacters.ToCharArray()) >= 0;
	}

	private void _OnConfirmButtonClicked()
	{
		try
		{
			SetName(input.text);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private async Task SetName(string pendingText)
	{
		if (isSetting)
		{
			return;
		}
		isSetting = true;
		hint.text = "......";
		if (!(await ValidateText(pendingText)))
		{
			isSetting = false;
			return;
		}
		hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_communicating;
		bool finish = false;
		bool success = false;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			SetNickNameReq setNickNameReq = new SetNickNameReq();
			setNickNameReq.NickName = pendingText;
			new GeneralGameAsyncOperationRequestOnly<SetNickNameReq>(setNickNameReq, UIBase.applicationState.ChillyRoomSDK.ProfileData.SetPlayerNickNameAsync).ManualRetry().OnComplete(delegate
			{
				success = true;
			}).OnOver(delegate
			{
				finish = true;
			})
				.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_saving_name);
		}
		else
		{
			finish = true;
			success = true;
		}
		await TaskHelper.Wait(() => finish);
		if (!success)
		{
			hint.text = base.textData.term_network_error_general_networkError;
			isSetting = false;
			return;
		}
		hint.text = "";
		isSetting = false;
		UIBase.applicationState.playerInfoManager.SetNickName(pendingText);
		DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.5f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private async Task<bool> ValidateText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_nickname_not_allow_empty;
			return false;
		}
		if (ContainsSpecialCharacter(text) || text.Any(char.IsControl))
		{
			hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_nickname_not_allow_special_characters;
			return false;
		}
		if (Encoding.Default.GetByteCount(text) > ScriptableObjectSingleton<FortressDataManager>.instance.playerNickNameByteLimit)
		{
			hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_nickname_too_long;
			return false;
		}
		if (GlobalConfigManager.instance.EnableBlockWordsCheck && !(await CensorCheck(text)))
		{
			return false;
		}
		return true;
	}

	private async Task<bool> CensorCheck(string text)
	{
		if (text == null)
		{
			return false;
		}
		bool censorCheckSuccess = false;
		bool networkError = false;
		try
		{
			switch (await ValidationUtil.Validate(text, TextUsageHint.NickName))
			{
			case TextValidationResult.Pass:
				censorCheckSuccess = true;
				break;
			case TextValidationResult.ContainsSensitivePhrase:
				censorCheckSuccess = false;
				break;
			default:
				networkError = true;
				break;
			}
		}
		catch
		{
			networkError = true;
		}
		if (networkError)
		{
			hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_networkError;
		}
		else
		{
			if (censorCheckSuccess)
			{
				return true;
			}
			hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_nickname_not_allow;
		}
		return false;
	}
}
