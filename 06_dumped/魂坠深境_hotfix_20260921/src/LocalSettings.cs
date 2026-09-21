using Sirenix.OdinInspector;

public class LocalSettings : SerializedScriptableObject
{
	public static bool autoUpdate => false;

	public static bool loadFromFirstScene
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public static bool hideInspectorDetail => false;
}
