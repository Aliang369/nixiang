using UnityEngine.UIElements;

public class zbSeasonTargetItem : zbViewBaseItem
{
	private Label m_lbTitle;

	private Label m_lbStatus;

	private VisualElement m_commonTask;

	private Label m_lbCommonTaskTitle;

	private Label m_lbCommonTaskContent;

	private VisualElement m_commonTaskProgress;

	private VisualElement m_lbCommonTaskProgressCount;

	private VisualElement m_completedForces;

	private VisualElement[] m_forces = (VisualElement[])(object)new VisualElement[4];

	private zbItem[] m_commTaskAwards = new zbItem[2];

	private zbItem[] TaskAwards = new zbItem[2];

	private VisualElement m_playerTask;

	private Label m_lbPlayerTaskTitle;

	private Label m_lbPlayerTaskContent;

	private VisualElement m_playerTaskProgress;

	private VisualElement m_lbPlayerTaskProgressCount;

	private zbItem[] m_playerTaskAwards = new zbItem[2];

	private Label m_lbUnlockVal;

	private zbSeasonTargetWnd m_wndParent;

	public zbSeasonTargetItem(zbSeasonTargetWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonTarget.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_wndParent = wndParent;
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		m_lbStatus = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_status", (string)null);
		m_commonTask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "commonTask", (string)null);
		m_lbCommonTaskTitle = UQueryExtensions.Q<Label>(m_commonTask, "lb_title", (string)null);
		m_lbCommonTaskContent = UQueryExtensions.Q<Label>(m_commonTask, "lb_content", (string)null);
		m_commonTaskProgress = UQueryExtensions.Q<VisualElement>(m_commonTask, "progress", (string)null);
		m_lbCommonTaskProgressCount = (VisualElement)(object)UQueryExtensions.Q<Label>(m_commonTask, "lb_count", (string)null);
		m_completedForces = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "completedForces", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_forces[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, $"force{i}", (string)null);
		}
		m_playerTask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerTask", (string)null);
		m_lbPlayerTaskTitle = UQueryExtensions.Q<Label>(m_playerTask, "lb_title", (string)null);
		m_lbPlayerTaskContent = UQueryExtensions.Q<Label>(m_playerTask, "lb_content", (string)null);
		m_playerTaskProgress = UQueryExtensions.Q<VisualElement>(m_playerTask, "progress", (string)null);
		m_lbPlayerTaskProgressCount = (VisualElement)(object)UQueryExtensions.Q<Label>(m_playerTask, "lb_count", (string)null);
		m_lbUnlockVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_unlockVal", (string)null);
		for (int j = 0; j < 2; j++)
		{
			m_commTaskAwards[j] = UQueryExtensions.Q<zbItem>(m_commonTask, "award" + j, (string)null);
			m_commTaskAwards[j].OnEnable(m_wndParent);
			m_playerTaskAwards[j] = UQueryExtensions.Q<zbItem>(m_playerTask, "award" + j, (string)null);
			m_playerTaskAwards[j].OnEnable(m_wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh()
	{
	}
}
