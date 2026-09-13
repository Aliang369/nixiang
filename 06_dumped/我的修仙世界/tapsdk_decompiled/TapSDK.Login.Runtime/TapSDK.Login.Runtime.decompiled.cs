using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Il2CppDummyDll;
using JetBrains.Annotations;
using Newtonsoft.Json;
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
	[Address(RVA = "0x2661A8C", Offset = "0x265DA8C", VA = "0x2661A8C")]
	private static MonoScriptData Get()
	{
		return default(MonoScriptData);
	}

	[Token(Token = "0x6000002")]
	[Address(RVA = "0x2661B50", Offset = "0x265DB50", VA = "0x2661B50")]
	public UnitySourceGeneratedAssemblyMonoScriptTypes_v1()
	{
	}
}
namespace TapSDK.Login
{
	[Token(Token = "0x2000004")]
	public class AccessToken
	{
		[Token(Token = "0x4000006")]
		[FieldOffset(Offset = "0x10")]
		[JsonProperty("kid")]
		public string kid;

		[Token(Token = "0x4000007")]
		[FieldOffset(Offset = "0x18")]
		[JsonProperty("token_type")]
		public string tokenType;

		[Token(Token = "0x4000008")]
		[FieldOffset(Offset = "0x20")]
		[JsonProperty("mac_key")]
		public string macKey;

		[Token(Token = "0x4000009")]
		[FieldOffset(Offset = "0x28")]
		[JsonProperty("mac_algorithm")]
		public string macAlgorithm;

		[Token(Token = "0x400000A")]
		[FieldOffset(Offset = "0x30")]
		[JsonProperty("scope")]
		public HashSet<string> scopeSet;

		[Token(Token = "0x6000003")]
		[Address(RVA = "0x2661B58", Offset = "0x265DB58", VA = "0x2661B58")]
		public AccessToken(string json)
		{
		}

		[Token(Token = "0x6000004")]
		[Address(RVA = "0x2661E80", Offset = "0x265DE80", VA = "0x2661E80")]
		public AccessToken(Dictionary<string, object> dic)
		{
		}

		[Token(Token = "0x6000005")]
		[Address(RVA = "0x2661EAC", Offset = "0x265DEAC", VA = "0x2661EAC")]
		public AccessToken()
		{
		}

		[Token(Token = "0x6000006")]
		[Address(RVA = "0x2661EB4", Offset = "0x265DEB4", VA = "0x2661EB4")]
		public Dictionary<string, string> ToDict()
		{
			return null;
		}

		[Token(Token = "0x6000007")]
		[Address(RVA = "0x2662040", Offset = "0x265E040", VA = "0x2662040")]
		public string ToJson()
		{
			return null;
		}

		[Token(Token = "0x6000008")]
		[Address(RVA = "0x2661C0C", Offset = "0x265DC0C", VA = "0x2661C0C")]
		private void ConstructFromDict(Dictionary<string, object> dic)
		{
		}
	}
	[Token(Token = "0x2000005")]
	public class Profile
	{
		[Token(Token = "0x400000B")]
		[FieldOffset(Offset = "0x10")]
		[JsonProperty("name")]
		public string name;

		[Token(Token = "0x400000C")]
		[FieldOffset(Offset = "0x18")]
		[JsonProperty("gender")]
		public string gender;

		[Token(Token = "0x400000D")]
		[FieldOffset(Offset = "0x20")]
		[JsonProperty("avatar")]
		public string avatar;

		[Token(Token = "0x400000E")]
		[FieldOffset(Offset = "0x28")]
		[JsonProperty("openid")]
		public string openid;

		[Token(Token = "0x400000F")]
		[FieldOffset(Offset = "0x30")]
		[JsonProperty("unionid")]
		public string unionid;

		[Token(Token = "0x4000010")]
		[FieldOffset(Offset = "0x38")]
		[JsonIgnore]
		public string email;

		[Token(Token = "0x4000011")]
		[FieldOffset(Offset = "0x40")]
		[JsonIgnore]
		public bool emailVerified;

		[Token(Token = "0x6000009")]
		[Address(RVA = "0x2662054", Offset = "0x265E054", VA = "0x2662054")]
		public Profile()
		{
		}

		[Token(Token = "0x600000A")]
		[Address(RVA = "0x266205C", Offset = "0x265E05C", VA = "0x266205C")]
		public Profile(string json)
		{
		}

