using System.Linq;
using UnityEngine;

public class BlockyDelicacyPersistentFlyVfx : MonoBehaviour
{
	private BattleFieldTransform ownerBft;

	private NFXPlayState playState;

	public static void Attach(NFXPlayState state, BattleFieldTransform owner)
	{
		if (!(state?.nfxInstance == null) && owner != null)
		{
			BlockyDelicacyPersistentFlyVfx blockyDelicacyPersistentFlyVfx = state.nfxInstance.GetComponent<BlockyDelicacyPersistentFlyVfx>();
			if (blockyDelicacyPersistentFlyVfx == null)
			{
				blockyDelicacyPersistentFlyVfx = state.nfxInstance.gameObject.AddComponent<BlockyDelicacyPersistentFlyVfx>();
			}
			blockyDelicacyPersistentFlyVfx.Bind(owner, state);
		}
	}

	public static void ClearFor(ApplicationStateInfo applicationState, BattleFieldTransform owner)
	{
		if (applicationState?.nfxManager?.playingNfxs == null || owner == null)
		{
			return;
		}
		foreach (NFXPlayState item in applicationState.nfxManager.playingNfxs.ToList())
		{
			BlockyDelicacyPersistentFlyVfx blockyDelicacyPersistentFlyVfx = ((item?.nfxInstance != null) ? item.nfxInstance.GetComponent<BlockyDelicacyPersistentFlyVfx>() : null);
			if (blockyDelicacyPersistentFlyVfx != null && blockyDelicacyPersistentFlyVfx.Matches(owner))
			{
				blockyDelicacyPersistentFlyVfx.Stop();
			}
		}
	}

	private void Bind(BattleFieldTransform owner, NFXPlayState state)
	{
		ownerBft = owner;
		playState = state;
	}

	private bool Matches(BattleFieldTransform owner)
	{
		if (owner != null)
		{
			return ownerBft == owner;
		}
		return false;
	}

	private void Stop()
	{
		if (playState != null)
		{
			playState.Stop();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
