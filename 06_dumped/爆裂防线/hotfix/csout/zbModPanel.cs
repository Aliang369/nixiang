using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModPanel : VisualElement
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

	public zbModPanel(VisualElement root)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
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
	}

	public void RefreshDetail(zbCombatTeam masterTeam, zbModCsvRecord modRecord)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)lbName).text = modRecord.name;
		((TextElement)lbDesc).text = modRecord.desc;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/select_modbg" + modRecord.quality + ".png");
		sp_bg.style.backgroundImage = new StyleBackground(val);
		BattleSkillDetail battleSkillDetail = ((IEnumerable<BattleSkillDetail>)masterTeam.battleTeamInfo.SkillList).First((BattleSkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(modRecord.icon);
		iconMod.style.backgroundImage = new StyleBackground(val2);
		iconNew.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((modRecord.modIndex != EModIndexDefine.Index1000) ? 1 : 0));
		zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(battleSkillDetail.SkillId);
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
		if ((Object)(object)val3 != (Object)null)
		{
			iconSkill.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
			iconSkill.MarkDirtyRepaint();
		}
	}
}
