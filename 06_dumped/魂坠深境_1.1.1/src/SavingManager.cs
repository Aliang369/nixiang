using System.Threading.Tasks;
using Common.UI;

public class SavingManager : GlobalGameManager, IFrameUpdate
{
	private bool toSaveProfile;

	public void SaveProfile()
	{
		toSaveProfile = false;
		Bootstrap.instance.SaveToCurrentProfileData(base.applicationState);
	}

	public Task SaveProfileAsync()
	{
		toSaveProfile = false;
		return Bootstrap.instance.SaveToCurrentProfileDataAsync(base.applicationState);
	}

	public void FrameUpdate()
	{
		if (toSaveProfile && base.applicationState.runningGameState == null)
		{
			SaveProfile();
		}
	}

	public void SetProfileDirtyToSave()
	{
		if (base.applicationState.runningGameState == null)
		{
			toSaveProfile = true;
		}
	}
}
