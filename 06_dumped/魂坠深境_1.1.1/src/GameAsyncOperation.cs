using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChillyRoom.Services.Core.Errors;
using UnityEngine;

public abstract class GameAsyncOperation
{
	public string waitingHint;

	public string stackTraceText;

	public string errorCode;

	public string traceId;

	public string faultCloseButtonText;

	private Action onComplete;

	private Action onFault;

	private Action onOver;

	private Action onFaultClose;

	private bool showWaitPanel = true;

	private bool showFaultPanel = true;

	public bool autoRetry;

	private int autoRetryTime;

	public bool manualRetry;

	private string errorText;

	private string loadingContent;

	private WaitPanelBase.Token waitPanelToken;

	public AsyncOperationWaitPanel attachPanel;

	public bool isComplete;

	public bool isFault;

	public bool isOver;

	public bool isRunning;

	public GameAsyncOperationLane Lane;

	private bool hideWaitPanelForLane
	{
		get
		{
			if (Lane != GameAsyncOperationLane.Background)
			{
				return Lane == GameAsyncOperationLane.DuringLoading;
			}
			return true;
		}
	}

	public bool BlocksLoading { get; private set; } = true;

	public bool shouldAutoRetry
	{
		get
		{
			if (autoRetry && isNetworkTimeout)
			{
				return autoRetryTime < 1;
			}
			return false;
		}
	}

	protected virtual bool isNetworkTimeout => false;

	protected abstract Task task { get; }

	public GameAsyncOperation OnComplete(Action onCompleteAction)
	{
		onComplete = onCompleteAction;
		return this;
	}

	public GameAsyncOperation OnFault(Action onFaultAction)
	{
		onFault = onFaultAction;
		return this;
	}

	public GameAsyncOperation OnOver(Action onOverAction)
	{
		onOver = onOverAction;
		return this;
	}

	public GameAsyncOperation HideFaultPanel()
	{
		showFaultPanel = false;
		return this;
	}

	public GameAsyncOperation DoesNotBlockLoading()
	{
		BlocksLoading = false;
		return this;
	}

	public GameAsyncOperation ManualRetry()
	{
		manualRetry = true;
		return this;
	}

	public GameAsyncOperation AutoRetry()
	{
		autoRetry = true;
		return this;
	}

	public GameAsyncOperation OnFaultClose(Action onFaultCloseAction)
	{
		onFaultClose = onFaultCloseAction;
		return this;
	}

	public void EnqueueRun(GameAsyncOperationLane lane = GameAsyncOperationLane.Foreground, string loadingContent = null)
	{
		EnqueueRunImpl(lane, loadingContent);
	}

	public void EnqueueRun(string loadingContent, GameAsyncOperationLane lane = GameAsyncOperationLane.Foreground)
	{
		EnqueueRunImpl(lane, loadingContent);
	}

	private void EnqueueRunImpl(GameAsyncOperationLane lane, string loadingContent)
	{
		this.loadingContent = loadingContent;
		SetLane(lane);
		ShowGlobalWaitPanelIfNeeded(loadingContent);
		BootstrapExtra.instance.gameAsyncOperationManager.EnqueueAsyncOperation(this);
	}

	public void Retry()
	{
		HideGlobalWaitPanel();
		Reset();
		ShowGlobalWaitPanelIfNeeded(loadingContent);
		BootstrapExtra.instance.gameAsyncOperationManager.EnqueueAsyncOperation(this);
	}

	internal void SetLane(GameAsyncOperationLane lane)
	{
		Lane = lane;
		showWaitPanel = !hideWaitPanelForLane;
		if (lane == GameAsyncOperationLane.Background)
		{
			showFaultPanel = false;
			BlocksLoading = false;
		}
	}

	protected virtual void Reset()
	{
		isComplete = false;
		isFault = false;
		isOver = false;
		isRunning = false;
		attachPanel = null;
		errorCode = null;
		traceId = null;
	}

