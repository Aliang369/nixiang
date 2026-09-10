using UnityEngine;

public class GameLoadDataManager : ScriptableObjectSingleton<GameLoadDataManager>
{
	[SerializeField]
	public TestMenuConfigData testMenuConfigData = new TestMenuConfigData();
}
