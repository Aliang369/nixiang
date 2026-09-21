using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterObsessionPanel : CellUI<PlayerCharacterState>, ISelectablePanel, IStackPanel
{
	[SerializeField]
	private ObsessionLevelCell[] cells;

	[SerializeField]
	private ObsessionDetailInfoPanel obsessionDetailInfoPanel;

	[SerializeField]
	private Image characterHalfBodyImage;

	[SerializeField]
	public UpgradeConfigPanel upgradeConfigPanel;

	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	private GameButton backButton;

	[NonSerialized]
	private ObsessionLevelCell selected;

	[NonSerialized]
	private NFXPlayState state;

	public PlayerCharacterState character => base.setupData;

	protected override void Init()
	{
		InitUI(obsessionDetailInfoPanel);
		InitUI(upgradeConfigPanel);
		ObsessionLevelCell[] array = cells;
		foreach (ObsessionLevelCell child in array)
		{
			InitUI(child);
		}
		selected = null;
		backButton.onClick.AddListener(base.Unset);
		helpButton.onClick.AddListener(delegate
		{
			CellUI<PlayerCharacterState>.fortressState.uiManager.fortressGuidePanel.characterObsessionGuideHint.Show();
		});
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		SetActiveAndDirtyWithFadeAnim(active: true);
		CellUI<PlayerCharacterState>.fortressState.uiManager.fortressGuidePanel.characterObsessionGuideHint.TryFirstOpenAutoPopup();
		characterHalfBodyImage.sprite = character.def.fullBody;
		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Setup(character.def.obsessionLevelDefs[i]);
		}
		if (character.obsessionLevel >= 1)
		{
			selected = cells[character.obsessionLevel - 1];
		}
		else
		{
			selected = cells[0];
		}
		UpdateDisplay();
	}

	protected override void OnUnset()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
		CellUI<PlayerCharacterState>.fortressState.uiManager.SetDirty();
		if (state != null)
		{
			state.Stop();
			state = null;
		}
	}

	protected override void UpdateDisplay()
	{
		if (selected == null)
		{
			obsessionDetailInfoPanel.Unset();
		}
		else
		{
			obsessionDetailInfoPanel.Setup(selected.setupData);
		}
	}

	public void PlayUpgradeVFX(int obsessionLevel)
	{
		ObsessionLevelCell obsessionLevelCell = cells[obsessionLevel - 1];
		if (!(obsessionLevelCell == null))
		{
			state = CellUI<PlayerCharacterState>.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.upgradeObsessionLevelVFX, CellUI<PlayerCharacterState>.fortressState.uiManager.vfxParent, obsessionLevelCell.levelPoint.GetWorldPosition());
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
		selected = module.parent as ObsessionLevelCell;
		SetDirty();
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		return selected == module.parent as ObsessionLevelCell;
	}
}
