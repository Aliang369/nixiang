using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFusionItem : zbViewBaseItem
{
	private zbFusion m_wndParent;

	private VisualElement m_root;

	private VisualElement m_color;

	private VisualElement m_weapon;

	public int curLevel;

	public int curExp;

	public int curColor;

	public int x;

	public int y;

	private Coroutine m_coroutine;

	public zbFusionItem(zbFusion parent, int x, int y)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		((VisualElement)this).style.width = StyleLength.op_Implicit(110f);
		((VisualElement)this).style.height = StyleLength.op_Implicit(116f);
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)this).style.left = StyleLength.op_Implicit(0f);
		((VisualElement)this).style.bottom = StyleLength.op_Implicit(0f);
		m_wndParent = parent;
		this.x = x;
		this.y = y;
		VisualElement val = (VisualElement)(object)zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_fusionitem.uxml").Instantiate();
		m_root = UQueryExtensions.Q<VisualElement>(val, "item", (string)null);
		m_color = UQueryExtensions.Q<VisualElement>(m_root, "color", (string)null);
		m_weapon = UQueryExtensions.Q<VisualElement>(m_root, "weapon", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_root, (Action)OnTouch, "", (TrickleDown)0);
		((VisualElement)this).Add(m_root);
	}

	public void Refresh(int curLevel, int curColor, int curExp)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		this.curColor = curColor;
		this.curLevel = curLevel;
		this.curExp = curExp;
		zbFusionLevelCsvRecord recordByKey = zbDataConfig.Instance.fusionLevelCsvTable.GetRecordByKey(curLevel);
		if (recordByKey != null)
		{
			string resName = $"ui/image/operate/fusion/color_{curColor}.jpg";
			m_color.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
			m_weapon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
	}

	public void CopyItem(zbFusionItem item)
	{
		Refresh(item.curLevel, item.curColor, item.curExp);
	}

	public void WeaponScale()
	{
		m_weapon.experimental.animation.Scale(1.4f, 200).OnCompleted((Action)delegate
		{
			m_weapon.experimental.animation.Scale(1f, 200);
		});
	}

	public void Reset()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		curExp = 0;
		curLevel = 0;
		curColor = 0;
		((VisualElement)this).style.bottom = StyleLength.op_Implicit((float)(y * 120 - 54 - 58 + 550));
	}

	public void EffectNode(int index)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		float num = m_wndParent.pointX[x];
		float num2 = m_wndParent.pointY[y];
		((VisualElement)this).style.left = StyleLength.op_Implicit(num);
		((VisualElement)this).style.bottom = StyleLength.op_Implicit(num2 + 620f);
		float startY = num2 + 620f;
		if (m_coroutine != null)
		{
			((MonoBehaviour)m_wndParent).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		m_coroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(MoveToTargetPos(num2, startY));
	}

	private IEnumerator MoveToTargetPos(float targetY, float startY)
	{
		float timeElapsed = 0f;
		float duration = 0.2f;
		while (timeElapsed < duration)
		{
			timeElapsed += Time.deltaTime;
			float num = Mathf.Clamp01(timeElapsed / duration);
			float num2 = Mathf.Lerp(startY, targetY, num);
			((VisualElement)this).style.bottom = StyleLength.op_Implicit(num2);
			yield return null;
		}
		((VisualElement)this).style.bottom = StyleLength.op_Implicit(targetY);
	}

	public void OnTouch()
	{
		m_wndParent.CaclNodeExp(x, y);
	}
}
