using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class ForerunnerWorldEvent : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choiceText;

	public ForerunnerLegacy currentLegacy;

	public StoryNoteFeedback feedback;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.Default;

	protected override void Init()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		currentLegacy = gameState.dungeonMapManager.currentLocation?.forerunnerLegacy;
		if (currentLegacy != null)
		{
			WorldEventChoice worldEventChoice = new WorldEventChoice();
			worldEventChoice.descriptionText = choiceText.choiceDescription;
			worldEventChoice.buttonText = choiceText.choiceName;
			worldEventChoice.executeAction = GatherLegacy;
			worldEventChoice.stayAfterChoose = true;
			currentDisplayInfo.AddChoice(worldEventChoice);
		}
		currentDisplayInfo.showLeaveButton = true;
		currentDisplayInfo.needRefresh = true;
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_enter_pioneer);
	}

	private void GatherLegacy()
	{
		StoryNoteInfo forerunnerLegacy = StoryNoteInfo.GetForerunnerLegacy(currentLegacy, delegate(StoryNoteFeedback feedback)
		{
			this.feedback = feedback;
			if (feedback.agree)
			{
				RefreshAfterChoose(choiceText.chosenDescription);
			}
			if (GlobalConfigManager.instance.enableNetwork && !currentLegacy.storyOnly && !string.IsNullOrEmpty(currentLegacy.guid))
			{
				FinishGetForerunnerLegacyRequest finishGetForerunnerLegacyRequest = new FinishGetForerunnerLegacyRequest();
				finishGetForerunnerLegacyRequest.ForerunnerLegacyGuid = currentLegacy.guid;
				finishGetForerunnerLegacyRequest.FeedbackType = (feedback.upvote.HasValue ? (feedback.upvote.Value ? GetForerunnerLegacyFeedbackType.Upvote : GetForerunnerLegacyFeedbackType.Downvote) : GetForerunnerLegacyFeedbackType.None);
				finishGetForerunnerLegacyRequest.CommentNote = "";
				new GeneralGameAsyncOperationRequestOnly<FinishGetForerunnerLegacyRequest>(finishGetForerunnerLegacyRequest, gameState.applicationState.ChillyRoomSDK.Forerunner.FinishGetForerunnerLegacyAsync).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_forerunner_data);
			}
			ThinkingAnalyticsExtends.TrackFinishForerunnerEvent(gameState, this);
		});
		EnqueueAction(new TriggerStoryNoteAction(forerunnerLegacy, halt: true));
	}
}
