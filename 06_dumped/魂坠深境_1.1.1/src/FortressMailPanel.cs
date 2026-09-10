using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.MailService.V1;
using Common.UI;
using TMPro;
using UnityEngine;

public class FortressMailPanel : UIBase, IStackPanel
{
	public GameButton backButton;

	public GameButton delReadMailButton;

	public GameButton accpetAllButton;

	public MailDetailInfoPanel mailDetailInfoPanel;

	public MailCell mailCellPrefab;

	public RectTransform mailCellParent;

	[SerializeField]
	private TextMeshProUGUI mailCountText;

	[SerializeField]
	private GameObject deleteConfirmPanel;

	[SerializeField]
	private GameButton confirmDeleteButton;

	[SerializeField]
	private GameButton cancelDeleteButton;

	private CellUIPrefabPool<MailCell, MailData> mailCellPool;

	private MailData selectingMail;

	private long lastMailUpdateTime;

	private List<MailData> topMostMailList = new List<MailData>();

	private List<MailData> middleMailList = new List<MailData>();

	private List<MailData> bottomMostMailList = new List<MailData>();

	public Guid selectingMailId = Guid.Empty;

	protected override void Init()
	{
		InitUI(mailDetailInfoPanel);
		mailCellPool = GetPrefabPool<MailCell, MailData>(mailCellPrefab, mailCellParent);
		backButton.onClick.AddListener(_OnCloseButtonClicked);
		delReadMailButton.onClick.AddListener(_OnDeleteReadMailButtonClicked);
		accpetAllButton.onClick.AddListener(_OnAccpetAllButtonClicked);
		UIBase.applicationState.eventManager.mailStateUpdateEvent.AddListener(_OnMailStateUpdate);
		confirmDeleteButton.onClick.AddListener(_OnDeleteReadMailButtonConfirmed);
		cancelDeleteButton.onClick.AddListener(_OnDeleteReadMailButtonCanceled);
		SortAndUpdateMailList();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.mailStateUpdateEvent.RemoveListener(_OnMailStateUpdate);
	}

	protected override void UpdateDisplay()
	{
		mailCellPool.DespawnAll();
		foreach (MailData topMostMail in topMostMailList)
		{
			mailCellPool.Spawn(topMostMail);
		}
		foreach (MailData middleMail in middleMailList)
		{
			mailCellPool.Spawn(middleMail);
		}
		foreach (MailData bottomMostMail in bottomMostMailList)
		{
			mailCellPool.Spawn(bottomMostMail);
		}
		if (selectingMail == null)
		{
			mailDetailInfoPanel.SetActiveAndDirty(active: false);
		}
		accpetAllButton.interactable = UIBase.applicationState.mailManager.mailList.Any((MailData m) => m.WithAttachmentToAccpet);
	}

	private void UpdateAutoSelectMail()
	{
		if (topMostMailList.Count > 0)
		{
			SelectMail(topMostMailList[0]);
		}
		else if (middleMailList.Count > 0)
		{
			SelectMail(middleMailList[0]);
		}
		else if (bottomMostMailList.Count > 0)
		{
			SelectMail(bottomMostMailList[0]);
		}
		else
		{
			SelectMail(null);
		}
	}

