using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class RouletteGamePanel : BonusLocationPanel.GamePanel, IFrameUpdate
{
	[SerializeField]
	private BonusLocationBonusCell cellPrefab;

	[SerializeField]
	private Transform axis;

	[SerializeField]
	private Transform resultTargeter;

	[SerializeField]
	private List<Transform> pivots;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private float rotateSpeed;

	[SerializeField]
	private float stopTime;

	[SerializeField]
	private GameObject selectArea;

	[SerializeField]
	private GameButton switchPanelButton;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[NonSerialized]
	private Dictionary<BonusLocationBonusCell, Transform> cellPivotPairs;

	[NonSerialized]
	private CellUIPrefabPool<BonusLocationBonusCell, BonusLocationBonus> cellPool;

	[NonSerialized]
	private bool rolling;

	[NonSerialized]
	private bool rollingFinish;

	[NonSerialized]
	private bool toRoll;

	void IFrameUpdate.FrameUpdate()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (KeyValuePair<BonusLocationBonusCell, Transform> item in cellPivotPairs.OrEmptyIfNull())
		{
			item.Key.transform.position = item.Value.transform.position;
		}
	}

	protected override void Init()
	{
		base.Init();
		GetPrefabPool(ref cellPool, cellPrefab, selectArea.transform, UIHelper.GetDefaultCommonInfoPanel());
		(parent as BonusLocationPanel).ConfigInfoPanel(cellPool);
		cellPivotPairs = new Dictionary<BonusLocationBonusCell, Transform>();
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		switchPanelButton.onClick.AddListener(_OnSwitchPanelButtonClick);
		base.gameObject.SetActive(value: true);
	}

	public override void Setup(BonusLocationEvent bonusEvent)
	{
		base.Setup(bonusEvent);
		selectArea.SetActive(value: true);
		confirmButton.interactable = true;
		toRoll = false;
		rolling = false;
		rollingFinish = false;
		SpawnBonus();
		if (bonusEvent.gameSwitched || !UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableSwitchBonusLocationGame))
		{
			switchPanelButton.gameObject.SetActive(value: false);
		}
	}

	public override void Unset()
	{
		base.Unset();
		cellPool.DespawnAll();
		cellPivotPairs.Clear();
		base.gameObject.SetActive(value: false);
	}

	private void SpawnBonus()
	{
		List<BonusLocationBonus> list = bonusEvent.GenerateRandomBonus();
		for (int i = 0; i < pivots.Count; i++)
		{
			Transform transform = pivots[i];
			BonusLocationBonusCell bonusLocationBonusCell = cellPool.Spawn(list[i]);
			bonusLocationBonusCell.transform.position = transform.transform.position;
			cellPivotPairs.Add(bonusLocationBonusCell, transform.transform);
		}
	}

	private IEnumerator RollingCor()
	{
		buttonText.text = "";
		yield return new WaitForSeconds(0.5f);
		buttonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_stop;
		rolling = true;
		float currentRotateSpeed = rotateSpeed;
		float stopTimer = 0f;
		while (true)
		{
			if (rollingFinish)
			{
				stopTimer += Time.deltaTime;
				currentRotateSpeed = Mathf.Lerp(rotateSpeed, 0f, stopTimer / stopTime);
				if (stopTimer > stopTime)
				{
					break;
				}
			}
			yield return new WaitForEndOfFrame();
			axis.transform.RotateAround(axis.transform.position, Vector3.forward, currentRotateSpeed * Time.deltaTime);
		}
		yield return new WaitForSeconds(0.3f);
		Transform resultPivot = pivots.OrderByDescending((Transform p) => Vector3.Distance(p.position, resultTargeter.position)).Last();
		axis.DORotate(new Vector3(0f, 0f, GetPivotAngle(resultPivot)) + axis.rotation.eulerAngles, 0.3f);
		yield return new WaitForSeconds(0.3f);
		BonusLocationBonusCell cell = cellPivotPairs.First((KeyValuePair<BonusLocationBonusCell, Transform> t) => t.Value == resultPivot).Key;
		cell.NotifyGet();
		yield return new WaitForSeconds(1f);
		UIBase.gameState.UIManager.bonusLocationPanel.FinishBonusLocation(new List<BonusLocationBonus> { cell.bonus });
	}

	private float GetPivotAngle(Transform pivot)
	{
		return Vector3.SignedAngle(pivot.position - axis.position, resultTargeter.position - axis.position, Vector3.forward);
	}

	private void _OnConfirmButtonClicked()
	{
		if (rolling)
		{
			rollingFinish = true;
			confirmButton.interactable = false;
		}
		else if (!toRoll)
		{
			UIBase.gameState.StartCoroutine(RollingCor);
			toRoll = true;
		}
	}

	private void _OnSwitchPanelButtonClick()
	{
		bonusEvent.SwitchToRandomBonusPanel();
	}
}
