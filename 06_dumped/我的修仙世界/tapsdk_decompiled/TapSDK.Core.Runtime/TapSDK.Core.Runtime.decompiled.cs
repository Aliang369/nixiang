using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Il2CppDummyDll;
using Newtonsoft.Json;
using TapSDK.Core.Internal;
using TapSDK.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

[assembly: AssemblyVersion("0.0.0.0")]
[Token(Token = "0x2000002")]
public class TapMessage : MonoBehaviour
{
	[Token(Token = "0x2000003")]
	public enum Time
	{
		[Token(Token = "0x4000002")]
		threeSecond,
		[Token(Token = "0x4000003")]
		twoSecond,
		[Token(Token = "0x4000004")]
		oneSecond
	}

	[Token(Token = "0x2000004")]
	public enum Position
	{
		[Token(Token = "0x4000006")]
		top,
		[Token(Token = "0x4000007")]
		bottom
	}

	[Token(Token = "0x6000001")]
	[Address(RVA = "0x26484BC", Offset = "0x26444BC", VA = "0x26484BC")]
	public static void ShowMessage(string msg, Position position, Time time)
	{
	}

	[Token(Token = "0x6000002")]
	[Address(RVA = "0x264867C", Offset = "0x264467C", VA = "0x264867C")]
	private static void SetPosition(RectTransform rectTransform, Position position)
	{
	}

	[Token(Token = "0x6000003")]
	[Address(RVA = "0x2648704", Offset = "0x2644704", VA = "0x2648704")]
	private static void RemoveClone(GameObject clone, Time time)
	{
	}

