using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonHudForceBuild : zbSeasonHudBase
{
	public zbSeasonHudForceBuild(Transform orderTF, int buildId)
		: base(orderTF)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/forceBuildHud.uxml");
		if ((Object)(object)val != (Object)null)
		{
			m_root = (VisualElement)(object)val.CloneTree();
		}
		m_buildId = buildId;
		m_iconFlag = UQueryExtensions.Q<VisualElement>(m_root, "icon_flag", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_root, "lb_lv", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_root, "title", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI(m_root);
	}

	public override void Init()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		m_iconFlag.style.backgroundImage = zbSelf.Instance.seasonHandler.GetForceSmallFlagByForceId(m_seasonArea.ForceId);
		zbSeasonAreaCsvRecord recordByKey = zbDataConfig.Instance.seasonAreaCsvTable.GetRecordByKey(m_seasonArea.AreaId);
		zbSeasonCityCsvRecord recordByKey2 = zbDataConfig.Instance.seasonCityCsvTable.GetRecordByKey(recordByKey.cityId);
		((TextElement)m_lbLv).text = recordByKey2.level.ToString();
		((TextElement)m_lbTitle).text = recordByKey2.name;
	}

	public override void Update()
	{
		base.Update();
		_ = active;
	}
}
