using Proto.Enums;
using UnityEngine;

public class zbNetUrl
{
	private static readonly string apiServer_Dev = "http://192.168.2.37:8000";

	private static readonly string reportServer_Dev = "http://192.168.2.25:9066/client_report";

	private static readonly EPlatformType playformType_Dev = EPlatformType.Dev;

	private static readonly string apiServer_Test = "http://192.168.2.25:8070";

	private static readonly string reportServer_Test = "http://192.168.2.25:9076/client_report";

	private static readonly EPlatformType playformType_Test = EPlatformType.Test;

	private static readonly string apiServer_Eva = "https://blfx.mydreamjoy.com/api_eva";

	private static readonly string reportServer_Eva = "https://blfx.mydreamjoy.com/report/client_report";

	private static readonly EPlatformType playformType_Eva = EPlatformType.Eva;

	private static readonly string apiServer_Demo = "https://blfx.mydreamjoy.com/api_demo";

	private static readonly string reportServer_Demo = "https://blfx.mydreamjoy.com/report/client_report";

	private static readonly EPlatformType playformType_Demo = EPlatformType.Demo;

	private static readonly string apiServer_Ga1 = "https://blfx.mydreamjoy.com/api";

	private static readonly string reportServer_Ga1 = "https://blfx.mydreamjoy.com/report/client_report";

	private static readonly EPlatformType playformType_Ga1 = EPlatformType.Ga1;

	public static string apiServer_url { get; private set; }

	public static string reportServer_url { get; private set; }

	public static EPlatformType playformType { get; private set; }

	public static string visitorPost_url { get; private set; }

	public static string visitorRegPost_url { get; private set; }

	public static string loginPost_url { get; private set; }

	public static void InitUrl()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		RuntimePlatform platform = Application.platform;
		if ((int)platform != 8)
		{
			if ((int)platform == 11)
			{
				apiServer_url = apiServer_Ga1;
				reportServer_url = reportServer_Ga1;
				playformType = playformType_Ga1;
			}
			else
			{
				apiServer_url = apiServer_Test;
				reportServer_url = reportServer_Test;
				playformType = playformType_Test;
			}
		}
		else
		{
			apiServer_url = apiServer_Ga1;
			reportServer_url = reportServer_Ga1;
			playformType = playformType_Ga1;
		}
		apiServer_url += "/api";
		InitZBApiUrl();
	}

	public static void InitZBApiUrl()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		string text = "";
		text = (((int)Application.platform != 8) ? "http://218.244.143.229:7000" : "http://user.zbjoy.com");
		visitorPost_url = text + "/user/fast_reg1";
		visitorRegPost_url = text + "/user/fast_reg2";
		loginPost_url = text + "/user/login";
	}
}
