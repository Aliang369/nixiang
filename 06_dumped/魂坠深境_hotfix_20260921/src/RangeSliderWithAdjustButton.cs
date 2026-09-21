using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class RangeSliderWithAdjustButton : MonoBehaviour
{
	[SerializeField]
	private Button addButton;

	[SerializeField]
	private Button subButton;

	[SerializeField]
	private EventTrigger addButtonTrigger;

	[SerializeField]
	private EventTrigger subButtonTrigger;

	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private TextMeshProUGUI minText;

	[SerializeField]
	private TextMeshProUGUI maxText;

	[SerializeField]
	private Slider slider;

	[NonSerialized]
	public UnityEvent<int> onValueChanged = new UnityEvent<int>();

	private const float startPressAddTime = 0.5f;

	private bool isPressing;

	private int pressDelta;

	private float nextPressTime;

	private float pressAddInitInterval => 40f / (float)(MaxValue - MinValue);

	private float PressAddFinalInterval => pressAddInitInterval / (nextPressTime / 0.5f);

	public int MaxValue
	{
		get
		{
			return (int)slider.maxValue;
		}
		set
		{
			slider.maxValue = value;
			UpdateDisplay();
		}
	}

	public int MinValue
	{
		get
		{
			return (int)slider.minValue;
		}
		set
		{
			slider.minValue = value;
			UpdateDisplay();
		}
	}

	public int CurrentValue
	{
		get
		{
			return (int)slider.value;
		}
		set
		{
			int num = Math.Clamp(value, MinValue, MaxValue);
			countText.text = num.ToString();
			slider.value = num;
			UpdateDisplay();
			onValueChanged?.Invoke(num);
		}
	}

	private void Awake()
	{
		addButton.onClick.AddListener(Add);
		subButton.onClick.AddListener(Sub);
		slider.onValueChanged.AddListener(OnSliderValueChanged);
		RegisterPressEvents(addButtonTrigger, 1);
		RegisterPressEvents(subButtonTrigger, -1);
	}

	private void Update()
	{
		if (!isPressing || Time.time < nextPressTime)
		{
			return;
		}
		while (isPressing && Time.time >= nextPressTime)
		{
			if (pressDelta > 0)
			{
				Add();
			}
			else
			{
				Sub();
			}
			nextPressTime += PressAddFinalInterval;
		}
	}

	private void OnDisable()
	{
		StopPressRepeat();
	}

	private void Add()
	{
		CurrentValue++;
	}

	private void Sub()
	{
		CurrentValue--;
	}

	private void OnSliderValueChanged(float value)
	{
		CurrentValue = (int)value;
	}

	private void UpdateDisplay()
	{
		if (minText != null)
		{
			minText.text = MinValue.ToString();
		}
		if (maxText != null)
		{
			maxText.text = MaxValue.ToString();
		}
	}

	private void RegisterPressEvents(EventTrigger trigger, int delta)
	{
		AddEventTrigger(trigger, EventTriggerType.PointerDown, delegate
		{
			StartPressRepeat(delta);
		});
		AddEventTrigger(trigger, EventTriggerType.PointerUp, delegate
		{
			StopPressRepeat();
		});
		AddEventTrigger(trigger, EventTriggerType.PointerExit, delegate
		{
			StopPressRepeat();
		});
	}

	private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityAction<BaseEventData> callback)
	{
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = eventType
		};
		entry.callback.AddListener(callback);
		trigger.triggers.Add(entry);
	}

	private void StartPressRepeat(int delta)
	{
		StopPressRepeat();
		isPressing = true;
		pressDelta = delta;
		nextPressTime = Time.time + 0.5f;
	}

	private void StopPressRepeat()
	{
		isPressing = false;
		pressDelta = 0;
		nextPressTime = 0f;
	}
}
