using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Il2CppDummyDll;
using Newtonsoft.Json;
using TapSDK.Compliance.Model;
using TapSDK.Core;
using TapSDK.Core.Internal.Init;

[assembly: AssemblyVersion("0.0.0.0")]
[Token(Token = "0x2000002")]
[CompilerGenerated]
[GeneratedCode("Unity.MonoScriptGenerator.MonoScriptInfoGenerator", null)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal class UnitySourceGeneratedAssemblyMonoScriptTypes_v1
{
	[Token(Token = "0x2000003")]
	private struct MonoScriptData
	{
		[Token(Token = "0x4000001")]
		[FieldOffset(Offset = "0x0")]
		public byte[] FilePathsData;

		[Token(Token = "0x4000002")]
		[FieldOffset(Offset = "0x8")]
		public byte[] TypesData;

		[Token(Token = "0x4000003")]
		[FieldOffset(Offset = "0x10")]
		public int TotalTypes;

		[Token(Token = "0x4000004")]
		[FieldOffset(Offset = "0x14")]
		public int TotalFiles;

		[Token(Token = "0x4000005")]
		[FieldOffset(Offset = "0x18")]
		public bool IsEditorOnly;
	}

	[MethodImpl((MethodImplOptions)256)]
	[Token(Token = "0x6000001")]
	[Address(RVA = "0x263F858", Offset = "0x263B858", VA = "0x263F858")]
	private static MonoScriptData Get()
	{
		return default(MonoScriptData);
	}

	[Token(Token = "0x6000002")]
	[Address(RVA = "0x263F91C", Offset = "0x263B91C", VA = "0x263F91C")]
	public UnitySourceGeneratedAssemblyMonoScriptTypes_v1()
	{
	}
}
namespace TapSDK.Compliance
{
	[Token(Token = "0x2000004")]
	public class ComplianceCodeData
	{
		[Token(Token = "0x17000001")]
		[JsonProperty("name")]
		public string Name
		{
			[Token(Token = "0x6000003")]
			[Address(RVA = "0x263F924", Offset = "0x263B924", VA = "0x263F924")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x6000004")]
			[Address(RVA = "0x263F92C", Offset = "0x263B92C", VA = "0x263F92C")]
			[CompilerGenerated]
			set
			{
			}
		}

		[Token(Token = "0x17000002")]
		[JsonProperty("idcard")]
		public string Idcard
		{
			[Token(Token = "0x6000005")]
			[Address(RVA = "0x263F934", Offset = "0x263B934", VA = "0x263F934")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x6000006")]
			[Address(RVA = "0x263F93C", Offset = "0x263B93C", VA = "0x263F93C")]
			[CompilerGenerated]
			set
			{
			}
		}

		[Token(Token = "0x17000003")]
		[JsonProperty("code")]
		public string Code
		{
			[Token(Token = "0x6000007")]
			[Address(RVA = "0x263F944", Offset = "0x263B944", VA = "0x263F944")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x6000008")]
			[Address(RVA = "0x263F94C", Offset = "0x263B94C", VA = "0x263F94C")]
			[CompilerGenerated]
			set
			{
			}
		}

		[Token(Token = "0x6000009")]
		[Address(RVA = "0x263F954", Offset = "0x263B954", VA = "0x263F954")]
		public ComplianceCodeData()
		{
		}
	}
	[Token(Token = "0x2000005")]
	public class ComplianceCodeResponse
	{
		[Token(Token = "0x17000004")]
		[JsonProperty("data")]
		public ComplianceCodeData Data
		{
			[Token(Token = "0x600000A")]
			[Address(RVA = "0x263F95C", Offset = "0x263B95C", VA = "0x263F95C")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x600000B")]
			[Address(RVA = "0x263F964", Offset = "0x263B964", VA = "0x263F964")]
			[CompilerGenerated]
			set
			{
			}
		}

		[Token(Token = "0x600000C")]
		[Address(RVA = "0x263F96C", Offset = "0x263B96C", VA = "0x263F96C")]
		public ComplianceCodeResponse()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x2000006")]
	public class CheckPayResult
	{
		[Token(Token = "0x400000A")]
		[FieldOffset(Offset = "0x10")]
		public int status;

		[Token(Token = "0x400000B")]
		[FieldOffset(Offset = "0x18")]
		public string title;

		[Token(Token = "0x400000C")]
		[FieldOffset(Offset = "0x20")]
		public string description;

		[Token(Token = "0x600000D")]
		[Address(RVA = "0x263F974", Offset = "0x263B974", VA = "0x263F974")]
		public CheckPayResult()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x2000007")]
	public class ComplianceCallbackOriginData
	{
		[Token(Token = "0x400000D")]
		[FieldOffset(Offset = "0x10")]
		public int code;

		[Token(Token = "0x400000E")]
		[FieldOffset(Offset = "0x18")]
		public string extras;

		[Token(Token = "0x600000E")]
		[Address(RVA = "0x263F97C", Offset = "0x263B97C", VA = "0x263F97C")]
		public ComplianceCallbackOriginData()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x2000008")]
	public class ComplianceCallbackData
	{
		[Token(Token = "0x400000F")]
		[FieldOffset(Offset = "0x10")]
		public int code;

		[Token(Token = "0x4000010")]
		[FieldOffset(Offset = "0x18")]
		public MsgExtraParams extras;

		[Token(Token = "0x600000F")]
		[Address(RVA = "0x263F984", Offset = "0x263B984", VA = "0x263F984")]
		public ComplianceCallbackData()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x2000009")]
	public class IdetntifyState
	{
		[Token(Token = "0x4000011")]
		[FieldOffset(Offset = "0x10")]
		public int authState;

		[Token(Token = "0x4000012")]
		[FieldOffset(Offset = "0x18")]
		public string ComplianceToken;

		[Token(Token = "0x4000013")]
		[FieldOffset(Offset = "0x20")]
		public int ageLimit;

		[Token(Token = "0x6000010")]
		[Address(RVA = "0x263F98C", Offset = "0x263B98C", VA = "0x263F98C")]
		public IdetntifyState()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x200000A")]
	public class IdentifyResult
	{
		[Token(Token = "0x4000014")]
		[FieldOffset(Offset = "0x10")]
		public int identifyState;

		[Token(Token = "0x6000011")]
		[Address(RVA = "0x263F994", Offset = "0x263B994", VA = "0x263F994")]
		public IdentifyResult()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x200000B")]
	public class MsgExtraParams
	{
		[Token(Token = "0x4000015")]
		[FieldOffset(Offset = "0x10")]
		public int userType;

		[Token(Token = "0x4000016")]
		[FieldOffset(Offset = "0x14")]
		public int strict_type;

		[Token(Token = "0x4000017")]
		[FieldOffset(Offset = "0x18")]
		public string description;

		[Token(Token = "0x4000018")]
		[FieldOffset(Offset = "0x20")]
		public string title;

		[Token(Token = "0x4000019")]
		[FieldOffset(Offset = "0x28")]
		public bool forceOpen;

		[Token(Token = "0x400001A")]
		[FieldOffset(Offset = "0x30")]
		public long remainTime;

		[Token(Token = "0x400001B")]
		[FieldOffset(Offset = "0x38")]
		public int extraSource;

		[Token(Token = "0x400001C")]
		[FieldOffset(Offset = "0x3C")]
		public int promptType;

		[Token(Token = "0x400001D")]
		[FieldOffset(Offset = "0x40")]
		public int restrictType;

		[Token(Token = "0x6000012")]
		[Address(RVA = "0x263F99C", Offset = "0x263B99C", VA = "0x263F99C")]
		public MsgExtraParams()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x200000C")]
	public class CheckPayResultParams
	{
		[Token(Token = "0x400001E")]
		[FieldOffset(Offset = "0x10")]
		public int success;

		[Token(Token = "0x400001F")]
		[FieldOffset(Offset = "0x14")]
		public int status;

		[Token(Token = "0x4000020")]
		[FieldOffset(Offset = "0x18")]
		public string title;

		[Token(Token = "0x4000021")]
		[FieldOffset(Offset = "0x20")]
		public string description;

		[Token(Token = "0x17000005")]
		public bool Success
		{
			[Token(Token = "0x6000013")]
			[Address(RVA = "0x263F9F4", Offset = "0x263B9F4", VA = "0x263F9F4")]
			get
			{
				return default(bool);
			}
		}

		[Token(Token = "0x6000014")]
		[Address(RVA = "0x263FA04", Offset = "0x263BA04", VA = "0x263FA04")]
		public CheckPayResultParams()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x200000D")]
	public class SubmitPayResultParams
	{
		[Token(Token = "0x6000015")]
		[Address(RVA = "0x263FA0C", Offset = "0x263BA0C", VA = "0x263FA0C")]
		public SubmitPayResultParams()
		{
		}
	}
	[Token(Token = "0x200000E")]
	public static class TapTapCompliance
	{
		[Token(Token = "0x4000022")]
		[FieldOffset(Offset = "0x0")]
		public static readonly string Version;

		[Token(Token = "0x6000016")]
		[Address(RVA = "0x263FA14", Offset = "0x263BA14", VA = "0x263FA14")]
		public static void RegisterComplianceCallback(Action<int, string> callback)
		{
		}

		[Token(Token = "0x6000017")]
		[Address(RVA = "0x263FB7C", Offset = "0x263BB7C", VA = "0x263FB7C")]
		public static void Startup(string userId)
		{
		}

		[Token(Token = "0x6000018")]
		[Address(RVA = "0x263FC30", Offset = "0x263BC30", VA = "0x263FC30")]
		public static void Exit()
		{
		}

		[Token(Token = "0x6000019")]
		[Address(RVA = "0x263FCD0", Offset = "0x263BCD0", VA = "0x263FCD0")]
		public static Task<int> GetAgeRange()
		{
			return null;
		}

		[Token(Token = "0x600001A")]
		[Address(RVA = "0x263FE34", Offset = "0x263BE34", VA = "0x263FE34")]
		public static Task<int> GetRemainingTime()
		{
			return null;
		}

		[Token(Token = "0x600001B")]
		[Address(RVA = "0x263FF98", Offset = "0x263BF98", VA = "0x263FF98")]
		public static Task<string> GetCurrentAccessToken()
		{
			return null;
		}

		[Token(Token = "0x600001C")]
		[Address(RVA = "0x26400FC", Offset = "0x263C0FC", VA = "0x26400FC")]
		public static void CheckPaymentLimit(long amount, Action<CheckPayResult> handleCheckPayLimit, Action<string> handleCheckPayLimitException)
		{
		}

		[Token(Token = "0x600001D")]
		[Address(RVA = "0x26401CC", Offset = "0x263C1CC", VA = "0x26401CC")]
		public static void SubmitPayment(long amount, Action handleSubmitPayResult, Action<string> handleSubmitPayResultException)
		{
		}

		[Token(Token = "0x600001E")]
		[Address(RVA = "0x264029C", Offset = "0x263C29C", VA = "0x264029C")]
		[Obsolete("该方法已失效，不需要再额外调用")]
		public static void SetTestEnvironment(bool enable)
		{
		}

		[Token(Token = "0x600001F")]
		[Address(RVA = "0x2640350", Offset = "0x263C350", VA = "0x2640350")]
		public static void OnInvokeExternalCallback(int code, string msg)
		{
		}
	}
	[Token(Token = "0x200000F")]
	public class TapTapComplianceOption : TapTapSdkBaseOptions
	{
		[Token(Token = "0x4000024")]
		[FieldOffset(Offset = "0x10")]
		[JsonProperty("moduleName")]
		private string _moduleName;

		[Token(Token = "0x4000025")]
		[FieldOffset(Offset = "0x18")]
		internal string clientId;

		[Token(Token = "0x4000026")]
		[FieldOffset(Offset = "0x20")]
		public bool showSwitchAccount;

		[Token(Token = "0x4000027")]
		[FieldOffset(Offset = "0x21")]
		public bool useAgeRange;

		[Token(Token = "0x17000006")]
		public static TapTapComplianceOption Config
		{
			[Token(Token = "0x6000021")]
			[Address(RVA = "0x264046C", Offset = "0x263C46C", VA = "0x264046C")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x6000022")]
			[Address(RVA = "0x26404B4", Offset = "0x263C4B4", VA = "0x26404B4")]
			[CompilerGenerated]
			set
			{
			}
		}

		[Token(Token = "0x17000007")]
		[JsonIgnore]
		public string moduleName
		{
			[Token(Token = "0x6000023")]
			[Address(RVA = "0x2640500", Offset = "0x263C500", VA = "0x2640500", Slot = "4")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000024")]
		[Address(RVA = "0x2640508", Offset = "0x263C508", VA = "0x2640508")]
		public TapTapComplianceOption(bool useAgeRange, bool showSwitchAccount)
		{
		}

		[Token(Token = "0x6000025")]
		[Address(RVA = "0x2640584", Offset = "0x263C584", VA = "0x2640584")]
		public TapTapComplianceOption()
		{
		}

		[Token(Token = "0x6000026")]
		[Address(RVA = "0x26405E8", Offset = "0x263C5E8", VA = "0x26405E8")]
		public Dictionary<string, object> ToDict()
		{
			return null;
		}
	}
}
namespace TapSDK.Compliance.Model
{
	[Token(Token = "0x2000010")]
	public interface IComplianceJob
	{
		[Token(Token = "0x17000008")]
		List<Action<int, string>> ExternalCallbackList
		{
			[Token(Token = "0x6000027")]
			get;
		}

		[Token(Token = "0x6000028")]
		Task<int> GetAgeRange();

		[Token(Token = "0x6000029")]
		Task<int> GetRemainingTime();

		[Token(Token = "0x600002A")]
		Task<string> GetCurrentToken();

		[Token(Token = "0x600002B")]
		void Init(string clientId, string clientToken, TapTapRegionType regionType, TapTapComplianceOption config);

		[Token(Token = "0x600002C")]
		void RegisterComplianceCallback(Action<int, string> callback);

		[Token(Token = "0x600002D")]
		void Startup(string userId);

		[Token(Token = "0x600002E")]
		void Exit();

		[Token(Token = "0x600002F")]
		void CheckPaymentLimit(long amount, Action<CheckPayResult> handleCheckPayLimit, Action<string> handleCheckPayLimitException);

		[Token(Token = "0x6000030")]
		void SubmitPayment(long amount, Action handleSubmitPayResult, Action<string> handleSubmitPayResultException);

		[Token(Token = "0x6000031")]
		void SetTestEnvironment(bool enable);

		[Token(Token = "0x6000032")]
		void OnInvokeExternalCallback(int code, string msg);
	}
	[Token(Token = "0x2000011")]
	public static class StartUpResult
	{
		[Token(Token = "0x4000028")]
		public const int LOGIN_SUCCESS = 500;

		[Token(Token = "0x4000029")]
		public const int EXITED = 1000;

		[Token(Token = "0x400002A")]
		public const int SWITCH_ACCOUNT = 1001;

		[Token(Token = "0x400002B")]
		public const int PERIOD_RESTRICT = 1030;

		[Token(Token = "0x400002C")]
		public const int DURATION_LIMIT = 1050;

		[Token(Token = "0x400002D")]
		public const int AGE_LIMIT = 1100;

		[Token(Token = "0x400002E")]
		public const int INVALID_CLIENT_OR_NETWORK_ERROR = 1200;

		[Token(Token = "0x400002F")]
		public const int REAL_NAME_STOP = 9002;

		[Token(Token = "0x6000033")]
		[Address(RVA = "0x264070C", Offset = "0x263C70C", VA = "0x264070C")]
		public static bool Contains(int val)
		{
			return default(bool);
		}
	}
}
namespace TapSDK.Compliance.Internal
{
	[Token(Token = "0x2000012")]
	internal static class ComplianceJobManager
	{
		[Token(Token = "0x4000030")]
		[FieldOffset(Offset = "0x0")]
		private static IComplianceJob _job;

		[Token(Token = "0x4000031")]
		[FieldOffset(Offset = "0x8")]
		private static bool _isInit;

		[Token(Token = "0x17000009")]
		internal static IComplianceJob Job
		{
			[Token(Token = "0x6000034")]
			[Address(RVA = "0x263FB20", Offset = "0x263BB20", VA = "0x263FB20")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x1700000A")]
		public static List<Action<int, string>> ExternalCallbackList
		{
			[Token(Token = "0x6000035")]
			[Address(RVA = "0x26408C8", Offset = "0x263C8C8", VA = "0x26408C8")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000036")]
		[Address(RVA = "0x2640964", Offset = "0x263C964", VA = "0x2640964")]
		private static IComplianceJob CreateJob(bool isNewJob)
		{
			return null;
		}

		[Token(Token = "0x6000037")]
		[Address(RVA = "0x2640778", Offset = "0x263C778", VA = "0x2640778")]
		private static void InitJob()
		{
		}

		[Token(Token = "0x6000038")]
		[Address(RVA = "0x2640DC4", Offset = "0x263CDC4", VA = "0x2640DC4")]
		internal static void Init(string clientId, string clientToken, TapTapRegionType regionType, TapTapComplianceOption config)
		{
		}

		[Token(Token = "0x6000039")]
		[Address(RVA = "0x2640EC0", Offset = "0x263CEC0", VA = "0x2640EC0")]
		internal static bool IsInit()
		{
			return default(bool);
		}
	}
}
namespace TapSDK.Compliance.Internal.Init
{
	[Token(Token = "0x2000014")]
	public class ComplianceInitTask : IInitTask
	{
		[Token(Token = "0x1700000B")]
		public int Order
		{
			[Token(Token = "0x6000042")]
			[Address(RVA = "0x264128C", Offset = "0x263D28C", VA = "0x264128C", Slot = "4")]
			get
			{
				return default(int);
			}
		}

		[Token(Token = "0x6000043")]
		[Address(RVA = "0x2641294", Offset = "0x263D294", VA = "0x2641294", Slot = "5")]
		public void Init(TapTapSdkOptions coreOption, TapTapSdkBaseOptions[] otherOptions)
		{
		}

		[Token(Token = "0x6000044")]
		[Address(RVA = "0x2641378", Offset = "0x263D378", VA = "0x2641378", Slot = "6")]
		public void Init(TapTapSdkOptions coreOption)
		{
		}

		[Token(Token = "0x6000045")]
		[Address(RVA = "0x26413E4", Offset = "0x263D3E4", VA = "0x26413E4")]
		public ComplianceInitTask()
		{
		}
	}
}
