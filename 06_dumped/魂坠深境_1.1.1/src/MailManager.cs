using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.MailService.V1;
using ChillyRoom.Services.ContainerOfSoul;

public class MailManager : GlobalGameManager
{
	[SaveField(null, null)]
	public List<MailData> mailList;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (mailList == null && GlobalConfigManager.instance.enableNetwork)
		{
			mailList = new List<MailData>();
		}
		if (mailList == null)
		{
			mailList = new List<MailData>();
		}
	}

	private static bool IsApplicationStateActive(ApplicationStateInfo targetApplicationState, BootStrapLoadData targetBootStrapLoadData = null)
	{
		if (targetApplicationState != null && targetApplicationState.inited && (targetBootStrapLoadData == null || targetApplicationState.bootStrapLoadData == targetBootStrapLoadData))
		{
			return Bootstrap.instance?.CurrentApplicationState == targetApplicationState;
		}
		return false;
	}

	public void SetMailRead(MailData mail)
	{
		mail.isRead = true;
		if (base.applicationState.fortressState != null)
		{
			base.applicationState.fortressState.uiManager.SetDirty();
		}
	}

	public void DeleteMail(List<MailData> toList)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		List<Guid> request = toList.Select((MailData m) => m.id).ToList();
		GeneralGameAsyncOperation<List<Guid>, DeleteMailsResponse> op = new GeneralGameAsyncOperation<List<Guid>, DeleteMailsResponse>(request, base.applicationState.ChillyRoomSDK.Mail.DeleteMailsAsync);
		op.OnComplete(delegate
		{
			List<MailData> successList = toList.Where((MailData m) => !op.OpResult.FailedMailBoxIds.Contains(m.id)).ToList();
			mailList.RemoveAll((MailData m) => successList.Contains(m));
			base.applicationState.eventManager.mailStateUpdateEvent.TriggerEvent(mailList);
			base.applicationState.fortressState.uiManager.fortressMailPanel.ResetLastMailUpdateTime();
			if (base.applicationState.fortressState != null)
			{
				base.applicationState.fortressState.uiManager.SetDirty();
			}
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_deleting_mail);
	}

	public void AccpetMail(List<MailData> mails)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		List<Guid> request = mails.Select((MailData m) => m.id).ToList();
		GeneralGameAsyncOperation<List<Guid>, AcceptAttachmentsResponse> op = new GeneralGameAsyncOperation<List<Guid>, AcceptAttachmentsResponse>(request, base.applicationState.ChillyRoomSDK.Mail.AcceptAttachmentsAsync);
		op.OnComplete(delegate
		{
			if (op.OpResult.ErrCode == MailErrCode.Ok)
			{
				AcceptMail(mails);
			}
			else if (op.OpResult.ErrCode == MailErrCode.PartialOk)
			{
				List<MailData> successRewardMails = mails.Where((MailData m) => !op.OpResult.FailedMailBoxIds.Contains(m.id)).ToList();
				AcceptMail(successRewardMails);
				TraceLog.Error("邮件附件无法获得" + op.OpResult.FailedMailBoxIds.Select((Guid a) => a.ToString()).Aggregate((string a, string b) => a + "," + b));
			}
			else
			{
				base.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_mail_attach_accept_failed);
				TraceLog.Error("邮件附件无法获得" + op.OpResult.FailedMailBoxIds.Select((Guid a) => a.ToString()).Aggregate((string a, string b) => a + "," + b));
			}
			StartClaimMailAttachmentPolling();
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_mail_attachment);
		void AcceptMail(List<MailData> successRewardMails)
		{
			foreach (MailData successRewardMail in successRewardMails)
			{
				if (successRewardMail.WithAttachmentToAccpet)
				{
					successRewardMail.isAccepted = true;
					successRewardMail.isRead = true;
					ThinkingAnalyticsExtends.TrackAcceptMail(base.applicationState, successRewardMail);
				}
			}
			base.applicationState.eventManager.mailStateUpdateEvent.TriggerEvent(base.applicationState.mailManager.mailList);
			if (base.applicationState.fortressState != null)
			{
				base.applicationState.fortressState.uiManager.SetDirty();
			}
		}
	}

	public void StartClaimMailAttachmentPolling()
	{
		ApplicationStateInfo targetApplicationState = base.applicationState;
		if (targetApplicationState == null)
		{
			return;
		}
		BootStrapLoadData targetBootStrapLoadData = targetApplicationState.bootStrapLoadData;
		GeneralGameAsyncOperationResultOnly<ClaimAttachmentsReply> claimOp = new GeneralGameAsyncOperationResultOnly<ClaimAttachmentsReply>(targetApplicationState.ChillyRoomSDK.MailBox.ClaimAttachmentsAsync);
		claimOp.OnComplete(delegate
		{
			if (IsApplicationStateActive(targetApplicationState, targetBootStrapLoadData) && !claimOp.OpResult.RewardInfos.IsNullOrEmpty())
			{
				List<GlobalRewardInfo> rewards = claimOp.OpResult.RewardInfos.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(new NetworkConvertContext(targetApplicationState))).ToList();
				DisplayMailRewards(rewards, targetApplicationState, targetBootStrapLoadData);
			}
		});
		new GameAsyncOperationPolling(claimOp, new List<float> { 0f, 1f, 2f, 4f, 8f }).AssignPolling();
	}

	private void DisplayMailRewards(List<GlobalRewardInfo> rewards, ApplicationStateInfo targetApplicationState = null, BootStrapLoadData targetBootStrapLoadData = null)
	{
		if (targetApplicationState == null)
		{
			targetApplicationState = base.applicationState;
		}
		if (!IsApplicationStateActive(targetApplicationState, targetBootStrapLoadData))
		{
			return;
		}
		foreach (GlobalRewardInfo reward in rewards)
		{
			reward.Give(targetApplicationState);
		}
		targetApplicationState.fortressState?.uiManager.globalRewardPackageDisplay.Setup(new GlobalRewardPackage(rewards));
	}

	public void StartGetMailsPolling()
	{
		Dictionary<string, string> request = new Dictionary<string, string>();
		ApplicationStateInfo targetApplicationState = base.applicationState;
		if (targetApplicationState == null)
		{
			return;
		}
		BootStrapLoadData targetBootStrapLoadData = targetApplicationState.bootStrapLoadData;
		GeneralGameAsyncOperation<Dictionary<string, string>, GetMailsResponse> GetMailsOp = new GeneralGameAsyncOperation<Dictionary<string, string>, GetMailsResponse>(request, targetApplicationState.ChillyRoomSDK.Mail.GetMailsAsync);
		GetMailsOp.OnComplete(delegate
		{
			if (IsApplicationStateActive(targetApplicationState, targetBootStrapLoadData))
			{
				mailList = GetMailsOp.OpResult.Mails.Select((ApiMailResponse amr) => new MailData(amr, targetApplicationState)).ToList();
				targetApplicationState.eventManager.mailStateUpdateEvent.TriggerEvent(mailList);
				TraceLog.Debug("[MailManager] - StartGetMailsPolling() - OnComplete");
				if (targetApplicationState.fortressState != null)
				{
					targetApplicationState.fortressState.uiManager.SetDirty();
				}
			}
		});
		GameAsyncOperationPolling gameAsyncOperationPolling = new GameAsyncOperationPolling(GetMailsOp, new List<float> { 0f, 10f, 10f });
		gameAsyncOperationPolling.stopOnComplete = false;
		gameAsyncOperationPolling.AssignPolling();
	}
}
