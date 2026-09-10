using FMODUnity;

public class ChangeEnchantAction : ExecuteAction, ISpellCastContextAction
{
	private int delta;

	private readonly EnchantDef enchantDef;

	private readonly bool popup;

	private SpellCastInfo spellInfo;

	private readonly object source;

	private readonly Item targetItem;

	public SpellCastInfo SpellInfo
	{
		get
		{
			return spellInfo;
		}
		set
		{
			spellInfo = value;
		}
	}

	public ChangeEnchantAction(Item targetItem, object source, EnchantDef enchantDef, int delta, bool popup = true, SpellCastInfo spellInfo = null)
	{
		this.targetItem = targetItem;
		this.source = source;
		this.enchantDef = enchantDef;
		this.delta = delta;
		this.popup = popup;
		this.spellInfo = spellInfo;
	}

	protected override void Update()
	{
		if (enchantDef == null || targetItem == null)
		{
			return;
		}
		SpellCastInfo spellCastInfo = spellInfo;
		if (spellCastInfo != null && spellCastInfo.suppressChangeEnchantAndSpawnItemEffects)
		{
			return;
		}
		Enchant enchant = targetItem.Enchants.Find((Enchant e) => e.def == enchantDef);
		if (delta >= 0 || enchant == null || enchant.def?.attachAbility?.Exists((Ability ability) => ability is EternalAbility) != true || source == targetItem)
		{
			if (enchant == null && delta > 0)
			{
				AddEnchant();
			}
			else if (enchant != null)
			{
				ModifyEnchant(enchant);
			}
		}
	}

	private void RemoveEnchant(EnchantDef def)
	{
		Enchant enchant = targetItem.Enchants.Find((Enchant e) => e.def == def);
		if (enchant != null)
		{
			gameState.gameEventManager.TriggerBeforeRemoveEnchant(source, targetItem, enchant);
			targetItem.RemoveEnchant(enchant);
		}
	}

	private void AddEnchant()
	{
		Enchant enchant = new Enchant(enchantDef, delta);
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerChangeEnchantResolve(delta, targetItem, enchant, source, GameEvent.TriggerType.Real);
		if (resolveInfo.reject)
		{
			return;
		}
		delta += resolveInfo.deltaValue;
		delta *= 1 + resolveInfo.deltaPercent / 100;
		enchant.value = delta;
		if (enchant.def.addConstraint?.CheckItemAccept(targetItem) ?? true)
		{
			targetItem.AddEnchant(enchant);
			EnqueueAction(new DisplayNFXAction(enchant.def.onAddNFX, targetItem));
			if (popup)
			{
				ShowPopTextAnim(enchant, enchant.value, valid: true);
			}
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerAddEnchant(source, targetItem, enchant);
			});
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerEnchantValueChange(source, targetItem, enchant, delta);
			});
		}
		else if (popup)
		{
			ShowPopTextAnim(enchant, enchant.value, valid: false);
		}
		PlayAddEnchantSFX(enchant);
	}

	private void ModifyEnchant(Enchant enchant)
	{
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerChangeEnchantResolve(delta, targetItem, enchant, source, GameEvent.TriggerType.Real);
		if (!resolveInfo.reject)
		{
			delta += resolveInfo.deltaValue;
			delta *= 1 + resolveInfo.deltaPercent / 100;
			if (enchantDef.stacking)
			{
				Enchant enchant2 = enchant;
				enchant2.value = (int)enchant2.value + delta;
			}
			ShowPopTextAnim(enchant, delta, valid: true);
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerEnchantValueChange(source, targetItem, enchant, delta);
			});
			if ((int)enchant.value <= 0 || (!enchantDef.stacking && delta < 0))
			{
				RemoveEnchant(enchant.def);
			}
			if (delta > 0)
			{
				PlayAddEnchantSFX(enchant);
			}
		}
	}

	private void ShowPopTextAnim(Enchant enchant, int delta, bool valid)
	{
		if (!targetItem.IsHidden(gameState) && delta > 0 && popup)
		{
			string text = (enchant.def.Name + (enchant.def.stacking ? delta.ToString() : "")).ColorText(enchant.def.displayColor);
			if (!valid)
			{
				text = (enchant.def.Name + (enchant.def.stacking ? delta.ToString() : "") + ScriptableObjectSingleton<TextDataPreset>.instance.term_enchant_add_invalid).ColorText(enchant.def.displayColor);
			}
			PopupTextAction popupTextAction = new PopupTextAction(targetItem, text, ScriptableObjectSingleton<VFXDataPreset>.instance.enchantPopupTextPrefab);
			popupTextAction.popupIcon = enchant.def.Icon;
			EnqueueAction(popupTextAction);
		}
	}

	private void PlayAddEnchantSFX(Enchant enchant)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!((EventReference)(ref enchant.def.addSoundOverride)).IsNull)
		{
			EnqueueAction(new PlayOneShotSoundAction(enchant.def.addSoundOverride));
		}
		else if (enchant.def.type == EnchantDef.EnchantType.Negative)
		{
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_enchant_debuff));
		}
		else
		{
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_enchant_buff));
		}
	}
}
