using System.Collections;
using TMPro;
using UnityEngine;

public class ForerunnerHintPanel : UIBase
{
	public GameButton giveButton;

	public GameButton noGiveButton;

	public GameButton shareButton;

	public GameButton noShareButton;

	public GameButton replaceShareButton;

	public GameButton noReplaceShareButton;

	public ForerunnerLegacyRecordCell toReplaceForerunnerLegacyCell;

	public TextMeshProUGUI hintText;

	private bool? toShare;

	private bool? toReplaceShare;

	private ForerunnerPanel parentPanel;

	private ForerunnerLegacy ForerunnerLegacy
	{
		get
		{
			return parentPanel.forerunnerLegacy;
		}
		set
		{
			parentPanel.forerunnerLegacy = value;
		}
	}

	protected override void Init()
	{
		InitUI(toReplaceForerunnerLegacyCell);
		parentPanel = GetUIInParents<ForerunnerPanel>();
		giveButton.onClick.AddListener(_OnGiveButtonClicked);
		noGiveButton.onClick.AddListener(_OnNoGiveButtonClicked);
		shareButton.onClick.AddListener(_OnShareButtonClicked);
		noShareButton.onClick.AddListener(_OnNoShareButtonClicked);
		replaceShareButton.onClick.AddListener(_OnReplaceShareButtonClicked);
		noReplaceShareButton.onClick.AddListener(_OnNoReplaceShareButtonClicked);
	}

	public void TriggerAfterGive()
	{
		UIBase.gameState.StartCoroutine(AfterGiveCor);
	}

	private IEnumerator AfterGiveCor()
	{
		base.gameObject.SetActive(value: true);
		parentPanel.forerunnerSelectPanel.SetActiveAndDirty(active: false);
		parentPanel.forerunnerLeaveNotePanel.SetActiveAndDirty(active: false);
		yield return ChooseShareCor();
		giveButton.gameObject.SetActive(value: false);
		noGiveButton.gameObject.SetActive(value: false);
		shareButton.gameObject.SetActive(value: false);
		noShareButton.gameObject.SetActive(value: false);
		replaceShareButton.gameObject.SetActive(value: false);
		noReplaceShareButton.gameObject.SetActive(value: false);
		yield return hintText.ScrollTextCor(ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_leave_legacy_hint_after_give_1, ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueBaseReadSpeed);
		yield return new WaitForSeconds(1f);
		yield return hintText.ScrollTextCor(ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_leave_legacy_hint_after_give_2, ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueBaseReadSpeed);
		yield return new WaitForSeconds(1f);
		parentPanel.UploadForrunnerLegacy(toShare == true);
	}

	private IEnumerator ChooseShareCor()
	{
		giveButton.gameObject.SetActive(value: false);
		noGiveButton.gameObject.SetActive(value: false);
		shareButton.gameObject.SetActive(value: true);
		noShareButton.gameObject.SetActive(value: true);
		replaceShareButton.gameObject.SetActive(value: false);
		noReplaceShareButton.gameObject.SetActive(value: false);
		yield return hintText.ScrollTextCor(ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_leave_legacy_hint_choose_share, ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueBaseReadSpeed);
		yield return new WaitUntil(() => toShare.HasValue);
		if (toShare == true && UIBase.gameState.applicationState.forerunnerManager.shareForerunnerLegacy != null)
		{
			yield return ChooseReplaceShareCor();
		}
	}

	private IEnumerator ChooseReplaceShareCor()
	{
		giveButton.gameObject.SetActive(value: false);
		noGiveButton.gameObject.SetActive(value: false);
		shareButton.gameObject.SetActive(value: false);
		noShareButton.gameObject.SetActive(value: false);
		replaceShareButton.gameObject.SetActive(value: true);
		noReplaceShareButton.gameObject.SetActive(value: true);
		toReplaceForerunnerLegacyCell.SetActiveAndDirty(active: true);
		toReplaceForerunnerLegacyCell.Setup(UIBase.gameState.applicationState.forerunnerManager.shareForerunnerLegacy);
		yield return hintText.ScrollTextCor(ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_leave_legacy_hint_choose_replace_share, ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueBaseReadSpeed);
		yield return new WaitUntil(() => toReplaceShare.HasValue);
		if (toReplaceShare == true)
		{
			toShare = true;
		}
		else
		{
			toShare = false;
		}
		toReplaceForerunnerLegacyCell.SetActiveAndDirty(active: false);
	}

	public void Setup(ForerunnerLegacy t)
	{
		ForerunnerLegacy = t;
		hintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_leave_legacy_hint_1;
		giveButton.gameObject.SetActive(value: true);
		noGiveButton.gameObject.SetActive(value: true);
		shareButton.gameObject.SetActive(value: false);
		noShareButton.gameObject.SetActive(value: false);
		replaceShareButton.gameObject.SetActive(value: false);
		noReplaceShareButton.gameObject.SetActive(value: false);
		toReplaceForerunnerLegacyCell.SetActiveAndDirty(active: false);
		toShare = null;
		toReplaceShare = null;
	}

	private void _OnGiveButtonClicked()
	{
		parentPanel.forerunnerSelectPanel.Setup(ForerunnerLegacy);
		parentPanel.forerunnerSelectPanel.SetActiveAndDirty(active: true);
	}

	private void _OnNoGiveButtonClicked()
	{
		ForerunnerLegacy = null;
		parentPanel.Finish();
	}

	private void _OnShareButtonClicked()
	{
		toShare = true;
	}

	private void _OnNoShareButtonClicked()
	{
		toShare = false;
	}

	private void _OnReplaceShareButtonClicked()
	{
		toReplaceShare = true;
	}

	private void _OnNoReplaceShareButtonClicked()
	{
		toReplaceShare = false;
	}
}
