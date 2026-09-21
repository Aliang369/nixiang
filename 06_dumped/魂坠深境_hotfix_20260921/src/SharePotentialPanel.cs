using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class SharePotentialPanel : UIBase
{
	public GameButton closeButton;

	public SharePotentialTalentCell sharePotentialTalentCellPrefab;

	public RectTransform sharePotentialTalentCellParent;

	[NonSerialized]
	private CellUIPrefabPool<SharePotentialTalentCell, PlayerCharacterState> sharePotentialTalentCellPool;

	protected override void Init()
	{
		sharePotentialTalentCellPool = GetPrefabPool<SharePotentialTalentCell, PlayerCharacterState>(sharePotentialTalentCellPrefab, sharePotentialTalentCellParent);
		closeButton.onClick.AddListener(_OnCloseButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		sharePotentialTalentCellPool.DespawnAll();
		List<PlayerCharacterState> list = UIBase.applicationState.characterManager.characterStates.FindAll((PlayerCharacterState t) => t != UIBase.gameState.playerState.character && t.def.sharePotential != null);
		list.Sort(delegate(PlayerCharacterState a, PlayerCharacterState b)
		{
			if (a.locked && !b.locked)
			{
				return -1;
			}
			if (!a.locked && b.locked)
			{
				return 1;
			}
			if (a.EnableSharePotential && !b.EnableSharePotential)
			{
				return -1;
			}
			return (!a.EnableSharePotential && b.EnableSharePotential) ? 1 : a.def.characterName.CompareTo(b.def.characterName);
		});
		foreach (PlayerCharacterState item in list)
		{
			sharePotentialTalentCellPool.Spawn(item);
		}
	}

	private void _OnCloseButtonClicked()
	{
		base.gameObject.SetActive(value: false);
	}
}
