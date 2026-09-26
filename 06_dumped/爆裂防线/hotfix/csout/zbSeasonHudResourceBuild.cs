using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonHudResourceBuild : zbSeasonHudBase
{
	private VisualElement m_icon;

	public zbSeasonHudResourceBuild(Transform orderTF, int buildId)
		: base(orderTF)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/resourceBuildHud.uxml");
		if ((Object)(object)val != (Object)null)
		{
			m_root = (VisualElement)(object)val.CloneTree();
		}
		m_buildId = buildId;
		m_icon = UQueryExtensions.Q<VisualElement>(m_root, "icon", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_root, "title", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI(m_root);
	}

	public override void Init()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		zbDataConfig.Instance.seasonAreaCsvTable.GetRecordByKey(m_seasonArea.AreaId);
		zbSeasonResourceCsvRecord recordByKey = zbDataConfig.Instance.seasonResourceCsvTable.GetRecordByKey(m_buildId);
		((TextElement)m_lbTitle).text = recordByKey.name;
		m_icon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/resource_" + recordByKey.nodeType + ".png"));
	}

	public override void Update()
	{
		base.Update();
		_ = active;
	}
}
