using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class DebugPanel : MonoBehaviour
{
	public abstract class Setter
	{
		public DebugPanel debugPanel;

		protected static ApplicationStateInfo applicationState => Bootstrap.applicationState;

		protected GameStateInfo gameState => applicationState?.runningGameState;

		protected FortressStateInfo fortressState => applicationState?.fortressState;

		public abstract string name { get; }

		protected void EnqueueAction(GameAction action)
		{
			gameState.actionManager.EnqueueAction(action);
		}

		public Setter(DebugPanel debugPanel)
		{
			this.debugPanel = debugPanel;
		}

		public void OnGroupSwitchButtonClicked()
		{
			debugPanel.ClearAllCells();
			OnSwitchGroup();
		}

		public abstract void OnSwitchGroup();
	}

	[SerializeField]
	public GameObject debugContentPanel;

	[SerializeField]
	public DebugGroupSwitchCell groupSwitchCellPrefab;

	[SerializeField]
	public DebugInvokeCell invokeCellPrefab;

	[SerializeField]
	public DebugToggleCell toggleCellPrefab;

	[SerializeField]
	public DebugSlideCell slideCellPrefab;

	[SerializeField]
	public DebugValueCell valueCellPrefab;

	[SerializeField]
	public DebugInputTextCell inputTextCellPrefab;

	[SerializeField]
	public DebugInvoke_InputValueCell invoke_InputValueCellPrefab;

	[SerializeField]
	public List<UniversalRendererData> rs;

	[SerializeField]
	public DebugTextInfo debugTextInfo;

	[SerializeField]
	public GameButton debugPanelTrigger;

	[SerializeField]
	public GameObject networkRequestingHint;

	[SerializeField]
	public TextMeshProUGUI fpsText;

	[SerializeField]
	public RectTransform panel1;

	[SerializeField]
	public RectTransform panel2;

	[SerializeField]
	public RectTransform panel3;

	[NonSerialized]
	public List<GameObject> currentCells;

	[NonSerialized]
	public List<Setter> setters;

	[NonSerialized]
	public Dictionary<int, List<MonoBehaviour>> panelPools;

	private float sampleDuration = 1f;

	private int frames;

	private float duration;

	private void Update()
	{
		if (Bootstrap.instance?.bootstrapExtra?.gameAsyncOperationManager != null)
		{
			networkRequestingHint.gameObject.SetActive(Bootstrap.instance.bootstrapExtra.gameAsyncOperationManager.IsCurrentRunning);
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		frames++;
		duration += unscaledDeltaTime;
		if (duration >= sampleDuration)
		{
			fpsText.SetText("{0:1}\n{1:1}", (float)frames / duration, 1000f * duration / (float)frames);
			frames = 0;
			duration = 0f;
		}
	}

	public void Init()
	{
	}

	public void AddInvokeCell(string name, UnityAction onClick, int panelIndex)
	{
		DebugInvokeCell debugInvokeCell = SpawnOnPanel(invokeCellPrefab, panelIndex);
		debugInvokeCell.text.text = name;
		debugInvokeCell.button.onClick.AddListener(onClick);
		debugInvokeCell.gameObject.SetActive(value: true);
		currentCells.Add(debugInvokeCell.gameObject);
	}

	public void AddToggleCell(string name, UnityAction onClick, Func<bool> getValue, int panelIndex)
	{
		DebugToggleCell cell = SpawnOnPanel(toggleCellPrefab, panelIndex);
		cell.button.onClick.AddListener(delegate
		{
			onClick();
			cell.text.text = name + ": " + (getValue() ? "开" : "关");
		});
		cell.text.text = name + ": " + (getValue() ? "开" : "关");
		cell.gameObject.SetActive(value: true);
		currentCells.Add(cell.gameObject);
	}

	public void AddSlideCell(string name, float initValue, UnityAction<float> onValueChange, int panelIndex, float min = 0f, float max = 1f)
	{
		DebugSlideCell cell = SpawnOnPanel(slideCellPrefab, panelIndex);
		cell.text.text = name;
		cell.slider.onValueChanged.AddListener(delegate(float f)
		{
			float num = Mathf.Lerp(min, max, f);
			cell.value.text = $"{num}";
			onValueChange?.Invoke(num);
		});
		cell.gameObject.SetActive(value: true);
		cell.min = min;
		cell.max = max;
		cell.slider.value = (initValue - min) / (max - min);
		cell.value.text = $"{initValue}";
		currentCells.Add(cell.gameObject);
	}

	public void AddValueCell(string name, UnityAction onMinus, UnityAction onPlus, Func<float> updateValue, int panelIndex)
	{
		DebugValueCell debugValueCell = SpawnOnPanel(valueCellPrefab, panelIndex);
		debugValueCell.text.text = name;
		debugValueCell.minusButton.onClick.AddListener(onMinus);
		debugValueCell.plusButton.onClick.AddListener(onPlus);
		debugValueCell.updateValue = updateValue;
		debugValueCell.gameObject.SetActive(value: true);
		currentCells.Add(debugValueCell.gameObject);
	}

	public void AddInputTextCell(string name, UnityAction<string> onTextChange, int panelIndex)
	{
		DebugInputTextCell debugInputTextCell = SpawnOnPanel(inputTextCellPrefab, panelIndex);
		debugInputTextCell.text.text = name;
		debugInputTextCell.inputField.onValueChanged.AddListener(onTextChange);
		debugInputTextCell.gameObject.SetActive(value: true);
		currentCells.Add(debugInputTextCell.gameObject);
	}

	public void AddDebugInvoke_InputValueCell(Func<string> name, UnityAction onClick, UnityAction<string> onTextChange, int panelIndex, Func<string> inputHint)
	{
		DebugInvoke_InputValueCell cell = SpawnOnPanel(invoke_InputValueCellPrefab, panelIndex);
		cell.text.text = name?.Invoke();
		cell.inputHint.text = inputHint?.Invoke();
		cell.inputField.onValueChanged.AddListener(delegate(string t)
		{
			onTextChange?.Invoke(t);
			cell.text.text = name?.Invoke();
			cell.inputHint.text = inputHint?.Invoke();
		});
		cell.button.onClick.AddListener(onClick);
		cell.gameObject.SetActive(value: true);
		currentCells.Add(cell.gameObject);
	}

	public void ClearAllCells()
	{
		foreach (GameObject item in currentCells.ToList())
		{
			UnityEngine.Object.Destroy(item);
			currentCells.Remove(item);
		}
	}

	public void ClearCells(IEnumerable<GameObject> cells)
	{
		foreach (GameObject item in cells.ToList())
		{
			UnityEngine.Object.Destroy(item);
			currentCells.Remove(item);
		}
	}

	public T SpawnOnPanel<T>(T prefab, int panelIndex) where T : MonoBehaviour
	{
		List<MonoBehaviour> list = panelPools[panelIndex];
		T val = UnityEngine.Object.Instantiate(prefab, panelIndex switch
		{
			1 => panel1, 
			2 => panel2, 
			_ => panel3, 
		});
		list.Add(val);
		return val;
	}

	public void DespawnAllInPanel(int panelIndex)
	{
		List<MonoBehaviour> list = panelPools[panelIndex];
		list.RemoveAll((MonoBehaviour m) => m == null);
		list.ForEach(delegate(MonoBehaviour m)
		{
			UnityEngine.Object.Destroy(m.gameObject);
		});
		list.Clear();
	}

	private void AddSetter(Setter setter)
	{
		DebugGroupSwitchCell debugGroupSwitchCell = SpawnOnPanel(groupSwitchCellPrefab, 1);
		debugGroupSwitchCell.text.text = setter.name;
		debugGroupSwitchCell.button.onClick.AddListener(setter.OnGroupSwitchButtonClicked);
		debugGroupSwitchCell.gameObject.SetActive(value: true);
	}

	private void _OnDebugPanelTriggerClicked()
	{
		debugContentPanel.SetActive(!debugContentPanel.activeSelf);
	}
}
