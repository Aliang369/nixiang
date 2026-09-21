using System;
using System.Collections.Generic;
using ChillyRoom.MailService.V1;
using Common.UI;
using TMPro;
using UnityEngine;

public class MailDetailInfoPanel : UIBase
{
	public TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI senderName;

	[SerializeField]
	private TextMeshProUGUI senderDate;

	[SerializeField]
	private GameObject rewardGotMask;

	public TextMeshProUGUI content;

	public GameButton accpetButton;

	public TextMeshProUGUI accpetButtonText;

	public GameObject attachRewardArea;

	public GlobalItemDisplay globalItemDisplayPrefab;

	public RectTransform globalItemDisplayParent;

	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> globalItemDisplayPool;

	private MailData mailData;

	protected override void Init()
	{
		globalItemDisplayPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(globalItemDisplayPrefab, globalItemDisplayParent);
		accpetButton.onClick.AddListener(_OnAccpetButtonClicked);
	}

	public void Setup(MailData mailData)
	{
		this.mailData = mailData;
		if (GlobalConfigManager.instance.enableNetwork && !mailData.isRead)
		{
			List<Guid> list = new List<Guid>();
			list.Add(mailData.id);
			new GeneralGameAsyncOperation<List<Guid>, ReadMailsResponse>(list, UIBase.applicationState.ChillyRoomSDK.Mail.ReadMailsAsync).OnComplete(delegate
			{
				UIBase.applicationState.mailManager.SetMailRead(mailData);
				if (!(this == null))
				{
					SetDirty();
				}
			}).EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_mail_attachment);
		}
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (mailData == null)
		{
			return;
		}
		title.text = mailData.title;
		senderName.text = mailData.sender;
		senderDate.text = mailData.SendTime.ToLocalTime().ToString("yyyy-MM-dd");
		content.text = mailData.content;
		attachRewardArea.SetActive(!mailData.attachRewards.IsNullOrEmpty());
		rewardGotMask.SetActive(mailData.isAccepted);
		accpetButton.interactable = !mailData.isAccepted;
		accpetButtonText.text = (mailData.isAccepted ? ScriptableObjectSingleton<TextDataPreset>.instance.term_mail_attachment_accpeted : ScriptableObjectSingleton<TextDataPreset>.instance.term_mail_accpet_attachment);
		globalItemDisplayPool.DespawnAll();
		foreach (GlobalRewardInfo item in mailData.attachRewards.OrEmptyIfNull())
		{
			globalItemDisplayPool.Spawn(item);
		}
	}

	private void _OnAccpetButtonClicked()
	{
		UIBase.applicationState.mailManager.AccpetMail(new List<MailData> { mailData });
	}
}
