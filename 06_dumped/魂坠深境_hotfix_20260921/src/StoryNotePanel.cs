using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryNotePanel : UIBase
{
	public TextMeshProUGUI contentText;

	public TextMeshProUGUI signatureText;

	public ItemDisplayBase itemDisplay;

	public SkillDisplay skillDisplay;

	public ClavisDisplay clavisDisplay;

	public CanvasGroup fadeGroup;

	public Image dissolveBaseImage;

	public SkillInfoDisplay skillInfoDisplay;

	public ItemInfoDisplayBase itemInfoDisplay;

	public SwitchPanel voteSwitchPanel;

	public SwitchTag upvoteSwitchTag;

	public SwitchTag downvoteSwitchTag;

	public GameObject upvoteRecordHint;

	public GameObject downvoteRecordHint;

	public GameButton confirmButton;

	public GameButton backButton;

	public GameButton reportButton;

	public GameButton emptyButton;

	public Sprite archiveIconSprite;

	public ForerunnerLegacyReportView reportViewPrefab;

	public float fadeTime = 0.3f;

	private GameButton currentVoteButton;

	private StoryNoteInfo noteInfo;

	private StoryNoteFeedback feedback;

	private bool hiding;

	private ForerunnerLegacyReportView reportView;

	private bool reportDebugSimulation;

	private const string DissolveFactor = "_DissolveFactor";

	private readonly List<GameObject> letterContentHiddenSnapshot = new List<GameObject>();

	private bool letterContentHidden;

	protected override void Init()
	{
		base.Init();
		InitUI(itemDisplay);
		InitUI(skillDisplay);
		InitUI(clavisDisplay);
		InitUI(skillInfoDisplay);
		InitUI(itemInfoDisplay);
		InitUI(voteSwitchPanel);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		emptyButton.onClick.AddListener(_OnEmptyButtonClicked);
		voteSwitchPanel.onTagSwitched = _OnVoteTagSwitched;
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		backButton.onClick.AddListener(_OnDeclineButtonClicked);
		reportButton.onClick.AddListener(_OnClickReportButton);
		reportView = UnityEngine.Object.Instantiate(reportViewPrefab, dissolveBaseImage.transform);
		reportView.Setup(this);
		InitUI(reportView);
		skillDisplay.GetComponent<GameButton>().onClick.AddListener(delegate
		{
			skillInfoDisplay.gameObject.SetActive(value: true);
			skillInfoDisplay.Setup(skillDisplay.setupData);
		});
		InputModule module = itemDisplay.GetModule<InputModule>();
		module.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(module.onPointerClick, (Action<InputModuleBase>)delegate
		{
			itemInfoDisplay.gameObject.SetActive(value: true);
			itemInfoDisplay.Setup(itemDisplay.item);
		});
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
		reportView?.Reset();
	}

	public void Show(StoryNoteInfo note)
	{
		letterContentHidden = false;
		letterContentHiddenSnapshot.Clear();
		base.gameObject.SetActive(value: true);
		fadeGroup.alpha = 1f;
		dissolveBaseImage.materialForRendering.DisableKeyword("ENABLE_DISSOLVE");
		dissolveBaseImage.materialForRendering.SetFloat("_DissolveFactor", 1f);
		contentText.text = note.content;
		signatureText.text = GetSignatureDisplayText(note);
		contentText.gameObject.SetActive(value: true);
		signatureText.gameObject.SetActive(value: true);
		if (note.item != null)
		{
			itemDisplay.SetActiveAndDirty(active: true);
			itemDisplay.Setup(note.item);
		}
		else
		{
			itemDisplay.SetActiveAndDirty(active: false);
		}
		if (note.skill != null)
		{
			skillDisplay.SetActiveAndDirty(active: true);
			skillDisplay.Setup(note.skill);
		}
		else
		{
			skillDisplay.SetActiveAndDirty(active: false);
		}
		if (note.clavisDef != null)
		{
			clavisDisplay.SetActiveAndDirty(active: true);
			clavisDisplay.Setup(note.clavisDef);
		}
		else
		{
			clavisDisplay.SetActiveAndDirty(active: false);
		}
		if (note.needConfirm)
		{
			confirmButton.gameObject.SetActive(value: true);
			emptyButton.interactable = false;
		}
		else
		{
			confirmButton.gameObject.SetActive(value: false);
			emptyButton.interactable = true;
		}
		if (note.canBack)
		{
			backButton.gameObject.SetActive(value: true);
			emptyButton.interactable = false;
		}
		else
		{
			backButton.gameObject.SetActive(value: false);
			emptyButton.interactable = true;
		}
		if (note.forerunnerLegacy != null)
		{
			if (note.toGetForerunnerLegacy)
			{
				voteSwitchPanel.gameObject.SetActive(value: true);
				upvoteRecordHint.gameObject.SetActive(value: false);
				downvoteRecordHint.gameObject.SetActive(value: false);
			}
			else
			{
				voteSwitchPanel.gameObject.SetActive(value: false);
				upvoteRecordHint.gameObject.SetActive(UIBase.applicationState.forerunnerManager.IsLegacyUpvoteByPlayer(note.forerunnerLegacy));
				downvoteRecordHint.gameObject.SetActive(UIBase.applicationState.forerunnerManager.IsLegacyDownvoteByPlayer(note.forerunnerLegacy));
			}
		}
		else
		{
			voteSwitchPanel.gameObject.SetActive(value: false);
			upvoteRecordHint.gameObject.SetActive(value: false);
			downvoteRecordHint.gameObject.SetActive(value: false);
		}
		reportView.Bind(note);
		if (reportDebugSimulation)
		{
			reportView.EnableDebugSimulation();
		}
		reportButton.gameObject.SetActive(reportDebugSimulation || CanShowReportEntry(note));
	}

	public void EnableReportDebugSimulation()
	{
		reportDebugSimulation = true;
		reportView.EnableDebugSimulation();
		if (noteInfo?.forerunnerLegacy != null)
		{
			reportButton.gameObject.SetActive(value: true);
		}
	}

	private static bool CanShowReportEntry(StoryNoteInfo note)
	{
		ForerunnerLegacy forerunnerLegacy = note.forerunnerLegacy;
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return false;
		}
		long playerId = Bootstrap.applicationState.ChillyRoomSDK.Accounts.StateManager.State.User.PlayerId;
		if (forerunnerLegacy != null && !forerunnerLegacy.storyOnly && forerunnerLegacy.sourcePlayerId > 0 && forerunnerLegacy.sourcePlayerId != playerId)
		{
			return !string.IsNullOrEmpty(forerunnerLegacy.guid);
		}
		return false;
	}

	public static string GetSignatureDisplayText(StoryNoteInfo note)
	{
		long num = note.forerunnerLegacy?.sourcePlayerId ?? 0;
		if (num <= 0)
		{
			return note.signature;
		}
		string text = $"<size=60%>PID{num}</size>";
		if (!string.IsNullOrEmpty(note.signature))
		{
			return note.signature + "\n" + text;
		}
		return text;
	}

	public void Hide()
	{
		reportView?.Reset();
		hiding = true;
		StartCoroutine(HideCor());
	}

	private IEnumerator HideCor()
	{
		if (feedback.agree)
		{
			DOTweenModuleUI.DOFade(fadeGroup, 0f, fadeTime);
			dissolveBaseImage.materialForRendering.EnableKeyword("ENABLE_DISSOLVE");
			dissolveBaseImage.materialForRendering.DOFloat(0f, "_DissolveFactor", fadeTime);
			yield return new WaitForSeconds(fadeTime);
			if (noteInfo.gatherGet)
			{
				if (UIBase.gameState != null)
				{
					MultiInfoDisplayPanel.SetupInfo setupInfo = new MultiInfoDisplayPanel.SetupInfo();
					if (noteInfo.item != null)
					{
						setupInfo.items = new List<Item> { noteInfo.item };
					}
					if (noteInfo.skill != null)
					{
						setupInfo.skills = new List<GameSkill> { noteInfo.skill };
					}
					UIBase.gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(setupInfo));
				}
				if (noteInfo.clavisDef != null && UIBase.fortressState != null)
				{
					UIBase.fortressState.uiManager.clavisHint.EnqueueShowClavisHint(noteInfo.clavisDef, ClavisHint.HintType.Unlock);
				}
			}
		}
		hiding = false;
		base.gameObject.SetActive(value: false);
		noteInfo.afterStoryNoteFinish?.Invoke(feedback);
	}

	public void Setup(StoryNoteInfo t)
	{
		noteInfo = t;
		reportDebugSimulation = false;
		feedback = new StoryNoteFeedback
		{
			storyNoteInfo = noteInfo
		};
		Show(t);
		SetDirty();
	}

	public void CloseReportView()
	{
		reportView.Close();
	}

	public void SetLetterContentVisible(bool visible)
	{
		if (!visible)
		{
			if (letterContentHidden)
			{
				return;
			}
			letterContentHidden = true;
			letterContentHiddenSnapshot.Clear();
			{
				foreach (GameObject item in EnumerateLetterContent())
				{
					if (item != null && item.activeSelf)
					{
						letterContentHiddenSnapshot.Add(item);
						item.SetActive(value: false);
					}
				}
				return;
			}
		}
		if (!letterContentHidden)
		{
			return;
		}
		letterContentHidden = false;
		foreach (GameObject item2 in letterContentHiddenSnapshot)
		{
			if (item2 != null)
			{
				item2.SetActive(value: true);
			}
		}
		letterContentHiddenSnapshot.Clear();
	}

	private IEnumerable<GameObject> EnumerateLetterContent()
	{
		yield return contentText.gameObject;
		yield return signatureText.gameObject;
		yield return itemDisplay.gameObject;
		yield return skillDisplay.gameObject;
		yield return clavisDisplay.gameObject;
		yield return voteSwitchPanel.gameObject;
		yield return upvoteRecordHint.gameObject;
		yield return downvoteRecordHint.gameObject;
		yield return confirmButton.gameObject;
		yield return backButton.gameObject;
		yield return reportButton.gameObject;
	}

	private void _OnEmptyAreaClicked()
	{
		itemInfoDisplay.SetActiveAndDirty(active: false);
		skillInfoDisplay.SetActiveAndDirty(active: false);
	}

	private void _OnEmptyButtonClicked()
	{
		feedback.agree = true;
		Hide();
	}

	private void _OnConfirmButtonClicked()
	{
		feedback.agree = true;
		Hide();
	}

	private void _OnDeclineButtonClicked()
	{
		feedback.agree = false;
		Hide();
	}

	private void _OnClickReportButton()
	{
		if (reportDebugSimulation)
		{
			reportView.EnableDebugSimulation();
		}
		reportView.OpenArchive();
	}

	private void _OnVoteTagSwitched()
	{
		if (voteSwitchPanel.targetShowTag == downvoteSwitchTag)
		{
			feedback.upvote = false;
		}
		else if (voteSwitchPanel.targetShowTag == upvoteSwitchTag)
		{
			feedback.upvote = true;
		}
		else
		{
			feedback.upvote = null;
		}
		SetDirty();
	}

	private void Set(ItemDefinition itemDefinition)
	{
		Show(StoryNoteInfo.GetForerunnerLegacy(new ForerunnerLegacy
		{
			legacyItem = new GameRecord.ItemRecord(new Item(itemDefinition))
		}));
	}

	private void Set(SkillDef skillDef)
	{
		Show(StoryNoteInfo.GetForerunnerLegacy(new ForerunnerLegacy
		{
			legacySkill = new GameRecord.SkillRecord(new GameSkill(new ProfileSkill(skillDef)), activated: true, consumed: false)
		}));
	}
}
