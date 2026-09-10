using DG.Tweening;
using UnityEngine;

public class FlickerSlotVFXAction : GameAction
{
	private readonly ItemDisplayBase display;

	private readonly bool halt;

	private Material mat;

	private float timer;

	public FlickerSlotVFXAction(Item item, bool halt = true)
	{
		if (item != null)
		{
			display = ItemDisplayBase.GetActiveItemSlotDisplay(item);
			this.halt = halt;
		}
	}

	protected override void Update()
	{
		if (display == null)
		{
			FinishAction();
			return;
		}
		if (mat == null)
		{
			FinishAction();
			return;
		}
		if (halt)
		{
			timer += Time.deltaTime;
			mat.SetFloat("_FlickerFactor", timer / ScriptableObjectSingleton<VFXDataPreset>.instance.itemSlotFlickerTime);
			if (timer > ScriptableObjectSingleton<VFXDataPreset>.instance.itemSlotFlickerTime)
			{
				mat.DisableKeyword("ENABLE_FLICKER");
				FinishAction();
			}
			return;
		}
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = base.actionTimeScale;
		sequence.Append(mat.DOFloat(1f, "_FlickerFactor", ScriptableObjectSingleton<VFXDataPreset>.instance.itemSlotFlickerTime));
		sequence.AppendCallback(delegate
		{
			mat.DisableKeyword("ENABLE_FLICKER");
		});
		sequence.Play();
		FinishAction();
	}
}
