using Common.UI;
using TMPro;
using UnityEngine;

public abstract class TextLinkObjectDisplayer
{
	public class ClickInfo
	{
		public string[] strings;

		public TextMeshProUGUI text;

		public bool click;

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is ClickInfo clickInfo)
			{
				if (clickInfo.click != click)
				{
					return false;
				}
				if (clickInfo.text != text)
				{
					return false;
				}
				if (strings == null || clickInfo.strings == null || strings.Length != clickInfo.strings.Length)
				{
					return false;
				}
				for (int i = 0; i < strings.Length; i++)
				{
					if (strings[i] != clickInfo.strings[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}

	protected TextMeshProUGUI tmpText;

	protected TextDataGenerator generator;

	protected ClickInfo lastClickInfo;

	public abstract void ShowInfo(object refLinkObject, CommonInfoData.ControlType controlType);

	public abstract void HideInfo(CommonInfoData.ControlType controlType);

	public void SetGenerator(TextDataGenerator generator)
	{
		this.generator = generator;
	}

	protected void TryDisplayRefLinkObject(int linkIndex)
	{
		if (generator == null || linkIndex < 0 || linkIndex >= tmpText.textInfo.linkInfo.Length)
		{
			return;
		}
		TMP_LinkInfo tMP_LinkInfo = tmpText.textInfo.linkInfo[linkIndex];
		string[] array = tMP_LinkInfo.GetLinkID().Split('_');
		if (array.Length != 2 || array[0] != "gen")
		{
			return;
		}
		ClickInfo clickInfo = new ClickInfo
		{
			text = tmpText,
			strings = array,
			click = true
		};
		CommonInfoData.ControlType controlType = ((!clickInfo.click) ? CommonInfoData.ControlType.Hover : CommonInfoData.ControlType.Click);
		if (clickInfo.Equals(lastClickInfo))
		{
			lastClickInfo = null;
			HideInfo(controlType);
			return;
		}
		lastClickInfo = clickInfo;
		int num = int.Parse(array[1]);
		if (num < 0 || num >= generator.refLinkObjects.Count)
		{
			TraceLog.Error($"文本引用对象索引越界: linkIndex={linkIndex}, refIndex={num}, refCount={generator.refLinkObjects.Count}, text={tmpText.text}");
		}
		else
		{
			ShowInfo(generator.refLinkObjects[num], controlType);
		}
	}

	protected void _OnTextClicked(int linkIndex)
	{
		if (linkIndex < tmpText.textInfo.linkInfo.Length)
		{
			TryDisplayRefLinkObject(linkIndex);
		}
	}

	protected void _OnTextEnter(int linkIndex)
	{
		if ((lastClickInfo == null || !lastClickInfo.click) && linkIndex < tmpText.textInfo.linkInfo.Length)
		{
			TryDisplayRefLinkObject(linkIndex);
		}
	}

	protected void _OnTextExit()
	{
		if (lastClickInfo != null && !lastClickInfo.click)
		{
			lastClickInfo = null;
			if (!PlatformHelper.IsMobile())
			{
				HideInfo(CommonInfoData.ControlType.Hover);
			}
		}
	}

	protected void OnHideAll(Transform transform)
	{
		lastClickInfo = null;
	}
}
