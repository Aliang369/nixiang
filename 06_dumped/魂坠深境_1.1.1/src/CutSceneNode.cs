using System.Collections;

public abstract class CutSceneNode
{
	public abstract IEnumerator Play(PlayCutSceneState state);
}
