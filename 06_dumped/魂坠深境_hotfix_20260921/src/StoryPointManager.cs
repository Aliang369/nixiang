using System.Collections.Generic;

public class StoryPointManager : GlobalGameManager
{
	public class StoryPointState : IValidate
	{
		[SaveField(null, null)]
		public StoryPoint storyPoint;

		[SaveField(null, null)]
		public int observeCount;

		[SaveField(null, null)]
		public bool finished;

		[SaveField(null, null)]
		public int storyPoint_version;

		public StoryPointState(StoryPoint story)
		{
			storyPoint = story;
		}

		private StoryPointState()
		{
		}

		public bool IsValidate()
		{
			return storyPoint != null;
		}
	}

	public class FortressNPCState : IValidate
	{
		[SaveField(null, null)]
		public string fortressNPCId;

		[SaveField(null, null)]
		public List<StoryPoint> finishedProgressStoryPoints;

		public bool IsValidate()
		{
			if (fortressNPCId == null)
			{
				return false;
			}
			if (finishedProgressStoryPoints.IsNullOrEmpty())
			{
				return true;
			}
			foreach (StoryPoint finishedProgressStoryPoint in finishedProgressStoryPoints)
			{
				if (finishedProgressStoryPoint == null || !finishedProgressStoryPoint.IsValidAsset())
				{
					return false;
				}
			}
			return true;
		}
	}

	[SaveField(null, null)]
	public List<StoryPointState> storyPointStates;

	[SaveField(null, null)]
	public List<FortressNPCState> fortressNPCStates;

	public Queue<StoryPoint> storyPointStateQueue = new Queue<StoryPoint>();

	public StoryPoint RunningStoryPoint
	{
		get
		{
			if (storyPointStateQueue.Count <= 0)
			{
				return null;
			}
			return storyPointStateQueue.Peek();
		}
	}

	public PlayerCharacterState MasteryTalentGuideCharacter { get; private set; }

	public TalentDef MasteryTalentGuideTalent { get; private set; }

	private bool MasteryTalentGuideTriggered { get; set; }

	public bool TryCaptureMasteryTalentGuideContext(PlayerCharacterState character, TalentDef talent)
	{
		if (MasteryTalentGuideCharacter != null || MasteryTalentGuideTalent != null || character == null || talent == null)
		{
			return false;
		}
		MasteryTalentGuideCharacter = character;
		MasteryTalentGuideTalent = talent;
		MasteryTalentGuideTriggered = false;
		return true;
	}

	public bool TryMarkMasteryTalentGuideTriggered(PlayerCharacterState character)
	{
		if (MasteryTalentGuideTriggered || MasteryTalentGuideCharacter != character || MasteryTalentGuideTalent == null)
		{
			return false;
		}
		MasteryTalentGuideTriggered = true;
		return true;
	}

	public void ClearMasteryTalentGuideContext()
	{
		MasteryTalentGuideCharacter = null;
		MasteryTalentGuideTalent = null;
		MasteryTalentGuideTriggered = false;
	}

	public StoryPointState GetStoryState(StoryPoint storyPoint)
	{
		if (storyPoint == null)
		{
			return null;
		}
		StoryPointState storyPointState = storyPointStates.Find((StoryPointState x) => x.storyPoint == storyPoint);
		if (storyPointState == null)
		{
			storyPointState = new StoryPointState(storyPoint);
			storyPointStates.Add(storyPointState);
		}
		return storyPointState;
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		storyPointStateQueue = new Queue<StoryPoint>();
		ClearMasteryTalentGuideContext();
		applicationState.assetCatalog.storyPointCatalog.ValidAssets.BuildToList(ref storyPointStates, (StoryPoint sp, StoryPointState sps) => sps.storyPoint == sp, (StoryPoint sp) => new StoryPointState(sp));
		applicationState.assetCatalog.fortressNPCCatalog.ValidAssets.BuildToList(ref fortressNPCStates, (FortressNPC fs, FortressNPCState fss) => fss.fortressNPCId == fs.FullId, (FortressNPC fs) => new FortressNPCState
		{
			fortressNPCId = fs.FullId,
			finishedProgressStoryPoints = new List<StoryPoint>()
		});
		foreach (StoryPointState storyPointState in storyPointStates)
		{
			if (!(storyPointState?.storyPoint == null) && storyPointState.storyPoint_version != storyPointState.storyPoint.storyVersion_openTest)
			{
				storyPointState.finished = false;
				storyPointState.storyPoint_version = storyPointState.storyPoint.storyVersion_openTest;
			}
		}
		foreach (StoryPoint validAsset in applicationState.assetCatalog.storyPointCatalog.ValidAssets)
		{
			validAsset.Init(applicationState);
		}
		if (applicationState.bootStrapLoadData.skipStory)
		{
			TestUnlockAll();
		}
	}

	public void TestUnlockAll()
	{
		foreach (StoryPointState storyPointState in storyPointStates)
		{
			if (!(storyPointState?.storyPoint == null))
			{
				storyPointState.storyPoint.Execute(base.applicationState, null, skip: true);
			}
		}
	}
}
