public sealed class CardSkinOfferSlot
{
	public int slotIndex;

	public ItemSkinSetBatch batch;

	public ItemSkinDef itemSkin;

	public bool soldOut => itemSkin == null;
}
