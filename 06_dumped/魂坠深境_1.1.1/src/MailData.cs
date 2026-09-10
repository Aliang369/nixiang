using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.MailService.V1;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class MailData
{
	private class GMItemSkillExtra
	{
		public int level;

		public int archtypeIndex;
	}

	[SaveField(null, null)]
	public Guid id;

	[SaveField(null, null)]
	public bool isRead;

	[SaveField(null, null)]
	public bool isAccepted;

	[SaveField(null, null)]
	public string title;

	[SaveField(null, null)]
	public string content;

	[SaveField(null, null)]
	public string sender;

	[SaveField(null, null)]
	public DateTimeOffset SendTime;

	[SaveField(null, null)]
	public DateTimeOffset? ExpiredAt;

	[SaveField(null, null)]
	public List<GlobalRewardInfo> attachRewards;

	public bool WithAttachmentToAccpet
	{
		get
		{
			if (!attachRewards.IsNullOrEmpty())
			{
				return !isAccepted;
			}
			return false;
		}
	}

	public MailData()
	{
	}

	public MailData(ApiMailResponse response, ApplicationStateInfo asi)
	{
		id = response.MailBox.MailBoxId;
		isRead = response.IsRead;
		isAccepted = response.IsAccepted;
		title = response.MailBox.Title;
		content = response.MailBox.Content;
		sender = response.MailBox.Sender;
		SendTime = response.MailBox.SendTime;
		ExpiredAt = response.MailBox.ExpiredAt;
		attachRewards = response.MailBox.Attachments.Select((Attachment a) => AttachmentToGlobalReward(asi, a)).ToList();
	}

	public static GlobalRewardInfo AttachmentToGlobalReward(ApplicationStateInfo asi, Attachment attachment)
	{
		NetworkConvertContext networkConvertContext = new NetworkConvertContext(asi);
		string text = attachment.Id;
		ICTAsset iCTAsset = networkConvertContext.FullIdToCTAsset<ICTAsset>(text);
		if (iCTAsset is MatterDef matterDef)
		{
			return new GlobalRewardInfo
			{
				matter = new MatterInfo(matterDef, (int)attachment.Count)
			};
		}
		if (iCTAsset is SkillDef def)
		{
			GMItemSkillExtra gMItemSkillExtra = JsonUtility.FromJson<GMItemSkillExtra>(attachment.Extra);
			ProfileSkill skill = new ProfileSkill(def, gMItemSkillExtra.archtypeIndex);
			return new GlobalRewardInfo
			{
				skill = skill
			};
		}
		if (iCTAsset is SkillGemDef def2)
		{
			SkillGem skillGem = new SkillGem(def2, (int)attachment.Count);
			return new GlobalRewardInfo
			{
				skillGem = skillGem
			};
		}
		if (iCTAsset is PlayerCharacterDef playerCharacterDef)
		{
			return new GlobalRewardInfo
			{
				matter = new MatterInfo(playerCharacterDef.obsession, 1)
			};
		}
		if (iCTAsset is ItemSkinDef unlockItemSkin)
		{
			return new GlobalRewardInfo
			{
				unlockItemSkin = unlockItemSkin
			};
		}
		if (iCTAsset is StartGameBuffDef startGameBuffDef)
		{
			int num = Mathf.Max(1, (int)attachment.Count);
			if (num == 1)
			{
				return new GlobalRewardInfo
				{
					getStartGameBuff = new StartGameBuff(startGameBuffDef)
				};
			}
			List<GlobalRewardInfo> list = new List<GlobalRewardInfo>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(new GlobalRewardInfo
				{
					getStartGameBuff = new StartGameBuff(startGameBuffDef)
				});
			}
			return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.None, list);
		}
		if (iCTAsset is SeasonDef unlockSeasonPass)
		{
			return new GlobalRewardInfo
			{
				unlockSeasonPass = unlockSeasonPass
			};
		}
		return null;
	}
}
