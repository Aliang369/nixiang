using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class EndEventBehaviour : PlayableBehaviour
{
	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		Debug.LogError("OnBehaviourPlay");
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		Debug.LogError("OnBehaviourPause");
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		double time = playable.GetTime();
		double duration = playable.GetDuration();
		double num = info.effectiveSpeed;
		int num2 = (int)(time * num);
		int num3 = (int)(duration * num);
		Debug.Log($"当前时间: {time:F2}s, 总时长: {duration:F2}s");
		Debug.Log($"当前帧: {num2}, 总帧数: {num3}");
		Debug.Log($"帧率: {num:F2}fps");
	}

	public override void OnPlayableCreate(Playable playable)
	{
		Debug.LogError("OnPlayableCreate");
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		Debug.LogError("OnPlayableDestroy");
	}

	public override void OnGraphStop(Playable playable)
	{
		Debug.LogError("OnGraphStop");
	}
}
