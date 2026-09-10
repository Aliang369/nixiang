using ChillyRoom.Announcement.V1;
using MainMenu;
using TMPro;
using UnityEngine;

public class AnnouncementButtonCell : CellUI<AnnouncementOpenAPIModel>
{
	public GameButton button;

	public GameObject activeGo;

	public GameObject inActiveGo;

	public TextMeshProUGUI title;

	public TextMeshProUGUI title2;

	public GameObject newHintGo;

	private new AnnouncementPanel parent;

	public bool Viewed => Bootstrap.instance.globalSaveConfigData.viewedAnnouncementIds.Contains(base.setupData.Id.ToString());

	protected override void Init()
	{
		parent = GetUIInParents<AnnouncementPanel>();
		button.onClick.AddListener(_OnButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		activeGo.gameObject.SetActive(parent.currentAnnouncementDisplayInfo == this);
		inActiveGo.gameObject.SetActive(parent.currentAnnouncementDisplayInfo != this);
		title.text = base.setupData.Title;
		title2.text = base.setupData.Title;
		newHintGo.SetActive(!Viewed);
	}

	private void _OnButtonClicked()
	{
		parent.SelectAnnouncement(this);
	}
}
