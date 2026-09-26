using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine;

public class zbSdkHelper
{
	private static int m_channel = 60001;

	public static int appSdkVersion = 0;

	public static string fcmToken;

	private static string m_userId;

	private static string m_userToken;

	private static int m_loginChannel;

	private static bool m_bloginState = false;

	private static zbSDKBridge s_bridge;

	public static void InitParams()
	{
		string text = "";
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			text = sDKBridge.GetInitParams();
		}
		if (!string.IsNullOrEmpty(text))
		{
			JToken obj = JToken.Parse(text);
			if (int.TryParse(obj.Value<string>((object)"appChannel"), out var result))
			{
				m_channel = result;
			}
			if (int.TryParse(obj.Value<string>((object)"appSdkVersion"), out var result2))
			{
				appSdkVersion = result2;
			}
			fcmToken = obj.Value<string>((object)"fcmToken") ?? "";
		}
	}

	public static string GetResVersion()
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			return sDKBridge.GetResVertion();
		}
		return string.Empty;
	}

	public static int GetChannel()
	{
		return m_channel;
	}

	public static void SetLoginState(bool state)
	{
		m_bloginState = state;
	}

	public static bool GetLoginState()
	{
		return m_bloginState;
	}

	public static string GetUserId()
	{
		return m_userId;
	}

	public static int GetLoginChannel()
	{
		return m_loginChannel;
	}

	public static string GetUserToken()
	{
		return m_userToken;
	}

	public static void SetLoginUserData(string uid, string token, int lChannel)
	{
		zbUIMgr.Instance.BubbleInfo("登录成功");
		m_userId = uid;
		m_userToken = token;
		m_loginChannel = lChannel;
		zbChannel_Login.OnUserLoginReq();
	}

	public static void LoginGame()
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.LoginGame("");
		}
	}

	public static void SwitchAccount()
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.SwitchAccount();
		}
	}

	public static void IdentityCheck(string uuid)
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.IdentityCheck(uuid);
		}
	}

	public static void PaymentCheck(string amount)
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.PaymentCheck(amount);
		}
	}

	public static void OnAutopatchState(string state)
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.OnAutopatchState(state);
		}
	}

	public static void Pay(string itemType, string goodsInfo)
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.Pay(itemType, goodsInfo);
		}
	}

	public static void ReportGameData(string eventType, string eventParams)
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.ReportGameData(eventType, eventParams);
		}
	}

	public static void Option(string option, string optionParams)
	{
		zbSDKBridge sDKBridge = GetSDKBridge();
		if ((Object)(object)sDKBridge != (Object)null)
		{
			sDKBridge.Option(option, optionParams);
		}
	}

	public static void OpenURL(string url)
	{
		if (!(zbAppLifecycleHandler.Instance.GetForegroundElapsedSeconds() <= 1f))
		{
			Application.OpenURL(url);
		}
	}

	public static void DoShare(string title = "", string contents = "")
	{
		((MonoBehaviour)zbAppLifecycleHandler.Instance).StartCoroutine(DoShareCoroutine(title, contents));
	}

	public static void OpenWxKf(string kfurl)
	{
		Option("openwxkf", kfurl);
	}

	private static IEnumerator DoShareCoroutine(string title, string contents)
	{
		yield return (object)new WaitForEndOfFrame();
		if (string.IsNullOrEmpty(title))
		{
			title = "爆裂防线｜构筑末日防线，迎战无尽尸潮";
		}
		if (string.IsNullOrEmpty(contents))
		{
			contents = "尸潮对轰，双向对战，末日废土 Roguelike 塔防射击手游！随机技能自由搭配，火力横扫海量僵尸，每一局都是全新生存挑战。快来建立你的防线！";
		}
		string optionParams;
		if (zbLoginUtil.appChannel != EChannelType.TapTap && zbLoginUtil.appChannel != EChannelType.Ios)
		{
			optionParams = ((zbLoginUtil.appChannel != EChannelType.Hykb) ? "" : JsonConvert.SerializeObject((object)new
			{
				imagePath = zbUtil.CaptureScreenshot(),
				id = 73205,
				contents = contents
			}));
		}
		else
		{
			string imagePath = zbUtil.CaptureScreenshot();
			string failUrl = "https://www.taptap.cn/app/895385";
			optionParams = JsonConvert.SerializeObject((object)new { imagePath, title, contents, failUrl });
		}
		Option("share", optionParams);
	}

	public static zbSDKBridge GetSDKBridge()
	{
		if ((Object)(object)s_bridge == (Object)null)
		{
			GameObject val = GameObject.Find("SDKBridgeObject");
			if ((Object)(object)val != (Object)null)
			{
				s_bridge = val.GetComponent<zbSDKBridge>();
			}
		}
		return s_bridge;
	}
}