		[Token(Token = "0x600000B")]
		[Address(RVA = "0x2662244", Offset = "0x265E244", VA = "0x2662244")]
		public string ToJson()
		{
			return null;
		}
	}
	[Token(Token = "0x2000006")]
	public class TapTapAccount
	{
		[Token(Token = "0x17000001")]
		[JsonProperty("access_token")]
		public AccessToken accessToken
		{
			[Token(Token = "0x600000C")]
			[Address(RVA = "0x266224C", Offset = "0x265E24C", VA = "0x266224C")]
			[CompilerGenerated]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000002")]
		[JsonProperty("openid")]
		public string openId
		{
			[Token(Token = "0x600000D")]
			[Address(RVA = "0x2662254", Offset = "0x265E254", VA = "0x2662254")]
			[CompilerGenerated]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000003")]
		[JsonProperty("unionid")]
		public string unionId
		{
			[Token(Token = "0x600000E")]
			[Address(RVA = "0x266225C", Offset = "0x265E25C", VA = "0x266225C")]
			[CompilerGenerated]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000004")]
		[JsonProperty("name")]
		[CanBeNull]
		public string name
		{
			[Token(Token = "0x600000F")]
			[Address(RVA = "0x2662264", Offset = "0x265E264", VA = "0x2662264")]
			[CompilerGenerated]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000005")]
		[CanBeNull]
		[JsonProperty("avatar")]
		public string avatar
		{
			[Token(Token = "0x6000010")]
			[Address(RVA = "0x266226C", Offset = "0x265E26C", VA = "0x266226C")]
			[CompilerGenerated]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000006")]
		[JsonProperty("email")]
		[CanBeNull]
		public string email
		{
			[Token(Token = "0x6000011")]
			[Address(RVA = "0x2662274", Offset = "0x265E274", VA = "0x2662274")]
			[CompilerGenerated]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000012")]
		[Address(RVA = "0x266227C", Offset = "0x265E27C", VA = "0x266227C")]
		public TapTapAccount(Dictionary<string, object> dict)
		{
		}

		[Token(Token = "0x6000013")]
		[Address(RVA = "0x2662444", Offset = "0x265E444", VA = "0x2662444")]
		public TapTapAccount(AccessToken accessToken, string openId, string unionId, string name, string avatar, string email)
		{
		}

		[Token(Token = "0x6000014")]
		[Address(RVA = "0x2662498", Offset = "0x265E498", VA = "0x2662498")]
		public string ToJson()
		{
			return null;
		}

		[Token(Token = "0x6000015")]
		[Address(RVA = "0x2662638", Offset = "0x265E638", VA = "0x2662638")]
		public TapTapAccount()
		{
		}
	}
	[Token(Token = "0x2000007")]
	public class TapTapLogin
	{
		[Token(Token = "0x4000018")]
		[FieldOffset(Offset = "0x0")]
		public static readonly string Version;

		[Token(Token = "0x4000019")]
		public const string TAP_LOGIN_SCOPE_BASIC_INFO = "basic_info";

		[Token(Token = "0x400001A")]
		public const string TAP_LOGIN_SCOPE_PUBLIC_PROFILE = "public_profile";

		[Token(Token = "0x400001B")]
		public const string TAP_LOGIN_SCOPE_EMAIL = "email";

		[Token(Token = "0x400001C")]
		public const string TAP_LOGIN_SCOPE_USER_FRIENDS = "user_friends";

		[Token(Token = "0x400001D")]
		[FieldOffset(Offset = "0x8")]
		private static TapTapLogin instance;

		[Token(Token = "0x17000007")]
		public static TapTapLogin Instance
		{
			[Token(Token = "0x6000017")]
			[Address(RVA = "0x2662648", Offset = "0x265E648", VA = "0x2662648")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000016")]
		[Address(RVA = "0x2662640", Offset = "0x265E640", VA = "0x2662640")]
		private TapTapLogin()
		{
		}

		[Token(Token = "0x6000018")]
		[Address(RVA = "0x26626E8", Offset = "0x265E6E8", VA = "0x26626E8")]
		public Task<TapTapAccount> LoginWithScopes(string[] scopes)
		{
			return null;
		}

		[Token(Token = "0x6000019")]
		[Address(RVA = "0x2662828", Offset = "0x265E828", VA = "0x2662828")]
		public void Logout()
		{
		}

		[Token(Token = "0x600001A")]
		[Address(RVA = "0x26628E4", Offset = "0x265E8E4", VA = "0x26628E4")]
		public Task<TapTapAccount> GetCurrentTapAccount()
		{
			return null;
		}
	}
}
namespace TapSDK.Login.Internal
{
	[Token(Token = "0x2000008")]
	public interface ITapTapLoginPlatform
	{
		[Token(Token = "0x600001C")]
		void Init(string clientId, TapTapRegionType regionType);

		[Token(Token = "0x600001D")]
		Task<TapTapAccount> Login(string[] scopes);

		[Token(Token = "0x600001E")]
		void Logout();

		[Token(Token = "0x600001F")]
		Task<TapTapAccount> GetCurrentAccount();
	}
	[Token(Token = "0x2000009")]
	public abstract class Region
	{
		[Token(Token = "0x6000020")]
		public abstract string WebHost();

		[Token(Token = "0x6000021")]
		public abstract string ApiHost();

		[Token(Token = "0x6000022")]
		public abstract string AccountHost();

		[Token(Token = "0x6000023")]
		[Address(RVA = "0x2662A00", Offset = "0x265EA00", VA = "0x2662A00")]
		public string CodeUrl()
		{
			return null;
		}

		[Token(Token = "0x6000024")]
		[Address(RVA = "0x2662A58", Offset = "0x265EA58", VA = "0x2662A58")]
		public string TokenUrl()
		{
			return null;
		}

		[Token(Token = "0x6000025")]
		[Address(RVA = "0x2662AB0", Offset = "0x265EAB0", VA = "0x2662AB0")]
		public string ProfileUrl(bool havePublicProfile = true)
		{
			return null;
		}

		[Token(Token = "0x6000026")]
		[Address(RVA = "0x2662B30", Offset = "0x265EB30", VA = "0x2662B30")]
		public string AccountUrl()
		{
			return null;
		}

		[Token(Token = "0x6000027")]
		[Address(RVA = "0x2662B88", Offset = "0x265EB88", VA = "0x2662B88")]
		protected Region()
		{
		}
	}
	[Token(Token = "0x200000A")]
	public class RegionCN : Region
	{
		[Token(Token = "0x400001E")]
		[FieldOffset(Offset = "0x0")]
		private static string webHost;

		[Token(Token = "0x400001F")]
		[FieldOffset(Offset = "0x8")]
		private static string apiHost;

		[Token(Token = "0x4000020")]
		[FieldOffset(Offset = "0x10")]
		private static string accountHost;

		[Token(Token = "0x6000028")]
		[Address(RVA = "0x2662B90", Offset = "0x265EB90", VA = "0x2662B90", Slot = "4")]
		public override string WebHost()
		{
			return null;
		}

		[Token(Token = "0x6000029")]
		[Address(RVA = "0x2662BE8", Offset = "0x265EBE8", VA = "0x2662BE8", Slot = "5")]
		public override string ApiHost()
		{
			return null;
		}

		[Token(Token = "0x600002A")]
		[Address(RVA = "0x2662C40", Offset = "0x265EC40", VA = "0x2662C40", Slot = "6")]
		public override string AccountHost()
		{
			return null;
		}

		[Token(Token = "0x600002B")]
		[Address(RVA = "0x2662C98", Offset = "0x265EC98", VA = "0x2662C98")]
		public RegionCN()
		{
		}
	}
	[Token(Token = "0x200000B")]
	public class RegionIO : Region
	{
		[Token(Token = "0x4000021")]
		[FieldOffset(Offset = "0x0")]
		private static string webHost;

		[Token(Token = "0x4000022")]
		[FieldOffset(Offset = "0x8")]
		private static string apiHost;

		[Token(Token = "0x4000023")]
		[FieldOffset(Offset = "0x10")]
		private static string accountHost;

		[Token(Token = "0x600002D")]
		[Address(RVA = "0x2662D44", Offset = "0x265ED44", VA = "0x2662D44", Slot = "4")]
		public override string WebHost()
		{
			return null;
		}

		[Token(Token = "0x600002E")]
		[Address(RVA = "0x2662D9C", Offset = "0x265ED9C", VA = "0x2662D9C", Slot = "5")]
		public override string ApiHost()
		{
			return null;
		}

		[Token(Token = "0x600002F")]
		[Address(RVA = "0x2662DF4", Offset = "0x265EDF4", VA = "0x2662DF4", Slot = "6")]
		public override string AccountHost()
		{
			return null;
		}

		[Token(Token = "0x6000030")]
		[Address(RVA = "0x2662E4C", Offset = "0x265EE4C", VA = "0x2662E4C")]
		public RegionIO()
		{
		}
	}
	[Token(Token = "0x200000C")]
	public class TapTapLoginManager
	{
		[Token(Token = "0x4000024")]
		[FieldOffset(Offset = "0x0")]
		private static TapTapLoginManager instance;

		[Token(Token = "0x4000025")]
		[FieldOffset(Offset = "0x10")]
		private ITapTapLoginPlatform platformWrapper;

		[Token(Token = "0x17000008")]
		public static TapTapLoginManager Instance
		{
			[Token(Token = "0x6000033")]
			[Address(RVA = "0x2662708", Offset = "0x265E708", VA = "0x2662708")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000032")]
		[Address(RVA = "0x2662EF8", Offset = "0x265EEF8", VA = "0x2662EF8")]
		private TapTapLoginManager()
		{
		}

		[Token(Token = "0x6000034")]
		[Address(RVA = "0x2662FCC", Offset = "0x265EFCC", VA = "0x2662FCC")]
		public void Init(string clientId, TapTapRegionType regionType)
		{
		}

		[Token(Token = "0x6000035")]
		[Address(RVA = "0x266277C", Offset = "0x265E77C", VA = "0x266277C")]
		public Task<TapTapAccount> Login(string[] scopes)
		{
			return null;
		}

		[Token(Token = "0x6000036")]
		[Address(RVA = "0x2662840", Offset = "0x265E840", VA = "0x2662840")]
		public void Logout()
		{
		}

		[Token(Token = "0x6000037")]
		[Address(RVA = "0x26628FC", Offset = "0x265E8FC", VA = "0x26628FC")]
		public Task<TapTapAccount> GetCurrentAccount()
		{
			return null;
		}
	}
	[Token(Token = "0x200000D")]
	public static class TapTapSdk
	{
		[Token(Token = "0x17000009")]
		public static string ClientId
		{
			[Token(Token = "0x6000038")]
			[Address(RVA = "0x2663084", Offset = "0x265F084", VA = "0x2663084")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x6000039")]
			[Address(RVA = "0x26630CC", Offset = "0x265F0CC", VA = "0x26630CC")]
			[CompilerGenerated]
			private set
			{
			}
		}

		[Token(Token = "0x1700000A")]
		public static Region CurrentRegion
		{
			[Token(Token = "0x600003A")]
			[Address(RVA = "0x2663118", Offset = "0x265F118", VA = "0x2663118")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x600003B")]
			[Address(RVA = "0x2663160", Offset = "0x265F160", VA = "0x2663160")]
			[CompilerGenerated]
			private set
			{
			}
		}

		[Token(Token = "0x600003C")]
		[Address(RVA = "0x26631AC", Offset = "0x265F1AC", VA = "0x26631AC")]
		public static void SDKInitialize(string clientId, bool isCn)
		{
		}
	}
}
namespace TapSDK.Login.Internal.Init
{
	[Token(Token = "0x200000E")]
	public class LoginInitTask : IInitTask
	{
		[Token(Token = "0x1700000B")]
		public int Order
		{
			[Token(Token = "0x600003D")]
			[Address(RVA = "0x26632B4", Offset = "0x265F2B4", VA = "0x26632B4", Slot = "4")]
			get
			{
				return default(int);
			}
		}

		[Token(Token = "0x600003E")]
		[Address(RVA = "0x26632BC", Offset = "0x265F2BC", VA = "0x26632BC", Slot = "5")]
		public void Init(TapTapSdkOptions coreOption, TapTapSdkBaseOptions[] otherOptions)
		{
		}

		[Token(Token = "0x600003F")]
		[Address(RVA = "0x26632E4", Offset = "0x265F2E4", VA = "0x26632E4", Slot = "6")]
		public void Init(TapTapSdkOptions coreOption)
		{
		}

		[Token(Token = "0x6000040")]
		[Address(RVA = "0x266330C", Offset = "0x265F30C", VA = "0x266330C")]
		public LoginInitTask()
		{
		}
	}
}
