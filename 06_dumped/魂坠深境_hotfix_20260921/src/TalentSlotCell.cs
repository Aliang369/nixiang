using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using ContainerOfSoul.Display.Fortress.DotManagers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TalentSlotCell : UIBase, IPointerClickHandler, IEventSystemHandler
{
	private enum TalentSlotState
	{
		None,
		Unlocked,
		Unlockable,
		NotUnlockable
	}

	public Image talentIconImage;

	public Image darkenImage;

	public TalentDef talent;

	public int unlockCost;

	public int classMatterCost;

	public List<TalentSlotCell> connectTalents;

	public Vector2Int treePos;

	public Color needUnlockDarkenColor;

	public Color unreachableDarkenColor;

	public HintRedDot redDot;

	[NonSerialized]
	private TalentSlotState talentSlotState;

	private TalentTree talentTreeParent => GetUIInParents<TalentTree>();

	private CharacterTalentUnlockPanel characterTalentUnlockPanelParent => GetUIInParents<CharacterTalentUnlockPanel>();

	public List<MatterInfo> UnlockCostMatters(PlayerCharacterDef character)
	{
		List<MatterInfo> list = new List<MatterInfo>();
		list.Add(new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.commonCurrencyMatter, unlockCost));
		if (character != null)
		{
			list.Add(new MatterInfo(character.attachClass.classMatter, classMatterCost));
		}
		return list;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (characterTalentUnlockPanelParent != null)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			characterTalentUnlockPanelParent.SetFocusSlotCell(this);
		}
	}

	protected override void Init()
	{
		InitUI(redDot);
		redDot.Setup(new CharacterInfoPanel_TalentCellModule
		{
			playerCharacterState = characterTalentUnlockPanelParent.character,
			cell = this
		});
		talentSlotState = TalentSlotState.None;
	}

	protected override void UpdateDisplay()
	{
		UpdateTalentCellDisplay();
		UpdateLockStateDisplay();
	}

	private void UpdateTalentCellDisplay()
	{
		if (!(talent == null))
		{
			PlayerCharacterState character = characterTalentUnlockPanelParent?.character;
			talentIconImage.sprite = talent.GetDisplayIcon(UIBase.applicationState, character);
		}
	}

	private void UpdateLockStateDisplay()
	{
		PlayerCharacterState playerCharacterState = characterTalentUnlockPanelParent?.character;
		TalentSlotState talentSlotState = this.talentSlotState;
		if (playerCharacterState == null || IsUnlocked(playerCharacterState))
		{
			this.talentSlotState = TalentSlotState.Unlocked;
			darkenImage.gameObject.SetActive(value: false);
		}
		else
		{
			darkenImage.gameObject.SetActive(value: true);
			if (IsUnlockAble(playerCharacterState))
			{
				this.talentSlotState = TalentSlotState.Unlockable;
				darkenImage.color = needUnlockDarkenColor;
			}
			else
			{
				this.talentSlotState = TalentSlotState.NotUnlockable;
				darkenImage.color = unreachableDarkenColor;
			}
		}
		switch (talentSlotState)
		{
		case TalentSlotState.NotUnlockable:
			if (this.talentSlotState == TalentSlotState.Unlockable)
			{
				UIBase.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_talent_to_unlockable, base.transform);
			}
			break;
		case TalentSlotState.None:
			return;
		}
		if (talentSlotState == TalentSlotState.Unlockable && this.talentSlotState == TalentSlotState.Unlocked)
		{
			UIBase.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_talentUnlock, base.transform);
		}
	}

	public void Unlock(ApplicationStateInfo applicationState, PlayerCharacterState character)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		UnlockTalentRequest request = new UnlockTalentRequest(character.def, talent);
		GeneralGameAsyncOperation<UnlockTalentRequest, UnlockTalentResult> op = new GeneralGameAsyncOperation<UnlockTalentRequest, UnlockTalentResult>(request, applicationState.ChillyRoomSDK.Character.UnlockTalentAsync);
		op.OnComplete(delegate
		{
			character.UnlockTalent(talent, applicationState);
			foreach (ChillyRoom.Services.ContainerOfSoul.MatterInfo matterCost in op.OpResult.MatterCosts)
			{
				MatterInfo matterInfo = matterCost.ToClient(new NetworkConvertContext(applicationState));
				applicationState.matterManager.ConsumeMatter(matterInfo, "解锁天赋");
			}
			AfterUnlock();
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_breaking_through_skill);
		void AfterUnlock()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_talents_upgrade);
			ThinkingAnalyticsExtends.TrackUnlockTalent(applicationState, talent, character);
			applicationState.savingManager.SetProfileDirtyToSave();
			characterTalentUnlockPanelParent.SetDirty();
			UIBase.fortressState.uiManager.SetDirty();
		}
	}

	public bool IsUnlocked(PlayerCharacterState character)
	{
		return character?.unlockedTalents.Contains(talent) ?? false;
	}

	public bool IsUnlockAble(PlayerCharacterState character)
	{
		TalentTree talentTree = talentTreeParent;
		if (talentTree == null || IsUnlocked(character))
		{
			return false;
		}
		TalentSlotCell[] source = talentTree.TalentSlotCells.FindAll((TalentSlotCell t) => t.connectTalents.Contains(this));
		if (source.IsNullOrEmpty())
		{
			return true;
		}
		if (source.Any((TalentSlotCell t) => t.IsUnlocked(character)))
		{
			return true;
		}
		return false;
	}
}
