using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class OutlineModule_Game : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	public Image outlineImage;

	[NonSerialized]
	private PlayerTableDisplayStateModule displayStateModule;

	[NonSerialized]
	private Sequence sequence;

	public override void Init()
	{
		outlineImage.material = UnityEngine.Object.Instantiate(ScriptableObjectSingleton<GameDataManager>.instance.itemOutlineMaterial);
	}

	public override void Setup()
	{
		displayStateModule = base.slotDisplay.GetModule<PlayerTableDisplayStateModule>();
		outlineImage.gameObject.SetActive(value: false);
	}

	public override void UpdateDisplay()
	{
		if (!(displayStateModule == null))
		{
			ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = UIDisplayModule.gameState.applicationState.vfxDataPreset.playerItemAnimConfig;
			if (ShouldShowOutline())
			{
				outlineImage.gameObject.SetActive(value: true);
				outlineImage.color = playerItemAnimConfig.triggerAbleColor;
			}
			else
			{
				outlineImage.gameObject.SetActive(value: false);
			}
		}
	}

	private bool ShouldShowOutline()
	{
		if (UIDisplayModule.gameState.battleState.turnPhase != TurnPhase.PlayerControlPhase || UIDisplayModule.gameState.UIManager.playerItemArea.currentState != PlayerItemArea.State.PlayerControl)
		{
			return false;
		}
		CastInfoCache castInfoCache = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.GetCastInfoCache(base.slotDisplay.item);
		if (!castInfoCache.coolDownCastAble)
		{
			return false;
		}
		if (!castInfoCache.canSpell)
		{
			return false;
		}
		return true;
	}
}
