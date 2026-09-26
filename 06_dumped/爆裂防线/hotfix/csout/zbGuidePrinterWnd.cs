using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuidePrinterWnd : zbBaseWnd
{
	private int turnIndex;

	private int talkId;

	private VisualElement m_guidePrinter;

	private Label m_lbGuidePrinterDesc;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_guidePrinter = UQueryExtensions.Q<VisualElement>(m_RootElement, "guidePrinter", (string)null);
		m_lbGuidePrinterDesc = UQueryExtensions.Q<Label>(m_RootElement, "guidePrinterDesc", (string)null);
	}

	public void RefreshPrinter(int _talkId)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		DelDark();
		turnIndex = 0;
		talkId = _talkId;
		m_guidePrinter.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(RefreshPrinterAgain());
	}

	private IEnumerator RefreshPrinterAgain()
	{
		((TextElement)m_lbGuidePrinterDesc).text = "";
		string fullText = "";
		turnIndex++;
		zbGuideTalkCsvRecord[] records = zbDataConfig.Instance.guideTalkCsvTable.GetRecords().records;
		foreach (zbGuideTalkCsvRecord zbGuideTalkCsvRecord2 in records)
		{
			if (talkId == zbGuideTalkCsvRecord2.talkId && turnIndex == zbGuideTalkCsvRecord2.turn)
			{
				fullText = zbGuideTalkCsvRecord2.desc;
				break;
			}
		}
		for (int j = 0; j < fullText.Length; j++)
		{
			((TextElement)m_lbGuidePrinterDesc).text = fullText.Substring(0, j + 1);
			yield return (object)new WaitForSeconds(0.05f);
		}
		yield return (object)new WaitForSecondsRealtime(3f);
		Hide();
	}
}
