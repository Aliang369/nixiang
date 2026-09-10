using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class SmallItemDamagePreviewModule : ItemDisplayBase.DisplayModule, IFrameUpdate
{
	[SerializeField]
	private Image damageIconImage;

	[SerializeField]
	private TextMeshProUGUI damageText;

	[SerializeField]
	private TextMeshProUGUI damageTextShadow;

	public override void Setup()
	{
		SetVisible(visible: false);
	}

	public override void UpdateDisplay()
	{
		RefreshDamagePreview();
	}

	void IFrameUpdate.FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy && base.item != null && (UIDisplayModule.gameState?.UIManager?.playerItemArea?.controlInfo?.displaySpellInfo)?.spellItem == base.item)
		{
			RefreshDamagePreview();
		}
	}

	public override void Unset()
	{
		SetVisible(visible: false);
	}

	private void RefreshDamagePreview()
	{
		if (damageIconImage == null || damageText == null)
		{
			return;
		}
		ItemSpell itemSpell = base.item?.CurrentSpell;
		if (UIDisplayModule.gameState == null || !UIDisplayModule.gameState.battleState.isInBattle || itemSpell?.spellNode == null || itemSpell.IntentType != IntentType.Attack)
		{
			SetVisible(visible: false);
			return;
		}
		NodeDisplayInfo spellNodeInfo = GetSpellNodeInfo(itemSpell);
		int? previewDamageValue = GetPreviewDamageValue(spellNodeInfo);
		if (!previewDamageValue.HasValue)
		{
			SetVisible(visible: false);
			return;
		}
		SetVisible(visible: true);
		int num = spellNodeInfo.times?.value ?? 1;
		string text = ((num != 1) ? (TextDisplayHelper.ToNumSpriteString(previewDamageValue.Value) + "<sprite=20 tint=1>" + TextDisplayHelper.ToNumSpriteString(num, 10)) : TextDisplayHelper.ToNumSpriteString(previewDamageValue.Value));
		damageText.text = text;
		if (damageTextShadow != null)
		{
			damageTextShadow.text = text;
		}
	}

	private NodeDisplayInfo GetSpellNodeInfo(ItemSpell spell)
	{
		SpellCastInfo battleDisplaySpellInfo = GetBattleDisplaySpellInfo();
		GameEvent eve = Effect.GenerateDisplayEvent(UIDisplayModule.gameState, base.item, battleDisplaySpellInfo, GameEvent.TriggerType.InBattleDisplay);
		return spell.GetSpellIntentEffectDisplayInfo(eve);
	}

	private static int? GetPreviewDamageValue(NodeDisplayInfo info)
	{
		int? num = info?.baseValue?.value;
		if (info?.damage != null && num.HasValue)
		{
			return num.Value + (info.deltaValue?.value).GetValueOrDefault();
		}
		return info?.value?.value;
	}

	private SpellCastInfo GetBattleDisplaySpellInfo()
	{
		PlayerControlInfo playerControlInfo = UIDisplayModule.gameState?.UIManager?.playerItemArea?.controlInfo;
		if (playerControlInfo?.displaySpellInfo?.spellItem == base.item)
		{
			return playerControlInfo.displaySpellInfo;
		}
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(UIDisplayModule.gameState);
		displaySpellCastInfo.spellItem = base.item;
		displaySpellCastInfo.SetDefaultTargetItem();
		displaySpellCastInfo.AddCost(GameEvent.TriggerType.InBattleDisplay);
		return displaySpellCastInfo;
	}

	private void SetVisible(bool visible)
	{
		if (damageIconImage != null)
		{
			damageIconImage.gameObject.SetActive(visible);
		}
		if (damageText != null)
		{
			damageText.gameObject.SetActive(visible);
		}
		if (damageTextShadow != null)
		{
			damageTextShadow.gameObject.SetActive(visible);
		}
	}
}
