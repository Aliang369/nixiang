using System;
using ChillyRoom.Services.ContainerOfSoul;

public class PurchaseRecord
{
	[SaveField(null, null)]
	public string orderId;

	[SaveField(null, null)]
	public int quantity;

	[SaveField(null, null)]
	public DateTime purchaseDateTime;

	[SaveField(null, null)]
	public OrderStatus orderStatus;
}
