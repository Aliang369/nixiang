using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;

public class GachaAnimPlayer
{
	private EventInstance? playingGachaSound;

	public ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public FortressStateInfo fortressState => applicationState.fortressState;

	public IEnumerator DisplayGachaAnim(List<GlobalRewardInfo> rewards, Func<IEnumerator> onHideGame, Func<IEnumerator> onRecoverGame)
	{
		QualityLevel rank = GetRank(rewards);
		CutsceneInfoData assetPreData = ScriptableObjectSingleton<GameDataManager>.instance.gachaCutSceneRef.GetAssetPreData(applicationState.assetCatalog);
		if (assetPreData != null)
		{
			PlayCutSceneState playCutSceneState = new PlayCutSceneState(assetPreData, fortressState, stayCutscene: false, skipFadeIn: false);
			playCutSceneState.onHideGame = onHideGame;
			playCutSceneState.onRecoverGame = onRecoverGame;
			playCutSceneState.onConfigCutscene = delegate(Cutscene cutscene)
			{
				cutscene.GetComponent<CutsceneRankController>()?.SetRank(rank);
			};
			playCutSceneState.onTimelineStarted = PlaySound;
			playCutSceneState.audioLeadTime = 0.35f;
			yield return playCutSceneState.PlayCutsceneCor();
			AudioManager.Stop(playingGachaSound, (STOP_MODE)0);
		}
		void PlaySound()
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			if (rewards.Any((GlobalRewardInfo r) => r.GlobalItemQuality == QualityLevel.Epic))
			{
				playingGachaSound = Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gacha_draw_epic);
			}
			else if (rewards.Any((GlobalRewardInfo r) => r.GlobalItemQuality == QualityLevel.Rare))
			{
				playingGachaSound = Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gacha_draw_rare);
			}
			else
			{
				playingGachaSound = Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gacha_draw);
			}
		}
	}

	private QualityLevel GetRank(List<GlobalRewardInfo> rewards)
	{
		QualityLevel rank = QualityLevel.Common;
		rewards.ForEach(delegate(GlobalRewardInfo r)
		{
			rank = (QualityLevel)Math.Max((int)rank, (int)r.GlobalItemQuality);
		});
		return rank;
	}
}