	public void Run()
	{
		try
		{
			Reset();
			isRunning = true;
			RunImpl();
		}
		catch (Exception ex)
		{
			TraceLog.Error($"[{GetType()}] Run Exception: {ex}");
			ToFault(ex);
		}
	}

	public IEnumerator WaitForEnd()
	{
		yield return new WaitUntil(() => isOver);
	}

	public async Task WaitForEndAsync()
	{
		await TaskHelper.Wait(() => isOver);
	}

	protected abstract void RunImpl();

	public void Update()
	{
		if (task == null)
		{
			return;
		}
		if (task.IsCompletedSuccessfully)
		{
			if (DebugState.enableNetworkFault)
			{
				ToFault(new Exception());
			}
			else
			{
				ToComplete();
			}
		}
		else if (task.IsFaulted || task.IsCanceled)
		{
			ToFault(task.Exception);
			errorText = $"[{GetType()}] - failed [{errorCode}] \n{FormatOperation()} \n- Exception: {FormatException(task.Exception)}";
			TraceLog.Error(errorText);
		}
	}

	protected void ToComplete()
	{
		if (!isComplete)
		{
			TraceLog.Debug("【异步请求成功】" + ToString());
			isComplete = true;
			try
			{
				onComplete?.Invoke();
			}
			catch (Exception e)
			{
				TraceLog.Exception(e, "异步操作完成回调错误");
			}
			ToOver();
		}
	}

