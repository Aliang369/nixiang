using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTokenConvertPanel : UIBase
{
	public Image characterHeadImage;

	public TextMeshProUGUI currentTokenCountText;

	public CharacterTokenConvertCell cellPrefab;

	public Transform cellParent;

	public GameButton backButton;

	[NonSerialized]
	private CharacterInfoPanel parentPanel;

	[NonSerialized]
	private CellUIPrefabPool<CharacterTokenConvertCell, (SkillDef, PlayerCharacterDef)> cellPool;

	[NonSerialized]
	private PlayerCharacterState character;

	protected override void Init()
	{
		parentPanel = GetUIInParents<CharacterInfoPanel>();
		cellPool = GetPrefabPool<CharacterTokenConvertCell, (SkillDef, PlayerCharacterDef)>(cellPrefab, cellParent);
		backButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
			parentPanel.SetDirty();
		});
	}

	public void Setup(PlayerCharacterState character)
	{
		this.character = character;
	}

	protected override void UpdateDisplay()
	{
		characterHeadImage.sprite = character.def.headIcon;
		currentTokenCountText.text = character.obsessionLevel.ToString();
		cellPool.DespawnAll();
		foreach (SkillDef item in character.def.characterSkill)
		{
			cellPool.Spawn((item, character.def));
		}
	}
}
