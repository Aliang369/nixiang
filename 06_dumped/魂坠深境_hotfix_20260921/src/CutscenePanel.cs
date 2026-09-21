using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutscenePanel : UIBase
{
	public Image cutsceneDarkenImage;

	public Image cutsceneAnimDarkenImage;

	public SkeletonGraphic cutsceneOverlaySpineGraphic;

	public TextMeshProUGUI subtitleText;

	public List<GameObject> cutsceneBlackSlide;

	public GameButton skipButton;

	protected override void Init()
	{
		skipButton.onClick.AddListener(delegate
		{
			UIBase.applicationState.currentCutSceneState?.Skip();
			skipButton.gameObject.SetActive(value: false);
		});
	}

	public void Clear()
	{
		cutsceneDarkenImage.DOKill();
		cutsceneDarkenImage.SetA(0f);
		cutsceneDarkenImage.gameObject.SetActive(value: false);
		cutsceneAnimDarkenImage.SetA(0f);
		cutsceneAnimDarkenImage.gameObject.SetActive(value: false);
		cutsceneBlackSlide.ForEach(delegate(GameObject go)
		{
			go.SetActive(value: false);
		});
		skipButton.gameObject.SetActive(value: false);
		subtitleText.gameObject.SetActive(value: false);
	}
}
