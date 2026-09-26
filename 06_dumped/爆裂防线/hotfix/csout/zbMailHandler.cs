using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;

public class zbMailHandler
{
	public RepeatedField<MailDetail> mailList = new RepeatedField<MailDetail>();

	public void Init()
	{
	}

	public MailDetail GetMailDetail(long mailId)
	{
		foreach (MailDetail mail in mailList)
		{
			if (mail.MailId == mailId)
			{
				return mail;
			}
		}
		return null;
	}

	public void UpdateMailList(RepeatedField<MailDetail> _mailList)
	{
		mailList = _mailList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Mail);
	}

	public void UpdateMailDetail(MailDetail mailDetail)
	{
		foreach (MailDetail mail in mailList)
		{
			if (mail.MailId == mailDetail.MailId)
			{
				mail.ReadState = mailDetail.ReadState;
				mail.AwardState = mailDetail.AwardState;
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Mail);
				break;
			}
		}
	}

	public void DeleteMailList(RepeatedField<long> IdList)
	{
		bool flag = false;
		foreach (long Id in IdList)
		{
			foreach (MailDetail mail in mailList)
			{
				if (mail.MailId == Id)
				{
					mailList.Remove(mail);
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Mail);
		}
	}

	public void UpdateMailRead(MailDetail mailDetail, MailContent mailContent)
	{
		foreach (MailDetail mail in mailList)
		{
			if (mail.MailId == mailDetail.MailId)
			{
				UpdateMailDetail(mailDetail);
				break;
			}
		}
		zbUIMgr.Instance.ShowMailDetail(mailDetail, mailContent);
	}

	public long[] CanDeleteIdList()
	{
		List<long> list = new List<long>();
		foreach (MailDetail mail in mailList)
		{
			if ((mail.ReadState == EBoolState.True && mail.AwardList.Count <= 0) || (mail.AwardList.Count > 0 && mail.AwardState == EBoolState.True))
			{
				list.Add(mail.MailId);
			}
		}
		return list.ToArray();
	}

	public int GetUnReadCount(int _currTab)
	{
		EMailLevel targetLevel = ((_currTab == 0) ? EMailLevel.Common : EMailLevel.Important);
		return ((IEnumerable<MailDetail>)mailList).Count((MailDetail mail) => mail != null && mail.MailLevel == targetLevel && mail.ReadState == EBoolState.False);
	}

	public bool HasUnRead()
	{
		foreach (MailDetail mail in mailList)
		{
			if (mail.ReadState == EBoolState.False)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAward()
	{
		foreach (MailDetail mail in mailList)
		{
			if (mail.AwardState == EBoolState.False && mail.AwardList.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasUnRead(int _currTab)
	{
		EMailLevel targetLevel = ((_currTab == 0) ? EMailLevel.Common : EMailLevel.Important);
		return ((IEnumerable<MailDetail>)mailList).Any((MailDetail mail) => mail != null && mail.MailLevel == targetLevel && mail.ReadState == EBoolState.False);
	}

	public bool HasAward(int _currTab)
	{
		EMailLevel targetLevel = ((_currTab == 0) ? EMailLevel.Common : EMailLevel.Important);
		return ((IEnumerable<MailDetail>)mailList).Any((MailDetail mail) => mail != null && mail.MailLevel == targetLevel && mail.AwardState == EBoolState.False && mail.AwardList.Count > 0);
	}

	public bool HasRedPoint()
	{
		if (HasUnRead())
		{
			return HasAward();
		}
		return false;
	}

	public List<MailDetail> GetSortedMailList(int _currTab)
	{
		List<MailDetail> list = _currTab switch
		{
			0 => new List<MailDetail>(((IEnumerable<MailDetail>)mailList).Where((MailDetail rec) => rec != null && rec.MailLevel != EMailLevel.Important).ToList()), 
			1 => new List<MailDetail>(((IEnumerable<MailDetail>)mailList).Where((MailDetail rec) => rec != null && rec.MailLevel == EMailLevel.Important).ToList()), 
			_ => new List<MailDetail>(), 
		};
		list.Sort(delegate(MailDetail v1, MailDetail v2)
		{
			bool flag = (v1.AwardList.Count > 0 && v1.AwardState == EBoolState.False) || (v1.AwardList.Count == 0 && v1.ReadState == EBoolState.False);
			bool flag2 = (v2.AwardList.Count > 0 && v2.AwardState == EBoolState.False) || (v2.AwardList.Count == 0 && v2.ReadState == EBoolState.False);
			if (flag && !flag2)
			{
				return -1;
			}
			if (!flag & flag2)
			{
				return 1;
			}
			return (v1.MailLevel == v2.MailLevel) ? v2.SendTime.CompareTo(v1.SendTime) : v1.MailLevel.CompareTo(v2.MailLevel);
		});
		return list;
	}
}
