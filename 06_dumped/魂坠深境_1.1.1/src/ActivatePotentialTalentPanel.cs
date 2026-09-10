using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;

public class ActivatePotentialTalentPanel : UIBase
{
	public GameButton closeButton;

	public PotentialTalentCell potentialTalentCellPrefab;

	private ActivatePotentialTalentInfo info;

	[NonSerialized]
	private CellUIPrefabPool<PotentialTalentCell, TalentDef> potentialTalentCellPool;

	[NonSerialized]
	private TalentDef activatingTalent;

	public bool IsAiDecisionActive
	{
		get
		{
			if (base.gameObject.activeInHierarchy && info != null)
			{
				return activatingTalent == null;
			}
			return false;
		}
	}

	public void Setup(ActivatePotentialTalentInfo t)
	{
		info = t;
		activatingTalent = null;
	}

	public void Unset()
	{
		info = null;
	}

	protected override void Init()
	{
		potentialTalentCellPool = GetPrefabPool<PotentialTalentCell, TalentDef>(potentialTalentCellPrefab, potentialTalentCellPrefab.transform.parent);
		closeButton.onClick.AddListener(_OnCloseButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		if (info == null)
		{
			return;
		}
		potentialTalentCellPool.DespawnAll();
		foreach (TalentDef item in UIBase.gameState.playerState.CurrentActivateAblePotentialTalent)
		{
			potentialTalentCellPool.Spawn(item);
		}
	}

	public void ConfirmActivateTalent(TalentDef talent)
	{
		if (!(activatingTalent != null) && talent.node is ActivatePotential activatePotential)
		{
			activatePotential.Trigger(talent, UIBase.gameState, delegate
			{
				info.onConfirm?.Invoke(talent);
				SetActiveAndDirty(active: false);
				UIHelper.HideAllInfo(base.transform);
				activatingTalent = null;
			});
			ThinkingAnalyticsExtends.TrackActivatePotential(UIBase.gameState, talent);
		}
	}

	public List<TalentDef> GetAiActivateAbleTalents()
	{
		if (!IsAiDecisionActive)
		{
			return new List<TalentDef>();
		}
		return UIBase.gameState.playerState.CurrentActivateAblePotentialTalent.Where((TalentDef talent) => talent != null && talent.node is ActivatePotential activatePotential && activatePotential.TriggerAble(talent, UIBase.gameState)).ToList();
	}

	public void CancelForAiSession()
	{
		if (IsAiDecisionActive)
		{
			_OnCloseButtonClicked();
		}
	}

	private void _OnCloseButtonClicked()
	{
		info.onCancel?.Invoke();
		SetActiveAndDirty(active: false);
		activatingTalent = null;
		UIHelper.HideAllInfo(base.transform);
	}
}
