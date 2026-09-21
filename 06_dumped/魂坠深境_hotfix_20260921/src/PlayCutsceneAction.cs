public class PlayCutsceneAction : ExecuteAction
{
	public CutsceneInfoData cutsceneInfo;

	private DungeonRoom cutsceneRoom;

	public bool stayCutscene;

	public PlayCutsceneAction(CutsceneInfoData cutsceneInfo, bool stayCutscene = false)
	{
		this.cutsceneInfo = cutsceneInfo;
		this.stayCutscene = stayCutscene;
	}

	public PlayCutsceneAction(CutsceneRef cutsceneRef, ApplicationStateInfo applicationState, bool stayCutscene = false)
	{
		cutsceneInfo = cutsceneRef.GetAssetPreData(applicationState.assetCatalog);
		this.stayCutscene = stayCutscene;
	}

	protected override void Update()
	{
		if (cutsceneInfo != null)
		{
			EnqueueAction(new EnumeratorAction(new PlayCutSceneState(cutsceneInfo, gameState, stayCutscene, skipFadeIn: false).PlayCutsceneCor));
		}
	}
}