	private void SortAndUpdateMailList()
	{
		int count = UIBase.applicationState.mailManager.mailList.Count;
		topMostMailList.Clear();
		middleMailList.Clear();
		bottomMostMailList.Clear();
		foreach (MailData mail in UIBase.applicationState.mailManager.mailList)
		{
			if (!mail.isRead)
			{
				Debug.Log($"[In TopMostMailList] Title: {mail.title}, isRead: {mail.isRead}, isAccepted: {mail.isAccepted}, withAttachment: {mail.attachRewards.Count}, expire: {mail.ExpiredAt}");
				topMostMailList.Add(mail);
			}
			else if (mail.isAccepted || (mail.attachRewards.IsNullOrEmpty() && mail.isRead))
			{
				Debug.Log($"[In BottomMostMailList] Title: {mail.title}, isRead: {mail.isRead}, isAccepted: {mail.isAccepted}, withAttachment: {mail.attachRewards.Count}, expire: {mail.ExpiredAt}");
				bottomMostMailList.Add(mail);
			}
			else
			{
				Debug.Log($"[In MiddleMailList] Title: {mail.title}, isRead: {mail.isRead}, isAccepted: {mail.isAccepted}, withAttachment: {mail.attachRewards.Count}, expire: {mail.ExpiredAt}");
				middleMailList.Add(mail);
			}
			if (topMostMailList.Count > 0)
			{
				topMostMailList.Sort((MailData a, MailData b) => a.SendTime.CompareTo(b.SendTime));
			}
			else if (middleMailList.Count > 0)
			{
				middleMailList.Sort((MailData a, MailData b) => a.SendTime.CompareTo(b.SendTime));
			}
			else if (bottomMostMailList.Count > 0)
			{
				bottomMostMailList.Sort((MailData a, MailData b) => a.SendTime.CompareTo(b.SendTime));
			}
		}
		UpdateAutoSelectMail();
		int num = UIBase.applicationState.mailManager.mailList.Count((MailData m) => !m.isRead);
		mailCountText.text = $"{num}/{count}";
	}

	public void FetchMailAndShow()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastMailUpdateTime < 60000)
		{
			SortAndUpdateMailList();
			SetActiveAndDirtyWithFadeAnim(active: true);
			parent.SetDirty();
			return;
		}
		ResetLastMailUpdateTime();
		Dictionary<string, string> request = new Dictionary<string, string>();
		GeneralGameAsyncOperation<Dictionary<string, string>, GetMailsResponse> op = new GeneralGameAsyncOperation<Dictionary<string, string>, GetMailsResponse>(request, UIBase.applicationState.ChillyRoomSDK.Mail.GetMailsAsync);
		op.OnComplete(delegate
		{
			UIBase.applicationState.mailManager.mailList = op.OpResult.Mails.Select((ApiMailResponse amr) => new MailData(amr, UIBase.applicationState)).ToList();
			if (!(this == null) && base.gameObject.activeInHierarchy)
			{
				SortAndUpdateMailList();
				SetActiveAndDirtyWithFadeAnim(active: true);
			}
		}).EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_order_data);
	}

	public void ResetLastMailUpdateTime()
	{
		lastMailUpdateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
	}

	private void _OnDeleteReadMailButtonConfirmed()
	{
		UIBase.applicationState.mailManager.DeleteMail(UIBase.applicationState.mailManager.mailList.Where((MailData m) => m.isRead && (m.isAccepted || m.attachRewards.IsNullOrEmpty())).ToList());
		deleteConfirmPanel.SetActive(value: false);
	}

	private void _OnDeleteReadMailButtonCanceled()
	{
		deleteConfirmPanel.SetActive(value: false);
	}

	private void _OnDeleteReadMailButtonClicked()
	{
		deleteConfirmPanel.SetActive(value: true);
	}

	private void _OnAccpetAllButtonClicked()
	{
		UIBase.applicationState.mailManager.AccpetMail(UIBase.applicationState.mailManager.mailList.Where((MailData m) => m.WithAttachmentToAccpet).ToList());
	}

	private void _OnCloseButtonClicked()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void _OnMailStateUpdate(List<MailData> mailDatas)
	{
		SortAndUpdateMailList();
		SetDirty();
	}

	public void SelectMail(MailData mailData)
	{
		selectingMail = mailData;
		if (mailData != null)
		{
			selectingMailId = mailData.id;
			mailDetailInfoPanel.Setup(mailData);
			mailDetailInfoPanel.SetActiveAndDirty(active: true);
		}
		SetDirty();
	}
}
