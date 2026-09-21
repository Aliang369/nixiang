using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class UpgradeRoomEvent : WorldEvent
{
	public class UpgradeRoomConfig
	{
		public DialogueSequence enterDialogue;

		public DialogueSequence exitDialogue;

		public int stageIncreasePrice = 1;

		public int upgradeBasePrice;

		public int upgradeUpPrice;
	}

	public const string npcIdleAniName = "#idle#";

	public const string npcEnterStoreAniName = "#入场#";

	public const string npcExitStoreAniName = "#exit#";

	public SkeletonDataAsset npcSkeletonDataAsset;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.UpgradeRoom;

	private SkeletonAnimation NpcSpineAnimationInstance => gameState.currentEncounter.room.GetComponentsInChildren<SkeletonAnimation>().FirstOrDefault((SkeletonAnimation s) => (Object)(object)((SkeletonRenderer)s).skeletonDataAsset == (Object)(object)npcSkeletonDataAsset);

	public UpgradeRoomConfig Config => ScriptableObjectSingleton<GameDataManager>.instance.upgradeConfig;

	public DialoguePlayer dialoguePlayer => gameState.currentEncounter.room.GetComponentInChildren<DialoguePlayer>();

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_UpgradeRoom;
	}

	protected override void Init()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		currentDisplayInfo = new WorldEventDisplayInfo();
		PlayRoomNpcShootAnimation("#入场#");
		Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_blacksmith);
		EnqueueAction(new TriggerDialogueAction(Config.enterDialogue, dialoguePlayer));
	}

	public override void FinishedWorldEvent()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		PlayRoomNpcShootAnimation("#exit#", resetToIdle: false);
		if (gameState.applicationState.settingManager.uiTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X1)
		{
			EnqueueAction(new TriggerDialogueAction(Config.exitDialogue, dialoguePlayer, halt: true));
		}
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_blacksmith);
		base.FinishedWorldEvent();
	}

	public void PlayRoomNpcShootAnimation(string aniName, bool resetToIdle = true)
	{
		SkeletonAnimation ani = NpcSpineAnimationInstance;
		if ((Object)(object)ani == null)
		{
			return;
		}
		TrackEntry trackEntry = ani.PlayAnimation(aniName, loop: false);
		if (resetToIdle)
		{
			trackEntry.Complete += delegate
			{
				ani.PlayAnimation("#idle#", loop: true);
			};
		}
	}
}
