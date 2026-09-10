using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Announcement.V1;
using ChillyRoom.Services.Core;
using Common.UI;
using Generated.ChillyRoomSdkClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu;

public class AnnouncementPanel : UIBase, IStackPanel
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private AnnouncementRichTextContentView richTextView;

	[SerializeField]
	private AnnouncementH5LinkContentView h5LinkView;

	[SerializeField]
	private AnnouncementH5PageContentView h5PageView;

	[SerializeField]
	private ScrollRect contentScrollRect;

	[SerializeField]
	private RectTransform announcementButtonParent;

	[SerializeField]
	private AnnouncementButtonCell selectAnnouncementButtonPrefab;

	[NonSerialized]
	public CellUIPrefabPool<AnnouncementButtonCell, AnnouncementOpenAPIModel> announcementButtonCellPrefabPool;

	[NonSerialized]
	public AnnouncementButtonCell currentAnnouncementDisplayInfo;

	[NonSerialized]
	private int requestVersion;

	public const int StopEnterAnnouncementKind = 0;

	public const int SocialPromotionAnnouncementKind = 20001;

	public bool announcementInited { get; private set; }

	protected override void Init()
	{
		backButton.onClick.AddListener(delegate
		{
			HideAllContentViews();
			SetActiveAndDirtyWithFadeAnim(active: false);
		});
		InitUI(richTextView);
		InitUI(h5LinkView);
		InitUI(h5PageView);
		HideAllContentViews();
		announcementButtonCellPrefabPool = GetPrefabPool<AnnouncementButtonCell, AnnouncementOpenAPIModel>(selectAnnouncementButtonPrefab, announcementButtonParent);
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Combine(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
		Request();
	}

	protected override void Dispose()
	{
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Remove(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
		HideAllContentViews();
	}

	private async void OnLanguageChanged(LocalizationManager.LanguageCode _)
	{
		try
		{
			await Request(currentAnnouncementDisplayInfo?.setupData?.Id.ToString());
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void ShowIfHasNewAnnouncement()
	{
		if (announcementButtonCellPrefabPool.GetActiveElements().Any((AnnouncementButtonCell d) => !d.Viewed))
		{
			OpenAnnouncementPanel();
		}
	}

	public void OpenAnnouncementPanel()
	{
		IReadOnlyList<AnnouncementButtonCell> activeElements = announcementButtonCellPrefabPool.GetActiveElements();
		AnnouncementButtonCell announcementButtonCell = activeElements.FirstOrDefault((AnnouncementButtonCell d) => !d.Viewed);
		if (announcementButtonCell != null)
		{
			SelectAnnouncement(announcementButtonCell);
		}
		if (currentAnnouncementDisplayInfo == null && activeElements.Count > 0)
		{
			SelectAnnouncement(activeElements.First());
		}
		SetActiveAndDirtyWithFadeAnim(active: true);
		ForceUpdate();
	}

	private Task Request()
	{
		return Request(null);
	}

	private async Task Request(string selectedAnnouncementId)
	{
		int currentRequestVersion = ++requestVersion;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			DateTimeOffset dateTimeOffset = await ChillyRoomService.GetCore().GetUtcDateTimeFromRemoteServer();
			IResult<List<AnnouncementOpenAPIModel>, Failure<FetchAnnouncementsErrorKind>> result = await ChillyRoomService.GetCore().FetchAnnouncements(dateTimeOffset.UtcDateTime);
			if (currentRequestVersion != requestVersion)
			{
				return;
			}
			announcementButtonCellPrefabPool.DespawnAll();
			currentAnnouncementDisplayInfo = null;
			if (!result.IsOk() || result.Data.Count < 1)
			{
				SetDirty();
			}
			else
			{
				foreach (AnnouncementOpenAPIModel item in result.Data.OrderBy((AnnouncementOpenAPIModel d) => d.AnnouncementKind))
				{
					if (item.AnnouncementKind != 20001)
					{
						announcementButtonCellPrefabPool.Spawn(item);
					}
				}
				if (!selectedAnnouncementId.IsNullOrEmpty())
				{
					AnnouncementButtonCell announcementButtonCell = announcementButtonCellPrefabPool.GetActiveElements().FirstOrDefault((AnnouncementButtonCell d) => d.setupData.Id.ToString() == selectedAnnouncementId);
					if (announcementButtonCell != null)
					{
						SelectAnnouncement(announcementButtonCell);
					}
				}
				if (base.gameObject.activeInHierarchy && currentAnnouncementDisplayInfo == null)
				{
					SelectAnnouncement(announcementButtonCellPrefabPool.GetActiveElements().First());
				}
			}
		}
		if (base.gameObject.activeInHierarchy)
		{
			SetDirty();
		}
		announcementInited = true;
	}

	public void SelectAnnouncement(AnnouncementButtonCell cell)
	{
		currentAnnouncementDisplayInfo = cell;
		if (!cell.Viewed)
		{
			SavingUtil.SaveGlobalSaveConfigData(Bootstrap.instance.globalSaveConfigData);
			Bootstrap.instance.globalSaveConfigData.viewedAnnouncementIds.AddUnique(cell.setupData.Id.ToString());
		}
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (currentAnnouncementDisplayInfo != null)
		{
			ShowAnnouncementContent(currentAnnouncementDisplayInfo.setupData);
			return;
		}
		title.text = "";
		HideAllContentViews();
	}

	private void ShowAnnouncementContent(AnnouncementOpenAPIModel data)
	{
		title.text = data.Title;
		HideAllContentViews();
		contentScrollRect.enabled = data.AnnouncementMultiLangKind != AnnouncementMultiLangKind.HtmlPage;
		switch (data.AnnouncementMultiLangKind)
		{
		case AnnouncementMultiLangKind.Text:
			richTextView.ShowContent(data);
			break;
		case AnnouncementMultiLangKind.Link:
			h5LinkView.ShowContent(data);
			break;
		case AnnouncementMultiLangKind.Html:
			richTextView.ShowContent(data);
			break;
		case AnnouncementMultiLangKind.HtmlPage:
			h5PageView.ShowContent(data);
			break;
		default:
			Debug.LogError("Unknown announcement type.");
			break;
		}
	}

	private void HideAllContentViews()
	{
		richTextView.HideContent();
		h5LinkView.HideContent();
		h5PageView.HideContent();
	}
}
