using TMPro;

public class ItemSkinSelectButton : CellUI<(Item, ItemSkinDef)>
{
	public GameButton selectedButton;

	public TextMeshProUGUI selectedButtonText;

	public GameButton notSelectedButton;

	public TextMeshProUGUI notSelectedButtonText;

	public GameButton unlockButton;

	public TextMeshProUGUI unlockText;

	private (Item, ItemSkinDef) data;

	private Item DisplayItem => data.Item1;

	private ItemSkinDef DisplaySkin => data.Item2;

	private bool IsBaseSkin => DisplaySkin == null;

	protected override void Init()
	{
		notSelectedButton.onClick.AddListener(_OnNotSelectedButtonClicked);
		unlockButton.onClick.AddListener(_OnUnlockButtonClicked);
	}

	protected override void OnSetup((Item, ItemSkinDef) t)
	{
		data = t;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		bool num = DisplaySkin == null || CellUI<(Item, ItemSkinDef)>.applicationState.itemSkinManager.IsItemSkinUnlocked(DisplaySkin);
		bool flag = false;
		if (num)
		{
			unlockButton.gameObject.SetActive(value: false);
			if (flag)
			{
				selectedButton.gameObject.SetActive(value: true);
				notSelectedButton.gameObject.SetActive(value: false);
			}
			else
			{
				selectedButton.gameObject.SetActive(value: false);
				notSelectedButton.gameObject.SetActive(value: true);
			}
		}
		else
		{
			selectedButton.gameObject.SetActive(value: false);
			notSelectedButton.gameObject.SetActive(value: false);
			unlockButton.gameObject.SetActive(value: true);
		}
		if (IsBaseSkin)
		{
			notSelectedButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_default_value;
			selectedButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_default_value;
			unlockText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_default_value;
		}
		else
		{
			notSelectedButtonText.text = DisplaySkin.itemSkinID;
			selectedButtonText.text = DisplaySkin.itemSkinID;
			unlockText.text = DisplaySkin.itemSkinID;
		}
	}

	private void _OnNotSelectedButtonClicked()
	{
	}

	private void _OnUnlockButtonClicked()
	{
	}
}
