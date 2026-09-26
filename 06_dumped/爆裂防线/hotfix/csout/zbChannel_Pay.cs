using Newtonsoft.Json;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbChannel_Pay
{
	public static bool m_bPaying;

	private static float startPayTime;

	public static int curPayGoodsId;

	public static void OnPay(int goodsId)
	{
		if (zbSelf.Instance.moneyHandler.GetCountByTid(3) == 0L)
		{
			OnPayWithMoney(goodsId);
			return;
		}
		bool flag = false;
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(goodsId);
		if (recordByKey != null)
		{
			flag = zbSelf.Instance.moneyHandler.HasEnoughMoney(3, recordByKey.voucherPrice);
			if (recordByKey.referId != 0)
			{
				zbVipShopCsvRecord recordByKey2 = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(recordByKey.referId);
				if (recordByKey2 != null)
				{
					flag = zbSelf.Instance.moneyHandler.HasEnoughMoney(3, recordByKey2.voucherPrice);
				}
			}
		}
		if (zbChannel_Login.IsOpenTSCheck() || !flag)
		{
			OnPayWithMoney(goodsId);
		}
		else
		{
			zbUIMgr.Instance.ShowPayFunc(goodsId);
		}
	}

	public static void OnPayWithMoney(int goodsId)
	{
		if (m_bPaying)
		{
			return;
		}
		m_bPaying = true;
		startPayTime = Time.time;
		curPayGoodsId = goodsId;
		if (zbChannelUtil.IsNeedIdentityCheck())
		{
			int key = goodsId;
			float num = 0f;
			zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(key);
			if (recordByKey != null && recordByKey.referId != 0)
			{
				key = recordByKey.referId;
			}
			zbVipGoodsPriceCsvRecord recordByKey2 = zbDataConfig.Instance.vipGoodsPriceCsvTable.GetRecordByKey(key);
			if (recordByKey2 != null)
			{
				num = recordByKey2.price;
			}
			zbSdkHelper.PaymentCheck($"{num * 100f}");
		}
		else
		{
			OnCreateOrderReq(goodsId);
		}
	}

	public static void OnCreateOrderReq(int goodsId)
	{
		if (zbLoginUtil.appChannel == EChannelType.AliGames || zbLoginUtil.appChannel == EChannelType.Vivo)
		{
			zbWsMsgHandler.Instance.Pay_CreateOrderSign_Req(goodsId);
		}
		else
		{
			zbWsMsgHandler.Instance.Pay_CreateOrder_Req(goodsId);
		}
	}

	public static void OnPayCheckAck()
	{
		OnCreateOrderReq(curPayGoodsId);
	}

	public static void OnCreateOrderAck(PayCreateOrderAck ack)
	{
		string itemType = "1";
		if (zbLoginUtil.appChannel == EChannelType.Ios)
		{
			string iapIdOrWarn = GetIapIdOrWarn(ack.GoodsId);
			if (iapIdOrWarn != null)
			{
				var anon = new
				{
					iapId = iapIdOrWarn,
					cpOrderId = ack.OrderId
				};
				zbUIMgr.Instance.ShowVipPaying();
				string goodsInfo = JsonConvert.SerializeObject((object)anon);
				zbSdkHelper.Pay(itemType, goodsInfo);
			}
		}
		else if (zbLoginUtil.appChannel == EChannelType.XiaoMi)
		{
			string iapIdOrWarn2 = GetIapIdOrWarn(ack.GoodsId);
			if (iapIdOrWarn2 != null)
			{
				string cpUserInfo = $"{ack.OrderId};{ack.GoodsId};{ack.Price};";
				string goodsInfo2 = JsonConvert.SerializeObject((object)new
				{
					cpOrderId = ack.OrderId,
					productCode = iapIdOrWarn2,
					cpUserInfo = cpUserInfo
				});
				zbSdkHelper.Pay(itemType, goodsInfo2);
			}
		}
		else if (zbLoginUtil.appChannel == EChannelType.M4399)
		{
			string ext = $"{ack.OrderId};{ack.GoodsId};{ack.Price};";
			string text = zbSelf.Instance.vipHandler.GetGoodsName(ack.GoodsId);
			if (string.IsNullOrEmpty(text))
			{
				text = $"good_{ack.GoodsId}";
			}
			string goodsInfo3 = JsonConvert.SerializeObject((object)new
			{
				goodsName = text,
				money = ack.Price,
				cpOrderId = ack.OrderId,
				ext = ext
			});
			zbSdkHelper.Pay(itemType, goodsInfo3);
		}
		else if (zbLoginUtil.appChannel == EChannelType.TapTap || zbLoginUtil.appChannel == EChannelType.Dyunion)
		{
			string iapIdOrWarn3 = GetIapIdOrWarn(ack.GoodsId);
			if (iapIdOrWarn3 != null)
			{
				string ext2 = $"{ack.OrderId};{ack.GoodsId};{ack.Price};";
				string goodsInfo4 = JsonConvert.SerializeObject((object)new
				{
					iapId = iapIdOrWarn3,
					ext = ext2
				});
				zbSdkHelper.Pay(itemType, goodsInfo4);
			}
		}
		else if (zbLoginUtil.appChannel == EChannelType.Hykb)
		{
			string ext3 = $"{ack.OrderId};{ack.GoodsId};{ack.Price};";
			string text2 = zbSelf.Instance.vipHandler.GetGoodsName(ack.GoodsId);
			if (string.IsNullOrEmpty(text2))
			{
				text2 = $"good_{ack.GoodsId}";
			}
			string goodsInfo5 = JsonConvert.SerializeObject((object)new
			{
				goodsName = text2,
				money = ack.Price,
				server = zbSelf.Instance.GetServerId(),
				cpOrderId = ack.OrderId,
				ext = ext3
			});
			zbSdkHelper.Pay(itemType, goodsInfo5);
		}
	}

	public static void OnCreateOrderSignAck(PayCreateOrderSignAck ack)
	{
		string goodsInfo = JsonConvert.SerializeObject((object)new { ack.OrderId, ack.GoodsId, ack.CCY, ack.Price, ack.OrderTm, ack.Verify, ack.Extra });
		zbSdkHelper.Pay("1", goodsInfo);
	}

	private static string GetIapIdOrWarn(int goodsId)
	{
		string iapIdByGoodsId = zbSelf.Instance.vipHandler.GetIapIdByGoodsId((int)zbLoginUtil.appChannel, goodsId);
		if (string.IsNullOrEmpty(iapIdByGoodsId))
		{
			Debug.Log((object)"zbtest  iapId get empty");
			return null;
		}
		return iapIdByGoodsId;
	}

	public static void OnPayCallback_SDK(string strParams)
	{
		m_bPaying = false;
		zbVipPaying zbVipPaying2 = zbUIMgr.Instance.GetWndById(EWndId.VipPaying) as zbVipPaying;
		if ((Object)(object)zbVipPaying2 != (Object)null)
		{
			zbVipPaying2.Hide();
		}
	}

	private static PayVerifyOrderReq OnPayCallback_IOS(zbPayBackParams payBackParams)
	{
		int goodsIdByIapId = zbSelf.Instance.vipHandler.GetGoodsIdByIapId((int)zbLoginUtil.appChannel, payBackParams.iapGoodsId);
		if (goodsIdByIapId == 0)
		{
			return null;
		}
		float floatPriceByGoodsId = zbSelf.Instance.vipHandler.GetFloatPriceByGoodsId(goodsIdByIapId);
		PayVerifyOrderReq payVerifyOrderReq = new PayVerifyOrderReq();
		payVerifyOrderReq.AppChannel = zbLoginUtil.appChannel;
		payVerifyOrderReq.GoodsId = goodsIdByIapId;
		payVerifyOrderReq.OrderId = payBackParams.gameOrderId;
		payVerifyOrderReq.Price = floatPriceByGoodsId;
		payVerifyOrderReq.OrderTm = 0L;
		payVerifyOrderReq.Data = payBackParams.iapGoodsId + ";" + payBackParams.orderId + ";" + payBackParams.environment + ";";
		return payVerifyOrderReq;
	}

	private static void OnPayVerify2(zbPayBackParams payBackParams)
	{
		PayVerifyOrderV2Req payVerifyOrderV2Req = new PayVerifyOrderV2Req();
		payVerifyOrderV2Req.AppChannel = zbLoginUtil.appChannel;
		payVerifyOrderV2Req.Data = payBackParams.iapGoodsId + ";" + payBackParams.orderId + ";" + payBackParams.purchaseToken;
		zbWsMsgHandler.Instance.Pay_VerifyOrderV2_Req(payVerifyOrderV2Req);
	}

	private static PayVerifyOrderReq OnPayCallback_Common(zbPayBackParams payBackParams)
	{
		if (string.IsNullOrEmpty(payBackParams.gameOrderId))
		{
			OnPayVerify2(payBackParams);
			return null;
		}
		int goodsIdByIapId = zbSelf.Instance.vipHandler.GetGoodsIdByIapId((int)zbLoginUtil.appChannel, payBackParams.iapGoodsId);
		if (goodsIdByIapId == 0)
		{
			return null;
		}
		float floatPriceByGoodsId = zbSelf.Instance.vipHandler.GetFloatPriceByGoodsId(goodsIdByIapId);
		string data = payBackParams.iapGoodsId + ";" + payBackParams.orderId + ";" + payBackParams.purchaseToken + ";";
		return new PayVerifyOrderReq
		{
			AppChannel = zbLoginUtil.appChannel,
			GoodsId = goodsIdByIapId,
			OrderId = payBackParams.gameOrderId,
			Price = floatPriceByGoodsId,
			OrderTm = 0L,
			Data = data
		};
	}

	public static void Update()
	{
		if (m_bPaying && Time.time - startPayTime >= 3f)
		{
			m_bPaying = false;
		}
	}
}
