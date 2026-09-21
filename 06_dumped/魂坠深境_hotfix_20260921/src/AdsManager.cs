using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreKit.Agent;
using UnityEngine;
using UnityEngine.UI;

public class AdsManager
{
	public const string freeMoneyAd = "free_money";

	public const string reviveAd = "revive";

	public const string refreshBlessAd = "refresh_bless";

	public bool inited;

	public int maxRetryTime = 30;

	private readonly List<Coroutine> autoLoadCoroutines = new List<Coroutine>();

	private MonoBehaviour coroutineRunner;

	private bool ShouldSkipRewardedAd
	{
		get
		{
			ApplicationStateInfo applicationState = Bootstrap.applicationState;
			if (applicationState == null)
			{
				return false;
			}
			return applicationState.playerInfoManager?.vip == true;
		}
	}

	public void Init()
	{
		if (!inited)
		{
			inited = true;
			TraceLog.Debug("[AdsManager] Init");
			coroutineRunner = Bootstrap.instance;
			autoLoadCoroutines.Add(coroutineRunner.StartCoroutine(AutoLoad()));
			Task.Run(delegate
			{
				AndroidJNI.AttachCurrentThread();
				Debug.Log("run线程是" + Environment.CurrentManagedThreadId);
				AgentHub.Instance.Ad.callInit();
				AndroidJNI.DetachCurrentThread();
			});
		}
	}

	public void Dispose()
	{
		if (coroutineRunner != null)
		{
			foreach (Coroutine autoLoadCoroutine in autoLoadCoroutines)
			{
				coroutineRunner.StopCoroutine(autoLoadCoroutine);
			}
		}
		autoLoadCoroutines.Clear();
		coroutineRunner = null;
		inited = false;
	}

	public void InitUmpButton(Button umpButton)
	{
		TraceLog.Debug("[AdsManager] InitUmpButton");
		umpButton.gameObject.SetActive(value: false);
	}

	public void EnterAdScene(string adName)
	{
		TraceLog.Debug("[AdsManager] EnterAdScene(" + adName + ")");
		if (ShouldSkipRewardedAd)
		{
			return;
		}
		ThinkingAnalyticsExtends.TrackEnterAdScene(Bootstrap.applicationState, adName, IsAdReady(adName));
		if (PlatformHelper.IsEditor() || !GlobalConfigManager.instance.enableNetwork || !PlatformHelper.IsMobile())
		{
			return;
		}
		try
		{
			AgentHub.Instance.Ad.callEntryAdScenario(adName);
		}
		catch (Exception ex)
		{
			TraceLog.Error("[AdsManager] EnterAdScene(" + adName + ") Exception: " + ex.Message);
		}
	}

	public void ShowAd(string adName, Action onAdComplete, Action onAdFail, Action onAdClose)
	{
		TraceLog.Debug("[AdsManager] ShowAd(" + adName + ")");
		if (ShouldSkipRewardedAd)
		{
			onAdComplete?.Invoke();
			return;
		}
		ThinkingAnalyticsExtends.TrackShowAdScene(Bootstrap.applicationState, adName);
		if (PlatformHelper.IsEditor() || !GlobalConfigManager.instance.enableNetwork)
		{
			onAdComplete?.Invoke();
		}
		else if (PlatformHelper.IsMobile())
		{
			BootstrapExtra.instance.adPanel.ShowLoading();
			if (!TryShow(adName, OnAdComplete, OnAdFail, OnAdClose))
			{
				OnAdFail();
			}
		}
		void OnAdClose()
		{
			BootstrapExtra.instance.adPanel.ShowFailed();
			onAdClose?.Invoke();
		}
		void OnAdComplete()
		{
			BootstrapExtra.instance.adPanel.Hide();
			onAdComplete?.Invoke();
		}
		void OnAdFail()
		{
			BootstrapExtra.instance.adPanel.ShowFailed();
			onAdFail?.Invoke();
		}
	}

