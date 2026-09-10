using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterMasterySelectCell : CellUI<Tuple<CharacterMastery, PlayerCharacterState>>, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Image masteryIcon;

	[SerializeField]
	private Image background;

	[SerializeField]
	private RectTransform lockedArea;

	[SerializeField]
	private Color unselectedTextColor;

	[SerializeField]
	private Color selectedTextColor;

	[SerializeField]
	private Color unselectedIconColor;

	[SerializeField]
	private Color selectedIconColor;

	[SerializeField]
	private float selectedSize = 1.1f;

	[NonSerialized]
	private CharacterInfoPanel characterInfoPanel;

	public Tuple<CharacterMastery, PlayerCharacterState> info => base.setupData;

	private PlayerCharacterState characterState => info.Item2;

	public bool IsUnlocked
	{
		get
		{
			if (!characterState.isPreview)
			{
				return characterState.IsMasteryUnlocked(info.Item1);
			}
			return false;
		}
	}

	private bool IsSelecting => characterState.currentMastery == info.Item1;

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
		characterInfoPanel.SelectMastery(info.Item1);
	}

	protected override void Init()
	{
		characterInfoPanel = GetUIInParents<CharacterInfoPanel>();
	}

	protected override void OnSetup(Tuple<CharacterMastery, PlayerCharacterState> _)
	{
		masteryIcon.sprite = info.Item1.icon;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		nameText.text = info.Item1.masteryName;
		if (IsUnlocked)
		{
			lockedArea.gameObject.SetActive(value: false);
		}
		else
		{
			lockedArea.gameObject.SetActive(value: true);
		}
		if (IsSelecting)
		{
			base.transform.localScale = Vector3.one * selectedSize;
			nameText.color = selectedTextColor;
			masteryIcon.color = selectedIconColor;
			background.color = selectedIconColor;
		}
		else
		{
			base.transform.localScale = Vector3.one;
			nameText.color = unselectedTextColor;
			masteryIcon.color = unselectedIconColor;
			background.color = unselectedIconColor;
		}
	}
}
