using UnityEngine;

public class ForgetShrineItemSlot : CellUI<ForgetShrineItemSlot.Data>
{
	public class Data
	{
		public EType type;

		public int index;
	}

	public enum EType
	{
		None,
		Deck,
		ForgetShrine
	}

	[SerializeField]
	public RectTransform pivot;

	private ForgetShrinePanel forgetShrinePanel => parent as ForgetShrinePanel;

	public ItemDisplayBase itemDisplay => forgetShrinePanel.GetItemDisplay(data.type, data.index);

	public Item item => forgetShrinePanel.GetItem(data.type, data.index);

	private Data data => base.setupData;

	protected override void OnSetup(Data setupData)
	{
		base.gameObject.SetActive(value: true);
		if (item != null && itemDisplay == null)
		{
			forgetShrinePanel.itemDisplayPool.Spawn(item);
			itemDisplay.GetModule<ForgetShrineModule>().OnSet();
		}
	}

	protected override void OnUnset()
	{
		if (itemDisplay != null)
		{
			forgetShrinePanel.itemDisplayPool.pool.Despawn(itemDisplay);
		}
	}

	public void RefreshItem()
	{
		if (item != null && itemDisplay == null)
		{
			forgetShrinePanel.itemDisplayPool.Spawn(item);
			itemDisplay.GetModule<ForgetShrineModule>().OnSet();
		}
	}
}
