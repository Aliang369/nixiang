using UnityEngine;

public class zbStartUp
{
	public static void Run()
	{
		Application.runInBackground = true;
		Application.targetFrameRate = 60;
		zbUxmlFactoryRegister.RegisterHotUpdateFactories();
		zbSdkHelper.OnAutopatchState("1");
		zbAssetLoader.Instance.Init();
		zbDataConfig.Instance.Init();
		zbLanguage.Instance.Init();
		zbFontReplacer.Instance.Init();
		zbGameStateMachine.Instance.Init();
		zbMusicStateMachine.Instance.Init();
		zbNetUrl.InitUrl();
		zbLoginUtil.Init();
		zbNewNoticeHandler.Init();
		zbCharChecker.InsertNewBadwords();
		InitLogCatcher();
		zbGameStateMachine.Instance.SetInitState(EGameState.Login);
	}

	private static void InitLogCatcher()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Object.FindFirstObjectByType<zbLogCatcher>() == (Object)null)
		{
			new GameObject("zbLogCatcher").AddComponent<zbLogCatcher>();
		}
	}
}
