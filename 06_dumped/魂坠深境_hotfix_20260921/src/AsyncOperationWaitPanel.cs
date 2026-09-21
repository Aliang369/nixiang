using System;
using TMPro;
using UnityEngine;

public class AsyncOperationWaitPanel : MonoBehaviour
{
	[SerializeField]
	private GameObject errorPanel;

	[SerializeField]
	private GameButton retryButton;

	[SerializeField]
	private GameButton closeButton;

	[SerializeField]
	private TextMeshProUGUI errorDetailText;

	[SerializeField]
	private TextMeshProUGUI errorCodeText;

	[SerializeField]
	private TextMeshProUGUI stackTraceText;

	[SerializeField]
	private TextMeshProUGUI playerIdText;

	[SerializeField]
	private TextMeshProUGUI traceIdText;

	[SerializeField]
	private TextMeshProUGUI closeButtonText;

	[SerializeField]
	private TextMeshProUGUI retryButtonText;

	private GameAsyncOperation operation;

	private void Awake()
	{
		retryButton.onClick.AddListener(Retry);
		closeButton.onClick.AddListener(Hide);
	}

	public static AsyncOperationWaitPanel Show(GameAsyncOperation operation)
	{
		AsyncOperationWaitPanel asyncOperationWaitPanel = BootstrapExtra.instance.gameAsyncOperationManager.asyncOperationPanelPool.Spawn();
		asyncOperationWaitPanel.ShowImpl(operation);
		return asyncOperationWaitPanel;
	}

	private void ShowImpl(GameAsyncOperation operation)
	{
		this.operation = operation;
		base.gameObject.SetActive(value: true);
		closeButton.gameObject.SetActive(value: false);
		retryButton.gameObject.SetActive(value: false);
		errorPanel.SetActive(value: false);
		if (operation != null && operation.isFault)
		{
			RefreshFaultState();
		}
	}

	public void Hide()
	{
		try
		{
			if (operation != null)
			{
				bool isFault = operation.isFault;
				operation.ToOver();
				if (isFault)
				{
					operation.InvokeFaultClose();
				}
				operation = null;
			}
			base.gameObject.SetActive(value: false);
			Bootstrap.instance.bootstrapExtra.gameAsyncOperationManager.asyncOperationPanelPool.Despawn(this);
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[AsyncOperationWaitPanel] Hide() - e: {arg}");
		}
	}

	private void Retry()
	{
		operation.Retry();
		operation = null;
		base.gameObject.SetActive(value: false);
		Bootstrap.instance.bootstrapExtra.gameAsyncOperationManager.asyncOperationPanelPool.Despawn(this);
	}

	private void Update()
	{
		playerIdText.text = Bootstrap.instance.CurrentAccountID + "(" + AppVersionDisplay.GetDisplayVersion() + ")";
		if (operation == null)
		{
			return;
		}
		if (operation.isComplete)
		{
			Hide();
		}
		else if (operation.isFault)
		{
			if (operation.shouldAutoRetry)
			{
				operation.Retry();
				return;
			}
			RefreshFaultState();
		}
		closeButtonText.text = ((operation.isFault && !string.IsNullOrEmpty(operation.faultCloseButtonText)) ? operation.faultCloseButtonText : CommonConfig.Instance.GetCurrentLauncherLocalization().Item2.term_close);
		retryButtonText.text = CommonConfig.Instance.GetCurrentLauncherLocalization().Item2.term_retry;
	}

	private void RefreshFaultState()
	{
		errorPanel.SetActive(value: true);
		errorDetailText.text = operation?.waitingHint;
		stackTraceText.text = operation?.stackTraceText;
		errorCodeText.text = CommonConfig.Instance.GetCurrentLauncherLocalization().Item2.term_network_error_code_arg0.FormatWith(operation.errorCode);
		traceIdText.text = operation?.traceId;
		closeButton.gameObject.SetActive(value: true);
		retryButton.gameObject.SetActive(operation.manualRetry);
	}
}
