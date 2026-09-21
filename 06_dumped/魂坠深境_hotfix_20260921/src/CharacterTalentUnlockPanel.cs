using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTalentUnlockPanel : CellUI<PlayerCharacterState>, IShowAsync, IHideAsync, IStackPanel
{
	[SerializeField]
	public RectTransform talentTreeParent;

	[SerializeField]
	private ScrollRectExtend scrollRect;

	[SerializeField]
	private UnlockTalentInfoDisplay unlockTalentInfoDisplay;

	[SerializeField]
	private GameButton exitButton;

	[SerializeField]
	private MatterCell currencyMatterCell;

	[SerializeField]
	private MatterCell essenceMatterCell;

	[SerializeField]
	private Image avatar;

	[SerializeField]
	private UnlockLevelInfoDisplay infoDisplay;

	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private WrapperEnumerator currentEnumerator;

	[NonSerialized]
	private TalentTree talentTree;

	[NonSerialized]
	private PlayerCharacterState closingCharacter;

	[NonSerialized]
	public CellUIPrefabPool<UnlockLevelInfoDisplay, TalentTree.LevelLockConfig> infoDisplayPool;

	public PlayerCharacterState character => base.setupData;

	public RectTransform exitButtonRect
	{
		get
		{
			if (!(exitButton != null))
			{
				return null;
			}
			return exitButton.GetComponent<RectTransform>();
		}
	}

	public RectTransform unlockButtonRect => GetComponentInChildren<UnlockTalentInfoModule_CharacterTalentUnlockPanel>(includeInactive: true)?.UnlockButtonRect;

	protected override void Init()
	{
		InitUI(unlockTalentInfoDisplay);
		InitUI(currencyMatterCell);
		InitUI(essenceMatterCell);
		exitButton.onClick.AddListener(_OnExitButtonClicked);
		unlockTalentInfoDisplay.gameObject.SetActive(value: false);
		CellUI<PlayerCharacterState>.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		helpButton.onClick.AddListener(_OnHelpButtonClicked);
		infoDisplay.gameObject.SetActive(value: false);
		GetPrefabPool(ref infoDisplayPool, infoDisplay);
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		closingCharacter = null;
		talentTree = UnityEngine.Object.Instantiate(character.def.talentTree, talentTreeParent);
		InitUI(talentTree);
		talentTree.Setup(character);
		scrollRect.content = talentTree.GetComponent<RectTransform>();
		unlockTalentInfoDisplay.Unset();
		CellUI<PlayerCharacterState>.applicationState.playerInfoManager.showUnlockTalent = false;
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<PlayerCharacterState>.applicationState, "主仪式间-角色详情-天赋", character.Name);
		manager.SetDirty();
	}

	protected override void OnUnset()
	{
		closingCharacter = character;
		if (talentTree != null)
		{
			DisposeUI(talentTree);
			UnityEngine.Object.Destroy(talentTree.gameObject);
			talentTree = null;
		}
		currentEnumerator?.Stop();
		currentEnumerator = null;
		unlockTalentInfoDisplay.Unset();
	}

	protected override void Dispose()
	{
		base.Dispose();
		exitButton.onClick.RemoveListener(_OnExitButtonClicked);
		CellUI<PlayerCharacterState>.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		if (character == null)
		{
			return;
		}
		try
		{
			currencyMatterCell.Setup(CellUI<PlayerCharacterState>.applicationState.matterManager.CommonCurrencyMatter);
			essenceMatterCell.Setup(CellUI<PlayerCharacterState>.applicationState.matterManager.GetMatterInfo(character.def.attachClass.classMatter));
			avatar.sprite = character.Avatar;
		}
		catch (Exception ex)
		{
			Debug.LogError($"CharacterTalentUnlockPanel - {CellUI<PlayerCharacterState>.applicationState.matterManager} - {CellUI<PlayerCharacterState>.applicationState.matterManager} - {character} - \n{ex}");
		}
	}

	void IShowAsync.OnShowStart()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
	}

	void IHideAsync.OnHideStart()
	{
		unlockTalentInfoDisplay.gameObject.SetActive(value: false);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
	}

	public void SetFocusSlotCell(TalentSlotCell cell)
	{
		unlockTalentInfoDisplay.Setup(cell.talent);
		SetDirty();
	}

	public TalentSlotCell GetSlotCell(TalentDef talentDef)
	{
		return talentTree?.TalentSlotCells.Find((TalentSlotCell c) => c.talent == talentDef);
	}

	private void _OnExitButtonClicked()
	{
		this.HideUI();
	}

	private void _OnEmptyAreaClicked()
	{
		unlockTalentInfoDisplay.Unset();
	}

	private void _OnHelpButtonClicked()
	{
		CellUI<PlayerCharacterState>.fortressState.uiManager.fortressGuidePanel.talentGuideHint.Show();
	}

	public void _OnAnimatorShowEnd()
	{
		((Behaviour)(object)animator).enabled = false;
	}

	public void _OnAnimatorHideEnd()
	{
		((Behaviour)(object)animator).enabled = false;
		base.gameObject.SetActive(value: false);
		if (closingCharacter != null)
		{
			CellUI<PlayerCharacterState>.applicationState.eventManager.characterTalentPanelClosedEvent.TriggerEvent(closingCharacter);
		}
		closingCharacter = null;
	}
}
