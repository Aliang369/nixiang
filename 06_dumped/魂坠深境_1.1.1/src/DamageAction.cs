using System.Collections.Generic;
using System.Linq;
using DamageNumbersPro;

public class DamageAction : ExecuteAction
{
	public const int ContinuousActionTriggerLimit = 1000;

	private const string BlockyDelicacyItemName = "块状珍馐";

	private const string BlockyDelicacyHitNfxName = "Shi";

	private const string BlockyDelicacyPersistentNfxName = "Fly_2";

	private const float BlockyDelicacyPersistentNfxDuration = 180f;

	public DamageCommand damageCommand;

	public static int ClampBatchCount(long requested)
	{
		if (requested <= 0)
		{
			return 0;
		}
		if (requested >= int.MaxValue)
		{
			return int.MaxValue;
		}
		return (int)requested;
	}

	public DamageAction(DamageCommand damageCommand)
	{
		this.damageCommand = damageCommand;
	}

	protected override void Update()
	{
		RealSpellCastInfo realSpellCastInfo = gameState?.playerState?.playerSpellInfo;
		if ((realSpellCastInfo?.continuousDamageActionTriggerCount).GetValueOrDefault() > 1000)
		{
			return;
		}
		if (realSpellCastInfo != null && realSpellCastInfo.continuousDamageActionTriggerCount.HasValue)
		{
			realSpellCastInfo.continuousDamageActionTriggerCount++;
		}
		if (damageCommand != null && damageCommand.baseDamage >= 0 && damageCommand.targetBFT != null && !IsDeathPendingEnemy(damageCommand.targetBFT))
		{
			Damage damage = Damage.GenerateDamage(damageCommand, gameState, GameEvent.TriggerType.Real);
			if (!IsDeathPendingEnemy(damage?.targetBFT) && !IsDeathPendingEnemy(damage?.targetItem?.OwnerBft))
			{
				DealDamage(damage);
			}
		}
	}

	private static bool IsDeathPendingEnemy(BattleFieldTransform bft)
	{
		if (bft != null && bft.isEnemy)
		{
			return bft.characterAI?.deathPending ?? false;
		}
		return false;
	}

