using System.Collections.Generic;
using CoreKit.Wrapper;
using SolarEngine;
using UnityEngine;

public static class SolarEngineAdapter
{
	private static bool CanReport => !Application.isEditor;

	private static bool DebugLog
	{
		get
		{
			if (!Application.isEditor)
			{
				return GlobalConfigManager.instance.debugLogTA;
			}
			return true;
		}
	}

	public static void PreInit()
	{
		if (DebugLog)
		{
			Debug.Log("[SolarEngine] preInitSeSdk");
		}
		if (CanReport)
		{
			SolarEngineWrapper.preInitSeSdk((string)null);
		}
	}

	public static void Init(bool logEnabled)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (DebugLog)
		{
			Debug.Log($"[SolarEngine] Init logEnabled={logEnabled}");
		}
		if (CanReport)
		{
			SEConfig val = default(SEConfig);
			((SEConfig)(ref val)).isImeiEnabled = false;
			((SEConfig)(ref val)).logEnabled = logEnabled;
			SolarEngineWrapper.Init(val, (string)null, (string)null);
		}
	}

	public static void SetSuperProperty(string key, object value)
	{
		if (DebugLog)
		{
			Debug.Log($"[SolarEngine] setSuperProperties {key}={value}");
		}
		if (CanReport)
		{
			SolarEngineWrapper.setSuperProperties(new Dictionary<string, object> { { key, value } });
		}
	}

	public static void TrackLogin(string loginType, string status)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (DebugLog)
		{
			Debug.Log("[SolarEngine] trackLogin type=" + loginType + " status=" + status);
		}
		if (CanReport)
		{
			LoginAttributes val = default(LoginAttributes);
			((LoginAttributes)(ref val)).login_type = loginType;
			((LoginAttributes)(ref val)).login_status = status;
			SolarEngineWrapper.trackLogin(val);
		}
	}

	public static void TrackRegister(string registerType, string status)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (DebugLog)
		{
			Debug.Log("[SolarEngine] trackRegister type=" + registerType + " status=" + status);
		}
		if (CanReport)
		{
			RegisterAttributes val = default(RegisterAttributes);
			((RegisterAttributes)(ref val)).register_type = registerType;
			((RegisterAttributes)(ref val)).register_status = status;
			SolarEngineWrapper.trackRegister(val);
		}
	}

	public static void TrackPurchase(string productName, string productId, int priceCents, bool success, string orderId, string currency, string payType)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (DebugLog)
		{
			Debug.Log($"[SolarEngine] trackPurchase name={productName} id={productId} priceCents={priceCents} success={success} order={orderId} currency={currency}");
		}
		if (CanReport)
		{
			ProductsAttributes val = default(ProductsAttributes);
			((ProductsAttributes)(ref val)).product_name = productName;
			((ProductsAttributes)(ref val)).product_id = productId;
			((ProductsAttributes)(ref val)).product_num = 1;
			((ProductsAttributes)(ref val)).pay_amount = (float)priceCents / 100f;
			((ProductsAttributes)(ref val)).paystatus = (PayStatus)(success ? 1 : 2);
			((ProductsAttributes)(ref val)).currency_type = currency;
			((ProductsAttributes)(ref val)).order_id = orderId;
			ProductsAttributes val2 = val;
			if (!string.IsNullOrEmpty(payType))
			{
				((ProductsAttributes)(ref val2)).pay_type = payType;
			}
			SolarEngineWrapper.trackPurchase(val2);
		}
	}
}
