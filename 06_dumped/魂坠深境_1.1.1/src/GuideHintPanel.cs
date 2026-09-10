using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GuideHintPanel : UIBase
{
	[SerializeField]
	private GameButton backgroundButton;

	[SerializeField]
	private GameButton beforeButton;

	[SerializeField]
	private GameButton nextButton;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private TextEmergeEffect dialogTextEmerge;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private RawImage videoPlayerImage;

	[SerializeField]
	public VideoClip presetVideo;

	[NonSerialized]
	private GuideUIPanel guideParentPanel;

	[NonSerialized]
	private FortressGuidePanel fortressGuideParentPanel;

	[NonSerialized]
	private GuideHintPanelSet guideHintPanelSet;

	[NonSerialized]
	private int index;

	private VideoPlayer videoPlayer => guideParentPanel?.videoPlayer ?? fortressGuideParentPanel.videoPlayer;

	protected override void Init()
	{
		beforeButton.gameObject.SetActive(value: false);
		nextButton.gameObject.SetActive(value: false);
		confirmButton.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: false);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		backgroundButton.onClick.AddListener(_OnBackgroundButtonClicked);
		guideParentPanel = GetUIInParents<GuideUIPanel>();
		fortressGuideParentPanel = GetUIInParents<FortressGuidePanel>();
		guideHintPanelSet = GetUIInParents<GuideHintPanelSet>();
		beforeButton.onClick.AddListener(delegate
		{
			Unset();
			guideHintPanelSet.ShowPrevious();
		});
		nextButton.onClick.AddListener(delegate
		{
			Unset();
			guideHintPanelSet.ShowNext();
		});
	}

	protected override void Dispose()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		videoPlayer.loopPointReached -= new EventHandler(OnVideoLoopPointReached);
		if (videoPlayer.targetTexture != null)
		{
			videoPlayer.targetTexture.Release();
		}
	}

	public void Setup(int index)
	{
		beforeButton.gameObject.SetActive(value: false);
		nextButton.gameObject.SetActive(value: false);
		confirmButton.gameObject.SetActive(value: false);
		this.index = index;
		if (index != 0)
		{
			beforeButton.gameObject.SetActive(value: true);
		}
		if (index != guideHintPanelSet.guideHintPanels.Count - 1)
		{
			nextButton.gameObject.SetActive(value: true);
		}
		if (index == guideHintPanelSet.guideHintPanels.Count - 1)
		{
			confirmButton.gameObject.SetActive(value: true);
		}
	}

	public void Unset()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		videoPlayer.loopPointReached -= new EventHandler(OnVideoLoopPointReached);
		base.gameObject.SetActive(value: false);
	}

	public void PrepareVideo(VideoClip clip)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		if (!((UnityEngine.Object)(object)clip == null))
		{
			videoPlayer.Stop();
			if (videoPlayer.targetTexture != null)
			{
				videoPlayer.targetTexture.Release();
				UnityEngine.Object.Destroy(videoPlayer.targetTexture);
			}
			RenderTexture renderTexture = new RenderTexture((int)clip.width, (int)clip.height, 24, RenderTextureFormat.Default);
			renderTexture.Create();
			videoPlayer.targetTexture = renderTexture;
			videoPlayer.timeUpdateMode = (VideoTimeUpdateMode)2;
			videoPlayer.audioOutputMode = (VideoAudioOutputMode)0;
			videoPlayer.isLooping = true;
			videoPlayer.playOnAwake = false;
			videoPlayer.clip = clip;
			videoPlayerImage.texture = videoPlayer.targetTexture;
			videoPlayer.prepareCompleted -= new EventHandler(OnVideoPrepared);
			videoPlayer.prepareCompleted += new EventHandler(OnVideoPrepared);
			videoPlayer.loopPointReached -= new EventHandler(OnVideoLoopPointReached);
			videoPlayer.loopPointReached += new EventHandler(OnVideoLoopPointReached);
			videoPlayer.Prepare();
		}
	}

	private void OnVideoPrepared(VideoPlayer source)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		source.prepareCompleted -= new EventHandler(OnVideoPrepared);
		source.Play();
	}

	private void OnVideoLoopPointReached(VideoPlayer source)
	{
		if (!((UnityEngine.Object)(object)source.clip == null))
		{
			source.isLooping = true;
			source.Stop();
			source.time = 0.0;
			source.Play();
		}
	}

	public bool IsVideoPlayerPrepared()
	{
		if (videoPlayer.isPrepared && videoPlayer.isPlaying)
		{
			return videoPlayer.time > 0.0;
		}
		return false;
	}

	public void PlayVideo(VideoClip clip)
	{
		if ((UnityEngine.Object)(object)videoPlayer.clip != (UnityEngine.Object)(object)clip)
		{
			PrepareVideo(clip);
			return;
		}
		videoPlayer.timeUpdateMode = (VideoTimeUpdateMode)2;
		videoPlayer.audioOutputMode = (VideoAudioOutputMode)0;
		videoPlayer.isLooping = true;
		if (!videoPlayer.isPlaying)
		{
			videoPlayer.Play();
		}
	}

	public void ShowHint()
	{
		base.gameObject.SetActive(value: true);
		SetConfirmButton(show: false);
		if (dialogTextEmerge.gameObject.activeSelf)
		{
			dialogTextEmerge.StartEmergeText(dialogTextEmerge.text.text, _OnEmergeFinish);
		}
		else
		{
			SetConfirmButton(show: true);
		}
		if ((UnityEngine.Object)(object)presetVideo == null)
		{
			videoPlayerImage.gameObject.SetActive(value: false);
			return;
		}
		videoPlayerImage.gameObject.SetActive(value: true);
		PlayVideo(presetVideo);
	}

	private void SetConfirmButton(bool show)
	{
		if (!(guideHintPanelSet != null))
		{
			confirmButton.gameObject.SetActive(show);
		}
	}

	private void _OnEmergeFinish()
	{
		SetConfirmButton(show: true);
	}

	private void _OnConfirmButtonClicked()
	{
		base.gameObject.SetActive(value: false);
		if (guideHintPanelSet != null)
		{
			guideHintPanelSet.Confirm();
		}
		Unset();
	}

	private void _OnBackgroundButtonClicked()
	{
		dialogTextEmerge.Finish();
	}
}
