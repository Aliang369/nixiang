using Newtonsoft.Json.Linq;
using UnityEngine;

public class zbSDKCallHandler : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad((Object)(object)this);
	}

	public void SDK_OptionCall(string strParams)
	{
		JToken val = JToken.Parse(strParams);
		string text = val.Value<string>((object)"option");
		if (!string.IsNullOrEmpty(text) && text.Equals("error"))
		{
			string msg = val.Value<string>((object)"errormsg");
			zbUIMgr.Instance.BubbleInfo(msg);
		}
	}

	public void SDK_SetLoginData(string strParams)
	{
		JToken obj = JToken.Parse(strParams);
		string uid = obj.Value<string>((object)"uid");
		string token = obj.Value<string>((object)"token");
		int lChannel = obj.Value<int>((object)"channel");
		zbSdkHelper.SetLoginUserData(uid, token, lChannel);
	}

	public void SDK_SwitchAccount(string strParams)
	{
		zbSdkHelper.SetLoginState(state: false);
		if (zbGameStateMachine.Instance.GetStateId() == EGameState.Main)
		{
			zbWsMsgHandler.Instance.Game_LoginOut_Req();
		}
	}

	public void SDK_SetLoginError(string strParams)
	{
		zbChannel_Login.OnLoginError();
	}

	public void SDK_SetPayData(string strParams)
	{
		zbChannel_Pay.OnPayCallback_SDK(strParams);
	}

	public void SDK_IdentityCheckAck(string strParams)
	{
		JToken obj = JToken.Parse(strParams);
		string s = obj.Value<string>((object)"state");
		string s2 = obj.Value<string>((object)"ageLimit");
		int num = int.Parse(s);
		zbLoginUtil.ageRange = int.Parse(s2);
		switch (num)
		{
		case 0:
			zbChannel_Login.OnIdentitySuccess();
			break;
		case 1:
			zbUIMgr.Instance.BubbleInfo("年龄大于等于12岁才可进入游戏");
			break;
		case 2:
			zbUIMgr.Instance.BubbleInfo("当前时间为非游玩时间段");
			break;
		case 3:
			zbUIMgr.Instance.ShowMessageConfirm("游玩时间已结束，请退出游戏", OnGameTimeEnd, showClostBtn: false);
			break;
		}
	}

	private void OnGameTimeEnd(bool isConfirm)
	{
		if (zbGameStateMachine.Instance.GetStateId() == EGameState.Main)
		{
			zbWsMsgHandler.Instance.Game_LoginOut_Req();
		}
	}

	public void SDK_PaymentCheckAck(string strParams)
	{
		if (int.Parse(JToken.Parse(strParams).Value<string>((object)"state")) == 0)
		{
			zbChannel_Pay.OnPayCheckAck();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo("当前已达到充值最大额度");
		}
	}
}
