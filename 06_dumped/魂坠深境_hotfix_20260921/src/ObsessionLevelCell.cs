using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObsessionLevelCell : CellUI<CharacterObsessionLevelConfig>, IDisplayModulesController
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI levelTitleText;

	[SerializeField]
	private TextMeshProUGUI obsessionLevelText;

	[SerializeField]
	private Image baseImage;

	[SerializeField]
	private Image lockMask;

	[SerializeField]
	private Image chip_UnlockMark;

	[SerializeField]
	private Image selectedMark_Point;

	[SerializeField]
	public Transform levelPoint;

	[NonSerialized]
	private CharacterObsessionPanel characterObsessionPanel;

	private CharacterObsessionLevelConfig levelConfig => base.setupData;

	private PlayerCharacterState character => characterObsessionPanel?.character;

	private bool isPreview => character?.isPreview ?? false;

	protected override void Init()
	{
		characterObsessionPanel = GetComponentInParent<CharacterObsessionPanel>(includeInactive: true);
		baseImage.alphaHitTestMinimumThreshold = 0.1f;
	}

	protected override void UpdateDisplay()
	{
		icon.sprite = levelConfig.icon;
		levelTitleText.text = levelConfig.title;
		obsessionLevelText.text = $"{character.ObsessionLevelNum(levelConfig)}";
		lockMask.gameObject.SetActive(value: false);
		chip_UnlockMark.gameObject.SetActive(value: false);
		if (character.IsUnlocked(levelConfig))
		{
			chip_UnlockMark.gameObject.SetActive(value: true);
		}
		else if (!character.CanUnlock(levelConfig))
		{
			lockMask.gameObject.SetActive(value: true);
		}
	}
}
