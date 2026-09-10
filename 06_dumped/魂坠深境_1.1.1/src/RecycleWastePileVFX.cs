using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;

public class RecycleWastePileVFX
{
	public class Token
	{
		public bool finished;
	}

	public int currentCount;

	private readonly GameStateInfo gameState;

	public ParticleSystem particleSystem;

	private Queue<Token> tokens;

	public GameObject vfxObject;

	public RecycleWastePileVFX(GameStateInfo gameState)
	{
		this.gameState = gameState;
	}

	public void PrepareVFX()
	{
		tokens = new Queue<Token>();
		vfxObject = Object.Instantiate(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_recycleWastePile.gameObject, gameState.UIManager.uiVFXRoot);
		particleSystem = vfxObject.GetComponentInChildren<ParticleSystem>(includeInactive: true);
		UIParticleAttractor componentInChildren = vfxObject.GetComponentInChildren<UIParticleAttractor>(includeInactive: true);
		componentInChildren.onAttracted.AddListener(OnAttracted);
		gameState.UIManager.battlePanel.drawPileButton.recycleWastePileVFX = this;
		particleSystem.Stop();
		vfxObject.transform.position = gameState.pivotManager.GetPivot(PivotManager.PivotType.WastePileButton).ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
		((Component)(object)componentInChildren).transform.position = gameState.pivotManager.GetPivot(PivotManager.PivotType.DrawPileButton).ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
	}

	public void PlayTransferItemAnim()
	{
		gameState.StartCoroutine(TransferItemAnim);
	}

	private IEnumerator TransferItemAnim()
	{
		Token token = new Token
		{
			finished = false
		};
		tokens.Enqueue(token);
		currentCount++;
		particleSystem.Emit(1);
		yield return new WaitUntil(() => token.finished);
		currentCount--;
		gameState.UIManager.battlePanel.drawPileButton.PlayFlick();
	}

	private void OnAttracted()
	{
		if (tokens.Count != 0)
		{
			tokens.Dequeue().finished = true;
		}
	}

	public bool IsVFXEnd()
	{
		return particleSystem.particleCount == 0;
	}

	public void Finish()
	{
		gameState.UIManager.battlePanel.drawPileButton.recycleWastePileVFX = null;
		Object.Destroy(vfxObject);
	}
}
