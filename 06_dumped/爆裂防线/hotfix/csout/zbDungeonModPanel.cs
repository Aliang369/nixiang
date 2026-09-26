using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonModPanel : VisualElement
{
	private const float PanelWidth = 224f;

	private const float PanelHeight = 460f;

	private VisualElement sp_bg;

	public VisualElement iconSkill;

	public VisualElement iconMod;

	public VisualElement iconNew;

	public Label lbName;

	public Label lbDesc;

	public Button btnSelect;

	public zbDungeonModPanel(VisualElement root)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_selectMod.uxml").CloneTree((VisualElement)(object)this);
		((VisualElement)this).style.width = StyleLength.op_Implicit(224f);
		((VisualElement)this).style.height = StyleLength.op_Implicit(460f);
		sp_bg = UQueryExtensions.Q<VisualElement>(root, "sp_bg", (string)null);
		lbName = UQueryExtensions.Q<Label>(root, "lb_name", (string)null);
		iconSkill = UQueryExtensions.Q<VisualElement>(root, "sp_icon", (string)null);
		iconMod = UQueryExtensions.Q<VisualElement>(root, "sp_modicon", (string)null);
		lbDesc = UQueryExtensions.Q<Label>(root, "lb_desc", (string)null);
		iconNew = UQueryExtensions.Q<VisualElement>(root, "sp_new", (string)null);
		btnSelect = UQueryExtensions.Q<Button>(root, "btn_select", (string)null);
		iconNew.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	public void RefreshDetail(zbModCsvRecord modRecord)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/select_modbg" + modRecord.quality + ".png");
		sp_bg.style.backgroundImage = new StyleBackground(val);
		DungeonBaseInfo dungeonBaseInfo = zbDungeonMgr.Instance.dungeonHandler.dungeonBaseInfo;
		if (dungeonBaseInfo != null)
		{
			((TextElement)lbName).text = modRecord.name;
			((TextElement)lbDesc).text = modRecord.desc;
			SkillDetail skillDetail = ((IEnumerable<SkillDetail>)dungeonBaseInfo.SkillList).First((SkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
			zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(modRecord.skillType[0]);
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(modRecord.icon);
			iconMod.style.backgroundImage = new StyleBackground(val2);
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(skillDetail.SkillId);
			Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
			iconSkill.style.backgroundImage = new StyleBackground(val3);
		}
	}
}
