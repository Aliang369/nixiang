using System;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

[Serializable]
public class AnimModule : ItemDisplayBase.DisplayModule, IFrameUpdate
{
	private const int id_Wave = 1;

	private static int id_State = Animator.StringToHash("State");

	private static int id_WaveTime = Animator.StringToHash("WaveTime");

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private PlayerTableDisplayStateModule stateModule;

	[NonSerialized]
	private bool show;

	public override void Init()
	{
		stateModule = base.slotDisplay.GetModule<PlayerTableDisplayStateModule>();
	}

	public override void Setup()
	{
		show = false;
	}

	void IFrameUpdate.FrameUpdate()
	{
		if (show)
		{
			animator.SetFloat(id_WaveTime, Time.time / 4f + ScriptableObjectSingleton<VFXDataPreset>.instance.playerItemAnimConfig.waveGap * (float)(base.slotDisplay.item?.ownerSlotIndex).GetValueOrDefault() % 1f);
		}
	}

	public override void UpdateDisplay()
	{
		if (stateModule.currentState == PlayerTableDisplayStateModule.State.Idle)
		{
			show = true;
		}
		else
		{
			show = false;
		}
		UpdateAnim();
	}

	private void UpdateAnim()
	{
		animator.SetLayerWeight(1, (float)(show ? 1 : 0));
		animator.SetInteger(id_State, show ? 1 : 0);
	}
}
