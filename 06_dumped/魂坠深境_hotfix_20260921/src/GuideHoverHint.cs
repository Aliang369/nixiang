using Common.UI;
using UnityEngine;

public class GuideHoverHint : UIBase, IFrameUpdate
{
	private RectTransform trans;

	private bool follow;

	private ConvertablePivot convertablePivot;

	private GuideUIPanel guideUIPanel;

	protected override void Init()
	{
		guideUIPanel = GetUIInParents<GuideUIPanel>();
	}

	public void ShowOnTarget(RectTransform trans, bool follow)
	{
		this.trans = trans;
		this.follow = follow;
		base.gameObject.SetActive(value: true);
		convertablePivot = new ConvertablePivot(trans);
		base.transform.position = convertablePivot.ToCanvasWorldPosition(guideUIPanel.guidePanelCanvas);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void FrameUpdate()
	{
		if (follow)
		{
			base.transform.position = convertablePivot.ToCanvasWorldPosition(guideUIPanel.guidePanelCanvas);
		}
	}
}
