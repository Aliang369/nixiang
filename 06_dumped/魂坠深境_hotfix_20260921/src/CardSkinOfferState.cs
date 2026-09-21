using System;
using System.Collections.Generic;

public sealed class CardSkinOfferState
{
	public readonly List<CardSkinOfferSlot> slots = new List<CardSkinOfferSlot>();

	public int dailyPeriod = int.MinValue;

	public int refreshRevision;

	public DateTime nextDailyRefreshAtUtc;

	public MatterInfo refreshCost;

	public bool canRefresh;

	public bool hasAnyOffer;

	public bool loaded;
}
