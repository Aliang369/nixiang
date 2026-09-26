using UnityEngine;

public class zbBaseScene : MonoBehaviour
{
	protected virtual void OnEnable()
	{
		zbCameraHelper.Instance.Init();
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		zbIntervalTimer.Instance.Update();
		zbGameStateMachine.Instance.Update();
		zbMusicStateMachine.Instance.Update();
		zbUIMgr.Instance.Update();
	}

	protected virtual void LateUpdate()
	{
	}

	protected virtual void OnDestroy()
	{
	}
}
