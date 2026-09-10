using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class RedrawUntilAllChaptersAction : ExecuteAction
{
	private sealed class RoundState
	{
		public int discardedCount;

		public int drawnCount;

		public bool reachedZeroHealth;
	}

	private readonly ItemTag chapterTag;

	private readonly AddChapter completionEffect;

	private readonly int drawCountOffset;

	private readonly int maxRounds;

	private readonly BattleFieldTransform owner;

	private readonly int redrawDamage;

	private readonly DealDamage redrawDamageEffect;

	private readonly int roundIndex;

	private readonly Item sourceItem;

	private readonly SpellCastInfo spellInfo;

	public RedrawUntilAllChaptersAction(BattleFieldTransform owner, Item sourceItem, SpellCastInfo spellInfo, ItemTag chapterTag, int drawCountOffset, int maxRounds, int redrawDamage, DealDamage redrawDamageEffect, AddChapter completionEffect, int roundIndex)
	{
		this.owner = owner;
		this.sourceItem = sourceItem;
		this.spellInfo = spellInfo;
		this.chapterTag = chapterTag;
		this.drawCountOffset = drawCountOffset;
		this.maxRounds = maxRounds;
		this.redrawDamage = redrawDamage;
		this.redrawDamageEffect = redrawDamageEffect;
		this.completionEffect = completionEffect;
		this.roundIndex = roundIndex;
	}

	protected override void Update()
	{
		if (ShouldStopForDeath())
		{
			return;
		}
		List<Item> currentNonChapters = GetCurrentNonChapters();
		if (currentNonChapters.Count == 0)
		{
			EnqueueCompletionEffect();
			return;
		}
		if (roundIndex >= maxRounds)
		{
			TraceLog.Warn($"[RedrawUntilAllChapters] Reached the {maxRounds}-round safety limit with {currentNonChapters.Count} non-chapter item(s) remaining.");
			EnqueueCompletionEffect();
			return;
		}
		RoundState state = new RoundState();
		foreach (Item item in currentNonChapters)
		{
			EnqueueAction(new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect, null, null, delegate
			{
				state.discardedCount++;
			}));
		}
		EnqueueAction(delegate
		{
			ContinueAfterDiscards(state);
		});
	}

	private List<Item> GetCurrentNonChapters()
	{
		ItemContainer table = owner?.tableContainer;
		if (table == null)
		{
			return new List<Item>();
		}
		return (from item in table.GetAllItems()
			where item != null && item.owner == table && item.ownerSlotIndex.HasValue && !item.HasTag(chapterTag)
			select item).ToList();
	}

	private void ContinueAfterDiscards(RoundState state)
	{
		if (ShouldStopForDeath())
		{
			return;
		}
		int num = Math.Max(0, state.discardedCount + drawCountOffset);
		for (int i = 0; i < num; i++)
		{
			EnqueueAction(new DrawFromPileAction(owner, sourceItem, 0, DrawFromPileAction.DrawMode.Random, delegate(Item drawnItem)
			{
				if (drawnItem != null)
				{
					state.drawnCount++;
				}
			}));
		}
		EnqueueAction(delegate
		{
			ContinueAfterDraws(state);
		});
	}

	private void ContinueAfterDraws(RoundState state)
	{
		if (ShouldStopForDeath())
		{
			return;
		}
		if (state.drawnCount <= 0)
		{
			EnqueueNextRound();
			return;
		}
		Action<GameEvent> healthChangeHandler = null;
		healthChangeHandler = delegate(GameEvent eve)
		{
			if (eve.type == GameEvent.Type.BftHealthChange && eve.TargetBft == owner && (int)owner.health <= 0)
			{
				state.reachedZeroHealth = true;
				gameState.gameEventManager.onTriggerGameEvent -= healthChangeHandler;
			}
		};
		gameState.gameEventManager.onTriggerGameEvent += healthChangeHandler;
		EnqueueAction(new DamageAction(new DamageCommand
		{
			baseDamage = redrawDamage,
			source = sourceItem,
			sourceOwnerOverride = owner,
			spellCastInfo = spellInfo,
			targetBFT = owner,
			overflow = redrawDamageEffect.overflow,
			pierce = redrawDamageEffect.pierce,
			transformSetup = redrawDamageEffect.transformVFXEffect,
			transform3DSetup = redrawDamageEffect.transformEffect3D,
			hitSetup = redrawDamageEffect.hitEffect,
			hit3DSetup = redrawDamageEffect.hitEffect3D,
			persistentHit3DSetup = redrawDamageEffect.persistentHitEffect3D
		}));
		EnqueueAction(delegate
		{
			gameState.gameEventManager.onTriggerGameEvent -= healthChangeHandler;
			if (!state.reachedZeroHealth && !ShouldStopForDeath())
			{
				EnqueueNextRound();
			}
		});
	}

	private void EnqueueNextRound()
	{
		EnqueueAction(new RedrawUntilAllChaptersAction(owner, sourceItem, spellInfo, chapterTag, drawCountOffset, maxRounds, redrawDamage, redrawDamageEffect, completionEffect, roundIndex + 1));
	}

	private void EnqueueCompletionEffect()
	{
		if (ShouldStopForDeath())
		{
			return;
		}
		EnqueueAction(delegate
		{
			if (!ShouldStopForDeath())
			{
				GameEvent eve = new GameEvent(GameEvent.Type.NodeCastSpell, GameEvent.TriggerType.Real)
				{
					gameState = gameState,
					spellInfo = spellInfo,
					Caster = owner
				};
				completionEffect.Cast(eve);
			}
		});
	}

	private bool ShouldStopForDeath()
	{
		if (owner != null && (int)owner.health > 0)
		{
			GameStateInfo gameStateInfo = gameState;
			if (gameStateInfo == null || !gameStateInfo.gameEnded)
			{
				if (owner.isPlayer)
				{
					GameUIManager uIManager = gameState.UIManager;
					if ((object)uIManager == null)
					{
						return false;
					}
					return uIManager.deathPendingPanel?.pending == true;
				}
				return owner.characterAI?.deathPending ?? false;
			}
		}
		return true;
	}
}
