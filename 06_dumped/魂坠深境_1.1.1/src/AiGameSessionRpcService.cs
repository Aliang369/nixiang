using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using RpcFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AiGameSessionRpcService
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

	private sealed class RuntimeRequestScope : IRpcRequestScope, IDisposable
	{
		public void Complete(RpcResult result)
		{
			lock (runtimeLogs)
			{
				if (runtimeLogs.Count != 0)
				{
					result.Logs.AddRange(runtimeLogs);
					runtimeLogs.Clear();
				}
			}
		}

		public void Dispose()
		{
		}
	}

	private sealed class LazyBackend : IAiSessionBackend
	{
		public ApplicationStateInfo ApplicationState { get; set; }

		public LazyBackend(ApplicationStateInfo applicationState)
		{
			ApplicationState = applicationState;
		}

		public AiObservation CreateSession(JObject request)
		{
			JObject jObject = GetController().CreateSession(request);
			ThrowIfError(jObject);
			return Map(jObject);
		}

		public AiObservation Observe(string sessionId)
		{
			EnsureSession(sessionId);
			return Map(GetController().Observe());
		}

		public AiActResult Act(string sessionId, string actionId, JObject request)
		{
			EnsureSession(sessionId);
			return MapAct(GetController().Act(request));
		}

		public AiObservation StepUntilDecision(string sessionId, JObject request)
		{
			EnsureSession(sessionId);
			return Map(GetController().StepUntilDecision(request.Value<int?>("maxActionSteps") ?? 200));
		}

		public void CloseSession(string sessionId)
		{
			EnsureSession(sessionId);
			ThrowIfError(GetController().CloseSession());
		}

		public JObject GetStatus()
		{
			AiGameSessionController aiGameSessionController = ApplicationState.aiGameSessionController;
			return new JObject
			{
				["active"] = aiGameSessionController?.Active ?? false,
				["sessionId"] = aiGameSessionController?.SessionId,
				["decisionSeq"] = aiGameSessionController?.DecisionSeq,
				["rootPath"] = RootPath,
				["aiSessionMode"] = ApplicationState.aiSessionMode,
				["runningGameLoaded"] = ApplicationState.runningGameState?.loaded ?? false,
				["runningGameEnded"] = ApplicationState.runningGameState?.gameEnded ?? false
			};
		}

		private AiGameSessionController GetController()
		{
			if (ApplicationState.aiGameSessionController == null)
			{
				ApplicationState.aiGameSessionController = new AiGameSessionController(ApplicationState);
			}
			return ApplicationState.aiGameSessionController;
		}

		private void EnsureSession(string sessionId)
		{
			AiGameSessionController controller = GetController();
			if (!controller.Active)
			{
				throw new InvalidOperationException("No active AI session.");
			}
			if (!string.Equals(controller.SessionId, sessionId, StringComparison.Ordinal))
			{
				throw new InvalidOperationException("Unknown AI session `" + sessionId + "`.");
			}
		}

		private static void ThrowIfError(JObject result)
		{
			if (result.Value<bool?>("ok") != false)
			{
				return;
			}
			string text = result.SelectToken("error.code")?.Value<string>();
			string text2 = result.SelectToken("error.message")?.Value<string>() ?? "AI session request failed.";
			throw new InvalidOperationException(string.IsNullOrWhiteSpace(text) ? text2 : (text + ": " + text2));
		}

		private static AiObservation Map(JObject legacy)
		{
			AiObservation obj = new AiObservation
			{
				SessionId = legacy.Value<string>("sessionId"),
				Phase = legacy.Value<string>("phase"),
				DecisionSeq = legacy.Value<int?>("decisionSeq").GetValueOrDefault()
			};
			int isGameOver;
			if (!string.Equals(legacy.Value<string>("phase"), "gameOver", StringComparison.OrdinalIgnoreCase))
			{
				JToken? jToken = legacy.SelectToken("game.ended");
				isGameOver = ((jToken != null && jToken.Value<bool?>() == true) ? 1 : 0);
			}
			else
			{
				isGameOver = 1;
			}
			obj.IsGameOver = (byte)isGameOver != 0;
			obj.State = legacy;
			AiObservation aiObservation = obj;
			if (legacy["waiting"] is JObject jObject)
			{
				aiObservation.Waiting = new AiWaitingInfo
				{
					Reason = jObject.Value<string>("reason"),
					Detail = jObject.Value<string>("detail"),
					ActionQueueBusy = (jObject.Value<bool?>("actionQueueBusy") == true),
					BlockingUi = jObject.Value<string>("blockingUi"),
					AutoContinuedCount = jObject.Value<int?>("autoContinuedCount").GetValueOrDefault()
				};
			}
			if (legacy["legalActions"] is JArray source)
			{
				foreach (JObject item in source.OfType<JObject>())
				{
					aiObservation.LegalActions.Add(new AiLegalAction
					{
						ActionId = item.Value<string>("actionId"),
						Kind = item.Value<string>("type"),
						Label = item.Value<string>("label"),
						Data = (item["payload"] as JObject),
						PayloadSchema = (item["payloadSchema"] as JObject)
					});
				}
			}
			if (legacy["data"] is JObject jObject2)
			{
				if (jObject2["automaticRewards"] is JArray source2)
				{
					foreach (JObject item2 in source2.OfType<JObject>())
					{
						aiObservation.AutomaticRewards.Add(new AiAutomaticEffect
						{
							EffectId = item2.Value<string>("effectId"),
							Kind = item2.Value<string>("kind"),
							Label = item2.Value<string>("label"),
							Data = (item2["data"] as JObject)
						});
					}
				}
				if (jObject2["choiceGroups"] is JArray source3)
				{
					foreach (JObject item3 in source3.OfType<JObject>())
					{
						AiChoiceGroup aiChoiceGroup = new AiChoiceGroup
						{
							GroupId = item3.Value<string>("groupId"),
							Kind = item3.Value<string>("kind"),
							Label = item3.Value<string>("label"),
							Required = (item3.Value<bool?>("required") == true),
							MinSelect = item3.Value<int?>("minSelect").GetValueOrDefault(),
							MaxSelect = (item3.Value<int?>("maxSelect") ?? 1),
							Data = (item3["data"] as JObject)
						};
						if (item3["options"] is JArray source4)
						{
							foreach (JObject item4 in source4.OfType<JObject>())
							{
								aiChoiceGroup.Options.Add(new AiChoiceOption
								{
									OptionId = item4.Value<string>("optionId"),
									Kind = item4.Value<string>("kind"),
									Label = item4.Value<string>("label"),
									Data = (item4["data"] as JObject)
								});
							}
						}
						aiObservation.ChoiceGroups.Add(aiChoiceGroup);
					}
				}
			}
			if (legacy["auxiliaryActions"] is JArray source5)
			{
				foreach (JObject item5 in source5.OfType<JObject>())
				{
					aiObservation.AuxiliaryActions.Add(new AiAuxiliaryAction
					{
						ActionId = item5.Value<string>("actionId"),
						Kind = item5.Value<string>("kind"),
						Label = item5.Value<string>("label"),
						Data = (item5["data"] as JObject)
					});
				}
			}
			return aiObservation;
		}

		private static AiActResult MapAct(JObject legacy)
		{
			return new AiActResult
			{
				Accepted = (legacy.Value<bool?>("ok") != false),
				Message = (legacy.SelectToken("error.message")?.Value<string>() ?? legacy.Value<string>("summary")),
				Observation = ((legacy.SelectToken("result") is JObject legacy2) ? Map(legacy2) : Map(legacy))
			};
		}
	}

	private const double PollIntervalSeconds = 0.1;

	private const int MaxResultFiles = 500;

	private const int MaxLogFiles = 500;

	private static readonly string RootDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "AiGameSessionRpc"));

	private static RpcFileStore fileStore;

	private static RpcCommandRouter router;

	private static RpcService service;

	private static RuntimeHost host;

	private static LazyBackend backend;

	private static double nextPollTime;

	private static bool processing;

	private static readonly List<RpcLogEntry> runtimeLogs = new List<RpcLogEntry>();

	public static string RootPath => RootDir;

	public static void FrameUpdate(ApplicationStateInfo applicationState)
	{
		if (applicationState == null || !applicationState.inited || processing)
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
		Application.logMessageReceived += OnRuntimeLogMessageReceived;
		try
		{
			service.PollOnce();
			CleanupOldFiles();
		}
		finally
		{
			Application.logMessageReceived -= OnRuntimeLogMessageReceived;
			processing = false;
		}
	}

	private static void OnRuntimeLogMessageReceived(string condition, string stackTrace, LogType type)
	{
		if (type != LogType.Warning && type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
		{
			return;
		}
		lock (runtimeLogs)
		{
			runtimeLogs.Add(new RpcLogEntry
			{
				Type = type.ToString(),
				Message = condition,
				StackTrace = stackTrace,
				Time = DateTime.UtcNow.ToString("o")
			});
		}
	}

	private static void EnsureService(ApplicationStateInfo applicationState)
	{
		if (fileStore == null)
		{
			fileStore = new RpcFileStore(RootDir);
			router = new RpcCommandRouter();
			host = new RuntimeHost(applicationState);
			backend = new LazyBackend(applicationState);
			router.Register("aiSession", new AiSessionFeature(backend));
			service = new RpcService(fileStore, router, host, null, new RpcServiceOptions
			{
				DefaultTool = "aiSession",
				DefaultAction = "observe",
				GetStatusPath = (RpcFileStore _, string _) => Path.Combine(fileStore.StatusPath, "runtime-status.json"),
				BuildEditorStatus = (IRpcHostState _, string reason) => BuildRuntimeStatus(host.ApplicationState, reason),
				BeginRequestScope = (RpcRequest _) => new RuntimeRequestScope(),
				BuildResultJson = BuildLegacyResultEnvelope
			});
		}
		else
		{
			host.ApplicationState = applicationState;
			backend.ApplicationState = applicationState;
		}
	}

	private static JObject BuildRuntimeStatus(ApplicationStateInfo applicationState, string reason)
	{
		AiGameSessionController aiGameSessionController = applicationState.aiGameSessionController;
		return new JObject
		{
			["time"] = DateTime.UtcNow.ToString("o"),
			["reason"] = reason,
			["rootPath"] = RootPath,
			["processing"] = processing,
			["applicationInited"] = applicationState.inited,
			["aiSessionMode"] = applicationState.aiSessionMode,
			["active"] = aiGameSessionController?.Active ?? false,
			["sessionId"] = aiGameSessionController?.SessionId,
			["decisionSeq"] = aiGameSessionController?.DecisionSeq,
			["runningGameLoaded"] = applicationState.runningGameState?.loaded ?? false,
			["runningGameEnded"] = applicationState.runningGameState?.gameEnded ?? false
		};
	}

	private static JObject BuildLegacyResultEnvelope(RpcResult result)
	{
		JObject jObject = result.ToJObject();
		jObject["result"] = jObject["data"];
		return jObject;
	}

	private static void CleanupOldFiles()
	{
		CleanupOldFiles(Path.Combine(RootDir, "results"), "*.result.json", 500);
		CleanupOldFiles(Path.Combine(RootDir, "logs"), "*.log.jsonl", 500);
	}

	private static void CleanupOldFiles(string directory, string pattern, int maxFiles)
	{
		if (!Directory.Exists(directory))
		{
			return;
		}
		string[] array = Directory.GetFiles(directory, pattern).OrderByDescending(File.GetCreationTimeUtc).ToArray();
		if (array.Length > maxFiles)
		{
			for (int i = maxFiles; i < array.Length; i++)
			{
				File.Delete(array[i]);
			}
		}
	}
}
