using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using RpcFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RuntimeRpcService
{
	private sealed class RuntimeHost : IRpcHostState
	{
		public ApplicationStateInfo ApplicationState { get; set; }

		public bool IsCompiling => false;

		public bool IsUpdating => false;

		public bool IsPlaying => true;

		public bool IsPlayingOrWillChangePlaymode => false;

		public bool EditorModeEnabled => false;

		public string? ActiveSceneName => SceneManager.GetActiveScene().name;

		public string? ActiveScenePath => SceneManager.GetActiveScene().path;

		public RuntimeHost(ApplicationStateInfo applicationState)
		{
			ApplicationState = applicationState;
		}
	}

	private sealed class RuntimeCommandBackend : IRuntimeCommandBackend
	{
		public ApplicationStateInfo ApplicationState { get; set; }

		public RuntimeCommandBackend(ApplicationStateInfo applicationState)
		{
			ApplicationState = applicationState;
		}

		public JObject GetStatus()
		{
			return BuildRuntimeStatus(ApplicationState, "getStatus");
		}

		public JObject GetSceneStatus()
		{
			Scene activeScene = SceneManager.GetActiveScene();
			return new JObject
			{
				["sceneName"] = activeScene.name,
				["scenePath"] = activeScene.path,
				["sceneCount"] = SceneManager.sceneCount,
				["loadedScenes"] = new JArray(Enumerable.Range(0, SceneManager.sceneCount).Select(delegate(int i)
				{
					Scene sceneAt = SceneManager.GetSceneAt(i);
					return new JObject
					{
						["name"] = sceneAt.name,
						["path"] = sceneAt.path,
						["isLoaded"] = sceneAt.isLoaded,
						["isActive"] = sceneAt == activeScene
					};
				}))
			};
		}

		public JArray FindObjects(JObject request)
		{
			string nameContains = request.Value<string>("nameContains");
			int count = Mathf.Clamp(request.Value<int?>("max") ?? 100, 1, 1000);
			return new JArray(from t in (from t in UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive: true)
					where string.IsNullOrWhiteSpace(nameContains) || t.name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0
					select t).Take(count)
				select new JObject
				{
					["id"] = t.gameObject.GetInstanceID().ToString(),
					["path"] = GetPath(t),
					["name"] = t.name,
					["active"] = t.gameObject.activeInHierarchy
				});
		}

		public IReadOnlyList<RuntimeUiElement> FindUiElements(JObject request)
		{
			return UiElementQuery.Find(request);
		}

		public JObject ClickUi(JObject request)
		{
			Selectable selectable = UiElementQuery.Resolve(request);
			if (selectable == null)
			{
				throw new InvalidOperationException("Unknown runtime UI element: " + (request.Value<string>("elementId") ?? request.Value<string>("path") ?? request.Value<string>("textContains")));
			}
			LeaveButton componentInParent = selectable.GetComponentInParent<LeaveButton>(includeInactive: true);
			if (componentInParent != null && componentInParent.button != null)
			{
				componentInParent.button.onClick.Invoke();
				return new JObject
				{
					["clicked"] = true,
					["path"] = GetPath(componentInParent.transform),
					["type"] = "LeaveButton"
				};
			}
			Button button = selectable.GetComponent<Button>() ?? selectable.GetComponentInParent<Button>(includeInactive: true);
			if (button == null)
			{
				throw new InvalidOperationException("Runtime UI element is not clickable: " + GetPath(selectable.transform));
			}
			if (!button.gameObject.activeInHierarchy || !button.interactable)
			{
				throw new InvalidOperationException("Runtime UI element is not active or interactable: " + GetPath(button.transform));
			}
			button.onClick.Invoke();
			return new JObject
			{
				["clicked"] = true,
				["path"] = GetPath(button.transform),
				["type"] = button.GetType().Name
			};
		}

		public JObject InvokeRegistered(JObject request)
		{
			string text = request.Value<string>("name");
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new InvalidOperationException("invokeRegistered is missing name.");
			}
			if (!RegisteredCommands.TryGetValue(text, out var value))
			{
				throw new InvalidOperationException("Unknown registered runtime command: " + text);
			}
			return value(request);
		}

		public bool IsConditionSatisfied(JObject request)
		{
			switch (request.Value<string>("name"))
			{
			case "applicationInited":
				return ApplicationState?.inited ?? false;
			case "runningGameLoaded":
			{
				ApplicationStateInfo applicationState2 = ApplicationState;
				return applicationState2 != null && applicationState2.runningGameState?.loaded == true;
			}
			case "runningGameEnded":
			{
				ApplicationStateInfo applicationState = ApplicationState;
				return applicationState != null && applicationState.runningGameState?.gameEnded == true;
			}
			case "sceneLoaded":
				return !string.IsNullOrWhiteSpace(SceneManager.GetActiveScene().name);
			default:
				return false;
			}
		}
	}

	private sealed class RuntimeDiagnosticsBackend : IRuntimeDiagnosticsBackend
	{
		public IReadOnlyList<RuntimeConsoleLogEntry> GetConsoleLog(int sinceSequence, int maxEntries, string? minLevel, string? messageContains = null, string? messageExcludeContains = null)
		{
			return Logs.Get(sinceSequence, maxEntries, minLevel, messageContains, messageExcludeContains);
		}

		public void ClearConsoleLog()
		{
			Logs.Clear();
		}

		public RuntimeScreenshotResult CaptureScreenshot(RuntimeScreenshotRequest request)
		{
			string text = Path.Combine(fileStore.ResultsPath, Guid.NewGuid().ToString("N") + ".screenshot.png");
			int num = Mathf.Max(1, request.Width);
			int num2 = Mathf.Max(1, request.Height);
			List<Camera> list = (from camera in UnityEngine.Object.FindObjectsOfType<Camera>()
				where camera != null && camera.isActiveAndEnabled
				orderby camera.depth
				select camera).ToList();
			if (list.Count == 0)
			{
				throw new InvalidOperationException("No active camera is available for runtime screenshot capture.");
			}
			Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, mipChain: false);
			RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 24, RenderTextureFormat.ARGB32);
			RenderTexture active = RenderTexture.active;
			Dictionary<Camera, RenderTexture> dictionary = list.ToDictionary((Camera camera) => camera, (Camera camera) => camera.targetTexture);
			try
			{
				RenderTexture.active = temporary;
				GL.Clear(clearDepth: true, clearColor: true, Color.black);
				foreach (Camera item in list)
				{
					item.targetTexture = temporary;
					item.Render();
				}
				RenderTexture.active = temporary;
				texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0, recalculateMipMaps: false);
				texture2D.Apply();
				File.WriteAllBytes(text, ImageConversion.EncodeToPNG(texture2D));
			}
			finally
			{
				foreach (KeyValuePair<Camera, RenderTexture> item2 in dictionary)
				{
					item2.Key.targetTexture = item2.Value;
				}
				RenderTexture.active = active;
				RenderTexture.ReleaseTemporary(temporary);
				UnityEngine.Object.Destroy(texture2D);
			}
			FileInfo fileInfo = new FileInfo(text);
			return new RuntimeScreenshotResult
			{
				Path = text,
				MimeType = "image/png",
				Width = num,
				Height = num2,
				Bytes = (fileInfo.Exists ? fileInfo.Length : 0)
			};
		}
	}

	private sealed class UnsupportedRuntimeScriptExecutor : IRuntimeScriptExecutor
	{
		public JObject ExecuteBody(RuntimeScriptExecuteRequest request)
		{
			throw new NotSupportedException("Runtime script execution is not enabled in the Unity adapter.");
		}
	}

	private sealed class RuntimeLogBuffer
	{
		private readonly int capacity;

		private readonly List<RuntimeConsoleLogEntry> entries = new List<RuntimeConsoleLogEntry>();

		private int sequence;

		public int CurrentSequence => sequence;

		public RuntimeLogBuffer(int capacity)
		{
			this.capacity = capacity;
		}

		public void Add(string type, string message, string stackTrace)
		{
			entries.Add(new RuntimeConsoleLogEntry
			{
				Sequence = ++sequence,
				Type = type,
				Message = message,
				StackTrace = stackTrace,
				Time = DateTime.UtcNow.ToString("o")
			});
			if (entries.Count > capacity)
			{
				entries.RemoveAt(0);
			}
		}

		public IReadOnlyList<RuntimeConsoleLogEntry> Get(int sinceSequence, int maxEntries, string minLevel, string messageContains, string messageExcludeContains)
		{
			return (from entry in entries
				where entry.Sequence > sinceSequence
				where RuntimeDiagnosticsFeature.LogLevelRank(entry.Type) >= RuntimeDiagnosticsFeature.LogLevelRank(minLevel)
				where string.IsNullOrWhiteSpace(messageContains) || (entry.Message?.IndexOf(messageContains, StringComparison.OrdinalIgnoreCase) ?? (-1)) >= 0
				where string.IsNullOrWhiteSpace(messageExcludeContains) || (entry.Message?.IndexOf(messageExcludeContains, StringComparison.OrdinalIgnoreCase) ?? (-1)) < 0
				select entry).TakeLast(Mathf.Clamp(maxEntries, 1, capacity)).ToList();
		}

		public void Clear()
		{
			entries.Clear();
		}
	}

	private static class UiElementQuery
	{
		public static IReadOnlyList<RuntimeUiElement> Find(JObject request)
		{
			string textContains = request.Value<string>("textContains");
			string nameContains = request.Value<string>("nameContains");
			string pathContains = request.Value<string>("pathContains");
			bool includeInactive = request.Value<bool?>("includeInactive") == true;
			int count = Mathf.Clamp(request.Value<int?>("max") ?? 100, 1, 1000);
			return (from selectable in UnityEngine.Object.FindObjectsOfType<Selectable>(includeInactive)
				select BuildElement(selectable, includeInactive) into element
				where element != null
				where string.IsNullOrWhiteSpace(textContains) || (element.Text?.IndexOf(textContains, StringComparison.OrdinalIgnoreCase) ?? (-1)) >= 0
				where string.IsNullOrWhiteSpace(nameContains) || (element.Name?.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) ?? (-1)) >= 0
				where string.IsNullOrWhiteSpace(pathContains) || (element.Path?.IndexOf(pathContains, StringComparison.OrdinalIgnoreCase) ?? (-1)) >= 0
				select element).Take(count).ToList();
		}

		public static Selectable Resolve(JObject request)
		{
			string elementId = request.Value<string>("elementId");
			string path = request.Value<string>("path");
			string textContains = request.Value<string>("textContains");
			string nameContains = request.Value<string>("nameContains");
			return UnityEngine.Object.FindObjectsOfType<Selectable>(request.Value<bool?>("includeInactive") == true).FirstOrDefault(delegate(Selectable selectable)
			{
				string path2 = GetPath(selectable.transform);
				if (!string.IsNullOrWhiteSpace(elementId) && BuildElementId(selectable) == elementId)
				{
					return true;
				}
				if (!string.IsNullOrWhiteSpace(path) && path2 == path)
				{
					return true;
				}
				if (!string.IsNullOrWhiteSpace(textContains) && (GetText(selectable.gameObject)?.IndexOf(textContains, StringComparison.OrdinalIgnoreCase) ?? (-1)) >= 0)
				{
					return true;
				}
				return (!string.IsNullOrWhiteSpace(nameContains) && selectable.name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0) ? true : false;
			});
		}

		private static RuntimeUiElement BuildElement(Selectable selectable, bool includeInactive)
		{
			if (selectable == null)
			{
				return null;
			}
			if (!includeInactive && !selectable.gameObject.activeInHierarchy)
			{
				return null;
			}
			return new RuntimeUiElement
			{
				Id = BuildElementId(selectable),
				Path = GetPath(selectable.transform),
				Name = selectable.name,
				Text = GetText(selectable.gameObject),
				Interactable = selectable.interactable,
				Active = selectable.gameObject.activeInHierarchy,
				Visible = selectable.IsActive()
			};
		}
	}

	private const double PollIntervalSeconds = 0.1;

	private const int MaxResultFiles = 500;

	private const int MaxLogFiles = 500;

	private static readonly string RootDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "RuntimeRpc"));

	private static readonly RuntimeLogBuffer Logs = new RuntimeLogBuffer(500);

	private static readonly Dictionary<string, Func<JObject, JObject>> RegisteredCommands = new Dictionary<string, Func<JObject, JObject>>(StringComparer.Ordinal);

	private static RpcFileStore fileStore;

	private static RpcService service;

	private static RuntimeHost host;

	private static RuntimeCommandBackend commandBackend;

	private static RuntimeDiagnosticsBackend diagnosticsBackend;

	private static bool processing;

	private static bool logSubscribed;

	private static double nextPollTime;

	public static string RootPath => RootDir;

	public static void FrameUpdate(ApplicationStateInfo applicationState)
	{
		if ((applicationState != null && !applicationState.inited) || processing)
		{
			return;
		}
		EnsureService(applicationState);
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if ((double)realtimeSinceStartup < nextPollTime)
		{
			return;
		}
		nextPollTime = (double)realtimeSinceStartup + 0.1;
		processing = true;
		try
		{
			service.PollOnce();
			CleanupOldFiles();
		}
		finally
		{
			processing = false;
		}
	}

	public static void RegisterCommand(string name, Func<JObject, JObject> handler)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Runtime command name is required.", "name");
		}
		RegisteredCommands[name] = handler ?? throw new ArgumentNullException("handler");
	}

	private static void EnsureService(ApplicationStateInfo applicationState)
	{
		if (!logSubscribed)
		{
			Application.logMessageReceived -= OnLogMessageReceived;
			Application.logMessageReceived += OnLogMessageReceived;
			logSubscribed = true;
		}
		if (fileStore == null)
		{
			fileStore = new RpcFileStore(RootDir);
			host = new RuntimeHost(applicationState);
			commandBackend = new RuntimeCommandBackend(applicationState);
			diagnosticsBackend = new RuntimeDiagnosticsBackend();
			RpcCommandRouter router = new RpcCommandRouter();
			new RuntimeCommandFeature(commandBackend).Register(router);
			new RuntimeDiagnosticsFeature(diagnosticsBackend).Register(router);
			new RuntimeScriptExecuteFeature(new UnsupportedRuntimeScriptExecutor()).Register(router);
			service = new RpcService(fileStore, router, host, null, new RpcServiceOptions
			{
				GetStatusPath = (RpcFileStore _, string _) => Path.Combine(fileStore.StatusPath, "runtime-status.json"),
				BuildEditorStatus = (IRpcHostState _, string reason) => BuildRuntimeStatus(host.ApplicationState, reason)
			});
			RegisterBuiltInCommands();
		}
		else
		{
			host.ApplicationState = applicationState;
			commandBackend.ApplicationState = applicationState;
		}
	}

	private static JObject BuildRuntimeStatus(ApplicationStateInfo applicationState, string reason)
	{
		return new JObject
		{
			["time"] = DateTime.UtcNow.ToString("o"),
			["reason"] = reason,
			["rootPath"] = RootPath,
			["processing"] = processing,
			["applicationInited"] = applicationState?.inited ?? false,
			["activeSceneName"] = SceneManager.GetActiveScene().name,
			["activeScenePath"] = SceneManager.GetActiveScene().path,
			["runningGameLoaded"] = applicationState?.runningGameState?.loaded == true,
			["runningGameEnded"] = applicationState?.runningGameState?.gameEnded == true,
			["aiSessionMode"] = applicationState?.aiSessionMode ?? false,
			["logSequence"] = Logs.CurrentSequence
		};
	}

	private static void RegisterBuiltInCommands()
	{
		RegisterCommand("runtime.ping", (JObject _) => new JObject { ["pong"] = true });
	}

	private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
	{
		Logs.Add(type.ToString(), condition, stackTrace);
	}

	private static void CleanupOldFiles()
	{
		CleanupOldFiles(Path.Combine(RootDir, "results"), "*.result.json", 500);
		CleanupOldFiles(Path.Combine(RootDir, "logs"), "*.log.jsonl", 500);
	}

	private static void CleanupOldFiles(string directory, string pattern, int maxFiles)
	{
		if (Directory.Exists(directory))
		{
			string[] array = Directory.GetFiles(directory, pattern).OrderByDescending(File.GetCreationTimeUtc).ToArray();
			for (int i = maxFiles; i < array.Length; i++)
			{
				File.Delete(array[i]);
			}
		}
	}

	private static string BuildElementId(Selectable selectable)
	{
		return selectable.gameObject.GetInstanceID().ToString();
	}

	private static string GetText(GameObject gameObject)
	{
		TMP_Text componentInChildren = gameObject.GetComponentInChildren<TMP_Text>(includeInactive: true);
		if (componentInChildren != null && !string.IsNullOrWhiteSpace(componentInChildren.text))
		{
			return componentInChildren.text;
		}
		Text componentInChildren2 = gameObject.GetComponentInChildren<Text>(includeInactive: true);
		if (!(componentInChildren2 != null))
		{
			return null;
		}
		return componentInChildren2.text;
	}

	private static string GetPath(Transform transform)
	{
		Stack<string> stack = new Stack<string>();
		Transform transform2 = transform;
		while (transform2 != null)
		{
			stack.Push(transform2.name);
			transform2 = transform2.parent;
		}
		return "/" + string.Join("/", stack);
	}
}
