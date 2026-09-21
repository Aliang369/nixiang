using ChillyRoom.Announcement.V1;

namespace MainMenu;

public abstract class AnnouncementContentView : UIBase
{
	public abstract void ShowContent(AnnouncementOpenAPIModel data);

	public virtual void HideContent()
	{
		SetActiveAndDirty(active: false);
	}
}