	protected void ToFault(Exception exception)
	{
		if (isFault)
		{
			return;
		}
		try
		{
			GenerateExceptionHint(exception);
			ToFault();
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "异步操作错误信息生成失败");
			ToFault();
		}
	}

	private string ParseExceptionError(int error)
	{
		return (CommonErrorCodes)error switch
		{
			CommonErrorCodes.INVALID_INPUT => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_invalidInput, 
			CommonErrorCodes.NOT_FOUND => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_notFound, 
			CommonErrorCodes.ALREADY_EXISTS => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_alreadyExists, 
			CommonErrorCodes.TEMPORARY_UNAVAILABLE => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_temporaryUnavaliable, 
			CommonErrorCodes.CLOSED => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_closed, 
			CommonErrorCodes.NETWORK_UNREACHABLE => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_networkUnreachable, 
			CommonErrorCodes.NETWORK_TIMEOUT => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_networkTimeout, 
			CommonErrorCodes.NETWORK_UNTRUSTED => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_networkUntrusted, 
			CommonErrorCodes.RATE_LIMITED => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_rateLimited, 
			CommonErrorCodes.OVERLOAD => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_overload, 
			CommonErrorCodes.INTERNAL_ERROR => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_internalError, 
			_ => ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_networkError, 
		};
	}

	private void GenerateExceptionHint(Exception exception)
	{
		errorCode = "";
		stackTraceText = "";
		waitingHint = "";
		if (exception is AggregateException { InnerException: not null } ex)
		{
			exception = ex.InnerException;
		}
		if (exception is ApiException<ErrorResponse> ex2)
		{
			errorCode = ex2.Result.Error.ToString();
			ex2.Result.Errordetails.TryGetValue("TraceId", out traceId);
			stackTraceText = ex2.StackTrace;
			waitingHint = TextDataPreset.GetServerTerm(errorCode);
			if (waitingHint.IsNullOrEmpty())
			{
				waitingHint = ParseExceptionError(ex2.Result.Error);
			}
		}
		else if (exception is HttpRequestException ex3)
		{
			waitingHint = ((Exception)(object)ex3).Message;
			stackTraceText = ((Exception)(object)ex3).StackTrace;
		}
		else
		{
			TraceLog.Error($"Exception: {exception}");
			waitingHint = waitingHint + "Unknown error" + Environment.NewLine;
			stackTraceText += exception.ToString();
		}
	}

	protected void ToFault()
	{
		if (!isFault)
		{
			isFault = true;
			if (showFaultPanel)
			{
				TrySpawnAttachPanel();
			}
			if (!showFaultPanel)
			{
				ToOver();
			}
		}
	}

	public void ToOver()
	{
		if (isOver)
		{
			return;
		}
		bool num = isFault;
		attachPanel = null;
		isOver = true;
		isRunning = false;
		HideGlobalWaitPanel();
		if (num)
		{
			try
			{
				onFault?.Invoke();
			}
			catch (Exception e)
			{
				TraceLog.Exception(e, "异步操作失败回调错误");
			}
		}
		try
		{
			onOver?.Invoke();
		}
		catch (Exception e2)
		{
			TraceLog.Exception(e2, "异步操作结束回调错误");
		}
	}

	public void InvokeFaultClose()
	{
		try
		{
			onFaultClose?.Invoke();
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "异步操作错误界面关闭回调错误");
		}
	}

	private void TrySpawnAttachPanel()
	{
		if (attachPanel == null)
		{
			attachPanel = AsyncOperationWaitPanel.Show(this);
		}
	}

	private void ShowGlobalWaitPanelIfNeeded(string loadingContent)
	{
		if (showWaitPanel && waitPanelToken == null)
		{
			waitPanelToken = BootstrapExtra.instance.waitPanelBase.Show(loadingContent);
		}
	}

	private void HideGlobalWaitPanel()
	{
		if (waitPanelToken != null)
		{
			BootstrapExtra.instance.waitPanelBase.Hide(waitPanelToken);
			waitPanelToken = null;
		}
	}

	protected string FormatException(Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (e is AggregateException ex)
		{
			foreach (Exception innerException in ex.InnerExceptions)
			{
				stringBuilder.Append("\n-- AggregateException:" + FormatException(innerException) + "\n");
			}
			return stringBuilder.ToString();
		}
		if (e is ApiException<ErrorResponse> ex2)
		{
			stringBuilder.Append($"- ApiException: \nstatus code: {ex2.StatusCode}, \nResponse: {ex2.Response}, \nResult: {ex2.Result}, \nError: {ex2.Result.Error}, \nMsg: {ex2.Result.Msg}");
			foreach (KeyValuePair<string, string> errordetail in ex2.Result.Errordetails)
			{
				stringBuilder.Append("\nerrordetail: " + errordetail.Key + " - " + errordetail.Value);
			}
			stringBuilder.Append("\nerrorCode: " + ex2.Result.Error);
			stringBuilder.Append("\nTraceId: " + traceId);
			ex2.Result.Errordetails.TryGetValue("TraceId", out traceId);
			return stringBuilder.ToString();
		}
		return e.ToString();
	}

	protected virtual string FormatOperation()
	{
		return "";
	}

	public static GeneralGameAsyncOperation<REQUEST, RESULT> Get<REQUEST, RESULT>(REQUEST request, Func<REQUEST, Task<RESULT>> operation)
	{
		return new GeneralGameAsyncOperation<REQUEST, RESULT>(request, operation);
	}

	public static GeneralGameAsyncOperationResultOnly<RESULT> Get<RESULT>(Func<Task<RESULT>> operation)
	{
		return new GeneralGameAsyncOperationResultOnly<RESULT>(operation);
	}

	public static bool IsTimeoutException(AggregateException e)
	{
		return e?.Flatten().InnerExceptions.Any((Exception ex) => ex is ApiException<ErrorResponse> e2 && IsTimeoutException(e2)) ?? false;
	}

	public static bool IsTimeoutException(ApiException<ErrorResponse> e)
	{
		if (e == null)
		{
			return false;
		}
		if (e.StatusCode == 500 || e.StatusCode == 100 || e.StatusCode == 30 || e.StatusCode == 31 || e.StatusCode == 32 || e.StatusCode == 33 || e.StatusCode == 34)
		{
			return true;
		}
		return false;
	}
}
