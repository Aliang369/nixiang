using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MintageRunePanel_Event : MintageRunePanel
{
	public class AiMintageCandidate
	{
		public int Index;

		public Item Item;

		public MintageStateReason Reason;

		public bool ForceMintage;
	}

	[SerializeField]
	private GameButton backButton;

	[NonSerialized]
	public Item attachItem;

	[NonSerialized]
	public List<Rune> attachRunes;

	[NonSerialized]
	public bool forceMintaged;

	[NonSerialized]
	public bool mintaged;

	[NonSerialized]
	private MintageRuneRoomEventPanel parentPanel;

	[NonSerialized]
	public bool rerolled;

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	private bool EnableReRollRune => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableRerollRune);

	private bool EnableForceMintageRune => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableForceMintageRune);

	protected int SwitchRunePrice
	{
		get
		{
			if (UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableRerollRuneNoCost))
			{
				return 0;
			}
			int variableValue = UIBase.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.MintageRuneFixedPrice);
			if (variableValue <= 0)
			{
				return parentPanel.config.switchRunePrice;
			}
			return variableValue;
		}
	}

	protected override void Init()
	{
		base.Init();
		parentPanel = GetUIInParents<MintageRuneRoomEventPanel>();
		backButton.onClick.AddListener(_OnBackButtonClicked);
		switchRuneButton.onClick.AddListener(_OnSwitchRuneButtonClicked);
	}

	public void Reset()
	{
		GenerateRunesForMintage();
		selected = null;
		itemArea.Setup(UIBase.gameState.playerState.DeckContainer);
		mintaged = false;
		rerolled = false;
		hintPanel.gameObject.SetActive(value: false);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		if (!EnableReRollRune || (rerolled && !parentPanel.EndlessRuneRegenerate))
		{
			switchRuneButton.gameObject.SetActive(value: false);
			return;
		}
		switchRuneButton.gameObject.SetActive(value: true);
		switchRuneButton.interactable = CanSwitchRune();
		switchRuneCostText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_coin_arg0_count.FormatWith(SwitchRunePrice.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.coinColor));
	}

	public void Unset()
	{
		itemArea.Unset();
		rerolled = false;
	}

	public List<AiMintageCandidate> GetAiMintageCandidates()
	{
		List<AiMintageCandidate> list = new List<AiMintageCandidate>();
		if (!parentPanel.IsAiMintagePanelActive || mintaged)
		{
			return list;
		}
		foreach (Item item in UIBase.gameState.playerState.DeckContainer.GetAllItems().OrEmptyIfNull())
		{
			if (item != null && CanMintage(item, out var reason))
			{
				list.Add(new AiMintageCandidate
				{
					Index = list.Count,
					Item = item,
					Reason = reason,
					ForceMintage = IsForceMintage(item)
				});
			}
		}
		return list;
	}

	public bool TryMintageForAiSession(AiMintageCandidate candidate)
	{
		if (candidate?.Item == null || !parentPanel.IsAiMintagePanelActive || mintaged)
		{
			return false;
		}
		if (!CanMintage(candidate.Item, out var _))
		{
			return false;
		}
		selected = null;
		hintPanel.gameObject.SetActive(value: false);
		MintageRune(candidate.Item, GetRunes());
		SetDirty();
		return true;
	}

	public void CloseForAiSession()
	{
		if (parentPanel.IsAiMintagePanelActive)
		{
			selected = null;
			hintPanel.gameObject.SetActive(value: false);
			ClosePanel();
		}
	}

	protected override void MintageRune(Item item, List<Rune> runes)
	{
		if (IsForceMintage(item))
		{
			forceMintaged = true;
			UIBase.gameState.actionManager.EnqueueAction(new ChangeCrystalAction(-GetForceMintageRunePrice(item)));
		}
		attachItem = item;
		base.MintageRune(item, runes);
		ThinkingAnalyticsExtends.TrackBonusRune(UIBase.gameState, runes.First(), item, this);
		parentPanel.mintageEvent.HideMintageEffectObject();
		mintaged = true;
	}

	private void RerollRune()
	{
		UIBase.gameState.actionManager.EnqueueAction(new ChangeCoinAction(-SwitchRunePrice));
		rerolled = true;
		GenerateRunesForMintage();
	}

	private void GenerateRunesForMintage()
	{
		List<RuneDef> excludeRunes = attachRunes?.ConvertAll((Rune r) => r.def);
		PathLocation currentLocation = UIBase.gameState.dungeonMapManager.currentLocation;
		bool flag = currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.AttachRuneDoubleRune;
		attachRunes = (flag ? UIBase.gameState.generationManager.GenerateMintageRunePairForSameItem(excludeRunes) : UIBase.gameState.generationManager.GenerateMintageRunes(1, excludeRunes));
		foreach (Rune attachRune in attachRunes)
		{
			ThinkingAnalyticsExtends.TrackExposeBonusRune(UIBase.gameState, attachRune, this);
		}
	}

	public override List<Rune> GetRunes()
	{
		return attachRunes;
	}

	public bool IsForceMintage(Item item)
	{
		if (!WithEnoughRuneSlot(item) && EnableForceMintageRune)
		{
			return true;
		}
		return false;
	}

	protected override bool CanMintage(Item item, out MintageStateReason reason)
	{
		if (!base.CanMintage(item, out reason))
		{
			return false;
		}
		if (!WithEnoughRuneSlot(item))
		{
			if (EnableForceMintageRune)
			{
				reason = MintageStateReason.ForceMintageWithCrystal;
				return (int)UIBase.gameState.playerState.crystalCount >= GetForceMintageRunePrice(item);
			}
			reason = MintageStateReason.WithoutEnoughRuneSlot;
			return false;
		}
		reason = MintageStateReason.MintageAble;
		return true;
	}

	protected bool CanSwitchRune()
	{
		if (GetRunes().Count > 0 && (!rerolled || parentPanel.EndlessRuneRegenerate))
		{
			return UIBase.gameState.playerState.HasEnoughCoin(SwitchRunePrice);
		}
		return false;
	}

	protected override void ClosePanel()
	{
		UIBase.gameState.UIManager.worldEventPanel_MintageRune.animState = MintageRuneRoomEventPanel.AnimState.Idle;
		UIBase.gameState.UIManager.worldEventPanel_MintageRune.SetDirty();
	}

	protected override void _OnMintageRuneButtonClicked()
	{
		if (selected.HasTag(ScriptableObjectSingleton<GameDataManager>.instance.curseTag))
		{
			hintPanelText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_hint_attach_rune_on_curse_confirm;
			hintPanel.gameObject.SetActive(value: true);
		}
		else if (selected.HasTag(ScriptableObjectSingleton<GameDataManager>.instance.oneOffTag))
		{
			hintPanelText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_hint_attach_rune_on_oneOff;
			hintPanel.gameObject.SetActive(value: true);
		}
		else
		{
			base._OnMintageRuneButtonClicked();
		}
		SetDirty();
	}

	private void _OnSwitchRuneButtonClicked()
	{
		selected = null;
		RerollRune();
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		ClosePanel();
	}
}
