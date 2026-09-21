using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public class TestBattleLevelSelector : SerializedMonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public bool randomInitItem;

	public bool newGame;

	public bool newProfile;

	public BootStrapLoadData.TargetContext target;

	public bool withIntro;

	public bool completeIntroBeforeFirstGame;

	public void OnPointerClick(PointerEventData eventData)
	{
		BootStrapLoadData bootStrapLoadData = BootStrapLoadData.FromBattleParams(randomInitItem, newGame, newProfile, target, withIntro, completeIntroBeforeFirstGame);
		Bootstrap.instance.Load(bootStrapLoadData);
	}
}
