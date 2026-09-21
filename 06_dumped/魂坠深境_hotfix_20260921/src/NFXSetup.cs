using System;
using System.Linq;
using FMODUnity;
using Spine.Unity;
using UnityEngine;

public class NFXSetup : CTUnityPrefabAsset
{
	public enum BFTPivotType
	{
		Upper = 1,
		Lower
	}

	public EventReference sound;

	public VfxType vfxType;

	public float shootTimeMultiply = 1f;

	public BFTPivotType pivotType;

	public CameraControlData cameraControlData;

	public override string CTAssetIDPrepend => "NFX";

	private bool IsTransformVFX
	{
		get
		{
			if (vfxType != VfxType.TransformUI)
			{
				return vfxType == VfxType.Transform3D;
			}
			return true;
		}
	}

	public bool IsLoopNFX
	{
		get
		{
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			SkeletonAnimation[] componentsInChildren2 = GetComponentsInChildren<SkeletonAnimation>();
			SkeletonGraphic[] componentsInChildren3 = GetComponentsInChildren<SkeletonGraphic>();
			if (componentsInChildren.Any(delegate(ParticleSystem p)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				MainModule main = p.main;
				return ((MainModule)(ref main)).loop;
			}))
			{
				return true;
			}
			if (componentsInChildren2.Any((SkeletonAnimation s) => s.loop))
			{
				return true;
			}
			if (componentsInChildren3.Any((SkeletonGraphic s) => s.startingLoop))
			{
				return true;
			}
			return false;
		}
	}

	public void DisplayNFXForTest()
	{
		GameStateInfo gameState = SceneInfoSingleton.instance.gameState;
		ConvertablePivot convertablePivot = gameState.battleState.enemy.bft.Get3DNFXPivot(this);
		ConvertablePivot start = gameState.battleState.player.Get3DNFXPivot(this);
		gameState.battleState.enemy.bft.GetUIPivot(gameState);
		Item firstItem = gameState.battleState.player.tableContainer.firstItem;
		Item firstItem2 = gameState.battleState.enemy.bft.tableContainer.firstItem;
		switch (vfxType)
		{
		case VfxType.HitUI:
			gameState.actionManager.EnqueueAction(new DisplayNFXAction(this, firstItem));
			break;
		case VfxType.TransformUI:
			gameState.actionManager.EnqueueAction(new PlayTransformVFXAction(this, firstItem, firstItem2, is3d: false));
			break;
		case VfxType.Hit3D:
			gameState.actionManager.EnqueueAction(new DisplayNFXAction(this, convertablePivot));
			break;
		case VfxType.Transform3D:
			gameState.actionManager.EnqueueAction(new PlayTransformVFXAction(this, start, convertablePivot, is3d: true));
			break;
		case VfxType.Persistent3D:
			gameState.actionManager.EnqueueAction(new DisplayNFXAction(this, convertablePivot));
			break;
		case VfxType.PersistentUI:
			gameState.actionManager.EnqueueAction(new DisplayNFXAction(this, firstItem));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case VfxType.None:
			break;
		}
	}
}
