using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InfluenceStateModule_InfoPanel : InfluenceDisplay.DisplayModule
{
	public IEnumerator TriggerAbilityAnim(Ability.AbilityAnimPriority priority)
	{
		GameStateInfo gameState = UIDisplayModule.applicationState.runningGameState;
		if (priority >= Ability.AbilityAnimPriority.Weak)
		{
			gameState.StartCoroutine(() => PlayIconEffect(1));
		}
		if (priority >= Ability.AbilityAnimPriority.Medium)
		{
			gameState.UIManager.influenceInfoPanel.ShowCommonInfo(new CommonInfoData(base.influence, CommonInfoData.ControlType.FixDisplay, null, false));
			yield return new WaitForSeconds(0.4f);
			gameState.UIManager.influenceInfoPanel.HideAllCommonInfo();
		}
	}

	private IEnumerator PlayIconEffect(int times)
	{
		Image image = base.display.image;
		List<Image> images = new List<Image>();
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < times; i++)
		{
			Image image2 = Object.Instantiate(image, base.transform);
			image2.gameObject.SetActive(value: true);
			images.Add(image2);
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image2, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image2.rectTransform.DOScale(new Vector3(2f, 2f, 1f), 1f));
		}
		sequence.Play();
		yield return sequence.WaitForKill();
		for (int j = 0; j < times; j++)
		{
			if (images[j]?.gameObject != null)
			{
				Object.Destroy(images[j].gameObject);
			}
		}
	}
}