	private bool TryShow(string adName, Action onAdComplete, Action onAdFail, Action onAdClose)
	{
		bool callBackInvoked = false;
		try
		{
			if (IsAdReady(adName))
			{
				AgentHub.Instance.Ad.callShowAd(adName, (Action)OnComplete, (Action)OnADFail, (Action)OnClose);
				return true;
			}
		}
		catch (Exception ex)
		{
			TraceLog.Error("[AdsManager] TryShow(" + adName + ") Exception: " + ex.Message);
			OnADFail();
			return true;
		}
		return false;
		void OnADFail()
		{
			TraceLog.Debug("[AdsManager] TryShow(" + adName + ") - OnADFail()");
			if (!callBackInvoked)
			{
				callBackInvoked = true;
				OnADPlayEnd(adName);
				onAdFail?.Invoke();
			}
		}
		void OnClose()
		{
			TraceLog.Debug("[AdsManager] TryShow(" + adName + ") - OnClose()");
			if (!callBackInvoked)
			{
				callBackInvoked = true;
				OnADPlayEnd(adName);
				onAdClose?.Invoke();
			}
		}
		void OnComplete()
		{
			TraceLog.Debug("[AdsManager] TryShow(" + adName + ") - OnComplete()");
			if (!callBackInvoked)
			{
				callBackInvoked = true;
				OnADPlayEnd(adName);
				onAdComplete?.Invoke();
			}
		}
	}

	private void OnADPlayEnd(string adName)
	{
		TraceLog.Debug("[AdsManager] OnADPlayEnd(" + adName + ")");
		if (!PlatformHelper.IsEditor() && GlobalConfigManager.instance.enableNetwork)
		{
			bool flag = IsAdReady(adName);
			TraceLog.Debug($"[AdsManager] OnADPlayEnd({adName}) - {flag}");
			if (!flag)
			{
				LoadAd(adName);
			}
		}
	}

	public bool IsAdReady(string adName)
	{
		if (PlatformHelper.IsEditor() || !GlobalConfigManager.instance.enableNetwork)
		{
			return true;
		}
		if (PlatformHelper.IsMobile())
		{
			try
			{
				bool flag = AgentHub.Instance.Ad.callIsAdReady(adName);
				TraceLog.Debug($"[AdsManager] IsAdReady({adName}) - result:{flag}");
				return flag;
			}
			catch (Exception ex)
			{
				TraceLog.Error("[AdsManager] IsAdReady(" + adName + ") Exception: " + ex.Message);
				return false;
			}
		}
		TraceLog.Error("[AdsManager] IsAdReady(" + adName + ") - false");
		return false;
	}

	public bool CanClaimReward(string adName)
	{
		if (!ShouldSkipRewardedAd)
		{
			return IsAdReady(adName);
		}
		return true;
	}

	public void LoadAd(string adName)
	{
		TraceLog.Debug("[AdsManager] LoadAd(" + adName + ")");
		if (ShouldSkipRewardedAd || PlatformHelper.IsEditor() || !GlobalConfigManager.instance.enableNetwork || !PlatformHelper.IsMobile())
		{
			return;
		}
		if (IsAdReady(adName))
		{
			TraceLog.Debug("[AdsManager] LoadAd(" + adName + ") - ready, not load");
			return;
		}
		try
		{
			AgentHub.Instance.Ad.callLoadAd(adName);
		}
		catch (Exception ex)
		{
			TraceLog.Error("[AdsManager] LoadAd(" + adName + ") Exception: " + ex.Message);
		}
	}

	public void LoadAll()
	{
		LoadAd("free_money");
		LoadAd("revive");
		LoadAd("refresh_bless");
	}

	private IEnumerator AutoLoad()
	{
		autoLoadCoroutines.Add(coroutineRunner.StartCoroutine(AutoLoad("free_money")));
		yield return new WaitForSeconds(3f);
		autoLoadCoroutines.Add(coroutineRunner.StartCoroutine(AutoLoad("refresh_bless")));
		yield return new WaitForSeconds(3f);
		autoLoadCoroutines.Add(coroutineRunner.StartCoroutine(AutoLoad("revive")));
		yield return new WaitForSeconds(3f);
		IEnumerator AutoLoad(string adName)
		{
			if (!ShouldSkipRewardedAd)
			{
				int retryTimes = 0;
				while (!ShouldSkipRewardedAd)
				{
					if (!IsAdReady(adName))
					{
						retryTimes++;
						LoadAd(adName);
					}
					yield return new WaitForSeconds(10f);
					if (IsAdReady(adName))
					{
						TraceLog.Debug($"[AdsManager] AutoLoad({adName}) - ready, {retryTimes}");
						ThinkingAnalyticsExtends.TrackAdLoaded(Bootstrap.applicationState, adName, retryTimes, success: true);
						retryTimes = 0;
					}
					else if (retryTimes > maxRetryTime)
					{
						TraceLog.Debug($"[AdsManager] AutoLoad({adName}) - failed, {retryTimes}");
						ThinkingAnalyticsExtends.TrackAdLoaded(Bootstrap.applicationState, adName, retryTimes, success: false);
						retryTimes = 0;
					}
				}
			}
		}
	}
}
