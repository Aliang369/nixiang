using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyItemArea : UIBase
{
	[SerializeField]
	private EnemyItemDisplay itemDisplayPrefab;

	[SerializeField]
	private Transform itemSlotDisplayParent;

	[SerializeField]
	public RectTransform _itemSpawnPivot;

	[SerializeField]
	public Image ringImage;

	[SerializeField]
	public Transform ringImagePivot;

	[SerializeField]
	public Image itemEffectImage;

	[SerializeField]
	public Transform defaultPivot;

	[SerializeField]
	public Image enemyAreaBG;

	[NonSerialized]
	public List<EnemyItemDisplay> itemDisplays;

	[NonSerialized]
	private CellUIPrefabPool<EnemyItemDisplay, Item> itemSlotDisplayPool;

	[NonSerialized]
	public ConvertablePivot itemSpawnPivot;

	[NonSerialized]
	private bool blockIndexChange;

	protected override void Init()
	{
		itemDisplays = new List<EnemyItemDisplay>();
		if (ValidateRequiredReferences())
		{
			itemSpawnPivot = new ConvertablePivot(_itemSpawnPivot);
			itemDisplayPrefab.gameObject.SetActive(value: false);
			itemSlotDisplayPool = GetPrefabPool<EnemyItemDisplay, Item>(itemDisplayPrefab, itemSlotDisplayParent);
			UIBase.gameState.actionManager.actionFrameFinishEvent += SetDirty;
			if (enemyAreaBG != null)
			{
				enemyAreaBG.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void Dispose()
	{
		DespawnAllItemDisplays();
		UIBase.gameState.actionManager.actionFrameFinishEvent -= SetDirty;
	}

	protected override void UpdateDisplay()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshSlotDisplays();
		}
	}

	public void Setup()
	{
		if (UIBase.gameState.battleState.enemy.BftDisplay.enemyTablePivot != null)
		{
			base.transform.position = UIBase.gameState.battleState.enemy.BftDisplay.enemyTablePivot.position;
		}
		else if (defaultPivot != null)
		{
			base.transform.position = defaultPivot.position;
		}
		blockIndexChange = false;
	}

	public void Unset()
	{
		DespawnAllItemDisplays();
		if (enemyAreaBG != null)
		{
			enemyAreaBG.gameObject.SetActive(value: false);
		}
	}

	public EnemyItemDisplay GetItemDisplay(Item item)
	{
		if (itemDisplays == null)
		{
			return null;
		}
		return itemDisplays.Find((EnemyItemDisplay display) => display.item == item);
	}

	public void OnSpawnItem(Item item, Item source)
	{
		if (CanSpawnItemDisplay(item))
		{
			UIBase.gameState.StartCoroutine(() => _OnSpawnItem(item, source));
		}
		IEnumerator _OnSpawnItem(Item item2, Item source2)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.AddItem);
			EnemyItemDisplay enemyItemDisplay = SpawnItemDisplay(item2);
			if (enemyItemDisplay == null)
			{
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.AddItem);
			}
			else
			{
				IEnumerator enumerator = enemyItemDisplay.stateModule.OnAddNewItem(source2);
				if (!UIBase.gameState.actionManager.skipFrameAnim)
				{
					yield return enumerator;
					yield return new WaitUntil(() => !HasAnim());
				}
				else
				{
					enemyItemDisplay.stateModule.SkipMutualExcludeAnim();
				}
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.AddItem);
			}
		}
	}

	public void OnAddItem(Item item, Vector3? fromPosition)
	{
		if (CanSpawnItemDisplay(item))
		{
			UIBase.gameState.StartCoroutine(() => _OnAddItem(item, fromPosition));
		}
		IEnumerator _OnAddItem(Item item2, Vector3? fromPosition2)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.AddItem);
			EnemyItemDisplay enemyItemDisplay = SpawnItemDisplay(item2);
			if (enemyItemDisplay == null)
			{
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.AddItem);
			}
			else
			{
				IEnumerator enumerator = enemyItemDisplay.stateModule.OnAddItem(fromPosition2);
				if (!UIBase.gameState.actionManager.skipFrameAnim)
				{
					yield return enumerator;
					yield return new WaitUntil(() => !HasAnim());
				}
				else
				{
					enemyItemDisplay.stateModule.SkipMutualExcludeAnim();
				}
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.AddItem);
			}
		}
	}

	public void OnDraw(Item item)
	{
		if (CanSpawnItemDisplay(item))
		{
			UIBase.gameState.StartCoroutine(() => _OnDraw(item));
		}
		IEnumerator _OnDraw(Item item2)
		{
			if (enemyAreaBG != null)
			{
				enemyAreaBG.gameObject.SetActive(value: true);
			}
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.DrawFromPile);
			EnemyItemDisplay enemyItemDisplay = SpawnItemDisplay(item2);
			if (enemyItemDisplay == null)
			{
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.DrawFromPile);
			}
			else
			{
				IEnumerator enumerator = enemyItemDisplay.stateModule.OnDraw();
				if (!UIBase.gameState.actionManager.skipFrameAnim)
				{
					yield return enumerator;
					yield return new WaitUntil(() => !HasAnim());
				}
				else
				{
					enemyItemDisplay.stateModule.SkipMutualExcludeAnim();
				}
				UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.DrawFromPile);
			}
		}
	}

	public void OnTransferOut(Item item)
	{
		EnemyItemDisplay itemDisplay = GetItemDisplay(item);
		if (!(itemDisplay == null))
		{
			itemDisplays.Remove(itemDisplay);
			itemDisplay.enableUpdate = false;
			itemSlotDisplayPool?.Despawn(itemDisplay);
			SetDirty();
		}
	}

	public void OnRemoveItem(Item item)
	{
		if (item == null)
		{
			return;
		}
		EnemyItemDisplay display = GetItemDisplay(item);
		if (!(display == null))
		{
			blockIndexChange = true;
			UIBase.gameState.StartCoroutine(() => _OnRemoveItem(display));
		}
		IEnumerator _OnRemoveItem(EnemyItemDisplay enemyItemDisplay)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
			blockIndexChange = true;
			itemDisplays.Remove(enemyItemDisplay);
			enemyItemDisplay.enableUpdate = false;
			IEnumerator enumerator = enemyItemDisplay.stateModule.OnRemove();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			itemSlotDisplayPool?.Despawn(enemyItemDisplay);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return new WaitUntil(() => !HasAnim());
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
			blockIndexChange = false;
		}
	}

	public void OnExileItem(Item item)
	{
		if (item == null)
		{
			return;
		}
		EnemyItemDisplay display = GetItemDisplay(item);
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => _OnExileItem(display));
		}
		IEnumerator _OnExileItem(EnemyItemDisplay enemyItemDisplay)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.RemoveItem);
			itemDisplays.Remove(enemyItemDisplay);
			enemyItemDisplay.enableUpdate = false;
			IEnumerator enumerator = enemyItemDisplay.stateModule.OnExile();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			itemSlotDisplayPool?.Despawn(enemyItemDisplay);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return new WaitUntil(() => !HasAnim());
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.RemoveItem);
		}
	}

	public void OnItemSpell(Item item)
	{
		if (item == null)
		{
			return;
		}
		EnemyItemDisplay display = GetItemDisplay(item);
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => _OnItemSpell(display));
		}
		IEnumerator _OnItemSpell(EnemyItemDisplay enemyItemDisplay)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
			IEnumerator enumerator = enemyItemDisplay.stateModule.OnSpell();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			SetDirty();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return new WaitUntil(() => !HasAnim());
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
			SetDirty();
		}
	}

	public void OnItemTransferFromPlayer(Item item)
	{
		if (CanSpawnItemDisplay(item))
		{
			UIBase.gameState.StartCoroutine(() => _OnItemTransferFromPlayer(item));
		}
		IEnumerator _OnItemTransferFromPlayer(Item item2)
		{
			EnemyItemDisplay enemyItemDisplay = SpawnItemDisplay(item2);
			if (!(enemyItemDisplay == null))
			{
				enemyItemDisplay.gameObject.SetActive(value: false);
			}
			yield break;
		}
	}

	public void OnItemTransferToPlayer(Item item)
	{
		if (item == null)
		{
			return;
		}
		EnemyItemDisplay display = GetItemDisplay(item);
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => _OnItemTransferToPlayer(display));
		}
		IEnumerator _OnItemTransferToPlayer(EnemyItemDisplay enemyItemDisplay)
		{
			itemDisplays.Remove(enemyItemDisplay);
			enemyItemDisplay.enableUpdate = false;
			itemSlotDisplayPool?.Despawn(enemyItemDisplay);
			yield break;
		}
	}

	public void SkipTransferFromPlayerAnim(Item item)
	{
		EnemyItemDisplay itemDisplay = GetItemDisplay(item);
		if (!(itemDisplay == null))
		{
			itemDisplay.stateModule.SkipMutualExcludeAnim();
			SetDirty();
		}
	}

	public void OnItemAbilityTrigger(Item item, Ability.AbilityAnimPriority priority)
	{
		if (item == null)
		{
			return;
		}
		EnemyItemDisplay display = GetItemDisplay(item);
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => _OnItemAbilityTrigger(display, priority));
		}
		static IEnumerator _OnItemAbilityTrigger(EnemyItemDisplay enemyItemDisplay, Ability.AbilityAnimPriority priority2)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Spell);
			IEnumerator enumerator = enemyItemDisplay.stateModule.OnItemAbilityTrigger(priority2);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Spell);
		}
	}

	public void OnItemAbilityTriggerEnd(Item item)
	{
		if (item == null)
		{
			return;
		}
		EnemyItemDisplay display = GetItemDisplay(item);
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => _OnItemAbilityTriggerEnd(display));
		}
		static IEnumerator _OnItemAbilityTriggerEnd(EnemyItemDisplay enemyItemDisplay)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Spell);
			IEnumerator enumerator = enemyItemDisplay.stateModule.OnItemAbilityTriggerEnd();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				enemyItemDisplay.stateModule.SkipMutualExcludeAnim();
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Spell);
		}
	}

	public void OnConvertItem(Item item)
	{
		if (item != null && !(GetItemDisplay(item) == null))
		{
			UIBase.gameState.StartCoroutine(() => ConvertItem(item));
		}
		IEnumerator ConvertItem(Item item2)
		{
			EnemyItemDisplay itemDisplay = GetItemDisplay(item2);
			itemDisplay.Unset();
			itemDisplay.Setup(item2);
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
			IEnumerator enumerator = itemDisplay.stateModule.OnConvert();
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				yield return enumerator;
			}
			else
			{
				itemDisplay.stateModule.SkipMutualExcludeAnim();
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		}
	}

	public void OnShuffle(Action onAnimEnd)
	{
		UIBase.gameState.StartCoroutine(Shuffle);
		IEnumerator Shuffle()
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.ShuffleTable);
			if (!UIBase.gameState.actionManager.skipFrameAnim)
			{
				List<WrapperEnumerator> list = new List<WrapperEnumerator>();
				foreach (EnemyItemDisplay itemDisplay in itemDisplays)
				{
					list.Add(UIBase.gameState.StartCoroutine(itemDisplay.stateModule.OnShuffle));
				}
				yield return WrapperEnumerator.WaitAll(list);
			}
			else
			{
				foreach (EnemyItemDisplay itemDisplay2 in itemDisplays)
				{
					itemDisplay2.stateModule.SkipMutualExcludeAnim();
				}
			}
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.ShuffleTable);
			onAnimEnd?.Invoke();
			SetDirty();
		}
	}

	private void RefreshSlotDisplays()
	{
		if (itemDisplays != null && !blockIndexChange)
		{
			itemDisplays.Sort((EnemyItemDisplay i, EnemyItemDisplay i2) => -i.stateModule.currentIndex.CompareTo(i2.stateModule.currentIndex));
			for (int num = 0; num < itemDisplays.Count; num++)
			{
				itemDisplays[num].transform.SetSiblingIndex(itemDisplays.Count - num - 1);
			}
		}
	}

	private bool HasAnim()
	{
		if (itemDisplays == null)
		{
			return false;
		}
		for (int i = 0; i < itemDisplays.Count; i++)
		{
			if (itemDisplays[i] != null && itemDisplays[i].stateModule.HasAnim())
			{
				return true;
			}
		}
		return false;
	}

	private bool ValidateRequiredReferences()
	{
		if (itemDisplayPrefab != null && _itemSpawnPivot != null)
		{
			return true;
		}
		if (itemDisplayPrefab == null)
		{
			Debug.LogError("EnemyItemArea on " + base.gameObject.name + " is missing itemDisplayPrefab. Bind ItemDisplay_Enemy.prefab's EnemyItemDisplay component.", this);
		}
		if (_itemSpawnPivot == null)
		{
			Debug.LogError("EnemyItemArea on " + base.gameObject.name + " is missing _itemSpawnPivot.", this);
		}
		return false;
	}

	private bool CanSpawnItemDisplay(Item item)
	{
		if (item == null || GetItemDisplay(item) != null)
		{
			return false;
		}
		if (itemSlotDisplayPool != null)
		{
			return true;
		}
		Debug.LogError("EnemyItemArea on " + base.gameObject.name + " has no item display pool. Check the itemDisplayPrefab binding and initialization log.", this);
		return false;
	}

	private EnemyItemDisplay SpawnItemDisplay(Item item)
	{
		if (!CanSpawnItemDisplay(item))
		{
			return null;
		}
		EnemyItemDisplay enemyItemDisplay = itemSlotDisplayPool.Spawn(item);
		if (enemyItemDisplay == null)
		{
			Debug.LogError(string.Format("{0} failed to spawn {1} for {2}. Check {3}.", "EnemyItemArea", "EnemyItemDisplay", item, "itemDisplayPrefab"), this);
			return null;
		}
		if (itemDisplays == null)
		{
			itemDisplays = new List<EnemyItemDisplay>();
		}
		itemDisplays.Add(enemyItemDisplay);
		return enemyItemDisplay;
	}

	private void DespawnAllItemDisplays()
	{
		if (itemDisplays == null)
		{
			return;
		}
		foreach (EnemyItemDisplay item in new List<EnemyItemDisplay>(itemDisplays))
		{
			itemSlotDisplayPool?.Despawn(item);
		}
		itemDisplays.Clear();
	}

	public void ConfigTransferFromPlayerTween1(Sequence sequence, Item item)
	{
		EnemyItemDisplay itemDisplay = GetItemDisplay(item);
		if (!(itemDisplay == null))
		{
			itemDisplay.stateModule.ConfigTween_TransferFromPlayer1(sequence);
		}
	}

	public void ConfigTransferFromPlayerTween2(Sequence sequence, Item item)
	{
		EnemyItemDisplay itemDisplay = GetItemDisplay(item);
		if (!(itemDisplay == null))
		{
			itemDisplay.stateModule.ConfigTween_TransferFromPlayer2(sequence);
		}
	}
}
