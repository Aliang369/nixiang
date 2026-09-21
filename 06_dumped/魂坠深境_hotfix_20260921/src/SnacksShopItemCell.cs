using System;
using TMPro;
using UnityEngine;

public class SnacksShopItemCell : CellUI<SnacksShopItemCell.Config>
{
	public enum Type
	{
		Heal,
		MaxHealth
	}

	public class Config
	{
		public int cost;

		public Type type;
	}

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private GameButton confirmButton;

	[NonSerialized]
	private Config config;

	[NonSerialized]
	private GoldStoreEventPanel panel;

	public static Config HealConfig => new Config
	{
		type = Type.Heal,
		cost = 6
	};

	public static Config MaxHealthConfig => new Config
	{
		type = Type.MaxHealth,
		cost = 6
	};

	protected override void Init()
	{
		panel = parent as GoldStoreEventPanel;
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
	}

	protected override void OnSetup(Config dailyContract)
	{
		config = dailyContract;
		switch (dailyContract.type)
		{
		case Type.Heal:
			text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_heal_health_arg0.FormatWith(10);
			break;
		case Type.MaxHealth:
			text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_heal_health_arg0.FormatWith(6);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		costText.text = $"{ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText()}{dailyContract.cost}";
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		confirmButton.interactable = CellUI<Config>.gameState.playerState.HasEnoughCoin(config.cost);
	}

	private void _OnConfirmButtonClicked()
	{
		switch (config.type)
		{
		case Type.Heal:
			CellUI<Config>.gameState.actionManager.EnqueueAction(new ChangeBftHealthAction(CellUI<Config>.gameState.playerState.bft, 10));
			CellUI<Config>.gameState.actionManager.EnqueueAction(new ChangeCoinAction(-config.cost));
			break;
		case Type.MaxHealth:
			CellUI<Config>.gameState.actionManager.EnqueueAction(new ChangeBftMaxHealthAction(CellUI<Config>.gameState.playerState.bft, 6, baseAdd: true));
			CellUI<Config>.gameState.actionManager.EnqueueAction(new ChangeCoinAction(-config.cost));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		panel.SetDirty();
	}
}
