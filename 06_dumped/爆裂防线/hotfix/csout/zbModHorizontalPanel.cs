using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModHorizontalPanel : VisualElement
{
	private const float PanelWidth = 704f;

	private const float PanelHeight = 180f;

	public VisualElement sp_bg;

	public VisualElement iconSkill;

	public VisualElement iconMod;

	public Label lbName;

	public Label lbDesc;

	public zbModHorizontalPanel(VisualElement root)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_modWheel.uxml").CloneTree((VisualElement)(object)this);
		((VisualElement)this).style.width = StyleLength.op_Implicit(704f);
		((VisualElement)this).style.height = StyleLength.op_Implicit(180f);
		sp_bg = UQueryExtensions.Q<VisualElement>(root, "sp_bg", (string)null);
		lbName = UQueryExtensions.Q<Label>(root, "lb_name", (string)null);
		iconSkill = UQueryExtensions.Q<VisualElement>(root, "sp_icon", (string)null);
		iconMod = UQueryExtensions.Q<VisualElement>(iconSkill, "sp_modicon", (string)null);
		lbDesc = UQueryExtensions.Q<Label>(root, "lb_desc", (string)null);
	}

	public void RefreshDetail(zbCombatTeam masterTeam, zbModCsvRecord modRecord)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)lbName).text = modRecord.name;
		((TextElement)lbDesc).text = modRecord.desc;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/wheel_skillitem" + modRecord.quality + ".png");
		sp_bg.style.backgroundImage = new StyleBackground(val);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(modRecord.icon);
		iconMod.style.backgroundImage = new StyleBackground(val2);
		BattleSkillDetail battleSkillDetail = ((IEnumerable<BattleSkillDetail>)masterTeam.battleTeamInfo.SkillList).First((BattleSkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
		zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(battleSkillDetail.SkillId);
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
		if ((Object)(object)val3 != (Object)null)
		{
			iconSkill.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
			iconSkill.MarkDirtyRepaint();
		}
	}
}
