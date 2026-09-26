using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbTopBar : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbTopBar>
	{
	}

	private VisualElement m_RootElement;

	private zbUIEvent m_uiEvent;

	private VisualElement m_rootThis;

	private Button m_btnMail;

	public Button m_btnFriend;

	private Button m_btnRank;

	private Button m_btnBag;

	private Button m_btnHub;

	private VisualElement m_notifyMail;

	private VisualElement m_notifyFriend;

	private VisualElement m_notifyRank;

	private VisualElement m_notifyBag;

	private VisualElement m_notifyHub;

	private VisualElement m_newNotice;

	public zbTopBar()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/topBar.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		m_RootElement = wndParent.GetRootElement();
		m_uiEvent = wndParent.m_uiEvent;
		m_rootThis = (VisualElement)(object)UQueryExtensions.Q<zbTopBar>(m_RootElement, (string)null, (string)null);
		m_btnMail = UQueryExtensions.Q<Button>(m_RootElement, "btn_mail", (string)null);
		m_btnFriend = UQueryExtensions.Q<Button>(m_RootElement, "btn_friend", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnBag = UQueryExtensions.Q<Button>(m_RootElement, "btn_bag", (string)null);
		m_btnHub = UQueryExtensions.Q<Button>(m_RootElement, "btn_hub", (string)null);
		m_notifyMail = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnMail, (Vector2?)new Vector2(10f, 10f), 0.8f, true);
		m_notifyFriend = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFriend, (Vector2?)new Vector2(10f, 10f), 0.8f, true);
		m_notifyRank = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRank, (Vector2?)new Vector2(10f, 10f), 0.8f, true);
		m_notifyBag = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBag, (Vector2?)new Vector2(10f, 10f), 0.8f, true);
		m_notifyHub = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnHub, (Vector2?)new Vector2(10f, 10f), 0.8f, true);
		m_newNotice = zbUtil.CreateNewTag((VisualElement)(object)m_btnHub, (Vector2?)new Vector2(0f, 0f), 0.8f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMail, (Action<ClickEvent>)OnMail, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFriend, (Action<ClickEvent>)OnFriend, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action<ClickEvent>)OnRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBag, (Action<ClickEvent>)OnBag, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHub, (Action<ClickEvent>)OnHub, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		m_rootThis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.MainQuestInfo.MainLevel < 4));
		((VisualElement)m_btnFriend).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Friends) ? 1 : 0));
		IosCheckHide();
	}

	public void OnMail(ClickEvent evt)
	{
		zbLinkHelper.OpenMail();
	}

	public void OnFriend(ClickEvent evt)
	{
		zbLinkHelper.OpenFriend();
	}

	public void OnRank(ClickEvent evt)
	{
		zbLinkHelper.OpenRank();
	}

	public void OnBag(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowBag();
	}

	public void OnHub(ClickEvent evt)
	{
		zbLoginUtil.NoticListReq(open: true);
	}

	public void UpdateRedPoint()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		m_notifyMail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Mail)));
		m_notifyFriend.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Friend)));
		m_notifyRank.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Rank)));
		m_notifyBag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Bag)));
		bool flag = false;
		m_notifyHub.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_newNotice.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag || !zbNewNoticeHandler.HasNewPoint()) ? 1 : 0));
	}

	public void IosCheckHide()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Application.platform == 8)
		{
			m_rootThis.style.top = StyleLength.op_Implicit(180f);
		}
		m_rootThis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
	}
}
