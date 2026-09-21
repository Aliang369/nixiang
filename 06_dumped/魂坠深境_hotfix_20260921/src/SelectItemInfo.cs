using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class SelectItemInfo
{
	public Func<bool> canCancel;

	public Func<bool> canConfirm;

	public Func<bool> shouldCancel;

	public Func<Item, bool> canSelect;

	public Func<string> panelHintGetter;

	public TextDataGenerator generator;

	public Action onCancel;

	public Action onConfirm;

	public BattleFieldTransform selectorBft;

	public object selectSource;

	public bool showUncollectedStatus;

	public bool singleSelect;

	public List<Item> output;

	public List<Item> pool;

	private Func<List<Item>> poolGetter;

	private bool poolResolved;

	public void SetPoolGetter(Func<List<Item>> getter)
	{
		poolGetter = getter;
		poolResolved = false;
		pool = null;
	}

	public void ResolvePool()
	{
		if (poolGetter != null && !poolResolved)
		{
			poolResolved = true;
			pool = poolGetter();
			poolGetter = null;
		}
	}

	public static SelectItemInfo SimpleSelectSingleItem(List<Item> pool, Action onConfirm, Action onCancel, List<Item> output, Func<Item, bool> selectableChecker = null, Func<string> panelHintGetter = null)
	{
		return SimpleSelectCountItem(pool, onConfirm, onCancel, output, 1, selectableChecker, panelHintGetter);
	}

	public static SelectItemInfo SimpleSelectCountItem(List<Item> pool, Action onConfirm, Action onCancel, List<Item> output, int count, Func<Item, bool> selectableChecker = null, Func<string> panelHintGetter = null)
	{
		return new SelectItemInfo
		{
			pool = pool,
			onConfirm = onConfirm,
			onCancel = onCancel,
			singleSelect = (count == 1),
			panelHintGetter = panelHintGetter,
			shouldCancel = () => pool.Count == 0,
			canSelect = (Item i) => selectableChecker == null || selectableChecker(i),
			canConfirm = () => output.Count == count,
			output = output,
			selectorBft = SceneInfoSingleton.instance.gameState.battleState.player
		};
	}

	public static SelectItemInfo SimpleSelectAnyItem(List<Item> pool, Action onSelect, Action onCancel, List<Item> output, Func<Item, bool> selectableChecker = null, Func<string> panelHintGetter = null)
	{
		return new SelectItemInfo
		{
			pool = pool,
			onConfirm = onSelect,
			onCancel = onCancel,
			panelHintGetter = panelHintGetter,
			shouldCancel = () => false,
			canSelect = (Item i) => selectableChecker == null || selectableChecker(i),
			canConfirm = () => true,
			output = output
		};
	}

	public bool CanConfirm()
	{
		bool flag = true;
		if (canConfirm != null)
		{
			Delegate[] invocationList = canConfirm.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Func<bool> func = (Func<bool>)invocationList[i];
				flag &= func();
			}
		}
		return flag;
	}

	public bool CanCancel()
	{
		bool flag = true;
		if (canCancel != null)
		{
			Delegate[] invocationList = canCancel.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Func<bool> func = (Func<bool>)invocationList[i];
				flag &= func();
			}
		}
		return flag;
	}

	private void AIRandomSelect(GameStateInfo gameState)
	{
		if (shouldCancel != null && shouldCancel())
		{
			onCancel?.Invoke();
			return;
		}
		new List<List<Item>>();
		List<Item> shufflePool = pool.Shuffle(gameState.randomManager.NextDouble);
		int maxStep = 10000;
		int step = 0;
		if (FindConfirmPattern())
		{
			onConfirm?.Invoke();
		}
		else
		{
			onCancel?.Invoke();
		}
		bool FindConfirmPattern()
		{
			if (step > maxStep)
			{
				return false;
			}
			foreach (Item item in shufflePool)
			{
				step++;
				if (!output.Contains(item) && (canSelect == null || canSelect(item)))
				{
					output.Add(item);
					if (canConfirm == null || canConfirm())
					{
						return true;
					}
					if (FindConfirmPattern())
					{
						return true;
					}
					output.Remove(item);
				}
			}
			return false;
		}
	}

	public void TriggerSelect(GameStateInfo gameState, PlayerSelectItemsEffect.SelectPanelType selectPanelType, bool prepareSelect)
	{
		BattleFieldTransform battleFieldTransform = selectorBft;
		if (battleFieldTransform != null && battleFieldTransform.isEnemy)
		{
			AIRandomSelect(gameState);
		}
		else
		{
			if (GameAutomationHooks.TryHandleSelectItem(gameState, this))
			{
				return;
			}
			UIHelper.HideAllInfo(null);
			switch (selectPanelType)
			{
			case PlayerSelectItemsEffect.SelectPanelType.HorizontalSelect:
				if (prepareSelect && selectSource is GameSkill)
				{
					gameState.UIManager.battleSkillItemSelectPanel.Setup(this);
				}
				else
				{
					gameState.UIManager.battleItemSelectPanel.Setup(this);
				}
				break;
			case PlayerSelectItemsEffect.SelectPanelType.PileSelect:
				if (prepareSelect && selectSource is GameSkill)
				{
					gameState.UIManager.skillPickItemPanel.Setup(this);
				}
				else
				{
					gameState.UIManager.deckItemSelectPanel.Setup(this);
				}
				break;
			case PlayerSelectItemsEffect.SelectPanelType.Table:
				if (prepareSelect && selectSource is GameSkill)
				{
					gameState.UIManager.battleSkillTableItemSelectPanel.Setup(this);
				}
				else
				{
					gameState.UIManager.battleTableItemSelectPanel.Setup(this);
				}
				break;
			}
		}
	}

	public static SelectItemInfo GenerateNodeSelectItemsInfo(GameEvent eve, ItemTargetSelector pool, CheckNode shouldCancel, CheckNode selectable, CheckNode confirmable, Action onConfirm, Action onCancel, Func<bool> canCancel, BattleFieldTransform selector, string panelHint, bool isSingleSelect)
	{
		SelectItemInfo info = new SelectItemInfo
		{
			output = eve.SelectedItems,
			singleSelect = isSingleSelect
		};
		info.SetPoolGetter(() => pool?.GetTargetItems(eve));
		info.panelHintGetter = () => FormatSelectPanelHint(eve, panelHint, out info.generator);
		info.shouldCancel = ShouldCancel;
		info.canConfirm = CanConfirm;
		info.canSelect = CanSelect;
		info.selectorBft = selector;
		info.onConfirm = onConfirm;
		info.onCancel = onCancel;
		info.canCancel = canCancel;
		info.selectSource = eve.GetEveObj(GameEvent.EveObjType.Caster);
		return info;
		bool CanConfirm()
		{
			if (confirmable == null)
			{
				return true;
			}
			return confirmable.Check(eve) == true;
		}
		bool CanSelect(Item item)
		{
			if (selectable != null)
			{
				Item nodeTempItem = eve.NodeTempItem;
				eve.NodeTempItem = item;
				bool result = selectable.Check(eve) == true;
				eve.NodeTempItem = nodeTempItem;
				return result;
			}
			return true;
		}
		bool ShouldCancel()
		{
			if (info.pool == null || info.pool.Count == 0)
			{
				return true;
			}
			if (shouldCancel != null && shouldCancel.Check(eve) == true)
			{
				return true;
			}
			return false;
		}
	}

	public static SelectItemInfo GenerateNodeSelectItemsCountInfo(GameEvent eve, ItemTargetSelector pool, ValueGenerator countGenerator, ItemConstraint selectable, ValueCompareCheckNode.CompareType compareType, Action onConfirm, Action onCancel, BattleFieldTransform selector, Func<bool> canCancel, string rawPanelHint)
	{
		_ = eve.SelectedItems;
		eve.SelectedItems = new List<Item>();
		int count = -1;
		SelectItemInfo info = null;
		info = new SelectItemInfo
		{
			canConfirm = CanConfirm,
			canSelect = CanSelect,
			canCancel = canCancel,
			onCancel = onCancel,
			onConfirm = onConfirm,
			shouldCancel = ShouldCancel,
			output = eve.SelectedItems,
			selectorBft = selector,
			selectSource = eve.Caster
		};
		info.SetPoolGetter(delegate
		{
			List<Item> result = pool?.GetTargetItems(eve);
			count = countGenerator?.GenerateValue(eve) ?? (-1);
			info.singleSelect = count == 1 && compareType != ValueCompareCheckNode.CompareType.Greater;
			return result;
		});
		info.panelHintGetter = () => FormatSelectPanelHint(eve, rawPanelHint, out info.generator);
		return info;
		bool CanConfirm()
		{
			if (countGenerator == null)
			{
				return true;
			}
			return compareType switch
			{
				ValueCompareCheckNode.CompareType.Equal => eve.SelectedItems.Count == count, 
				ValueCompareCheckNode.CompareType.Greater => eve.SelectedItems.Count > count, 
				ValueCompareCheckNode.CompareType.Less => eve.SelectedItems.Count < count, 
				ValueCompareCheckNode.CompareType.GreaterEqual => eve.SelectedItems.Count >= count, 
				ValueCompareCheckNode.CompareType.LessEqual => eve.SelectedItems.Count <= count, 
				_ => false, 
			};
		}
		bool CanSelect(Item item)
		{
			if (selectable != null && selectable.CheckItemAccept(item) == false)
			{
				return false;
			}
			if (countGenerator == null)
			{
				return true;
			}
			int count2 = eve.SelectedItems.Count;
			bool flag = count == 1 && count2 == 1;
			switch (compareType)
			{
			case ValueCompareCheckNode.CompareType.Equal:
				if (!flag)
				{
					return count2 != count;
				}
				return true;
			case ValueCompareCheckNode.CompareType.Greater:
				return true;
			case ValueCompareCheckNode.CompareType.Less:
				return count2 < count - 1;
			case ValueCompareCheckNode.CompareType.GreaterEqual:
				return true;
			case ValueCompareCheckNode.CompareType.LessEqual:
				if (!flag)
				{
					return count2 < count;
				}
				return true;
			default:
				return false;
			}
		}
		bool ShouldCancel()
		{
			if (info.pool == null || info.pool.Count == 0)
			{
				return true;
			}
			int num = info.pool.Count((Item i) => selectable == null || selectable.CheckItemAccept(i) == true);
			if (countGenerator != null)
			{
				switch (compareType)
				{
				case ValueCompareCheckNode.CompareType.Equal:
					if (info.pool.Count < count || num < count)
					{
						return true;
					}
					break;
				case ValueCompareCheckNode.CompareType.Greater:
					if (info.pool.Count <= count || num <= count)
					{
						return true;
					}
					break;
				case ValueCompareCheckNode.CompareType.GreaterEqual:
					if (info.pool.Count < count || num < count)
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
	}

	public static SelectItemInfo GenerateSelectItemsInfo(GameEvent eve, List<Item> poolItems, Func<bool> shouldCancel, Func<Item, bool> selectable, Func<bool> confirmable, Action onConfirm, Action onCancel, Func<bool> canCancel, BattleFieldTransform selector, Func<string> rawPanelHintGetter, bool isSingleSelect, List<Item> output, Item source)
	{
		SelectItemInfo info = new SelectItemInfo
		{
			pool = poolItems,
			output = output,
			singleSelect = isSingleSelect
		};
		info.panelHintGetter = () => FormatSelectPanelHint(eve, rawPanelHintGetter(), out info.generator);
		info.shouldCancel = ShouldCancel;
		info.canConfirm = confirmable;
		info.canSelect = selectable;
		info.selectorBft = selector;
		info.onConfirm = onConfirm;
		info.onCancel = onCancel;
		info.canCancel = canCancel;
		info.selectSource = source;
		return info;
		bool ShouldCancel()
		{
			if (poolItems == null || poolItems.Count == 0)
			{
				return true;
			}
			if (shouldCancel != null && shouldCancel())
			{
				return true;
			}
			return false;
		}
	}

	private static string FormatSelectPanelHint(GameEvent selectEve, string rawHint, out TextDataGenerator textGenerator)
	{
		if (!rawHint.IsNullOrEmpty() && selectEve.GetEveObj(GameEvent.EveObjType.Caster) is IEffectObjectDisplay obj)
		{
			return TextDataPreset.FormatEffectObjectDescription(obj, rawHint, selectEve, out textGenerator);
		}
		textGenerator = null;
		return rawHint;
	}
}
