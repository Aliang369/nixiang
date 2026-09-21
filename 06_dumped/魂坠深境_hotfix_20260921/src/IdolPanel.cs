using System;
using System.Collections;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IdolPanel : CellUI<CharacterClassState>, IShowAsync, IHideAsync
{
	[SerializeField]
	private Image classStatuesImage;

	[SerializeField]
	private GameButton worshipButton;

	[SerializeField]
	private TextMeshProUGUI worshipButtonText;

	[SerializeField]
	private Image worshipButtonStateImage;

	[SerializeField]
	private Image soulIcon;

	[SerializeField]
	private Sprite worshipButtonAvailableSprite;

	[SerializeField]
	private Sprite worshipButtonUnavailableSprite;

	[SerializeField]
	private TextMeshProUGUI worshipButtonStateText;

	[SerializeField]
	private TextMeshProUGUI lvText;

	[SerializeField]
	private TextMeshProUGUI gotSoulText;

	[SerializeField]
	private IdolLevelLinePanel idolLevelLinePanel;

	[SerializeField]
	private DifficultyTrialPanel difficultyTrialPanel;

	[SerializeField]
	private SeasonTrialPanel seasonTrialPanel;

	[SerializeField]
	private CharacterTrialPanel characterTrialPanel;

	[SerializeField]
	private Animator sliderAnimator;

	[SerializeField]
	private GameButton sliderSwitchButton;

	[SerializeField]
	private GameObject worshipRedDot;

	[SerializeField]
	private Material worshipButtonRedDotEffectMaterial;

	[SerializeField]
	private GameButton trialHintButton;

	[SerializeField]
	public SimpleHintPanel hintPanel;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private int sliderState;

	[NonSerialized]
	private Coroutine worshipCompleteCoroutine;

	private static readonly Color32 WorshipMaxLevelTextColor = new Color32(219, 217, 185, byte.MaxValue);

	private static readonly Color32 WorshipInsufficientTextColor = new Color32(198, 130, 117, byte.MaxValue);

	private CharacterClassState characterClass => base.setupData;

	private IdolState currentIdol => characterClass.GetIdolState();

	public RectTransform WorshipButtonRect
	{
		get
		{
			if (!(worshipButton != null))
			{
				return null;
			}
			return worshipButton.transform as RectTransform;
		}
	}

	protected override void Init()
	{
		InitUI(idolLevelLinePanel);
		InitUI(difficultyTrialPanel);
		InitUI(seasonTrialPanel);
		InitUI(characterTrialPanel);
		InitUI(hintPanel);
		worshipButton.onClick.AddListener(OnWorshipButtonClick);
		sliderState = 0;
		sliderSwitchButton.onClick.AddListener(delegate
		{
			sliderState = ((sliderState == 0) ? 1 : 0);
			sliderAnimator.SetInteger(AnimKeywords.state, sliderState);
		});
		trialHintButton.onClick.AddListener(delegate
		{
			hintPanel.Show();
		});
		void OnWorshipButtonClick()
		{
			if (currentIdol != null && !idolLevelLinePanel.isPlayingAnim)
			{
				if (!currentIdol.CanUpgrade(CellUI<CharacterClassState>.applicationState))
				{
					ShowWorshipUnavailableHint();
				}
				else
				{
					currentIdol.Upgrade(CellUI<CharacterClassState>.applicationState, delegate(GlobalRewardPackage pack)
					{
						if (worshipCompleteCoroutine != null)
						{
							StopCoroutine(worshipCompleteCoroutine);
						}
						worshipCompleteCoroutine = StartCoroutine(CompleteWorshipAfterLayoutRefresh(pack));
					});
				}
			}
		}
		void ShowWorshipUnavailableHint()
		{
			if (currentIdol != null)
			{
				string s = (currentIdol.IsMaxLevel() ? CellUI<CharacterClassState>.textData.term_idol_max_level_reached : CellUI<CharacterClassState>.textData.term_matter_insufficient.FormatWith(TextDisplayHelper.SoulText(currentIdol.idolDef.attachClass)));
				CellUI<CharacterClassState>.fortressState.uiManager.quickHintPanel.HintText(s);
			}
		}
	}

	protected override void Dispose()
	{
		if (worshipCompleteCoroutine != null)
		{
			StopCoroutine(worshipCompleteCoroutine);
			worshipCompleteCoroutine = null;
		}
		if (CellUI<CharacterClassState>.applicationState != null)
		{
			CellUI<CharacterClassState>.applicationState.eventManager.matterChangeEvent.RemoveListener(OnMatterChange);
		}
	}

	protected override void UpdateDisplay()
	{
		UpdateWorshipButtonState();
		lvText.text = $"{currentIdol.currentLevel}";
		gotSoulText.text = CellUI<CharacterClassState>.textData.term_got_soul.FormatWith(TextDisplayHelper.SoulText(currentIdol.idolDef.attachClass), GetAllTrialSoulCount());
	}

	private int GetAllTrialSoulCount()
	{
		return GetDifficultyTrialSoulCount() + GetSeasonTrialSoulCount() + GetCharacterTrialSoulCount();
	}

	private int GetDifficultyTrialSoulCount()
	{
		return ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.GetDifficultySoulRewardCount(currentIdol.currentDifficultyLevel, CellUI<CharacterClassState>.applicationState.assetCatalog);
	}

	private int GetSeasonTrialSoulCount()
	{
		return currentIdol.seasonSoul;
	}

	private int GetCharacterTrialSoulCount()
	{
		return CellUI<CharacterClassState>.applicationState.characterManager.characterStates.Where((PlayerCharacterState c) => c.def.attachClass.mClass == currentIdol.idolDef.attachClass).Sum((PlayerCharacterState c) => currentIdol.CharacterSoulCount(c.def));
	}

	protected override void OnSetup(CharacterClassState setupData)
	{
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<CharacterClassState>.applicationState, "神像间-魂祭界面", currentIdol.GetCharacterClassDef().FullID);
		base.gameObject.SetActive(value: true);
		idolLevelLinePanel.Setup(currentIdol);
		difficultyTrialPanel.Setup(currentIdol);
		seasonTrialPanel.Setup(currentIdol);
		characterTrialPanel.Setup(currentIdol);
		if (classStatuesImage != null && currentIdol.idolDef.classStatueSprite != null)
		{
			classStatuesImage.sprite = currentIdol.idolDef.classStatueSprite;
		}
		SetDirty();
	}

	public IEnumerator ShowAnim()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		FortressGuidePanel fortressGuidePanel = CellUI<CharacterClassState>.fortressState.uiManager.fortressGuidePanel;
		if (fortressGuidePanel == null || !fortressGuidePanel.IsPlayingIdolWorshipGuide)
		{
			fortressGuidePanel?.idolGuideHint.TryFirstOpenAutoPopup();
		}
	}

	public IEnumerator HideAnim()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
	}

	private void UpdateWorshipButtonState()
	{
		if (currentIdol == null)
		{
			UpdateSoulIcon(show: false);
			return;
		}
		bool flag = currentIdol.CanUpgrade(CellUI<CharacterClassState>.applicationState);
		bool flag2 = currentIdol.IsMaxLevel();
		string text = CellUI<CharacterClassState>.textData.term_current_soul.FormatWith(TextDisplayHelper.SoulText(currentIdol.idolDef.attachClass), currentIdol.soul.count);
		UpdateSoulIcon(flag);
		if (worshipButton == null)
		{
			return;
		}
		if (flag2)
		{
			worshipButton.gameObject.SetActive(value: true);
			worshipButtonText.text = text;
			worshipButton.interactable = false;
			UpdateWorshipButtonVisual(canUpgrade: false, CellUI<CharacterClassState>.textData.term_idol_max_level_reached, WorshipMaxLevelTextColor);
			UpdateWorshipRedDot(show: false);
			return;
		}
		worshipButton.gameObject.SetActive(value: true);
		if (flag)
		{
			worshipButton.interactable = true;
			worshipButtonText.text = text;
			UpdateWorshipButtonVisual(canUpgrade: true, CellUI<CharacterClassState>.textData.term_worship_button, Color.white);
			UpdateWorshipRedDot(show: true);
		}
		else
		{
			worshipButton.interactable = false;
			worshipButtonText.text = text;
			UpdateWorshipButtonVisual(canUpgrade: false, CellUI<CharacterClassState>.textData.term_soul_matter_insufficient, WorshipInsufficientTextColor);
			UpdateWorshipRedDot(show: false);
		}
	}

	private void UpdateWorshipButtonVisual(bool canUpgrade, string stateText, Color textColor)
	{
		if (worshipButtonStateImage != null)
		{
			Sprite sprite = (canUpgrade ? worshipButtonAvailableSprite : worshipButtonUnavailableSprite);
			if (sprite != null)
			{
				worshipButtonStateImage.sprite = sprite;
			}
		}
		if (!(worshipButtonStateText == null))
		{
			worshipButtonStateText.text = stateText;
			worshipButtonStateText.enableVertexGradient = canUpgrade;
			worshipButtonStateText.color = textColor;
		}
	}

	private void UpdateSoulIcon(bool show)
	{
		Image image = GetSoulIcon();
		if (!(image == null))
		{
			Sprite sprite = (image.sprite = currentIdol?.GetCharacterClassDef()?.soulMatter?.icon);
			image.gameObject.SetActive(show && sprite != null);
		}
	}

	private Image GetSoulIcon()
	{
		if (soulIcon != null)
		{
			return soulIcon;
		}
		soulIcon = GetComponentsInChildren<Image>(includeInactive: true).FirstOrDefault((Image i) => i != null && i.name == "soulIcon");
		return soulIcon;
	}

	private void UpdateWorshipRedDot(bool show)
	{
		if (worshipRedDot != null)
		{
			worshipRedDot.SetActive(show);
		}
		UpdateWorshipButtonEffect(show);
	}

	private void UpdateWorshipButtonEffect(bool show)
	{
		if (!(worshipButtonStateImage == null))
		{
			worshipButtonStateImage.material = (show ? worshipButtonRedDotEffectMaterial : null);
		}
	}

	private void OnMatterChange(MatterInfo matterInfo)
	{
		SetDirty();
	}

	private IEnumerator CompleteWorshipAfterLayoutRefresh(GlobalRewardPackage pack)
	{
		if (CellUI<CharacterClassState>.fortressState != null)
		{
			CellUI<CharacterClassState>.fortressState.StartCoroutine(() => CellUI<CharacterClassState>.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(pack));
		}
		yield return new WaitForSeconds(0.1f);
		SetDirty();
		idolLevelLinePanel.SetDirty();
		yield return null;
		Canvas.ForceUpdateCanvases();
		if (base.gameObject.activeInHierarchy)
		{
			idolLevelLinePanel.SnapToUpgradeEndInstant(currentIdol.currentLevel + 1);
		}
		worshipCompleteCoroutine = null;
	}
}
