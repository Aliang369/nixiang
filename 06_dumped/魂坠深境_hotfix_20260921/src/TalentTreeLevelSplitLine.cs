using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TalentTreeLevelSplitLine : CellUI<TalentTree.LevelLockConfig>
{
	public TextMeshProUGUI text;

	public CanvasGroup canvasGroup;

	public TalentTree.LevelLockConfig config;

	private TalentTree talentTreeParent;

	protected override void Init()
	{
		talentTreeParent = GetUIInParents<TalentTree>();
	}

	protected override void OnSetup(TalentTree.LevelLockConfig config)
	{
		this.config = config;
		Vector2 anchoredPosition = base.rectTransform.anchoredPosition;
		anchoredPosition.y = (parent as TalentTree).GetPosition(new Vector2(0f, (float)config.position + 0.5f)).y;
		base.rectTransform.anchoredPosition = anchoredPosition;
		text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_tree_level_arg0.FormatWith(config.level);
	}

	public IEnumerator Hide()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 1f));
		sequence.Play();
		yield return sequence.WaitForKill();
	}
}