	[Token(Token = "0x6000004")]
	[Address(RVA = "0x26487E0", Offset = "0x26447E0", VA = "0x26487E0")]
	public TapMessage()
	{
	}
}
[Token(Token = "0x2000005")]
[CompilerGenerated]
[EditorBrowsable(EditorBrowsableState.Never)]
[GeneratedCode("Unity.MonoScriptGenerator.MonoScriptInfoGenerator", null)]
internal class UnitySourceGeneratedAssemblyMonoScriptTypes_v1
{
	[Token(Token = "0x2000006")]
	private struct MonoScriptData
	{
		[Token(Token = "0x4000008")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		public byte[] FilePathsData;

		[Token(Token = "0x4000009")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
		public byte[] TypesData;

		[Token(Token = "0x400000A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		public int TotalTypes;

		[Token(Token = "0x400000B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x14")]
		public int TotalFiles;

		[Token(Token = "0x400000C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		public bool IsEditorOnly;
	}

	[MethodImpl((MethodImplOptions)256)]
	[Token(Token = "0x6000005")]
	[Address(RVA = "0x26487E8", Offset = "0x26447E8", VA = "0x26487E8")]
	private static MonoScriptData Get()
	{
		return default(MonoScriptData);
	}

	[Token(Token = "0x6000006")]
	[Address(RVA = "0x26488AC", Offset = "0x26448AC", VA = "0x26488AC")]
	public UnitySourceGeneratedAssemblyMonoScriptTypes_v1()
	{
	}
}
namespace TapTap.UI
{
	[Token(Token = "0x2000007")]
	public class GraphicRaycasterBugFixed : GraphicRaycaster
	{
		[NonSerialized]
		[Token(Token = "0x400000D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x48")]
		private List<Graphic> m_RaycastResults;

		[Token(Token = "0x400000E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x50")]
		private Canvas _Canvas;

		[NonSerialized]
		[Token(Token = "0x400000F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static readonly List<Graphic> s_SortedGraphics;

		[Token(Token = "0x17000001")]
		private Canvas canvasBugFixed
		{
			[Token(Token = "0x6000007")]
			[Address(RVA = "0x26488B4", Offset = "0x26448B4", VA = "0x26488B4")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000008")]
		[Address(RVA = "0x2648948", Offset = "0x2644948", VA = "0x2648948", Slot = "17")]
		public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
		{
		}

		[Token(Token = "0x6000009")]
		[Address(RVA = "0x264942C", Offset = "0x264542C", VA = "0x264942C")]
		private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
		{
		}

		[Token(Token = "0x600000A")]
		[Address(RVA = "0x2649980", Offset = "0x2645980", VA = "0x2649980")]
		public GraphicRaycasterBugFixed()
		{
		}
	}
}
namespace TapSDK.UI
{
	[Token(Token = "0x2000009")]
	public enum EOpenMode
	{
		[Token(Token = "0x4000013")]
		Sync,
		[Token(Token = "0x4000014")]
		Async
	}
	[Token(Token = "0x200000A")]
	[Flags]
	public enum EAnimationMode
	{
		[Token(Token = "0x4000016")]
		None = 0,
		[Token(Token = "0x4000017")]
		Alpha = 1,
		[Token(Token = "0x4000018")]
		Scale = 2,
		[Token(Token = "0x4000019")]
		RightSlideIn = 4,
		[Token(Token = "0x400001A")]
		UpSlideIn = 8,
		[Token(Token = "0x400001B")]
		ToastSlideIn = 0x10,
		[Token(Token = "0x400001C")]
		AlphaAndScale = Alpha | Scale
	}
	[Token(Token = "0x200000B")]
	public class LoadingPanelController : BasePanelController
	{
		[Token(Token = "0x400001D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x68")]
		public Transform rotater;

		[Token(Token = "0x400001E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x70")]
		public float speed;

		[Token(Token = "0x600000F")]
		[Address(RVA = "0x2649B60", Offset = "0x2645B60", VA = "0x2649B60", Slot = "7")]
		protected override void BindComponents()
		{
		}

		[Token(Token = "0x6000010")]
		[Address(RVA = "0x2649BC4", Offset = "0x2645BC4", VA = "0x2649BC4")]
		private void Update()
		{
		}

		[Token(Token = "0x6000011")]
		[Address(RVA = "0x2649CA8", Offset = "0x2645CA8", VA = "0x2649CA8")]
		public LoadingPanelController()
		{
		}
	}
	[Token(Token = "0x200000C")]
	public class MonoSingleton<T> : MonoBehaviour where T : UnityEngine.Component
	{
		[Token(Token = "0x400001F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static T _instance;

		[Token(Token = "0x17000002")]
		public static T Instance
		{
			[Token(Token = "0x6000012")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000013")]
		private static void CreateInstance()
		{
		}

		[Token(Token = "0x6000014")]
		public static void DestroyInstance()
		{
		}

		[Token(Token = "0x6000015")]
		public static bool HasInstance()
		{
			return default(bool);
		}

		[Token(Token = "0x6000016")]
		protected virtual void Awake()
		{
		}

		[Token(Token = "0x6000017")]
		protected virtual void OnDestroy()
		{
		}

		[Token(Token = "0x6000018")]
		public virtual void Init()
		{
		}

		[Token(Token = "0x6000019")]
		public virtual void Uninit()
		{
		}

		[Token(Token = "0x600001A")]
		public MonoSingleton()
		{
		}
	}
	[Token(Token = "0x200000D")]
	public class Singleton<T> where T : class
	{
		[Token(Token = "0x4000020")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static T _instance;

		[Token(Token = "0x4000021")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static readonly object _lock;

		[Token(Token = "0x17000003")]
		public static T Instance
		{
			[Token(Token = "0x600001B")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x600001C")]
		public Singleton()
		{
		}
	}
	[Token(Token = "0x200000E")]
	public class TipPanelOpenParam : AbstractOpenPanelParameter
	{
		[Token(Token = "0x4000022")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		public string text;

		[Token(Token = "0x4000023")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		public Color textColor;

		[Token(Token = "0x4000024")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		public TextAnchor textAnchor;

		[Token(Token = "0x600001E")]
		[Address(RVA = "0x2649CD8", Offset = "0x2645CD8", VA = "0x2649CD8")]
		public TipPanelOpenParam(string text, Color textColor, TextAnchor textAnchor)
		{
		}
	}
	[Token(Token = "0x200000F")]
	public class TipPanelController : BasePanelController
	{
		[Token(Token = "0x4000025")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x68")]
		public Text text;

		[Token(Token = "0x600001F")]
		[Address(RVA = "0x2649D38", Offset = "0x2645D38", VA = "0x2649D38", Slot = "7")]
		protected override void BindComponents()
		{
		}

		[Token(Token = "0x6000020")]
		[Address(RVA = "0x2649DC0", Offset = "0x2645DC0", VA = "0x2649DC0", Slot = "8")]
		protected override void OnLoadSuccess()
		{
		}

		[Token(Token = "0x6000021")]
		[Address(RVA = "0x2649E9C", Offset = "0x2645E9C", VA = "0x2649E9C")]
		public TipPanelController()
		{
		}
	}
	[Token(Token = "0x2000010")]
	public class ToastPanelOpenParam : AbstractOpenPanelParameter
	{
		[Token(Token = "0x4000026")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		public float popupTime;

		[Token(Token = "0x4000027")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		public string text;

		[Token(Token = "0x4000028")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		public Texture icon;

		[Token(Token = "0x6000022")]
		[Address(RVA = "0x2649EB0", Offset = "0x2645EB0", VA = "0x2649EB0")]
		public ToastPanelOpenParam(string text, float popupTime, [Optional] Texture icon)
		{
		}
	}
	[Token(Token = "0x2000011")]
	public class ToastBlackPanelController : BasePanelController
	{
		[Token(Token = "0x4000029")]
		private const int MAX_CHAR_COUNT = 87;

		[Token(Token = "0x400002A")]
		private const float MIN_HORIZONTAL_LENGTH = 210f;

		[Token(Token = "0x400002B")]
		private const float MAX_HORIZONTAL_LENGTH = 1252f;

		[Token(Token = "0x400002C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x68")]
		[HideInInspector]
		public Text text;

		[Token(Token = "0x400002D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x70")]
		[HideInInspector]
		public RectTransform background;

		[Token(Token = "0x400002E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x78")]
		[HideInInspector]
		public LayoutGroup layout;

		[Token(Token = "0x400002F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x80")]
		[HideInInspector]
		public RawImage iconImage;

		[Token(Token = "0x4000030")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x88")]
		public float fixVal;

		[Token(Token = "0x4000031")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8C")]
		public float animationTime;

		[Token(Token = "0x4000032")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x90")]
		[SerializeField]
		private float sizeDeltaX;

		[Token(Token = "0x6000023")]
		[Address(RVA = "0x2649EEC", Offset = "0x2645EEC", VA = "0x2649EEC", Slot = "7")]
		protected override void BindComponents()
		{
		}

		[Token(Token = "0x6000024")]
		[Address(RVA = "0x264A088", Offset = "0x2646088", VA = "0x264A088", Slot = "8")]
		protected override void OnLoadSuccess()
		{
		}

		[Token(Token = "0x6000025")]
		[Address(RVA = "0x264A46C", Offset = "0x264646C", VA = "0x264A46C")]
		protected float CalculateLengthOfText(out bool horizontalOverflow)
		{
			return default(float);
		}

		[Token(Token = "0x6000026")]
		[Address(RVA = "0x264A4D8", Offset = "0x26464D8", VA = "0x264A4D8")]
		public ToastBlackPanelController()
		{
		}
	}
	[Token(Token = "0x2000012")]
	public class ToastWhitePanelController : BasePanelController
	{
		[Token(Token = "0x4000033")]
		private const int MAX_CHAR_COUNT = 69;

		[Token(Token = "0x4000034")]
		private const float MIN_HORIZONTAL_LENGTH = 120f;

		[Token(Token = "0x4000035")]
		private const float MAX_HORIZONTAL_LENGTH = 1252f;

		[Token(Token = "0x4000036")]
		private const float BACKGROUND_WIDTH_HELPER = 60f;

		[Token(Token = "0x4000037")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x68")]
		[HideInInspector]
		public Text text;

		[Token(Token = "0x4000038")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x70")]
		[HideInInspector]
		public RectTransform background;

		[Token(Token = "0x4000039")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x78")]
		[HideInInspector]
		public RawImage iconImage;

		[Token(Token = "0x400003A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x80")]
		public float fixVal;

		[Token(Token = "0x400003B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x84")]
		public float animationTime;

		[Token(Token = "0x400003C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x88")]
		public ToastPanelOpenParam toastParam;

		[Token(Token = "0x6000027")]
		[Address(RVA = "0x264A4F4", Offset = "0x26464F4", VA = "0x264A4F4", Slot = "7")]
		protected override void BindComponents()
		{
		}

		[Token(Token = "0x6000028")]
		[Address(RVA = "0x264A63C", Offset = "0x264663C", VA = "0x264A63C", Slot = "8")]
		protected override void OnLoadSuccess()
		{
		}

		[Token(Token = "0x6000029")]
		[Address(RVA = "0x264AA08", Offset = "0x2646A08", VA = "0x264AA08")]
		private float CalculateLengthOfText()
		{
			return default(float);
		}

		[Token(Token = "0x600002A")]
		[Address(RVA = "0x264AA54", Offset = "0x2646A54", VA = "0x264AA54")]
		public void Refresh(float refreshTime)
		{
		}

		[Token(Token = "0x600002B")]
		[Address(RVA = "0x264AAC0", Offset = "0x2646AC0", VA = "0x264AAC0")]
		public ToastWhitePanelController()
		{
		}
	}
	[Token(Token = "0x2000013")]
	public class UIManager : MonoSingleton<UIManager>
	{
		[Token(Token = "0x2000014")]
		public enum GeneralToastLevel
		{
			[Token(Token = "0x400004F")]
			None,
			[Token(Token = "0x4000050")]
			Info,
			[Token(Token = "0x4000051")]
			Warning,
			[Token(Token = "0x4000052")]
			Error,
			[Token(Token = "0x4000053")]
			Success
		}

		[Token(Token = "0x2000015")]
		private class ToastConfig
		{
			[Token(Token = "0x4000054")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			public bool white;

			[Token(Token = "0x4000055")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
			public string text;

			[Token(Token = "0x4000056")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public float popupTime;

			[Token(Token = "0x4000057")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public Texture icon;

			[Token(Token = "0x6000053")]
			[Address(RVA = "0x264CA68", Offset = "0x2648A68", VA = "0x264CA68")]
			public ToastConfig()
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x2000017")]
		[CompilerGenerated]
		private struct <OpenLoadingAsync>d__54 : IAsyncStateMachine
		{
			[Token(Token = "0x400005B")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x400005C")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public AsyncTaskMethodBuilder <>t__builder;

			[Token(Token = "0x400005D")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public UIManager <>4__this;

			[Token(Token = "0x400005E")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			private TaskAwaiter<LoadingPanelController> <>u__1;

			[Token(Token = "0x6000058")]
			[Address(RVA = "0x264D384", Offset = "0x2649384", VA = "0x264D384", Slot = "4")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x6000059")]
			[Address(RVA = "0x264D634", Offset = "0x2649634", VA = "0x264D634", Slot = "5")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x2000018")]
		[CompilerGenerated]
		private struct <OpenUIAsync>d__34<TPanelController> : IAsyncStateMachine where TPanelController : BasePanelController
		{
			[Token(Token = "0x400005F")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x4000060")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public AsyncTaskMethodBuilder<TPanelController> <>t__builder;

			[Token(Token = "0x4000061")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public UIManager <>4__this;

			[Token(Token = "0x4000062")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public Action<BasePanelController> onAsyncGet;

			[Token(Token = "0x4000063")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public string path;

			[Token(Token = "0x4000064")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public AbstractOpenPanelParameter param;

			[Token(Token = "0x4000065")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			private ResourceRequest <request>5__2;

			[Token(Token = "0x4000066")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			private YieldAwaitable.YieldAwaiter <>u__1;

			[Token(Token = "0x600005A")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x600005B")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x2000019")]
		[CompilerGenerated]
		private struct <OpenUIAsync>d__35 : IAsyncStateMachine
		{
			[Token(Token = "0x4000067")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x4000068")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public AsyncTaskMethodBuilder<BasePanelController> <>t__builder;

			[Token(Token = "0x4000069")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public Type panelType;

			[Token(Token = "0x400006A")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public UIManager <>4__this;

			[Token(Token = "0x400006B")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
			public Action<BasePanelController> onAsyncGet;

			[Token(Token = "0x400006C")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
			public string path;

			[Token(Token = "0x400006D")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
			public AbstractOpenPanelParameter param;

			[Token(Token = "0x400006E")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x48")]
			private ResourceRequest <request>5__2;

			[Token(Token = "0x400006F")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x50")]
			private YieldAwaitable.YieldAwaiter <>u__1;

			[Token(Token = "0x600005C")]
			[Address(RVA = "0x264D69C", Offset = "0x264969C", VA = "0x264D69C", Slot = "4")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x600005D")]
			[Address(RVA = "0x264DD20", Offset = "0x2649D20", VA = "0x264DD20", Slot = "5")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[Token(Token = "0x400003D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static Texture whiteToastSuccessIcon;

		[Token(Token = "0x400003E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
		private static Texture whiteToastErrorIcon;

		[Token(Token = "0x400003F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private static Texture whiteToastInfoIcon;

		[Token(Token = "0x4000040")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		private static Texture whiteToastWarningIcon;

		[Token(Token = "0x4000041")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		private static Texture taptapToastIcon;

		[Token(Token = "0x4000042")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		private List<ToastConfig> _toastCacheData;

		[Token(Token = "0x4000043")]
		public const int TOPPEST_SORTING_ORDER = 32767;

		[Token(Token = "0x4000044")]
		private const int LOADING_PANEL_SORTING_ORDER = 32767;

		[Token(Token = "0x4000045")]
		private const int TOAST_PANEL_SORTING_ORDER = 32757;

		[Token(Token = "0x4000046")]
		private const int TIP_PANEL_SORTING_ORDER = 32747;

		[Token(Token = "0x4000047")]
		private const int REFERENCE_WIDTH = 1920;

		[Token(Token = "0x4000048")]
		private const int REFERENCE_HEIGHT = 1080;

		[Token(Token = "0x4000049")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		private readonly Dictionary<Type, BasePanelController> _registerPanels;

		[Token(Token = "0x400004A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
		private Camera _uiCamera;

		[Token(Token = "0x400004B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
		private GameObject _uiRoot;

		[Token(Token = "0x400004C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
		private GameObject _uiConstantRoot;

		[Token(Token = "0x400004D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x48")]
		private Canvas _uiRootCanvas;

		[Token(Token = "0x17000004")]
		public static Texture WhiteToastSuccessIcon
		{
			[Token(Token = "0x600002C")]
			[Address(RVA = "0x264AAD4", Offset = "0x2646AD4", VA = "0x264AAD4")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000005")]
		public static Texture WhiteToastErrorIcon
		{
			[Token(Token = "0x600002D")]
			[Address(RVA = "0x264ABA0", Offset = "0x2646BA0", VA = "0x264ABA0")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000006")]
		public static Texture WhiteToastWarningIcon
		{
			[Token(Token = "0x600002E")]
			[Address(RVA = "0x264AC70", Offset = "0x2646C70", VA = "0x264AC70")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000007")]
		public static Texture WhiteToastInfoIcon
		{
			[Token(Token = "0x600002F")]
			[Address(RVA = "0x264AD40", Offset = "0x2646D40", VA = "0x264AD40")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000008")]
		public static Texture TapTapToastIcon
		{
			[Token(Token = "0x6000030")]
			[Address(RVA = "0x264AE10", Offset = "0x2646E10", VA = "0x264AE10")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x17000009")]
		public Camera UiCamera
		{
			[Token(Token = "0x6000031")]
			[Address(RVA = "0x264AEE0", Offset = "0x2646EE0", VA = "0x264AEE0")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000032")]
		[Address(RVA = "0x264AEE8", Offset = "0x2646EE8", VA = "0x264AEE8", Slot = "6")]
		public override void Init()
		{
		}

		[Token(Token = "0x6000033")]
		[Address(RVA = "0x264AF90", Offset = "0x2646F90", VA = "0x264AF90")]
		private void CreateUIRoot()
		{
		}

		[Token(Token = "0x6000034")]
		[Address(RVA = "0x264B2D0", Offset = "0x26472D0", VA = "0x264B2D0", Slot = "7")]
		public override void Uninit()
		{
		}

		[Token(Token = "0x6000035")]
		[AsyncStateMachine(typeof(<OpenUIAsync>d__34<>))]
		public Task<TPanelController> OpenUIAsync<TPanelController>(string path, [Optional] AbstractOpenPanelParameter param, [Optional] Action<BasePanelController> onAsyncGet) where TPanelController : BasePanelController
		{
			return null;
		}

		[Token(Token = "0x6000036")]
		[Address(RVA = "0x264B320", Offset = "0x2647320", VA = "0x264B320")]
		[AsyncStateMachine(typeof(<OpenUIAsync>d__35))]
		public Task<BasePanelController> OpenUIAsync(Type panelType, string path, [Optional] AbstractOpenPanelParameter param, [Optional] Action<BasePanelController> onAsyncGet)
		{
			return null;
		}

		[Token(Token = "0x6000037")]
		public TPanelController OpenUI<TPanelController>(string path, [Optional] AbstractOpenPanelParameter param) where TPanelController : BasePanelController
		{
			return null;
		}

		[Token(Token = "0x6000038")]
		[Address(RVA = "0x264B434", Offset = "0x2647434", VA = "0x264B434")]
		public BasePanelController OpenUI(Type panelType, string path, [Optional] AbstractOpenPanelParameter param)
		{
			return null;
		}

		[Token(Token = "0x6000039")]
		[Address(RVA = "0x264B7D4", Offset = "0x26477D4", VA = "0x264B7D4")]
		public BasePanelController GetUI(Type panelType)
		{
			return null;
		}

		[Token(Token = "0x600003A")]
		public TPanelController GetUI<TPanelController>() where TPanelController : BasePanelController
		{
			return null;
		}

		[Token(Token = "0x600003B")]
		[Address(RVA = "0x264BD14", Offset = "0x2647D14", VA = "0x264BD14")]
		public bool CloseUI(Type panelType)
		{
			return default(bool);
		}

		[Token(Token = "0x600003C")]
		public bool CloseUI<TPanelController>() where TPanelController : BasePanelController
		{
			return default(bool);
		}

		[Token(Token = "0x600003D")]
		[Address(RVA = "0x264BE58", Offset = "0x2647E58", VA = "0x264BE58")]
		public void AddUI(BasePanelController panel)
		{
		}

		[Token(Token = "0x600003E")]
		[Address(RVA = "0x264C1DC", Offset = "0x26481DC", VA = "0x264C1DC")]
		public void RemoveUI(BasePanelController panel)
		{
		}

		[Token(Token = "0x600003F")]
		[Address(RVA = "0x264C388", Offset = "0x2648388", VA = "0x264C388")]
		private void CheckToastCache()
		{
		}

		[Token(Token = "0x6000040")]
		[Address(RVA = "0x264C5B4", Offset = "0x26485B4", VA = "0x264C5B4")]
		public void ToppedUI(Type panelType)
		{
		}

		[Token(Token = "0x6000041")]
		[Address(RVA = "0x264C67C", Offset = "0x264867C", VA = "0x264C67C")]
		public void ToppedUI(BasePanelController panel, bool toppedSepcialUI = true)
		{
		}

		[Token(Token = "0x6000042")]
		[Address(RVA = "0x264C728", Offset = "0x2648728", VA = "0x264C728")]
		public void OpenToast(bool white, string text, [Optional][DefaultParameterValue(3f)] float popupTime, [Optional] Texture icon)
		{
		}

		[Token(Token = "0x6000043")]
		[Address(RVA = "0x264C96C", Offset = "0x264896C", VA = "0x264C96C")]
		private bool ToastTryMerge(bool white, string text, float popupTime, Texture icon)
		{
			return default(bool);
		}

		[Token(Token = "0x6000044")]
		[Address(RVA = "0x264CA70", Offset = "0x2648A70", VA = "0x264CA70")]
		public void OpenToast(string text, GeneralToastLevel toastLevel, float popupTime = 3f)
		{
		}

		[Token(Token = "0x6000045")]
		[Address(RVA = "0x264CAB4", Offset = "0x2648AB4", VA = "0x264CAB4")]
		private Texture GetToastIcon(GeneralToastLevel toastLevel)
		{
			return null;
		}

		[Token(Token = "0x6000046")]
		[Address(RVA = "0x264C444", Offset = "0x2648444", VA = "0x264C444")]
		private void InternalOpenToast(bool white, string text, float popupTime, Texture icon)
		{
		}

		[Token(Token = "0x6000047")]
		[Address(RVA = "0x264CAF0", Offset = "0x2648AF0", VA = "0x264CAF0")]
		public void OpenTip(string text, Color textColor, TextAnchor textAnchor = TextAnchor.MiddleCenter)
		{
		}

		[Token(Token = "0x6000048")]
		[Address(RVA = "0x264CD38", Offset = "0x2648D38", VA = "0x264CD38")]
		public void OpenLoading()
		{
		}

		[Token(Token = "0x6000049")]
		[Address(RVA = "0x264CE14", Offset = "0x2648E14", VA = "0x264CE14")]
		[AsyncStateMachine(typeof(<OpenLoadingAsync>d__54))]
		public Task OpenLoadingAsync()
		{
			return null;
		}

		[Token(Token = "0x600004A")]
		[Address(RVA = "0x264CED4", Offset = "0x2648ED4", VA = "0x264CED4")]
		public void CloseLoading()
		{
		}

		[Token(Token = "0x600004B")]
		[Address(RVA = "0x264CF90", Offset = "0x2648F90", VA = "0x264CF90")]
		public void CloseTip()
		{
		}

		[Token(Token = "0x600004C")]
		[Address(RVA = "0x264B8C0", Offset = "0x26478C0", VA = "0x264B8C0")]
		private bool IsNeedCorrectRectTransform(BasePanelController controller)
		{
			return default(bool);
		}

		[Token(Token = "0x600004D")]
		[Address(RVA = "0x264D04C", Offset = "0x264904C", VA = "0x264D04C")]
		private void InternalOnPanelLoaded(Type tPanelController, BasePanelController basePanel, [Optional] AbstractOpenPanelParameter param)
		{
		}

		[Token(Token = "0x600004E")]
		[Address(RVA = "0x264B9DC", Offset = "0x26479DC", VA = "0x264B9DC")]
		private void EnsureSpecialPanel()
		{
		}

		[Token(Token = "0x600004F")]
		[Address(RVA = "0x264D0E0", Offset = "0x26490E0", VA = "0x264D0E0")]
		public Transform GetUIRootTransform(BasePanelController panel)
		{
			return null;
		}

		[Token(Token = "0x6000050")]
		[Address(RVA = "0x264D124", Offset = "0x2649124", VA = "0x264D124")]
		public void SetSortOrder(int order)
		{
		}

		[Token(Token = "0x6000051")]
		[Address(RVA = "0x264D140", Offset = "0x2649140", VA = "0x264D140")]
		public Camera GetUICamera()
		{
			return null;
		}

		[Token(Token = "0x6000052")]
		[Address(RVA = "0x264D148", Offset = "0x2649148", VA = "0x264D148")]
		public UIManager()
		{
		}
	}
	[Token(Token = "0x200001A")]
	[RequireComponent(typeof(GraphicRaycaster))]
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class BasePanelController : MonoBehaviour
	{
		[Token(Token = "0x200001B")]
		[CompilerGenerated]
		private sealed class <FadeInCoroutine>d__25 : IEnumerator<object>, IEnumerator, IDisposable
		{
			[Token(Token = "0x400007B")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			private int <>1__state;

			[Token(Token = "0x400007C")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
			private object <>2__current;

			[Token(Token = "0x400007D")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public BasePanelController <>4__this;

			[Token(Token = "0x400007E")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public float time;

			[Token(Token = "0x1700000B")]
			private object System.Collections.Generic.IEnumerator<System.Object>.Current
			{
				[Token(Token = "0x6000075")]
				[Address(RVA = "0x264E880", Offset = "0x264A880", VA = "0x264E880", Slot = "4")]
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[Token(Token = "0x1700000C")]
			private object System.Collections.IEnumerator.Current
			{
				[Token(Token = "0x6000077")]
				[Address(RVA = "0x264E8C0", Offset = "0x264A8C0", VA = "0x264E8C0", Slot = "7")]
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[Token(Token = "0x6000072")]
			[Address(RVA = "0x264E440", Offset = "0x264A440", VA = "0x264E440")]
			[DebuggerHidden]
			public <FadeInCoroutine>d__25(int <>1__state)
			{
			}

			[Token(Token = "0x6000073")]
			[Address(RVA = "0x264E60C", Offset = "0x264A60C", VA = "0x264E60C", Slot = "5")]
			[DebuggerHidden]
			private void System.IDisposable.Dispose()
			{
			}

			[Token(Token = "0x6000074")]
			[Address(RVA = "0x264E610", Offset = "0x264A610", VA = "0x264E610", Slot = "6")]
			private bool MoveNext()
			{
				return default(bool);
			}

			[Token(Token = "0x6000076")]
			[Address(RVA = "0x264E888", Offset = "0x264A888", VA = "0x264E888", Slot = "8")]
			[DebuggerHidden]
			private void System.Collections.IEnumerator.Reset()
			{
			}
		}

		[Token(Token = "0x200001C")]
		[CompilerGenerated]
		private sealed class <FadeOutCoroutine>d__26 : IEnumerator<object>, IEnumerator, IDisposable
		{
			[Token(Token = "0x400007F")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			private int <>1__state;

			[Token(Token = "0x4000080")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
			private object <>2__current;

			[Token(Token = "0x4000081")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public BasePanelController <>4__this;

			[Token(Token = "0x4000082")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public float time;

			[Token(Token = "0x1700000D")]
			private object System.Collections.Generic.IEnumerator<System.Object>.Current
			{
				[Token(Token = "0x600007B")]
				[Address(RVA = "0x264EB68", Offset = "0x264AB68", VA = "0x264EB68", Slot = "4")]
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[Token(Token = "0x1700000E")]
			private object System.Collections.IEnumerator.Current
			{
				[Token(Token = "0x600007D")]
				[Address(RVA = "0x264EBA8", Offset = "0x264ABA8", VA = "0x264EBA8", Slot = "7")]
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[Token(Token = "0x6000078")]
			[Address(RVA = "0x264E468", Offset = "0x264A468", VA = "0x264E468")]
			[DebuggerHidden]
			public <FadeOutCoroutine>d__26(int <>1__state)
			{
			}

			[Token(Token = "0x6000079")]
			[Address(RVA = "0x264E8C8", Offset = "0x264A8C8", VA = "0x264E8C8", Slot = "5")]
			[DebuggerHidden]
			private void System.IDisposable.Dispose()
			{
			}

			[Token(Token = "0x600007A")]
			[Address(RVA = "0x264E8CC", Offset = "0x264A8CC", VA = "0x264E8CC", Slot = "6")]
			private bool MoveNext()
			{
				return default(bool);
			}

			[Token(Token = "0x600007C")]
			[Address(RVA = "0x264EB70", Offset = "0x264AB70", VA = "0x264EB70", Slot = "8")]
			[DebuggerHidden]
			private void System.Collections.IEnumerator.Reset()
			{
			}
		}

		[Token(Token = "0x4000070")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		[HideInInspector]
		public Canvas canvas;

		[Token(Token = "0x4000071")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		[HideInInspector]
		public CanvasGroup canvasGroup;

		[Token(Token = "0x4000072")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
		protected float fadeAnimationTime;

		[Token(Token = "0x4000073")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x34")]
		private float _animationElapse;

		[Token(Token = "0x4000074")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
		private Vector2 _screenSize;

		[Token(Token = "0x4000075")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
		private Vector2 _cachedAnchorPos;

		[Token(Token = "0x4000076")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x48")]
		private RectTransform _rectTransform;

		[Token(Token = "0x4000077")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x50")]
		private Coroutine _animationCoroutine;

		[Token(Token = "0x4000078")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x58")]
		protected internal AbstractOpenPanelParameter openParam;

		[Token(Token = "0x4000079")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x60")]
		public BasePanelConfig panelConfig;

		[Token(Token = "0x400007A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x64")]
		public int toppedOrder;

		[Token(Token = "0x1700000A")]
		public virtual Transform AttachedParent
		{
			[Token(Token = "0x600005F")]
			[Address(RVA = "0x264DDA8", Offset = "0x2649DA8", VA = "0x264DDA8", Slot = "5")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x600005E")]
		[Address(RVA = "0x264DD9C", Offset = "0x2649D9C", VA = "0x264DD9C", Slot = "4")]
		protected virtual float GetToastSlideInOffset()
		{
			return default(float);
		}

		[Token(Token = "0x6000060")]
		[Address(RVA = "0x264DDFC", Offset = "0x2649DFC", VA = "0x264DDFC", Slot = "6")]
		protected virtual void Awake()
		{
		}

		[Token(Token = "0x6000061")]
		[Address(RVA = "0x2649DBC", Offset = "0x2645DBC", VA = "0x2649DBC", Slot = "7")]
		protected virtual void BindComponents()
		{
		}

		[Token(Token = "0x6000062")]
		[Address(RVA = "0x264B940", Offset = "0x2647940", VA = "0x264B940")]
		public void OnLoaded([Optional] AbstractOpenPanelParameter param)
		{
		}

		[Token(Token = "0x6000063")]
		[Address(RVA = "0x264DED8", Offset = "0x2649ED8", VA = "0x264DED8")]
		private void InitCanvasSetting()
		{
		}

		[Token(Token = "0x6000064")]
		[Address(RVA = "0x2649E98", Offset = "0x2645E98", VA = "0x2649E98", Slot = "8")]
		protected virtual void OnLoadSuccess()
		{
		}

		[Token(Token = "0x6000065")]
		[Address(RVA = "0x264DFA0", Offset = "0x2649FA0", VA = "0x264DFA0", Slot = "9")]
		protected virtual void OnShowEffectStart()
		{
		}

		[Token(Token = "0x6000066")]
		[Address(RVA = "0x264E218", Offset = "0x264A218", VA = "0x264E218", Slot = "10")]
		protected virtual void OnShowEffectEnd()
		{
		}

		[Token(Token = "0x6000067")]
		[Address(RVA = "0x264E24C", Offset = "0x264A24C", VA = "0x264E24C", Slot = "11")]
		protected virtual void OnCloseEffectStart()
		{
		}

		[Token(Token = "0x6000068")]
		[Address(RVA = "0x264E3CC", Offset = "0x264A3CC", VA = "0x264E3CC", Slot = "12")]
		protected virtual void OnCloseEffectEnd()
		{
		}

		[Token(Token = "0x6000069")]
		[Address(RVA = "0x264E168", Offset = "0x264A168", VA = "0x264E168")]
		private void OnEffectStart()
		{
		}

		[Token(Token = "0x600006A")]
		[Address(RVA = "0x264E21C", Offset = "0x264A21C", VA = "0x264E21C")]
		private void OnEffectEnd()
		{
		}

		[Token(Token = "0x600006B")]
		[Address(RVA = "0x264E1A8", Offset = "0x264A1A8", VA = "0x264E1A8")]
		[IteratorStateMachine(typeof(<FadeInCoroutine>d__25))]
		private IEnumerator FadeInCoroutine(float time)
		{
			return null;
		}

		[Token(Token = "0x600006C")]
		[Address(RVA = "0x264E35C", Offset = "0x264A35C", VA = "0x264E35C")]
		[IteratorStateMachine(typeof(<FadeOutCoroutine>d__26))]
		private IEnumerator FadeOutCoroutine(float time)
		{
			return null;
		}

		[Token(Token = "0x600006D")]
		[Address(RVA = "0x264E490", Offset = "0x264A490", VA = "0x264E490", Slot = "13")]
		protected virtual void UIAdapt(Vector2Int res)
		{
		}

		[Token(Token = "0x600006E")]
		[Address(RVA = "0x264E494", Offset = "0x264A494", VA = "0x264E494", Slot = "14")]
		public virtual void Close()
		{
		}

		[Token(Token = "0x600006F")]
		[Address(RVA = "0x264E4E8", Offset = "0x264A4E8", VA = "0x264E4E8")]
		public void SetOpenOrder(int openOrder)
		{
		}

		[Token(Token = "0x6000070")]
		[Address(RVA = "0x264E580", Offset = "0x264A580", VA = "0x264E580", Slot = "15")]
		public virtual void Dispose()
		{
		}

		[Token(Token = "0x6000071")]
		[Address(RVA = "0x2649CC4", Offset = "0x2645CC4", VA = "0x2649CC4")]
		protected BasePanelController()
		{
		}
	}
	[Serializable]
	[Token(Token = "0x200001D")]
	public struct BasePanelConfig
	{
		[Token(Token = "0x4000083")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		public EAnimationMode animationType;

		[Token(Token = "0x600007E")]
		[Address(RVA = "0x264EBB0", Offset = "0x264ABB0", VA = "0x264EBB0")]
		public BasePanelConfig(EAnimationMode animationMode = EAnimationMode.None)
		{
		}
	}
	[Token(Token = "0x200001E")]
	public abstract class AbstractOpenPanelParameter
	{
		[Token(Token = "0x1700000F")]
		public virtual bool NeedConstantResolution
		{
			[Token(Token = "0x600007F")]
			[Address(RVA = "0x264EBB8", Offset = "0x264ABB8", VA = "0x264EBB8", Slot = "4")]
			get
			{
				return default(bool);
			}
		}

		[Token(Token = "0x6000080")]
		[Address(RVA = "0x2649D30", Offset = "0x2645D30", VA = "0x2649D30")]
		protected AbstractOpenPanelParameter()
		{
		}
	}
}
namespace TapSDK.UI.AillieoTech
{
	[Token(Token = "0x200001F")]
	public class SimpleObjPool<T>
	{
		[Token(Token = "0x4000084")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private readonly Stack<T> stack;

		[Token(Token = "0x4000085")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private readonly Func<T> ctor;

		[Token(Token = "0x4000086")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private readonly Action<T> onRecycle;

		[Token(Token = "0x4000087")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private int size;

		[Token(Token = "0x4000088")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private int usedCount;

		[Token(Token = "0x6000081")]
		public SimpleObjPool([Optional][DefaultParameterValue(7)] int max, [Optional] Action<T> onRecycle, [Optional] Func<T> ctor)
		{
		}

		[Token(Token = "0x6000082")]
		public T Get()
		{
			return (T)null;
		}

		[Token(Token = "0x6000083")]
		public void Recycle(T item)
		{
		}

		[Token(Token = "0x6000084")]
		public void Purge()
		{
		}

		[Token(Token = "0x6000085")]
		public override string ToString()
		{
			return null;
		}
	}
	[Token(Token = "0x2000020")]
	[RequireComponent(typeof(RectTransform))]
	[DisallowMultipleComponent]
	public class ScrollView : ScrollRect
	{
		[Token(Token = "0x2000021")]
		public enum ItemLayoutType
		{
			[Token(Token = "0x400009D")]
			Vertical = 1,
			[Token(Token = "0x400009E")]
			Horizontal = 2,
			[Token(Token = "0x400009F")]
			VerticalThenHorizontal = 4,
			[Token(Token = "0x40000A0")]
			HorizontalThenVertical = 5
		}

		[Token(Token = "0x2000022")]
		protected static class CriticalItemType
		{
			[Token(Token = "0x40000A1")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public static byte UpToHide;

			[Token(Token = "0x40000A2")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x1")]
			public static byte DownToHide;

			[Token(Token = "0x40000A3")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x2")]
			public static byte UpToShow;

			[Token(Token = "0x40000A4")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x3")]
			public static byte DownToShow;
		}

		[Token(Token = "0x2000023")]
		private class ScrollItemWithRect
		{
			[Token(Token = "0x40000A5")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			public RectTransform item;

			[Token(Token = "0x40000A6")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
			public Rect rect;

			[Token(Token = "0x40000A7")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public bool rectDirty;

			[Token(Token = "0x60000AE")]
			[Address(RVA = "0x264FC58", Offset = "0x264BC58", VA = "0x264FC58")]
			public ScrollItemWithRect()
			{
			}
		}

		[Token(Token = "0x2000025")]
		[CompilerGenerated]
		private sealed class <DelayUpdateData>d__38 : IEnumerator<object>, IEnumerator, IDisposable
		{
			[Token(Token = "0x40000AA")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			private int <>1__state;

			[Token(Token = "0x40000AB")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
			private object <>2__current;

			[Token(Token = "0x40000AC")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public ScrollView <>4__this;

			[Token(Token = "0x17000010")]
			private object System.Collections.Generic.IEnumerator<System.Object>.Current
			{
				[Token(Token = "0x60000B5")]
				[Address(RVA = "0x2650D78", Offset = "0x264CD78", VA = "0x2650D78", Slot = "4")]
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[Token(Token = "0x17000011")]
			private object System.Collections.IEnumerator.Current
			{
				[Token(Token = "0x60000B7")]
				[Address(RVA = "0x2650DB8", Offset = "0x264CDB8", VA = "0x2650DB8", Slot = "7")]
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[Token(Token = "0x60000B2")]
			[Address(RVA = "0x264FAE8", Offset = "0x264BAE8", VA = "0x264FAE8")]
			[DebuggerHidden]
			public <DelayUpdateData>d__38(int <>1__state)
			{
			}

			[Token(Token = "0x60000B3")]
			[Address(RVA = "0x2650CDC", Offset = "0x264CCDC", VA = "0x2650CDC", Slot = "5")]
			[DebuggerHidden]
			private void System.IDisposable.Dispose()
			{
			}

			[Token(Token = "0x60000B4")]
			[Address(RVA = "0x2650CE0", Offset = "0x264CCE0", VA = "0x2650CE0", Slot = "6")]
			private bool MoveNext()
			{
				return default(bool);
			}

			[Token(Token = "0x60000B6")]
			[Address(RVA = "0x2650D80", Offset = "0x264CD80", VA = "0x2650D80", Slot = "8")]
			[DebuggerHidden]
			private void System.Collections.IEnumerator.Reset()
			{
			}
		}

		[Token(Token = "0x4000089")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x130")]
		[Tooltip("默认item尺寸")]
		public Vector2 defaultItemSize;

		[Token(Token = "0x400008A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x138")]
		[Tooltip("item的模板")]
		public RectTransform itemTemplate;

		[Token(Token = "0x400008B")]
		protected const int flagScrollDirection = 1;

		[Token(Token = "0x400008C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x140")]
		[FormerlySerializedAs("m_layoutType")]
		[SerializeField]
		protected ItemLayoutType layoutType;

		[Token(Token = "0x400008D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x148")]
		protected int[] criticalItemIndex;

		[Token(Token = "0x400008E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x150")]
		protected Action<int, RectTransform> updateFunc;

		[Token(Token = "0x400008F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x158")]
		protected Func<int, Vector2> itemSizeFunc;

		[Token(Token = "0x4000090")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x160")]
		protected Func<int> itemCountFunc;

		[Token(Token = "0x4000091")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x168")]
		protected Func<int, RectTransform> itemGetFunc;

		[Token(Token = "0x4000092")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x170")]
		protected Action<RectTransform> itemRecycleFunc;

		[Token(Token = "0x4000093")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x178")]
		private readonly List<ScrollItemWithRect> managedItems;

		[Token(Token = "0x4000094")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x180")]
		private Rect refRect;

		[Token(Token = "0x4000095")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x190")]
		private SimpleObjPool<RectTransform> itemPool;

		[Token(Token = "0x4000096")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x198")]
		private int dataCount;

		[Token(Token = "0x4000097")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x19C")]
		[SerializeField]
		[Tooltip("初始化时池内item数量")]
		private int poolSize;

		[Token(Token = "0x4000098")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1A0")]
		private bool initialized;

		[Token(Token = "0x4000099")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1A4")]
		private int willUpdateData;

		[Token(Token = "0x400009A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1A8")]
		private Vector3[] viewWorldConers;

		[Token(Token = "0x400009B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1B0")]
		private Vector3[] rectCorners;

		[Token(Token = "0x6000086")]
		[Address(RVA = "0x264EBC0", Offset = "0x264ABC0", VA = "0x264EBC0", Slot = "61")]
		public virtual void SetUpdateFunc(Action<int, RectTransform> func)
		{
		}

		[Token(Token = "0x6000087")]
		[Address(RVA = "0x264EBC8", Offset = "0x264ABC8", VA = "0x264EBC8", Slot = "62")]
		public virtual void SetItemSizeFunc(Func<int, Vector2> func)
		{
		}

		[Token(Token = "0x6000088")]
		[Address(RVA = "0x264EBD0", Offset = "0x264ABD0", VA = "0x264EBD0", Slot = "63")]
		public virtual void SetItemCountFunc(Func<int> func)
		{
		}

		[Token(Token = "0x6000089")]
		[Address(RVA = "0x264EBD8", Offset = "0x264ABD8", VA = "0x264EBD8")]
		public void SetItemGetAndRecycleFunc(Func<int, RectTransform> getFunc, Action<RectTransform> recycleFunc)
		{
		}

		[Token(Token = "0x600008A")]
		[Address(RVA = "0x264EC00", Offset = "0x264AC00", VA = "0x264EC00")]
		public void ResetAllDelegates()
		{
		}

		[Token(Token = "0x600008B")]
		[Address(RVA = "0x264EC58", Offset = "0x264AC58", VA = "0x264EC58")]
		public void UpdateData(bool immediately = true)
		{
		}

		[Token(Token = "0x600008C")]
		[Address(RVA = "0x264F16C", Offset = "0x264B16C", VA = "0x264F16C")]
		public void UpdateDataIncrementally(bool immediately = true)
		{
		}

		[Token(Token = "0x600008D")]
		[Address(RVA = "0x264F1C0", Offset = "0x264B1C0", VA = "0x264F1C0")]
		public void ScrollTo(int index)
		{
		}

		[Token(Token = "0x600008E")]
		[Address(RVA = "0x264F1D0", Offset = "0x264B1D0", VA = "0x264F1D0", Slot = "5")]
		protected override void OnEnable()
		{
		}

		[Token(Token = "0x600008F")]
		[Address(RVA = "0x264F20C", Offset = "0x264B20C", VA = "0x264F20C", Slot = "7")]
		protected override void OnDisable()
		{
		}

		[Token(Token = "0x6000090")]
		[Address(RVA = "0x264F218", Offset = "0x264B218", VA = "0x264F218", Slot = "64")]
		protected virtual void InternalScrollTo(int index)
		{
		}

		[Token(Token = "0x6000091")]
		[Address(RVA = "0x264F54C", Offset = "0x264B54C", VA = "0x264F54C", Slot = "47")]
		protected override void SetContentAnchoredPosition(Vector2 position)
		{
		}

		[Token(Token = "0x6000092")]
		[Address(RVA = "0x264F660", Offset = "0x264B660", VA = "0x264F660", Slot = "49")]
		protected override void SetNormalizedPosition(float value, int axis)
		{
		}

		[Token(Token = "0x6000093")]
		[Address(RVA = "0x264F328", Offset = "0x264B328", VA = "0x264F328")]
		protected void EnsureItemRect(int index)
		{
		}

		[Token(Token = "0x6000094")]
		[Address(RVA = "0x264F9EC", Offset = "0x264B9EC", VA = "0x264F9EC", Slot = "8")]
		protected override void OnDestroy()
		{
		}

		[Token(Token = "0x6000095")]
		[Address(RVA = "0x264FA44", Offset = "0x264BA44", VA = "0x264FA44")]
		protected Rect GetItemLocalRect(int index)
		{
			return default(Rect);
		}

		[Token(Token = "0x6000096")]
		[Address(RVA = "0x264F950", Offset = "0x264B950", VA = "0x264F950")]
		private static Vector2 GetLeftTop(Rect rect)
		{
			return default(Vector2);
		}

		[Token(Token = "0x6000097")]
		[Address(RVA = "0x264F948", Offset = "0x264B948", VA = "0x264F948")]
		private static Rect CreateWithLeftTopAndSize(Vector2 leftTop, Vector2 size)
		{
			return default(Rect);
		}

		[Token(Token = "0x6000098")]
		[Address(RVA = "0x264F10C", Offset = "0x264B10C", VA = "0x264F10C")]
		[IteratorStateMachine(typeof(<DelayUpdateData>d__38))]
		private IEnumerator DelayUpdateData()
		{
			return null;
		}

		[Token(Token = "0x6000099")]
		[Address(RVA = "0x264ECC0", Offset = "0x264ACC0", VA = "0x264ECC0")]
		private void InternalUpdateData()
		{
		}

		[Token(Token = "0x600009A")]
		[Address(RVA = "0x264F67C", Offset = "0x264B67C", VA = "0x264F67C")]
		private void ResetCriticalItems()
		{
		}

		[Token(Token = "0x600009B")]
		[Address(RVA = "0x264FF00", Offset = "0x264BF00", VA = "0x264FF00")]
		private RectTransform GetCriticalItem(int type)
		{
			return null;
		}

		[Token(Token = "0x600009C")]
		[Address(RVA = "0x264F568", Offset = "0x264B568", VA = "0x264F568")]
		private void UpdateCriticalItems()
		{
		}

		[Token(Token = "0x600009D")]
		[Address(RVA = "0x264FF98", Offset = "0x264BF98", VA = "0x264FF98")]
		private bool CheckAndHideItem(int criticalItemType)
		{
			return default(bool);
		}

		[Token(Token = "0x600009E")]
		[Address(RVA = "0x26501C4", Offset = "0x264C1C4", VA = "0x26501C4")]
		private bool CheckAndShowItem(int criticalItemType)
		{
			return default(bool);
		}

		[Token(Token = "0x600009F")]
		[Address(RVA = "0x264FCE4", Offset = "0x264BCE4", VA = "0x264FCE4")]
		private bool ShouldItemSeenAtIndex(int index)
		{
			return default(bool);
		}

		[Token(Token = "0x60000A0")]
		[Address(RVA = "0x2650404", Offset = "0x264C404", VA = "0x2650404")]
		private bool ShouldItemFullySeenAtIndex(int index)
		{
			return default(bool);
		}

		[Token(Token = "0x60000A1")]
		[Address(RVA = "0x26504FC", Offset = "0x264C4FC", VA = "0x26504FC")]
		private bool IsRectContains(Rect outRect, Rect inRect, bool bothDimensions = false)
		{
			return default(bool);
		}

		[Token(Token = "0x60000A2")]
		[Address(RVA = "0x2650580", Offset = "0x264C580", VA = "0x2650580")]
		private void InitPool()
		{
		}

		[Token(Token = "0x60000A3")]
		[Address(RVA = "0x264FEBC", Offset = "0x264BEBC", VA = "0x264FEBC")]
		private void OnGetItemForDataIndex(RectTransform item, int index)
		{
		}

		[Token(Token = "0x60000A4")]
		[Address(RVA = "0x264FDF8", Offset = "0x264BDF8", VA = "0x264FDF8")]
		private void SetDataForItemAtIndex(RectTransform item, int index)
		{
		}

		[Token(Token = "0x60000A5")]
		[Address(RVA = "0x2650768", Offset = "0x264C768", VA = "0x2650768")]
		private void SetPosForItemAtIndex(RectTransform item, int index)
		{
		}

		[Token(Token = "0x60000A6")]
		[Address(RVA = "0x264F908", Offset = "0x264B908", VA = "0x264F908")]
		private Vector2 GetItemSize(int index)
		{
			return default(Vector2);
		}

		[Token(Token = "0x60000A7")]
		[Address(RVA = "0x264FE44", Offset = "0x264BE44", VA = "0x264FE44")]
		private RectTransform GetNewItem(int index)
		{
			return null;
		}

		[Token(Token = "0x60000A8")]
		[Address(RVA = "0x264FC68", Offset = "0x264BC68", VA = "0x264FC68")]
		private void RecycleOldItem(RectTransform item)
		{
		}

		[Token(Token = "0x60000A9")]
		[Address(RVA = "0x264FB10", Offset = "0x264BB10", VA = "0x264FB10")]
		private void InitScrollView()
		{
		}

		[Token(Token = "0x60000AA")]
		[Address(RVA = "0x2650814", Offset = "0x264C814", VA = "0x2650814")]
		private void UpdateRefRect()
		{
		}

		[Token(Token = "0x60000AB")]
		[Address(RVA = "0x264F958", Offset = "0x264B958", VA = "0x264F958")]
		private void MovePos(ref Vector2 pos, Vector2 size)
		{
		}

		[Token(Token = "0x60000AC")]
		[Address(RVA = "0x2650994", Offset = "0x264C994", VA = "0x2650994")]
		public ScrollView()
		{
		}
	}
	[Token(Token = "0x2000026")]
	[RequireComponent(typeof(RectTransform))]
	[DisallowMultipleComponent]
	public class ScrollViewEx : ScrollView
	{
		[Token(Token = "0x40000AD")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1B8")]
		[SerializeField]
		[FormerlySerializedAs("m_pageSize")]
		private int pageSize;

		[Token(Token = "0x40000AE")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1BC")]
		private int startOffset;

		[Token(Token = "0x40000AF")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1C0")]
		private Func<int> realItemCountFunc;

		[Token(Token = "0x40000B0")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1C8")]
		private Vector2 lastPosition;

		[Token(Token = "0x40000B1")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x1D0")]
		private bool reloadFlag;

		[Token(Token = "0x60000B8")]
		[Address(RVA = "0x2650DC0", Offset = "0x264CDC0", VA = "0x2650DC0", Slot = "61")]
		public override void SetUpdateFunc(Action<int, RectTransform> func)
		{
		}

		[Token(Token = "0x60000B9")]
		[Address(RVA = "0x2650E84", Offset = "0x264CE84", VA = "0x2650E84", Slot = "62")]
		public override void SetItemSizeFunc(Func<int, Vector2> func)
		{
		}

		[Token(Token = "0x60000BA")]
		[Address(RVA = "0x2650F48", Offset = "0x264CF48", VA = "0x2650F48", Slot = "63")]
		public override void SetItemCountFunc(Func<int> func)
		{
		}

		[Token(Token = "0x60000BB")]
		[Address(RVA = "0x2651010", Offset = "0x264D010", VA = "0x2651010", Slot = "46")]
		public override void OnDrag(PointerEventData eventData)
		{
		}

		[Token(Token = "0x60000BC")]
		[Address(RVA = "0x265107C", Offset = "0x264D07C", VA = "0x265107C", Slot = "4")]
		protected override void Awake()
		{
		}

		[Token(Token = "0x60000BD")]
		[Address(RVA = "0x2651164", Offset = "0x264D164", VA = "0x2651164", Slot = "64")]
		protected override void InternalScrollTo(int index)
		{
		}

		[Token(Token = "0x60000BE")]
		[Address(RVA = "0x2651224", Offset = "0x264D224", VA = "0x2651224")]
		private void OnValueChanged(Vector2 position)
		{
		}

		[Token(Token = "0x60000BF")]
		[Address(RVA = "0x26514E0", Offset = "0x264D4E0", VA = "0x26514E0")]
		public ScrollViewEx()
		{
		}
	}
}
namespace TapSDK.Core
{
	[Token(Token = "0x200002A")]
	public static class DataStorage
	{
		[Token(Token = "0x40000B8")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static Dictionary<string, string> dataCache;

		[Token(Token = "0x40000B9")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
		private static byte[] Keys;

		[Token(Token = "0x60000C6")]
		[Address(RVA = "0x2651594", Offset = "0x264D594", VA = "0x2651594")]
		public static void SaveString(string key, string value)
		{
		}

		[Token(Token = "0x60000C7")]
		[Address(RVA = "0x2651B70", Offset = "0x264DB70", VA = "0x2651B70")]
		public static string LoadString(string key)
		{
			return null;
		}

		[Token(Token = "0x60000C8")]
		[Address(RVA = "0x26516FC", Offset = "0x264D6FC", VA = "0x26516FC")]
		private static void SaveStringToCache(string key, string value)
		{
		}

		[Token(Token = "0x60000C9")]
		[Address(RVA = "0x2651CC8", Offset = "0x264DCC8", VA = "0x2651CC8")]
		private static string LoadStringFromCache(string key)
		{
			return null;
		}

		[Token(Token = "0x60000CA")]
		[Address(RVA = "0x2651880", Offset = "0x264D880", VA = "0x2651880")]
		private static string EncodeString(string encryptString)
		{
			return null;
		}

		[Token(Token = "0x60000CB")]
		[Address(RVA = "0x2651E14", Offset = "0x264DE14", VA = "0x2651E14")]
		private static string DecodeString(string decryptString)
		{
			return null;
		}

		[Token(Token = "0x60000CC")]
		[Address(RVA = "0x2652208", Offset = "0x264E208", VA = "0x2652208")]
		private static string GetMacAddress()
		{
			return null;
		}

		[Token(Token = "0x60000CD")]
		[Address(RVA = "0x2651618", Offset = "0x264D618", VA = "0x2651618")]
		private static string GenerateStorageKey(string originKey)
		{
			return null;
		}
	}
	[Token(Token = "0x200002B")]
	public interface ITapPropertiesProxy
	{
		[Token(Token = "0x60000CF")]
		string GetProperties();
	}
	[Token(Token = "0x200002C")]
	public static class Json
	{
		[Token(Token = "0x200002D")]
		private sealed class Parser : IDisposable
		{
			[Token(Token = "0x200002E")]
			private enum TOKEN
			{
				[Token(Token = "0x40000BD")]
				NONE,
				[Token(Token = "0x40000BE")]
				CURLY_OPEN,
				[Token(Token = "0x40000BF")]
				CURLY_CLOSE,
				[Token(Token = "0x40000C0")]
				SQUARED_OPEN,
				[Token(Token = "0x40000C1")]
				SQUARED_CLOSE,
				[Token(Token = "0x40000C2")]
				COLON,
				[Token(Token = "0x40000C3")]
				COMMA,
				[Token(Token = "0x40000C4")]
				STRING,
				[Token(Token = "0x40000C5")]
				NUMBER,
				[Token(Token = "0x40000C6")]
				TRUE,
				[Token(Token = "0x40000C7")]
				FALSE,
				[Token(Token = "0x40000C8")]
				NULL
			}

			[Token(Token = "0x40000BA")]
			private const string WORD_BREAK = "{}[],:\"";

			[Token(Token = "0x40000BB")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			private StringReader json;

			[Token(Token = "0x17000012")]
			private char PeekChar
			{
				[Token(Token = "0x60000DD")]
				[Address(RVA = "0x26531B0", Offset = "0x264F1B0", VA = "0x26531B0")]
				get
				{
					return default(char);
				}
			}

			[Token(Token = "0x17000013")]
			private char NextChar
			{
				[Token(Token = "0x60000DE")]
				[Address(RVA = "0x2652FE0", Offset = "0x264EFE0", VA = "0x2652FE0")]
				get
				{
					return default(char);
				}
			}

			[Token(Token = "0x17000014")]
			private string NextWord
			{
				[Token(Token = "0x60000DF")]
				[Address(RVA = "0x2653058", Offset = "0x264F058", VA = "0x2653058")]
				get
				{
					return null;
				}
			}

			[Token(Token = "0x17000015")]
			private TOKEN NextToken
			{
				[Token(Token = "0x60000E0")]
				[Address(RVA = "0x2652874", Offset = "0x264E874", VA = "0x2652874")]
				get
				{
					return default(TOKEN);
				}
			}

			[Token(Token = "0x60000D2")]
			[Address(RVA = "0x26525FC", Offset = "0x264E5FC", VA = "0x26525FC")]
			public static bool IsWordBreak(char c)
			{
				return default(bool);
			}

			[Token(Token = "0x60000D3")]
			[Address(RVA = "0x2652698", Offset = "0x264E698", VA = "0x2652698")]
			private Parser(string jsonString)
			{
			}

			[Token(Token = "0x60000D4")]
			[Address(RVA = "0x26523DC", Offset = "0x264E3DC", VA = "0x26523DC")]
			public static object Parse(string jsonString)
			{
				return null;
			}

			[Token(Token = "0x60000D5")]
			[Address(RVA = "0x2652728", Offset = "0x264E728", VA = "0x2652728", Slot = "4")]
			public void Dispose()
			{
			}

			[Token(Token = "0x60000D6")]
			[Address(RVA = "0x2652750", Offset = "0x264E750", VA = "0x2652750")]
			private Dictionary<string, object> ParseObject()
			{
				return null;
			}

			[Token(Token = "0x60000D7")]
			[Address(RVA = "0x2652CB8", Offset = "0x264ECB8", VA = "0x2652CB8")]
			private List<object> ParseArray()
			{
				return null;
			}

			[Token(Token = "0x60000D8")]
			[Address(RVA = "0x265270C", Offset = "0x264E70C", VA = "0x265270C")]
			private object ParseValue()
			{
				return null;
			}

			[Token(Token = "0x60000D9")]
			[Address(RVA = "0x2652DE0", Offset = "0x264EDE0", VA = "0x2652DE0")]
			private object ParseByToken(TOKEN token)
			{
				return null;
			}

			[Token(Token = "0x60000DA")]
			[Address(RVA = "0x2652A38", Offset = "0x264EA38", VA = "0x2652A38")]
			private string ParseString()
			{
				return null;
			}

			[Token(Token = "0x60000DB")]
			[Address(RVA = "0x2652ED0", Offset = "0x264EED0", VA = "0x2652ED0")]
			private object ParseNumber()
			{
				return null;
			}

			[Token(Token = "0x60000DC")]
			[Address(RVA = "0x265310C", Offset = "0x264F10C", VA = "0x265310C")]
			private void EatWhitespace()
			{
			}
		}

		[Token(Token = "0x200002F")]
		private sealed class Serializer
		{
			[Token(Token = "0x40000C9")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			private StringBuilder builder;

			[Token(Token = "0x60000E1")]
			[Address(RVA = "0x2653228", Offset = "0x264F228", VA = "0x2653228")]
			private Serializer()
			{
			}

			[Token(Token = "0x60000E2")]
			[Address(RVA = "0x2652588", Offset = "0x264E588", VA = "0x2652588")]
			public static string Serialize(object obj)
			{
				return null;
			}

			[Token(Token = "0x60000E3")]
			[Address(RVA = "0x265328C", Offset = "0x264F28C", VA = "0x265328C")]
			private void SerializeValue(object value)
			{
			}

			[Token(Token = "0x60000E4")]
			[Address(RVA = "0x2653A78", Offset = "0x264FA78", VA = "0x2653A78")]
			private void SerializeObject(IDictionary obj)
			{
			}

			[Token(Token = "0x60000E5")]
			[Address(RVA = "0x2653718", Offset = "0x264F718", VA = "0x2653718")]
			private void SerializeArray(IList anArray)
			{
			}

			[Token(Token = "0x60000E6")]
			[Address(RVA = "0x265345C", Offset = "0x264F45C", VA = "0x265345C")]
			private void SerializeString(string str)
			{
			}

			[Token(Token = "0x60000E7")]
			[Address(RVA = "0x2653F04", Offset = "0x264FF04", VA = "0x2653F04")]
			private void SerializeOther(object value)
			{
			}
		}

		[Token(Token = "0x60000D0")]
		[Address(RVA = "0x26523D0", Offset = "0x264E3D0", VA = "0x26523D0")]
		public static object Deserialize(string json)
		{
			return null;
		}

		[Token(Token = "0x60000D1")]
		[Address(RVA = "0x2652584", Offset = "0x264E584", VA = "0x2652584")]
		public static string Serialize(object obj)
		{
			return null;
		}
	}
	[Token(Token = "0x2000030")]
	public class TapLogger
	{
		[Token(Token = "0x17000016")]
		public static Action<TapLogLevel, string> LogDelegate
		{
			[Token(Token = "0x60000E8")]
			[Address(RVA = "0x26541DC", Offset = "0x26501DC", VA = "0x26541DC")]
			[CompilerGenerated]
			get
			{
				return null;
			}
			[Token(Token = "0x60000E9")]
			[Address(RVA = "0x2654224", Offset = "0x2650224", VA = "0x2654224")]
			[CompilerGenerated]
			set
			{
			}
		}

		[Token(Token = "0x60000EA")]
		[Address(RVA = "0x2654270", Offset = "0x2650270", VA = "0x2654270")]
		public static void Debug(string log)
		{
		}

		[Token(Token = "0x60000EB")]
		[Address(RVA = "0x26542E0", Offset = "0x26502E0", VA = "0x26542E0")]
		public static void Debug(string format, params object[] args)
		{
		}

		[Token(Token = "0x60000EC")]
		[Address(RVA = "0x2654368", Offset = "0x2650368", VA = "0x2654368")]
		public static void Warn(string log)
		{
		}

		[Token(Token = "0x60000ED")]
		[Address(RVA = "0x26543D8", Offset = "0x26503D8", VA = "0x26543D8")]
		public static void Warn(string format, params object[] args)
		{
		}

		[Token(Token = "0x60000EE")]
		[Address(RVA = "0x2654460", Offset = "0x2650460", VA = "0x2654460")]
		public static void Error(string log)
		{
		}

		[Token(Token = "0x60000EF")]
		[Address(RVA = "0x26544D0", Offset = "0x26504D0", VA = "0x26544D0")]
		public static void Error(string format, params object[] args)
		{
		}

		[Token(Token = "0x60000F0")]
		[Address(RVA = "0x2654558", Offset = "0x2650558", VA = "0x2654558")]
		public static void Error(Exception e)
		{
		}

		[Token(Token = "0x60000F1")]
		[Address(RVA = "0x2654658", Offset = "0x2650658", VA = "0x2654658")]
		public TapLogger()
		{
		}
	}
	[Token(Token = "0x2000031")]
	public enum TapLogLevel
	{
		[Token(Token = "0x40000CC")]
		Debug,
		[Token(Token = "0x40000CD")]
		Warn,
		[Token(Token = "0x40000CE")]
		Error
	}
	[Token(Token = "0x2000032")]
	public class Platform
	{
		[Token(Token = "0x60000F2")]
		[Address(RVA = "0x2652150", Offset = "0x264E150", VA = "0x2652150")]
		public static bool IsAndroid()
		{
			return default(bool);
		}

		[Token(Token = "0x60000F3")]
		[Address(RVA = "0x26521AC", Offset = "0x264E1AC", VA = "0x26521AC")]
		public static bool IsIOS()
		{
			return default(bool);
		}

		[Token(Token = "0x60000F4")]
		[Address(RVA = "0x2654660", Offset = "0x2650660", VA = "0x2654660")]
		public static bool IsWin32()
		{
			return default(bool);
		}

		[Token(Token = "0x60000F5")]
		[Address(RVA = "0x26546BC", Offset = "0x26506BC", VA = "0x26546BC")]
		public static bool IsMacOS()
		{
			return default(bool);
		}

		[Token(Token = "0x60000F6")]
		[Address(RVA = "0x2654718", Offset = "0x2650718", VA = "0x2654718")]
		public Platform()
		{
		}
	}
	[Token(Token = "0x2000033")]
	public enum RegionType
	{
		[Token(Token = "0x40000D0")]
		CN,
		[Token(Token = "0x40000D1")]
		IO
	}
	[Token(Token = "0x2000034")]
	public static class SafeDictionary
	{
		[Token(Token = "0x60000F7")]
		public static T GetValue<T>(Dictionary<string, object> dic, string key, [Optional] T defaultVal)
		{
			return (T)null;
		}
	}
	[Token(Token = "0x2000035")]
	public class TapError
	{
		[Token(Token = "0x40000D2")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		public int code;

		[Token(Token = "0x40000D3")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		public string errorDescription;

		[Token(Token = "0x60000F8")]
		[Address(RVA = "0x2654720", Offset = "0x2650720", VA = "0x2654720")]
		public TapError(string json)
		{
		}

		[Token(Token = "0x60000F9")]
		[Address(RVA = "0x2654844", Offset = "0x2650844", VA = "0x2654844")]
		public TapError()
		{
		}

		[Token(Token = "0x60000FA")]
		[Address(RVA = "0x265484C", Offset = "0x265084C", VA = "0x265484C")]
		public static TapError SafeConstructorTapError(string json)
		{
			return null;
		}

		[Token(Token = "0x60000FB")]
		[Address(RVA = "0x26548BC", Offset = "0x26508BC", VA = "0x26548BC")]
		public static TapError UndefinedError()
		{
			return null;
		}

		[Token(Token = "0x60000FC")]
		[Address(RVA = "0x2654968", Offset = "0x2650968", VA = "0x2654968")]
		public static TapError LoginCancelError()
		{
			return null;
		}

		[Token(Token = "0x60000FD")]
		[Address(RVA = "0x26549E4", Offset = "0x26509E4", VA = "0x26549E4")]
		public TapError(int code, string errorDescription)
		{
		}

		[Token(Token = "0x60000FE")]
		[Address(RVA = "0x2654A14", Offset = "0x2650A14", VA = "0x2654A14")]
		private static TapErrorCode ParseCode(int parseCode)
		{
			return default(TapErrorCode);
		}

		[Token(Token = "0x60000FF")]
		[Address(RVA = "0x2654938", Offset = "0x2650938", VA = "0x2654938")]
		public TapError(TapErrorCode code, string errorDescription)
		{
		}
	}
	[Token(Token = "0x2000036")]
	public enum TapErrorCode
	{
		[Token(Token = "0x40000D5")]
		ERROR_CODE_UNDEFINED = 80000,
		[Token(Token = "0x40000D6")]
		ERROR_CODE_UNINITIALIZED = 80001,
		[Token(Token = "0x40000D7")]
		ERROR_CODE_BIND_CANCEL = 80002,
		[Token(Token = "0x40000D8")]
		ERROR_CODE_BIND_ERROR = 80003,
		[Token(Token = "0x40000D9")]
		ERROR_CODE_LOGOUT_INVALID_LOGIN_STATE = 80004,
		[Token(Token = "0x40000DA")]
		ERROR_CODE_LOGOUT_KICKED = 80007,
		[Token(Token = "0x40000DB")]
		ERROR_CODE_BRIDGE_EXECUTE = 80080,
		[Token(Token = "0x40000DC")]
		ERROR_CODE_LOGIN_CANCEL = 80081
	}
	[Token(Token = "0x2000037")]
	public class TapException : Exception
	{
		[Token(Token = "0x40000DD")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8C")]
		public int code;

		[Token(Token = "0x40000DE")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x90")]
		public string message;

		[Token(Token = "0x17000017")]
		public int Code
		{
			[Token(Token = "0x6000100")]
			[Address(RVA = "0x2654B94", Offset = "0x2650B94", VA = "0x2654B94")]
			get
			{
				return default(int);
			}
			[Token(Token = "0x6000101")]
			[Address(RVA = "0x2654B9C", Offset = "0x2650B9C", VA = "0x2654B9C")]
			set
			{
			}
		}

		[Token(Token = "0x6000102")]
		[Address(RVA = "0x2654BA4", Offset = "0x2650BA4", VA = "0x2654BA4")]
		public TapException(int code, string message)
		{
		}
	}
	[Token(Token = "0x2000038")]
	public enum TapLanguage
	{
		[Token(Token = "0x40000E0")]
		AUTO,
		[Token(Token = "0x40000E1")]
		ZH_HANS,
		[Token(Token = "0x40000E2")]
		EN,
		[Token(Token = "0x40000E3")]
		ZH_HANT,
		[Token(Token = "0x40000E4")]
		JA,
		[Token(Token = "0x40000E5")]
		KO,
		[Token(Token = "0x40000E6")]
		TH,
		[Token(Token = "0x40000E7")]
		ID,
		[Token(Token = "0x40000E8")]
		DE,
		[Token(Token = "0x40000E9")]
		ES,
		[Token(Token = "0x40000EA")]
		FR,
		[Token(Token = "0x40000EB")]
		PT,
		[Token(Token = "0x40000EC")]
		RU,
		[Token(Token = "0x40000ED")]
		TR,
		[Token(Token = "0x40000EE")]
		VI
	}
	[Token(Token = "0x2000039")]
	public class TapLocalizeManager
	{
		[Token(Token = "0x40000EF")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static TapLocalizeManager _instance;

		[Token(Token = "0x40000F0")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
		private static readonly object ObjLock;

		[Token(Token = "0x40000F1")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private TapLanguage _language;

		[Token(Token = "0x40000F2")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x14")]
		private bool _regionIsCn;

		[Token(Token = "0x17000018")]
		public static TapLocalizeManager Instance
		{
			[Token(Token = "0x6000103")]
			[Address(RVA = "0x2654C1C", Offset = "0x2650C1C", VA = "0x2654C1C")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000104")]
		[Address(RVA = "0x2654DC4", Offset = "0x2650DC4", VA = "0x2654DC4")]
		public static void SetCurrentRegion(bool isCn)
		{
		}

		[Token(Token = "0x6000105")]
		[Address(RVA = "0x2654E28", Offset = "0x2650E28", VA = "0x2654E28")]
		public static void SetCurrentLanguage(TapLanguage language)
		{
		}

		[Token(Token = "0x6000106")]
		[Address(RVA = "0x2654E88", Offset = "0x2650E88", VA = "0x2654E88")]
		public static TapLanguage GetCurrentLanguage()
		{
			return default(TapLanguage);
		}

		[Token(Token = "0x6000107")]
		[Address(RVA = "0x2655058", Offset = "0x2651058", VA = "0x2655058")]
		public static string GetCurrentLanguageString()
		{
			return null;
		}

		[Token(Token = "0x6000108")]
		[Address(RVA = "0x265517C", Offset = "0x265117C", VA = "0x265517C")]
		public static string GetCurrentLanguageString2()
		{
			return null;
		}

		[Token(Token = "0x6000109")]
		[Address(RVA = "0x2654F80", Offset = "0x2650F80", VA = "0x2654F80")]
		private static TapLanguage GetSystemLanguage()
		{
			return default(TapLanguage);
		}

		[Token(Token = "0x600010A")]
		[Address(RVA = "0x2654DBC", Offset = "0x2650DBC", VA = "0x2654DBC")]
		public TapLocalizeManager()
		{
		}
	}
	[Token(Token = "0x200003A")]
	public class TapTapEvent
	{
		[Token(Token = "0x40000F3")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static ITapEventPlatform platformWrapper;

		[Token(Token = "0x600010C")]
		[Address(RVA = "0x2655310", Offset = "0x2651310", VA = "0x2655310")]
		static TapTapEvent()
		{
		}

		[Token(Token = "0x600010D")]
		[Address(RVA = "0x2655808", Offset = "0x2651808", VA = "0x2655808")]
		internal static void Init(TapTapEventOptions eventOptions)
		{
		}

		[Token(Token = "0x600010E")]
		[Address(RVA = "0x26558D8", Offset = "0x26518D8", VA = "0x26558D8")]
		public static void SetUserID(string userID)
		{
		}

		[Token(Token = "0x600010F")]
		[Address(RVA = "0x26559B4", Offset = "0x26519B4", VA = "0x26559B4")]
		public static void SetUserID(string userID, string properties)
		{
		}

		[Token(Token = "0x6000110")]
		[Address(RVA = "0x2655AA4", Offset = "0x2651AA4", VA = "0x2655AA4")]
		public static void ClearUser()
		{
		}

		[Token(Token = "0x6000111")]
		[Address(RVA = "0x2655B78", Offset = "0x2651B78", VA = "0x2655B78")]
		public static string GetDeviceId()
		{
			return null;
		}

		[Token(Token = "0x6000112")]
		[Address(RVA = "0x2655C88", Offset = "0x2651C88", VA = "0x2655C88")]
		public static void LogEvent(string name, string properties)
		{
		}

		[Token(Token = "0x6000113")]
		[Address(RVA = "0x2655D78", Offset = "0x2651D78", VA = "0x2655D78")]
		public static void DeviceInitialize(string properties)
		{
		}

		[Token(Token = "0x6000114")]
		[Address(RVA = "0x2655E54", Offset = "0x2651E54", VA = "0x2655E54")]
		public static void DeviceUpdate(string properties)
		{
		}

		[Token(Token = "0x6000115")]
		[Address(RVA = "0x2655F30", Offset = "0x2651F30", VA = "0x2655F30")]
		public static void DeviceAdd(string properties)
		{
		}

		[Token(Token = "0x6000116")]
		[Address(RVA = "0x265600C", Offset = "0x265200C", VA = "0x265600C")]
		public static void UserInitialize(string properties)
		{
		}

		[Token(Token = "0x6000117")]
		[Address(RVA = "0x26560E8", Offset = "0x26520E8", VA = "0x26560E8")]
		public static void UserUpdate(string properties)
		{
		}

		[Token(Token = "0x6000118")]
		[Address(RVA = "0x26561C4", Offset = "0x26521C4", VA = "0x26561C4")]
		public static void UserAdd(string properties)
		{
		}

		[Token(Token = "0x6000119")]
		[Address(RVA = "0x26562A0", Offset = "0x26522A0", VA = "0x26562A0")]
		public static void AddCommonProperty(string key, string value)
		{
		}

		[Token(Token = "0x600011A")]
		[Address(RVA = "0x2656390", Offset = "0x2652390", VA = "0x2656390")]
		public static void AddCommon(string properties)
		{
		}

		[Token(Token = "0x600011B")]
		[Address(RVA = "0x265646C", Offset = "0x265246C", VA = "0x265646C")]
		public static void ClearCommonProperty(string key)
		{
		}

		[Token(Token = "0x600011C")]
		[Address(RVA = "0x2656548", Offset = "0x2652548", VA = "0x2656548")]
		public static void ClearCommonProperties(string[] keys)
		{
		}

		[Token(Token = "0x600011D")]
		[Address(RVA = "0x2656624", Offset = "0x2652624", VA = "0x2656624")]
		public static void ClearAllCommonProperties()
		{
		}

		[Token(Token = "0x600011E")]
		[Address(RVA = "0x26566F8", Offset = "0x26526F8", VA = "0x26566F8")]
		public static void LogPurchasedEvent(string orderID, string productName, long amount, string currencyType, string paymentMethod, string properties)
		{
		}

		[Token(Token = "0x600011F")]
		[Address(RVA = "0x2656820", Offset = "0x2652820", VA = "0x2656820")]
		public static void RegisterDynamicProperties(Func<string> callback)
		{
		}

		[Token(Token = "0x6000120")]
		[Address(RVA = "0x26568FC", Offset = "0x26528FC", VA = "0x26568FC")]
		public static void SetOAID(string value)
		{
		}

		[Token(Token = "0x6000121")]
		[Address(RVA = "0x26569D8", Offset = "0x26529D8", VA = "0x26569D8")]
		public static void LogDeviceLoginEvent()
		{
		}

		[Token(Token = "0x6000122")]
		[Address(RVA = "0x2656AAC", Offset = "0x2652AAC", VA = "0x2656AAC")]
		public TapTapEvent()
		{
		}
	}
	[Token(Token = "0x200003B")]
	public class TapTapSDK
	{
		[Token(Token = "0x40000F4")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		public static readonly string Version;

		[Token(Token = "0x40000F5")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
		public static string SDKPlatform;

		[Token(Token = "0x40000F6")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		public static TapTapSdkOptions taptapSdkOptions;

		[Token(Token = "0x40000F7")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		private static ITapCorePlatform platformWrapper;

		[Token(Token = "0x40000F8")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		private static bool disableDurationStatistics;

		[Token(Token = "0x17000019")]
		public static bool DisableDurationStatistics
		{
			[Token(Token = "0x6000123")]
			[Address(RVA = "0x2656AB4", Offset = "0x2652AB4", VA = "0x2656AB4")]
			get
			{
				return default(bool);
			}
			[Token(Token = "0x6000124")]
			[Address(RVA = "0x2656B0C", Offset = "0x2652B0C", VA = "0x2656B0C")]
			set
			{
			}
		}

		[Token(Token = "0x6000125")]
		[Address(RVA = "0x2656B6C", Offset = "0x2652B6C", VA = "0x2656B6C")]
		static TapTapSDK()
		{
		}

		[Token(Token = "0x6000126")]
		[Address(RVA = "0x2656C98", Offset = "0x2652C98", VA = "0x2656C98")]
		public static void Init(TapTapSdkOptions coreOption)
		{
		}

		[Token(Token = "0x6000127")]
		[Address(RVA = "0x26575F0", Offset = "0x26535F0", VA = "0x26575F0")]
		public static void Init(TapTapSdkOptions coreOption, TapTapSdkBaseOptions[] otherOptions)
		{
		}

		[Token(Token = "0x6000128")]
		[Address(RVA = "0x26574F0", Offset = "0x26534F0", VA = "0x26574F0")]
		private static TapTapEventOptions HandleEventOptions(TapTapSdkOptions coreOption, [Optional] TapTapSdkBaseOptions[] otherOptions)
		{
			return null;
		}

		[Token(Token = "0x6000129")]
		[Address(RVA = "0x2657CD8", Offset = "0x2653CD8", VA = "0x2657CD8")]
		public static void UpdateLanguage(TapTapLanguageType language)
		{
		}

		[Token(Token = "0x600012A")]
		[Address(RVA = "0x2657DB4", Offset = "0x2653DB4", VA = "0x2657DB4")]
		public static Task<bool> IsLaunchedFromTapTapPC()
		{
			return null;
		}

		[Token(Token = "0x600012B")]
		[Address(RVA = "0x265720C", Offset = "0x265320C", VA = "0x265720C")]
		private static Type[] GetInitTypeList()
		{
			return null;
		}

		[Token(Token = "0x600012C")]
		[Address(RVA = "0x2657E94", Offset = "0x2653E94", VA = "0x2657E94")]
		public TapTapSDK()
		{
		}
	}
	[Token(Token = "0x200003E")]
	public interface TapTapSdkBaseOptions
	{
		[Token(Token = "0x1700001A")]
		string moduleName
		{
			[Token(Token = "0x6000135")]
			get;
		}
	}
	[Token(Token = "0x200003F")]
	public enum TapTapRegionType
	{
		[Token(Token = "0x4000100")]
		CN,
		[Token(Token = "0x4000101")]
		Overseas
	}
	[Token(Token = "0x2000040")]
	public enum TapTapLanguageType
	{
		[Token(Token = "0x4000103")]
		Auto,
		[Token(Token = "0x4000104")]
		zh_Hans,
		[Token(Token = "0x4000105")]
		en,
		[Token(Token = "0x4000106")]
		zh_Hant,
		[Token(Token = "0x4000107")]
		ja,
		[Token(Token = "0x4000108")]
		ko,
		[Token(Token = "0x4000109")]
		th,
		[Token(Token = "0x400010A")]
		id,
		[Token(Token = "0x400010B")]
		de,
		[Token(Token = "0x400010C")]
		es,
		[Token(Token = "0x400010D")]
		fr,
		[Token(Token = "0x400010E")]
		pt,
		[Token(Token = "0x400010F")]
		ru,
		[Token(Token = "0x4000110")]
		tr,
		[Token(Token = "0x4000111")]
		vi
	}
	[Token(Token = "0x2000041")]
	public class TapTapSdkOptions : TapTapSdkBaseOptions
	{
		[Token(Token = "0x4000112")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		public string clientId;

		[Token(Token = "0x4000113")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		public string clientToken;

		[Token(Token = "0x4000114")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		public string clientPublicKey;

		[Token(Token = "0x4000115")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		public TapTapRegionType region;

		[Token(Token = "0x4000116")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x2C")]
		public TapTapLanguageType preferredLanguage;

		[Token(Token = "0x4000117")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
		[Obsolete("该属性已废弃，请在 TapTapEventOptions 设置对应属性")]
		public string channel;

		[Token(Token = "0x4000118")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
		public string gameVersion;

		[Token(Token = "0x4000119")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
		[Obsolete("该属性已废弃，请在 TapTapEventOptions 设置对应属性")]
		public string propertiesJson;

		[Token(Token = "0x400011A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x48")]
		public string caid;

		[Token(Token = "0x400011B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x50")]
		[Obsolete("该属性已废弃，请在 TapTapEventOptions 设置对应属性")]
		public bool overrideBuiltInParameters;

		[Token(Token = "0x400011C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x51")]
		public bool enableAdvertiserIDCollection;

		[Token(Token = "0x400011D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x52")]
		[Obsolete("该属性已废弃，请在 TapTapEventOptions 设置对应属性")]
		public bool enableAutoIAPEvent;

		[Token(Token = "0x400011E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x58")]
		public string oaidCert;

		[Token(Token = "0x400011F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x60")]
		public bool enableLog;

		[Token(Token = "0x4000120")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x61")]
		public bool disableReflectionOAID;

		[Token(Token = "0x4000121")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x62")]
		[Obsolete("该属性已废弃，请在 TapTapEventOptions 设置对应属性")]
		public bool disableAutoLogDeviceLogin;

		[Token(Token = "0x4000122")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x64")]
		public int screenOrientation;

		[Token(Token = "0x4000123")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x68")]
		[JsonProperty("moduleName")]
		private string _moduleName;

		[Token(Token = "0x1700001B")]
		[JsonIgnore]
		public string moduleName
		{
			[Token(Token = "0x6000136")]
			[Address(RVA = "0x265811C", Offset = "0x265411C", VA = "0x265811C", Slot = "4")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000137")]
		[Address(RVA = "0x2658124", Offset = "0x2654124", VA = "0x2658124")]
		public TapTapSdkOptions()
		{
		}
	}
	[Token(Token = "0x2000042")]
	public class TapTapEventOptions : TapTapSdkBaseOptions
	{
		[Token(Token = "0x4000124")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		public bool enableTapTapEvent;

		[Token(Token = "0x4000125")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		public string channel;

		[Token(Token = "0x4000126")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		public string propertiesJson;

		[Token(Token = "0x4000127")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		public bool overrideBuiltInParameters;

		[Token(Token = "0x4000128")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x29")]
		public bool enableAutoIAPEvent;

		[Token(Token = "0x4000129")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x2A")]
		public bool disableAutoLogDeviceLogin;

		[Token(Token = "0x400012A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
		[JsonProperty("moduleName")]
		private string _moduleName;

		[Token(Token = "0x1700001C")]
		[JsonIgnore]
		public string moduleName
		{
			[Token(Token = "0x6000138")]
			[Address(RVA = "0x2658180", Offset = "0x2654180", VA = "0x2658180", Slot = "4")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x6000139")]
		[Address(RVA = "0x2657C7C", Offset = "0x2653C7C", VA = "0x2657C7C")]
		public TapTapEventOptions()
		{
		}
	}
}
namespace TapSDK.Core.Internal
{
	[Token(Token = "0x2000043")]
	public interface ITapCorePlatform
	{
		[Token(Token = "0x600013A")]
		void Init(TapTapSdkOptions config);

		[Token(Token = "0x600013B")]
		void Init(TapTapSdkOptions coreOption, TapTapSdkBaseOptions[] otherOptions);

		[Token(Token = "0x600013C")]
		void UpdateLanguage(TapTapLanguageType language);

		[Token(Token = "0x600013D")]
		Task<bool> IsLaunchedFromTapTapPC();
	}
	[Token(Token = "0x2000044")]
	public interface ITapEventPlatform
	{
		[Token(Token = "0x600013E")]
		void Init(TapTapEventOptions eventOptions);

		[Token(Token = "0x600013F")]
		void SetUserID(string userID);

		[Token(Token = "0x6000140")]
		void SetUserID(string userID, string properties);

		[Token(Token = "0x6000141")]
		void ClearUser();

		[Token(Token = "0x6000142")]
		string GetDeviceId();

		[Token(Token = "0x6000143")]
		void LogEvent(string name, string properties);

		[Token(Token = "0x6000144")]
		void DeviceInitialize(string properties);

		[Token(Token = "0x6000145")]
		void DeviceUpdate(string properties);

		[Token(Token = "0x6000146")]
		void DeviceAdd(string properties);

		[Token(Token = "0x6000147")]
		void UserInitialize(string properties);

		[Token(Token = "0x6000148")]
		void UserUpdate(string properties);

		[Token(Token = "0x6000149")]
		void UserAdd(string properties);

		[Token(Token = "0x600014A")]
		void AddCommonProperty(string key, string value);

		[Token(Token = "0x600014B")]
		void AddCommon(string properties);

		[Token(Token = "0x600014C")]
		void ClearCommonProperty(string key);

		[Token(Token = "0x600014D")]
		void ClearCommonProperties(string[] keys);

		[Token(Token = "0x600014E")]
		void ClearAllCommonProperties();

		[Token(Token = "0x600014F")]
		void LogChargeEvent(string orderID, string productName, long amount, string currencyType, string paymentMethod, string properties);

		[Token(Token = "0x6000150")]
		void RegisterDynamicProperties(Func<string> callback);

		[Token(Token = "0x6000151")]
		void SetOAID(string value);

		[Token(Token = "0x6000152")]
		void LogDeviceLoginEvent();
	}
	[Token(Token = "0x2000045")]
	public static class PlatformTypeUtils
	{
		[Token(Token = "0x6000153")]
		[Address(RVA = "0x2655450", Offset = "0x2651450", VA = "0x2655450")]
		public static object CreatePlatformImplementationObject(Type interfaceType, string startWith)
		{
			return null;
		}
	}
}
namespace TapSDK.Core.Internal.Utils
{
	[Token(Token = "0x2000048")]
	public static class BridgeUtils
	{
		[Token(Token = "0x1700001D")]
		public static bool IsSupportMobilePlatform
		{
			[Token(Token = "0x600015A")]
			[Address(RVA = "0x26582AC", Offset = "0x26542AC", VA = "0x26582AC")]
			get
			{
				return default(bool);
			}
		}

		[Token(Token = "0x1700001E")]
		public static bool IsSupportStandalonePlatform
		{
			[Token(Token = "0x600015B")]
			[Address(RVA = "0x2658330", Offset = "0x2654330", VA = "0x2658330")]
			get
			{
				return default(bool);
			}
		}

		[Token(Token = "0x600015C")]
		[Address(RVA = "0x26583D4", Offset = "0x26543D4", VA = "0x26583D4")]
		public static object CreateBridgeImplementation(Type interfaceType, string startWith)
		{
			return null;
		}
	}
	[Token(Token = "0x200004B")]
	public sealed class EventManager : Singleton<EventManager>
	{
		[Token(Token = "0x4000136")]
		public const string OnApplicationPause = "OnApplicationPause";

		[Token(Token = "0x4000137")]
		public const string OnApplicationQuit = "OnApplicationQuit";

		[Token(Token = "0x4000138")]
		public const string OnComplianceUserChanged = "OnComplianceUserChanged";

		[Token(Token = "0x4000139")]
		public const string IsLaunchedFromTapTapPCFinished = "IsLaunchedFromTapTapPCFinished";

		[Token(Token = "0x400013A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private Dictionary<string, Action<object>> eventRegistries;

		[Token(Token = "0x6000166")]
		[Address(RVA = "0x265A408", Offset = "0x2656408", VA = "0x265A408")]
		public static void AddListener(string eventName, Action<object> listener)
		{
		}

		[Token(Token = "0x6000167")]
		[Address(RVA = "0x265A59C", Offset = "0x265659C", VA = "0x265A59C")]
		public static void RemoveListener(string eventName, Action<object> listener)
		{
		}

		[Token(Token = "0x6000168")]
		[Address(RVA = "0x265A704", Offset = "0x2656704", VA = "0x265A704")]
		public static void TriggerEvent(string eventName, object message)
		{
		}

		[Token(Token = "0x6000169")]
		[Address(RVA = "0x265A7D4", Offset = "0x26567D4", VA = "0x265A7D4")]
		public EventManager()
		{
		}
	}
	[Token(Token = "0x200004C")]
	public class ImageUtils
	{
		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x200004D")]
		[CompilerGenerated]
		private struct <CacheImage>d__6 : IAsyncStateMachine
		{
			[Token(Token = "0x400013E")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x400013F")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public AsyncTaskMethodBuilder <>t__builder;

			[Token(Token = "0x4000140")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public string url;

			[Token(Token = "0x4000141")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public Texture2D tex;

			[Token(Token = "0x4000142")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
			private FileStream <fileStream>5__2;

			[Token(Token = "0x4000143")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
			private TaskAwaiter <>u__1;

			[Token(Token = "0x6000174")]
			[Address(RVA = "0x265B160", Offset = "0x2657160", VA = "0x265B160", Slot = "4")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x6000175")]
			[Address(RVA = "0x265B55C", Offset = "0x265755C", VA = "0x265B55C", Slot = "5")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x200004E")]
		[CompilerGenerated]
		private struct <FetchImage>d__4 : IAsyncStateMachine
		{
			[Token(Token = "0x4000144")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x4000145")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public AsyncTaskMethodBuilder<Texture2D> <>t__builder;

			[Token(Token = "0x4000146")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public string url;

			[Token(Token = "0x4000147")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public int timeout;

			[Token(Token = "0x4000148")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
			private UnityWebRequest <request>5__2;

			[Token(Token = "0x4000149")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
			private UnityWebRequestAsyncOperation <operation>5__3;

			[Token(Token = "0x400014A")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
			private TaskAwaiter <>u__1;

			[Token(Token = "0x6000176")]
			[Address(RVA = "0x265B5C4", Offset = "0x26575C4", VA = "0x265B5C4", Slot = "4")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x6000177")]
			[Address(RVA = "0x265BB40", Offset = "0x2657B40", VA = "0x265BB40", Slot = "5")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x200004F")]
		[CompilerGenerated]
		private struct <LoadCachedImaged>d__5 : IAsyncStateMachine
		{
			[Token(Token = "0x400014B")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x400014C")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public AsyncTaskMethodBuilder<Texture> <>t__builder;

			[Token(Token = "0x400014D")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public string url;

			[Token(Token = "0x400014E")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public int timeout;

			[Token(Token = "0x400014F")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
			private string <cachedImagePath>5__2;

			[Token(Token = "0x4000150")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
			private UnityWebRequest <request>5__3;

			[Token(Token = "0x4000151")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
			private UnityWebRequestAsyncOperation <operation>5__4;

			[Token(Token = "0x4000152")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x48")]
			private TaskAwaiter <>u__1;

			[Token(Token = "0x6000178")]
			[Address(RVA = "0x265BBBC", Offset = "0x2657BBC", VA = "0x265BBBC", Slot = "4")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x6000179")]
			[Address(RVA = "0x265C27C", Offset = "0x265827C", VA = "0x265C27C", Slot = "5")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x2000050")]
		[CompilerGenerated]
		private struct <LoadImage>d__3 : IAsyncStateMachine
		{
			[Token(Token = "0x4000153")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x4000154")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public AsyncTaskMethodBuilder<Texture> <>t__builder;

			[Token(Token = "0x4000155")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public string url;

			[Token(Token = "0x4000156")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public int timeout;

			[Token(Token = "0x4000157")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x2C")]
			public bool useMemoryCache;

			[Token(Token = "0x4000158")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
			private TaskAwaiter<Texture> <>u__1;

			[Token(Token = "0x4000159")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
			private TaskAwaiter<Texture2D> <>u__2;

			[Token(Token = "0x600017A")]
			[Address(RVA = "0x265C2F8", Offset = "0x26582F8", VA = "0x265C2F8", Slot = "4")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x600017B")]
			[Address(RVA = "0x265CAA0", Offset = "0x2658AA0", VA = "0x265CAA0", Slot = "5")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[Token(Token = "0x400013B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static readonly string OldCacheDirName;

		[Token(Token = "0x400013C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
		private static readonly MD5 md5;

		[Token(Token = "0x400013D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private static readonly Dictionary<string, WeakReference<Texture>> cachedTextures;

		[Token(Token = "0x1700001F")]
		private static string CacheDirPath
		{
			[Token(Token = "0x6000171")]
			[Address(RVA = "0x265ADF8", Offset = "0x2656DF8", VA = "0x265ADF8")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x600016A")]
		[Address(RVA = "0x265A888", Offset = "0x2656888", VA = "0x265A888")]
		[AsyncStateMachine(typeof(<LoadImage>d__3))]
		public static Task<Texture> LoadImage(string url, int timeout = 30, bool useMemoryCache = true)
		{
			return null;
		}

		[Token(Token = "0x600016B")]
		[Address(RVA = "0x265A988", Offset = "0x2656988", VA = "0x265A988")]
		[AsyncStateMachine(typeof(<FetchImage>d__4))]
		public static Task<Texture2D> FetchImage(string url, int timeout = 30)
		{
			return null;
		}

		[Token(Token = "0x600016C")]
		[Address(RVA = "0x265AA78", Offset = "0x2656A78", VA = "0x265AA78")]
		[AsyncStateMachine(typeof(<LoadCachedImaged>d__5))]
		private static Task<Texture> LoadCachedImaged(string url, int timeout = 30)
		{
			return null;
		}

		[Token(Token = "0x600016D")]
		[Address(RVA = "0x265AB68", Offset = "0x2656B68", VA = "0x265AB68")]
		[AsyncStateMachine(typeof(<CacheImage>d__6))]
		private static Task CacheImage(string url, Texture2D tex)
		{
			return null;
		}

		[Token(Token = "0x600016E")]
		[Address(RVA = "0x265AC38", Offset = "0x2656C38", VA = "0x265AC38")]
		private static void RemoveCachedImage(string cachedImagePath)
		{
		}

		[Token(Token = "0x600016F")]
		[Address(RVA = "0x265AC40", Offset = "0x2656C40", VA = "0x265AC40")]
		private static string ToHex(byte[] bytes)
		{
			return null;
		}

		[Token(Token = "0x6000170")]
		[Address(RVA = "0x265AD20", Offset = "0x2656D20", VA = "0x265AD20")]
		private static string GetCachedPath(string url)
		{
			return null;
		}

		[Token(Token = "0x6000172")]
		[Address(RVA = "0x265B094", Offset = "0x2657094", VA = "0x265B094")]
		public ImageUtils()
		{
		}
	}
	[Token(Token = "0x2000051")]
	public class TapLoom : MonoBehaviour
	{
		[Token(Token = "0x2000052")]
		public struct DelayedQueueItem
		{
			[Token(Token = "0x4000164")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public float time;

			[Token(Token = "0x4000165")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public Action action;
		}

		[Token(Token = "0x400015A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		public static int maxThreads;

		[Token(Token = "0x400015B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x4")]
		private static int numThreads;

		[Token(Token = "0x400015C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
		private static TapLoom _current;

		[Token(Token = "0x400015D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		private int _count;

		[Token(Token = "0x400015E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x24")]
		private bool isPause;

		[Token(Token = "0x400015F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private static bool initialized;

		[Token(Token = "0x4000160")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		private List<Action> _actions;

		[Token(Token = "0x4000161")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
		private List<DelayedQueueItem> _delayed;

		[Token(Token = "0x4000162")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
		private List<DelayedQueueItem> _currentDelayed;

		[Token(Token = "0x4000163")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
		private List<Action> _currentActions;

		[Token(Token = "0x17000020")]
		public static TapLoom Current
		{
			[Token(Token = "0x600017C")]
			[Address(RVA = "0x265CB1C", Offset = "0x2658B1C", VA = "0x265CB1C")]
			get
			{
				return null;
			}
		}

		[Token(Token = "0x600017D")]
		[Address(RVA = "0x265CB78", Offset = "0x2658B78", VA = "0x265CB78")]
		private void Awake()
		{
		}

		[Token(Token = "0x600017E")]
		[Address(RVA = "0x2659F64", Offset = "0x2655F64", VA = "0x2659F64")]
		public static void Initialize()
		{
		}

		[Token(Token = "0x600017F")]
		[Address(RVA = "0x265CBDC", Offset = "0x2658BDC", VA = "0x265CBDC")]
		public static void QueueOnMainThread(Action action)
		{
		}

		[Token(Token = "0x6000180")]
		[Address(RVA = "0x265CC34", Offset = "0x2658C34", VA = "0x265CC34")]
		public static void QueueOnMainThread(Action action, float time)
		{
		}

		[Token(Token = "0x6000181")]
		[Address(RVA = "0x265CF60", Offset = "0x2658F60", VA = "0x265CF60")]
		public static Thread RunAsync(Action a)
		{
			return null;
		}

		[Token(Token = "0x6000182")]
		[Address(RVA = "0x265D068", Offset = "0x2659068", VA = "0x265D068")]
		private static void RunAction(object action)
		{
		}

		[Token(Token = "0x6000183")]
		[Address(RVA = "0x265D1FC", Offset = "0x26591FC", VA = "0x265D1FC")]
		private void OnDisable()
		{
		}

		[Token(Token = "0x6000184")]
		[Address(RVA = "0x265D2B0", Offset = "0x26592B0", VA = "0x265D2B0")]
		private void Start()
		{
		}

		[Token(Token = "0x6000185")]
		[Address(RVA = "0x265D2B4", Offset = "0x26592B4", VA = "0x265D2B4")]
		private void Update()
		{
		}

		[Token(Token = "0x6000186")]
		[Address(RVA = "0x265D970", Offset = "0x2659970", VA = "0x265D970")]
		private void OnApplicationPause(bool pauseStatus)
		{
		}

		[Token(Token = "0x6000187")]
		[Address(RVA = "0x265DA24", Offset = "0x2659A24", VA = "0x265DA24")]
		private void OnApplicationQuit()
		{
		}

		[Token(Token = "0x6000188")]
		[Address(RVA = "0x265DAA0", Offset = "0x2659AA0", VA = "0x265DAA0")]
		public TapLoom()
		{
		}
	}
	[Token(Token = "0x2000054")]
	public class TapVerifyInitStateUtils
	{
		[Token(Token = "0x600018D")]
		[Address(RVA = "0x265DC6C", Offset = "0x2659C6C", VA = "0x265DC6C")]
		public static void ShowVerifyErrorMsg(string error, string errorMsg)
		{
		}

		[Token(Token = "0x600018E")]
		[Address(RVA = "0x265DD9C", Offset = "0x2659D9C", VA = "0x265DD9C")]
		public TapVerifyInitStateUtils()
		{
		}
	}
	[Token(Token = "0x2000055")]
	public class UrlUtils
	{
		[Token(Token = "0x600018F")]
		[Address(RVA = "0x265DDA4", Offset = "0x2659DA4", VA = "0x265DDA4")]
		public static NameValueCollection ParseQueryString(string query)
		{
			return null;
		}

		[Token(Token = "0x6000190")]
		[Address(RVA = "0x265DF34", Offset = "0x2659F34", VA = "0x265DF34")]
		public static string ToQueryString(NameValueCollection nvc)
		{
			return null;
		}

		[Token(Token = "0x6000191")]
		[Address(RVA = "0x265E11C", Offset = "0x265A11C", VA = "0x265E11C")]
		public UrlUtils()
		{
		}
	}
}
namespace TapSDK.Core.Internal.Log
{
	[Token(Token = "0x2000058")]
	public class TapLog
	{
		[Token(Token = "0x400016B")]
		private const string TAG = "TapSDK";

		[Token(Token = "0x400016C")]
		private const string InfoColor = "#FFFFFF";

		[Token(Token = "0x400016D")]
		private const string WarningColor = "#FFFF00";

		[Token(Token = "0x400016E")]
		private const string ErrorColor = "#FF0000";

		[Token(Token = "0x400016F")]
		private const string MainThreadColor = "#00FF00";

		[Token(Token = "0x4000170")]
		private const string IOThreadColor = "#FF00FF";

		[Token(Token = "0x4000171")]
		private const string TagColor = "#00FFFF";

		[Token(Token = "0x4000172")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		public static bool Enabled;

		[Token(Token = "0x4000173")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private string module;

		[Token(Token = "0x4000174")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		private string tag;

		[Token(Token = "0x6000197")]
		[Address(RVA = "0x265E248", Offset = "0x265A248", VA = "0x265E248")]
		public TapLog(string module, string tag = "TapSDK")
		{
		}

		[Token(Token = "0x6000198")]
		[Address(RVA = "0x265E274", Offset = "0x265A274", VA = "0x265E274")]
		public void Log(string message, [Optional] string detail)
		{
		}

		[Token(Token = "0x6000199")]
		[Address(RVA = "0x265E288", Offset = "0x265A288", VA = "0x265E288")]
		public void Warning(string message, [Optional] string detail)
		{
		}

		[Token(Token = "0x600019A")]
		[Address(RVA = "0x265E37C", Offset = "0x265A37C", VA = "0x265E37C")]
		public void Error(string message, [Optional] string detail)
		{
		}

		[Token(Token = "0x600019B")]
		[Address(RVA = "0x2657B9C", Offset = "0x2653B9C", VA = "0x2657B9C")]
		public static void Log(string message, [Optional] string detail, [Optional][DefaultParameterValue("TapSDK")] string tag, [Optional] string module)
		{
		}

		[Token(Token = "0x600019C")]
		[Address(RVA = "0x265E29C", Offset = "0x265A29C", VA = "0x265E29C")]
		public static void Warning(string message, [Optional] string detail, [Optional][DefaultParameterValue("TapSDK")] string tag, [Optional] string module)
		{
		}

		[Token(Token = "0x600019D")]
		[Address(RVA = "0x265DCF4", Offset = "0x2659CF4", VA = "0x265DCF4")]
		public static void Error(string message, [Optional] string detail, [Optional][DefaultParameterValue("TapSDK")] string tag, [Optional] string module)
		{
		}

		[Token(Token = "0x600019E")]
		[Address(RVA = "0x265E390", Offset = "0x265A390", VA = "0x265E390")]
		private static string GetFormattedMessage(string message, string detail, string colorHex, string tag, string module)
		{
			return null;
		}

		[Token(Token = "0x600019F")]
		[Address(RVA = "0x265E6A4", Offset = "0x265A6A4", VA = "0x265E6A4")]
		private static string GetThreadInfo()
		{
			return null;
		}

		[Token(Token = "0x60001A0")]
		[Address(RVA = "0x265E8B0", Offset = "0x265A8B0", VA = "0x265E8B0")]
		private static bool IsMobilePlatform()
		{
			return default(bool);
		}
	}
}
namespace TapSDK.Core.Internal.Json
{
	[Token(Token = "0x2000059")]
	public class TapJsonConverter : JsonConverter
	{
		[Token(Token = "0x4000175")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		public static readonly TapJsonConverter Default;

		[Token(Token = "0x60001A1")]
		[Address(RVA = "0x265E934", Offset = "0x265A934", VA = "0x265E934", Slot = "6")]
		public override bool CanConvert(Type objectType)
		{
			return default(bool);
		}

		[Token(Token = "0x60001A2")]
		[Address(RVA = "0x265E9BC", Offset = "0x265A9BC", VA = "0x265E9BC", Slot = "4")]
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
		}

		[Token(Token = "0x60001A3")]
		[Address(RVA = "0x265E9D4", Offset = "0x265A9D4", VA = "0x265E9D4", Slot = "5")]
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return null;
		}

		[Token(Token = "0x60001A4")]
		[Address(RVA = "0x265EC60", Offset = "0x265AC60", VA = "0x265EC60")]
		public TapJsonConverter()
		{
		}
	}
}
namespace TapSDK.Core.Internal.Init
{
	[Token(Token = "0x200005A")]
	public interface IInitTask
	{
		[Token(Token = "0x17000021")]
		int Order
		{
			[Token(Token = "0x60001A6")]
			get;
		}

		[Token(Token = "0x60001A7")]
		void Init(TapTapSdkOptions coreOption, TapTapSdkBaseOptions[] otherOptions);

		[Token(Token = "0x60001A8")]
		void Init(TapTapSdkOptions coreOption);
	}
}
namespace TapSDK.Core.Internal.Http
{
	[Token(Token = "0x200005B")]
	public class TapHttpClient
	{
		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x200005D")]
		[CompilerGenerated]
		private struct <FillHeaders>d__16 : IAsyncStateMachine
		{
			[Token(Token = "0x400017E")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x400017F")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public AsyncTaskMethodBuilder <>t__builder;

			[Token(Token = "0x4000180")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
			public Dictionary<string, object> reqHeaders;

			[Token(Token = "0x4000181")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
			public HttpRequestHeaders headers;

			[Token(Token = "0x4000182")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
			public TapHttpClient <>4__this;

			[Token(Token = "0x4000183")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
			private Dictionary<string, Func<Task<string>>>.Enumerator <>7__wrap1;

			[Token(Token = "0x4000184")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x60")]
			private KeyValuePair<string, Func<Task<string>>> <kv>5__3;

			[Token(Token = "0x4000185")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x70")]
			private TaskAwaiter<string> <>u__1;

			[Token(Token = "0x60001B7")]
			[Address(RVA = "0x265F70C", Offset = "0x265B70C", VA = "0x265F70C", Slot = "4")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x60001B8")]
			[Address(RVA = "0x265FE44", Offset = "0x265BE44", VA = "0x265FE44", Slot = "5")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[Token(Token = "0x200005E")]
		[CompilerGenerated]
		private struct <Request>d__13<T> : IAsyncStateMachine
		{
			[Token(Token = "0x4000186")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public int <>1__state;

			[Token(Token = "0x4000187")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public AsyncTaskMethodBuilder<T> <>t__builder;

			[Token(Token = "0x4000188")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public TapHttpClient <>4__this;

			[Token(Token = "0x4000189")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public string path;

			[Token(Token = "0x400018A")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public Dictionary<string, object> queryParams;

			[Token(Token = "0x400018B")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public HttpMethod method;

			[Token(Token = "0x400018C")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public Dictionary<string, object> headers;

			[Token(Token = "0x400018D")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			public object data;

			[Token(Token = "0x400018E")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			private HttpRequestMessage <request>5__2;

			[Token(Token = "0x400018F")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			private HttpResponseMessage <response>5__3;

			[Token(Token = "0x4000190")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			private TaskAwaiter <>u__1;

			[Token(Token = "0x4000191")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			private TaskAwaiter<HttpResponseMessage> <>u__2;

			[Token(Token = "0x4000192")]
			[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
			private TaskAwaiter<string> <>u__3;

			[Token(Token = "0x60001B9")]
			private void MoveNext()
			{
			}

			[Token(Token = "0x60001BA")]
			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}
		}

		[Token(Token = "0x4000176")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private readonly string clientId;

		[Token(Token = "0x4000177")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		private readonly string clientToken;

		[Token(Token = "0x4000178")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		private readonly string serverUrl;

		[Token(Token = "0x4000179")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		private readonly HttpClient client;

		[Token(Token = "0x400017A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
		private Dictionary<string, Func<Task<string>>> runtimeHeaderTasks;

		[Token(Token = "0x400017B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
		private Dictionary<string, string> additionalHeaders;

		[Token(Token = "0x60001A9")]
		[Address(RVA = "0x265ECC4", Offset = "0x265ACC4", VA = "0x265ECC4")]
		public TapHttpClient(string clientID, string clientToken, string serverUrl)
		{
		}

		[Token(Token = "0x60001AA")]
		[Address(RVA = "0x265EEA0", Offset = "0x265AEA0", VA = "0x265EEA0")]
		public void AddRuntimeHeaderTask(string key, Func<Task<string>> task)
		{
		}

		[Token(Token = "0x60001AB")]
		[Address(RVA = "0x265EF2C", Offset = "0x265AF2C", VA = "0x265EF2C")]
		public void AddAddtionalHeader(string key, string value)
		{
		}

		[Token(Token = "0x60001AC")]
		public Task<T> Get<T>(string path, [Optional] Dictionary<string, object> headers, [Optional] Dictionary<string, object> queryParams, bool withAPIVersion = true)
		{
			return null;
		}

		[Token(Token = "0x60001AD")]
		public Task<T> Post<T>(string path, [Optional] Dictionary<string, object> headers, [Optional] object data, [Optional] Dictionary<string, object> queryParams, bool withAPIVersion = true)
		{
			return null;
		}

		[Token(Token = "0x60001AE")]
		public Task<T> Put<T>(string path, [Optional] Dictionary<string, object> headers, [Optional] object data, [Optional] Dictionary<string, object> queryParams, bool withAPIVersion = true)
		{
			return null;
		}

		[Token(Token = "0x60001AF")]
		[Address(RVA = "0x265EFC4", Offset = "0x265AFC4", VA = "0x265EFC4")]
		public Task Delete(string path, [Optional] Dictionary<string, object> headers, [Optional] object data, [Optional] Dictionary<string, object> queryParams, bool withAPIVersion = true)
		{
			return null;
		}

		[Token(Token = "0x60001B0")]
		[AsyncStateMachine(typeof(<Request>d__13<>))]
		private Task<T> Request<T>(string path, HttpMethod method, [Optional] Dictionary<string, object> headers, [Optional] object data, [Optional] Dictionary<string, object> queryParams, bool withAPIVersion = true)
		{
			return null;
		}

		[Token(Token = "0x60001B1")]
		[Address(RVA = "0x265F0AC", Offset = "0x265B0AC", VA = "0x265F0AC")]
		private TapException HandleErrorResponse(HttpStatusCode statusCode, string responseContent)
		{
			return null;
		}

		[Token(Token = "0x60001B2")]
		[Address(RVA = "0x265F344", Offset = "0x265B344", VA = "0x265F344")]
		private string BuildUrl(string path, Dictionary<string, object> queryParams)
		{
			return null;
		}

		[Token(Token = "0x60001B3")]
		[Address(RVA = "0x265F554", Offset = "0x265B554", VA = "0x265F554")]
		[AsyncStateMachine(typeof(<FillHeaders>d__16))]
		private Task FillHeaders(HttpRequestHeaders headers, [Optional] Dictionary<string, object> reqHeaders)
		{
			return null;
		}
	}
	[Token(Token = "0x200005F")]
	public class TapHttpUtils
	{
		[Token(Token = "0x60001BB")]
		[Address(RVA = "0x265FEAC", Offset = "0x265BEAC", VA = "0x265FEAC")]
		public static void PrintRequest(HttpClient client, HttpRequestMessage request, [Optional] string content)
		{
		}

		[Token(Token = "0x60001BC")]
		[Address(RVA = "0x266092C", Offset = "0x265C92C", VA = "0x266092C")]
		public static void PrintResponse(HttpResponseMessage response, [Optional] string content)
		{
		}

		[Token(Token = "0x60001BD")]
		[Address(RVA = "0x2660B50", Offset = "0x265CB50", VA = "0x2660B50")]
		public static NameValueCollection ParseQueryString(string queryString)
		{
			return null;
		}

		[Token(Token = "0x60001BE")]
		[Address(RVA = "0x2660D30", Offset = "0x265CD30", VA = "0x2660D30")]
		public TapHttpUtils()
		{
		}
	}
}
