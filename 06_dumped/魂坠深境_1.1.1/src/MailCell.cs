using System;
using TMPro;
using UnityEngine;

public class MailCell : CellUI<MailData>
{
	public TextMeshProUGUI mailTitle;

	public TextMeshProUGUI sender;

	public TextMeshProUGUI sendTime;

	public GameButton selectButton;

	public GameObject gotMask;

	public GameObject rewardHint;

	public GameObject selectedMark;

	public GameObject checkedMark;

	private MailData data;

	private FortressMailPanel fortressMailPanel;

	protected override void Init()
	{
		fortressMailPanel = GetUIInParents<FortressMailPanel>();
		selectButton.onClick.AddListener(delegate
		{
			fortressMailPanel.SelectMail(data);
			SetDirty();
		});
		gotMask.SetActive(value: false);
		rewardHint.SetActive(value: false);
		selectedMark.SetActive(value: false);
	}

	protected override void OnSetup(MailData setupData)
	{
		data = setupData;
	}

	protected override void UpdateDisplay()
	{
		mailTitle.text = data.title;
		sender.text = data.sender;
		if (!data.ExpiredAt.HasValue)
		{
			sendTime.text = "";
		}
		else
		{
			int? num = (data.ExpiredAt - DateTimeOffset.Now)?.Days;
			sendTime.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_remain_arg0_days_expire.FormatWith(num);
		}
		checkedMark.SetActive(data.isRead);
		if (data.isRead && data.attachRewards == null)
		{
			gotMask.SetActive(value: true);
		}
		else
		{
			gotMask.SetActive(data.isAccepted);
		}
		rewardHint.SetActive(data.WithAttachmentToAccpet);
		selectedMark.SetActive(fortressMailPanel.selectingMailId == data.id);
	}
}