	private void DealDamage(Damage damage)
	{
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		gameState.gameEventManager.TriggerBeforeDamage(damage);
		Item item = damage.source as Item;
		BattleFieldTransform battleFieldTransform = damage.targetBFT ?? damage.targetItem.OwnerBft;
		bool flag = damage.targetBFT != null && damage.targetBFT.isEnemy && damage.SourceOwnerBft != null && damage.SourceOwnerBft.isPlayer && !damage.isOverflowDamage && (item == null || !item.Companion);
		if (flag && damage.castDamage > ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.strongDamageThreshold)
		{
			EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.strongAttackPrepareShake, delay: true));
		}
		PivotManager.IWithPivot withPivot = null;
		if (damage.targetItem == null && damage.targetBFT != null)
		{
			if (damage.targetBFT.isPlayer)
			{
				withPivot = damage.targetBFT.GetUIPivot(gameState);
			}
			else if (damage.targetBFT.isEnemy)
			{
				withPivot = damage.targetBFT.GetHitPivot();
			}
		}
		else if (damage.targetItem != null)
		{
			withPivot = damage.targetItem;
		}
		if (damage.SourceOwnerBft != null && !damage.isOverflowDamage && !damage.IsFromSpell && item != null)
		{
			PlayTransformVFXAction(item, withPivot);
		}
		if (damage.SourceOwnerBft != null && damage.isHealthDamage && !damage.isOverflowDamage)
		{
			PlayTransform3DNFXAction(damage.SourceOwnerBft, battleFieldTransform);
		}
		if (battleFieldTransform != null)
		{
			if (damage.isHealthDamage)
			{
				EnqueueAction(new DisplayNFXAction(this.damageCommand.hitSetup?.setup, battleFieldTransform.GetUIPivot(gameState)));
			}
			if (battleFieldTransform.isPlayer && damage.isHealthDamage)
			{
				if (damage.subCounter > ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.strongDamageThreshold)
				{
					EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.strongDamagedShake));
				}
				else
				{
					EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.weakDamagedShake));
				}
			}
			else if (battleFieldTransform.isPlayer && damage.targetItem != null && damage.targetItem.Defence)
			{
				EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.DefendDamageShake));
			}
			if (flag)
			{
				if (damage.castDamage > ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.strongDamageThreshold)
				{
					EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.strongAttackShake));
				}
				else
				{
					EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.weakAttackShake));
				}
			}
		}
		if (damage.targetItem != null)
		{
			EnqueueAction(new PopupTextAction(damage.targetItem, damage.subCounter, ScriptableObjectSingleton<VFXDataPreset>.instance.defenceDamagePopupTextPrefab));
		}
		else
		{
			DamageNumberGUI damageNumbersPrefab = (battleFieldTransform.isPlayer ? ScriptableObjectSingleton<VFXDataPreset>.instance.playerHealthDamagePopupTextPrefab : ScriptableObjectSingleton<VFXDataPreset>.instance.enemyHealthDamagePopupTextPrefab);
			EnqueueAction(new PopupTextAction(withPivot, damage.finalDamage, damageNumbersPrefab));
		}
		int? originalDefendCounterBeforeBreak = null;
		GameEvent.ResolveInfo overflowResolveInfo = null;
		if (damage.targetItem != null && damage.IsRealDamageDealt && !damage.isHealthDamage)
		{
			Item targetItem = damage.targetItem;
			int currentDefendCounter = targetItem.GetCurrentDefendCounter();
			originalDefendCounterBeforeBreak = currentDefendCounter;
			bool flag2 = currentDefendCounter > 0 && currentDefendCounter - damage.subCounter <= 0 && targetItem.ShouldBreakWhenDefendDepleted();
			foreach (Damage.ReplacementArmorConsumeInfo item2 in damage.replacementArmorConsumes.OrEmptyIfNull())
			{
				if (item2.enchant != null && item2.absorb > 0)
				{
					EnqueueAction(new ChangeEnchantAction(targetItem, damage.source as Item, item2.enchant.def, -item2.absorb, popup: false));
				}
			}
			if (damage.itemCounterAbsorb > 0)
			{
				EnqueueAction(new ChangeCounterAction(-damage.itemCounterAbsorb, targetItem, damage.source));
			}
			if (targetItem.DefenceLike || targetItem.Companion)
			{
				EnqueueAction(new DisplayNFXAction(this.damageCommand.hitSetup?.setup, targetItem));
				if (flag2)
				{
					Item targetItem2 = damage.targetItem;
					if (targetItem2 != null && targetItem2.owner?.items.Count((Item it) => it.DefenceLike) == 1)
					{
						EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_defence_hit_final_break));
					}
					else
					{
						EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_defence_hit_break));
					}
					EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_slot_DefendBreak, targetItem));
					if (battleFieldTransform != null && battleFieldTransform.isEnemy)
					{
						EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_enemy_defence_break, battleFieldTransform.Get3DNFXPivot(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_enemy_defence_break)));
					}
				}
				else
				{
					EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_defence_hit));
					EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_slot_Defend, targetItem));
					if (battleFieldTransform != null && battleFieldTransform.isEnemy)
					{
						EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_enemy_defence, battleFieldTransform.Get3DNFXPivot(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_enemy_defence)));
					}
				}
			}
			EnqueueAction(new WaitAction(ScriptableObjectSingleton<VFXDataPreset>.instance.itemDefenceDelayTime / base.actionTimeScale));
		}
		PlayHitEffect3D(damage, battleFieldTransform);
		PlayPersistentHitEffect3D(damage, battleFieldTransform);
		if (damage.isHealthDamage)
		{
			if (battleFieldTransform.isPlayer)
			{
				EnqueueAction(new VignetteAction(ScriptableObjectSingleton<VFXDataPreset>.instance.damagedVignetteColor, ScriptableObjectSingleton<VFXDataPreset>.instance.damagedVignetteIntensity, ScriptableObjectSingleton<VFXDataPreset>.instance.damagedVignetteTime));
			}
			else
			{
				EnqueueAction(new PlayPerformanceAction(null, battleFieldTransform, EnemyAnimationType.hit));
			}
		}
		if (damage.isHealthDamage)
		{
			int num = -damage.subCounter;
			GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerDamageHealthChangeResolve(damage, damage.targetBFT, num, GameEvent.TriggerType.Real);
			if (!resolveInfo.reject)
			{
				num += resolveInfo.deltaValue;
				EnqueueAction(new ChangeBftHealthAction(damage.targetBFT, num, damage.source, damage, resolveInfo.realHealthOnly));
			}
			else
			{
				damage.subCounter = 0;
			}
		}
		if (damage.targetItem != null && damage.IsWithDmaageOverflow)
		{
			overflowResolveInfo = gameState.gameEventManager.TriggerGenerateOverflowDamageCommandResolve(damage);
			if (overflowResolveInfo.replaceItem != null)
			{
				EnqueueAction(new ConvertItemAction(damage.targetItem, overflowResolveInfo.replaceItem));
			}
			if (damage.allowOverflow && !overflowResolveInfo.reject && overflowResolveInfo.replaceItem == null)
			{
				DamageCommand damageCommand = new DamageCommand();
				damageCommand.baseDamage = damage.overflowDamage;
				damageCommand.lastDamageInfo = new DamageCommand.OverflowLastDamageInfo
				{
					lastDamageItem = damage.targetItem,
					damagedItems = (this.damageCommand.lastDamageInfo?.damagedItems.Copy().Append(damage.targetItem).ToList() ?? new List<Item> { damage.targetItem })
				};
				damageCommand.source = damage.source;
				damageCommand.sourceOwnerOverride = damage.sourceOwnerOverride;
				damageCommand.targetBFT = ((overflowResolveInfo.overrideBftTarget == null) ? damage.targetItem.OwnerBft : overflowResolveInfo.overrideBftTarget);
				damageCommand.overflow = damage.allowOverflow;
				damageCommand.spellCastInfo = damage.sourceSpellInfo;
				damageCommand.pierce = damage.pierce;
				damageCommand.ignoreSourceDamageModifiers = this.damageCommand.ignoreSourceDamageModifiers;
				damageCommand.transformSetup = this.damageCommand.transformSetup;
				damageCommand.transform3DSetup = null;
				damageCommand.hitSetup = this.damageCommand.hitSetup;
				damageCommand.hit3DSetup = null;
				damageCommand.persistentHit3DSetup = null;
				EnqueueAction(new DamageAction(damageCommand), passIgnoreLocks: false);
			}
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAfterDamage(damage);
		});
		EnqueueAction(delegate
		{
			bool flag3 = overflowResolveInfo != null && (overflowResolveInfo.replaceItem != null || overflowResolveInfo.suppressBreak);
			if (damage.targetItem != null && !flag3 && damage.targetItem.GetCurrentDefendCounter() <= 0 && damage.targetItem.ShouldBreakWhenDefendDepleted())
			{
				EnqueueAction(new BreakItemAction(damage.targetItem, damage.source, damage, originalDefendCounterBeforeBreak));
			}
		});
		if (!damage.isOverflowDamage)
		{
			EnqueueAction(new WaitAction(0.1f / base.actionTimeScale));
		}
	}

	private void PlayHitEffect3D(Damage damage, BattleFieldTransform bft)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (bft != null)
		{
			HitNFX3DConfig hitNFX3DConfig = GetHit3DConfig(damage);
			if (hitNFX3DConfig?.setup == null && GetPersistentHit3DConfig(damage)?.setup == null)
			{
				hitNFX3DConfig = new HitNFX3DConfig
				{
					setup = ScriptableObjectSingleton<VFXDataPreset>.instance.effect_defaultHit3D
				};
				EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_hit_default));
			}
			if (hitNFX3DConfig?.setup != null)
			{
				EnqueueAction(new DisplayNFXAction(hitNFX3DConfig.setup, bft.Get3DNFXPivot(hitNFX3DConfig.setup)));
			}
			if (damage.source is Item item && item.HasTag(ScriptableObjectSingleton<GameDataManager>.instance.enemyAttackIntentTag) && item.OwnerBft != null && item.OwnerBft.isEnemy && item.OwnerBft.characterAI?.component?.attackNfxSetup != null)
			{
				NFXSetup attackNfxSetup = item.OwnerBft.characterAI.component.attackNfxSetup;
				EnqueueAction(new DisplayNFXAction(attackNfxSetup, bft.Get3DNFXPivot(attackNfxSetup)));
			}
			if (damage.targetBFT != null && damage.targetBFT.isEnemy && damage.targetBFT.characterAI?.component?.damagedNfxSetup != null)
			{
				NFXSetup damagedNfxSetup = damage.targetBFT.characterAI.component.damagedNfxSetup;
				EnqueueAction(new DisplayNFXAction(damagedNfxSetup, bft.Get3DNFXPivot(damagedNfxSetup)));
			}
		}
	}

	private void PlayPersistentHitEffect3D(Damage damage, BattleFieldTransform bft)
	{
		PersistentHitNFX3DConfig persistentHit3DConfig = GetPersistentHit3DConfig(damage);
		if (bft != null && !(persistentHit3DConfig?.setup == null) && !(persistentHit3DConfig.duration <= 0f) && IsPersistentHitDamageValid(damage))
		{
			EnqueueAction(new DisplayPersistentNFXAction(persistentHit3DConfig.setup, bft.Get3DNFXPivot(persistentHit3DConfig.setup), persistentHit3DConfig.duration, bft));
		}
	}

	private PersistentHitNFX3DConfig GetPersistentHit3DConfig(Damage damage)
	{
		PersistentHitNFX3DConfig persistentHit3DSetup = damageCommand.persistentHit3DSetup;
		if (!IsBlockyDelicacyDamage(damage))
		{
			return persistentHit3DSetup;
		}
		PersistentHitNFX3DConfig blockyDelicacyPersistentHitConfig = GetBlockyDelicacyPersistentHitConfig(damage);
		if (blockyDelicacyPersistentHitConfig != null)
		{
			return blockyDelicacyPersistentHitConfig;
		}
		return persistentHit3DSetup;
	}

	private HitNFX3DConfig GetHit3DConfig(Damage damage)
	{
		HitNFX3DConfig hit3DSetup = damageCommand.hit3DSetup;
		if (!IsBlockyDelicacyDamage(damage))
		{
			return hit3DSetup;
		}
		if (hit3DSetup?.setup != null && hit3DSetup.setup.name == "Shi")
		{
			return hit3DSetup;
		}
		if (!(damage?.source is Item item))
		{
			return hit3DSetup;
		}
		HitNFX3DConfig hitNFX3DConfig = item.CurrentSpell?.spellNode?.GetDescendantNodeOfType<DealDamage>(includeSelf: true).Select((DealDamage node) => node.hitEffect3D).FirstOrDefault((HitNFX3DConfig config) => config?.setup != null && config.setup.name == "Shi");
		if (hitNFX3DConfig != null)
		{
			return hitNFX3DConfig;
		}
		NFXSetup nFXSetup = FindNfxSetup("Shi");
		if (nFXSetup == null)
		{
			return hit3DSetup;
		}
		return new HitNFX3DConfig
		{
			setup = nFXSetup
		};
	}

	private PersistentHitNFX3DConfig GetBlockyDelicacyPersistentHitConfig(Damage damage)
	{
		PersistentHitNFX3DConfig persistentHitNFX3DConfig = (damage?.source as Item)?.CurrentSpell?.spellNode?.GetDescendantNodeOfType<DealDamage>(includeSelf: true).Select((DealDamage node) => node.persistentHitEffect3D).FirstOrDefault((PersistentHitNFX3DConfig config) => config?.setup != null && config.setup.name == "Fly_2" && config.duration > 0f);
		if (persistentHitNFX3DConfig != null)
		{
			return persistentHitNFX3DConfig;
		}
		return GetBlockyDelicacyPersistentHitFallback();
	}

	private PersistentHitNFX3DConfig GetBlockyDelicacyPersistentHitFallback()
	{
		NFXSetup nFXSetup = FindNfxSetup("Fly_2");
		if (nFXSetup == null)
		{
			return null;
		}
		return new PersistentHitNFX3DConfig
		{
			setup = nFXSetup,
			duration = 180f
		};
	}

	private NFXSetup FindNfxSetup(string nfxName)
	{
		return gameState?.applicationState?.assetCatalog?.nfxCatalog?.ValidAssets?.FirstOrDefault((NFXSetup nfx) => nfx != null && nfx.name == nfxName);
	}

	private static bool IsPersistentHitDamageValid(Damage damage)
	{
		if (damage != null)
		{
			return damage.finalDamage > 0;
		}
		return false;
	}

	private static bool IsBlockyDelicacyDamage(Damage damage)
	{
		if (damage?.source is Item item)
		{
			return item.ItemName == "块状珍馐";
		}
		return false;
	}

	private void PlayTransformVFXAction(PivotManager.IWithPivot start, PivotManager.IWithPivot end)
	{
		TransformNFXConfig transformNFXConfig = damageCommand.transformSetup;
		if (transformNFXConfig?.setup == null)
		{
			transformNFXConfig = new TransformNFXConfig
			{
				setup = ScriptableObjectSingleton<VFXDataPreset>.instance.effect_defaultTransform
			};
		}
		EnqueueAction(new PlayTransformVFXAction(transformNFXConfig.setup, start, end, is3d: false));
	}

	private void PlayTransform3DNFXAction(BattleFieldTransform from, BattleFieldTransform to)
	{
		if (damageCommand.transform3DSetup?.setup != null)
		{
			ConvertablePivot start = from.Get3DNFXPivot(damageCommand.transform3DSetup.setup);
			ConvertablePivot target = to.Get3DNFXPivot(damageCommand.transform3DSetup.setup);
			EnqueueAction(new PlayTransformVFXAction(damageCommand.transform3DSetup.setup, start, target, is3d: true));
		}
	}
}
