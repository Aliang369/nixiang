using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public class AiGameSessionController : IGameAutomationHandler
{
	private class AiLegalAction
	{
		private readonly string id;

		private readonly string type;

		private readonly string label;

		private readonly JObject payload;

		private readonly JObject payloadSchema;

		private readonly Action<JObject> apply;

		public AiLegalAction(string id, string type, string label, JObject payload, JObject payloadSchema, Action<JObject> apply)
		{
			this.id = id;
			this.type = type;
			this.label = label;
			this.payload = payload;
			this.payloadSchema = payloadSchema;
			this.apply = apply;
		}

		public void Apply(JObject request)
		{
			apply?.Invoke(request);
		}

		public JObject ToJson()
		{
			return new JObject
			{
				["actionId"] = id,
				["type"] = type,
				["label"] = label,
				["payload"] = payload,
				["payloadSchema"] = payloadSchema
			};
		}
	}

	private class AiPendingDecision
	{
		private sealed class RewardChoiceGroupPlan
		{
			private readonly Action<string> apply;

			private readonly Action skip;

			private readonly HashSet<string> validOptions;

			public string GroupId { get; }

			public bool Required { get; }

			public RewardChoiceGroupPlan(string groupId, bool required, IEnumerable<string> validOptions, Action<string> apply, Action skip)
			{
				GroupId = groupId;
				Required = required;
				this.validOptions = new HashSet<string>(validOptions ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
				this.apply = apply;
				this.skip = skip;
			}

			public void Validate(string optionId)
			{
				if (!validOptions.Contains(optionId))
				{
					throw new InvalidOperationException("Invalid option `" + optionId + "` for reward choice group `" + GroupId + "`.");
				}
			}

			public void Apply(string optionId)
			{
				apply(optionId);
			}

			public void Skip()
			{
				skip();
			}
		}

		private SelectItemInfo selectItemInfo;

		private Action<AiGameSessionController, JObject, AiPendingDecision> buildElementSelectActions;

		private Func<bool> canCancelElementSelection;

		private Action cancelElementSelection;

		public string Phase { get; private set; }

		public BattleBonus Bonus { get; private set; }

		public bool EndEncounterAfterBonus { get; private set; }

		public bool Completed { get; set; }

		public string Signature => $"{Phase}:{Completed}:{(Bonus?.bonuses?.Count).GetValueOrDefault()}";

		private AiPendingDecision()
		{
		}

		public static AiPendingDecision SelectItems(SelectItemInfo info)
		{
			return new AiPendingDecision
			{
				Phase = "select",
				selectItemInfo = info
			};
		}

		public static AiPendingDecision SelectElements<T>(ElementSelectPanel<T>.SelectInfo info) where T : class
		{
			return new AiPendingDecision
			{
				Phase = "select",
				buildElementSelectActions = delegate(AiGameSessionController controller, JObject data, AiPendingDecision decision)
				{
					BuildElementSelectActionsGeneric(controller, data, decision, info);
				},
				canCancelElementSelection = info.CanCancel,
				cancelElementSelection = delegate
				{
					info.onCancel?.Invoke();
				}
			};
		}

		public static AiPendingDecision CreateBonus(BattleBonus bonus, bool endEncounterAfterBonus, bool isBattle)
		{
			return new AiPendingDecision
			{
				Phase = "reward",
				Bonus = bonus,
				EndEncounterAfterBonus = endEncounterAfterBonus
			};
		}

		public void BuildLegalActions(AiGameSessionController controller, JObject data)
		{
			if (selectItemInfo != null)
			{
				BuildSelectItemActions(controller, data);
			}
			else if (buildElementSelectActions != null)
			{
				buildElementSelectActions(controller, data, this);
			}
			else if (Bonus != null)
			{
				BuildBonusActions(controller, data);
			}
		}

		public bool TryAbort(out string code, out string message)
		{
			code = null;
			message = null;
			if (Completed)
			{
				return true;
			}
			if (selectItemInfo != null)
			{
				if (!selectItemInfo.CanCancel())
				{
					code = "PendingSelectionCannotCancel";
					message = "Pending item selection cannot be canceled.";
					return false;
				}
				selectItemInfo.onCancel?.Invoke();
				Completed = true;
				return true;
			}
			if (cancelElementSelection != null)
			{
				if (canCancelElementSelection != null && !canCancelElementSelection())
				{
					code = "PendingSelectionCannotCancel";
					message = "Pending element selection cannot be canceled.";
					return false;
				}
				cancelElementSelection();
				Completed = true;
				return true;
			}
			if (Bonus != null)
			{
				code = "PendingRewardCannotAbort";
				message = "Pending reward decisions must be resolved before the AI session can be closed or replaced.";
				return false;
			}
			code = "PendingDecisionCannotAbort";
			message = "Pending decision cannot be safely aborted.";
			return false;
		}

		private void BuildSelectItemActions(AiGameSessionController controller, JObject data)
		{
			data["kind"] = "item";
			data["hint"] = selectItemInfo.panelHintGetter?.Invoke();
			data["selectedCount"] = selectItemInfo.output?.Count ?? 0;
			List<Item> pool = selectItemInfo.pool ?? new List<Item>();
			for (int i = 0; i < pool.Count; i++)
			{
				Item item = pool[i];
				if (item == null)
				{
					continue;
				}
				List<Item> output = selectItemInfo.output;
				if (output != null && output.Contains(item))
				{
					continue;
				}
				Func<Item, bool> canSelect = selectItemInfo.canSelect;
				if (canSelect != null && !canSelect(item))
				{
					continue;
				}
				int index = i;
				controller.AddAction($"select.item.{i}", "selectItem", item.ItemNameWithUpgrade, new JObject
				{
					["itemIndex"] = i,
					["item"] = BuildItem(item, i)
				}, delegate
				{
					if (selectItemInfo.singleSelect)
					{
						selectItemInfo.output.Clear();
					}
					selectItemInfo.output.Add(pool[index]);
				});
			}
			if (selectItemInfo.CanConfirm())
			{
				controller.AddAction("select.confirm", "confirmSelect", "Confirm selection", null, delegate
				{
					selectItemInfo.onConfirm?.Invoke();
					Completed = true;
				});
			}
			if (selectItemInfo.CanCancel())
			{
				controller.AddAction("select.cancel", "cancelSelect", "Cancel selection", null, delegate
				{
					selectItemInfo.onCancel?.Invoke();
					Completed = true;
				});
			}
		}

		private static void BuildElementSelectActionsGeneric<T>(AiGameSessionController controller, JObject data, AiPendingDecision decision, ElementSelectPanel<T>.SelectInfo info) where T : class
		{
			data["kind"] = "element";
			data["hint"] = info.panelHintGetter?.Invoke();
			List<T> pool = info.pool ?? new List<T>();
			for (int i = 0; i < pool.Count; i++)
			{
				T val = pool[i];
				if (val == null || info.output.Contains(val) || !info.CanSelect(val))
				{
					continue;
				}
				int index = i;
				controller.AddAction($"select.element.{i}", "selectElement", LabelForObject(val), new JObject
				{
					["elementIndex"] = i,
					["label"] = LabelForObject(val)
				}, delegate
				{
					if (info.singleSelect && info.output.Count > 0)
					{
						info.output.Clear();
					}
					info.output.Add(pool[index]);
				});
			}
			if (info.CanConfirm())
			{
				controller.AddAction("select.confirm", "confirmSelect", "Confirm selection", null, delegate
				{
					info.onConfirm?.Invoke();
					decision.Completed = true;
				});
			}
			if (info.CanCancel())
			{
				controller.AddAction("select.cancel", "cancelSelect", "Cancel selection", null, delegate
				{
					info.onCancel?.Invoke();
					decision.Completed = true;
				});
			}
		}

		private void BuildBonusActions(AiGameSessionController controller, JObject data)
		{
			data["title"] = Bonus.title;
			List<BattleBonusObject> list = Bonus.bonuses ?? new List<BattleBonusObject>();
			data["automaticRewards"] = BuildAutomaticRewards(list);
			data["choiceGroups"] = BuildChoiceGroups(list);
			controller.AddAction("reward.resolve", "resolveReward", "Resolve rewards", new JObject
			{
				["automaticRewardCount"] = data["automaticRewards"]?.Count() ?? 0,
				["choiceGroupCount"] = data["choiceGroups"]?.Count() ?? 0
			}, new JObject { ["choices"] = "object mapping choiceGroups[].groupId to choiceGroups[].options[].optionId or null" }, delegate(JObject request)
			{
				ResolveReward(controller, request);
			});
			for (int num = 0; num < list.Count; num++)
			{
				BattleBonusObject bonusObject = list[num];
				if (bonusObject == null || bonusObject.view || !bonusObject.isChoose)
				{
					continue;
				}
				SelectGetItemBonus itemBonus = bonusObject as SelectGetItemBonus;
				if (itemBonus != null)
				{
					List<Item> items = itemBonus.allItems ?? new List<Item>();
					for (int num2 = 0; num2 < items.Count; num2++)
					{
						Item item = items[num2];
						if (item == null)
						{
							continue;
						}
						int itemIndex = num2;
						controller.AddAction($"bonus.{num}.item.{num2}", "chooseBonusItem", item.ItemNameWithUpgrade, new JObject
						{
							["bonusIndex"] = num,
							["itemIndex"] = num2,
							["item"] = BuildItem(item, num2)
						}, delegate
						{
							controller.ApplyAutomaticBonuses(this);
							itemBonus.view = true;
							itemBonus.selectItems = new List<Item> { items[itemIndex] };
							itemBonus.TriggerBonus(controller.applicationState.runningGameState, delegate
							{
								controller.OnBonusGot(this);
							});
						});
					}
					continue;
				}
				if (bonusObject is AddInfluenceBonus bonus)
				{
					BuildInfluenceBonusActions(controller, bonus, num, "chooseBonusInfluence");
					continue;
				}
				if (bonusObject is AddBossInfluenceBonus bonus2)
				{
					BuildBossInfluenceBonusActions(controller, bonus2, num);
					continue;
				}
				controller.AddAction($"bonus.{num}.trigger", "triggerBonus", bonusObject.previewText, new JObject { ["bonusIndex"] = num }, delegate
				{
					controller.ApplyAutomaticBonuses(this);
					bonusObject.view = true;
					bonusObject.TriggerBonus(controller.applicationState.runningGameState, delegate
					{
						controller.OnBonusGot(this);
					});
				});
			}
		}

		private static JArray BuildAutomaticRewards(List<BattleBonusObject> bonuses)
		{
			JArray jArray = new JArray();
			for (int i = 0; i < bonuses.Count; i++)
			{
				BattleBonusObject battleBonusObject = bonuses[i];
				if (battleBonusObject != null && !battleBonusObject.isChoose)
				{
					jArray.Add(new JObject
					{
						["effectId"] = $"bonus.{i}",
						["kind"] = BonusKind(battleBonusObject),
						["label"] = battleBonusObject.previewText,
						["data"] = BuildBonusSummary(battleBonusObject, i)
					});
				}
			}
			return jArray;
		}

		private static JArray BuildChoiceGroups(List<BattleBonusObject> bonuses)
		{
			JArray jArray = new JArray();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < bonuses.Count; i++)
			{
				BattleBonusObject battleBonusObject = bonuses[i];
				if (battleBonusObject == null || battleBonusObject.view || !battleBonusObject.isChoose)
				{
					continue;
				}
				if (battleBonusObject is SelectGetItemBonus selectGetItemBonus)
				{
					string text = ((num == 0) ? "cardReward" : $"cardReward.{num}");
					num++;
					jArray.Add(new JObject
					{
						["groupId"] = text,
						["kind"] = "item",
						["label"] = battleBonusObject.previewText,
						["required"] = selectGetItemBonus.minSelectCount > 0,
						["minSelect"] = selectGetItemBonus.minSelectCount,
						["maxSelect"] = SelectGetItemBonus.maxSelectCount,
						["data"] = BuildBonusSummary(battleBonusObject, i),
						["options"] = new JArray(from entry in (selectGetItemBonus.allItems ?? new List<Item>()).Select((Item item, int index) => new { item, index })
							where entry.item != null
							select new JObject
							{
								["optionId"] = $"card.{entry.index}",
								["kind"] = "item",
								["label"] = entry.item.ItemNameWithUpgrade,
								["data"] = BuildItem(entry.item, entry.index)
							})
					});
				}
				else if (battleBonusObject is AddInfluenceBonus addInfluenceBonus)
				{
					string groupId = ((num2 == 0) ? "influenceReward" : $"influenceReward.{num2}");
					num2++;
					jArray.Add(BuildInfluenceChoiceGroup(groupId, "influence", battleBonusObject, addInfluenceBonus.selectableInfluence, i));
				}
				else if (battleBonusObject is AddBossInfluenceBonus addBossInfluenceBonus)
				{
					string groupId2 = ((num3 == 0) ? "bossInfluenceReward" : $"bossInfluenceReward.{num3}");
					num3++;
					jArray.Add(BuildInfluenceChoiceGroup(groupId2, "bossInfluence", battleBonusObject, addBossInfluenceBonus.selectableInfluence, i));
				}
				else
				{
					string text2 = ((num4 == 0) ? "choiceReward" : $"choiceReward.{num4}");
					num4++;
					jArray.Add(new JObject
					{
						["groupId"] = text2,
						["kind"] = BonusKind(battleBonusObject),
						["label"] = battleBonusObject.previewText,
						["required"] = false,
						["minSelect"] = 0,
						["maxSelect"] = 1,
						["data"] = BuildBonusSummary(battleBonusObject, i),
						["options"] = new JArray
						{
							new JObject
							{
								["optionId"] = "trigger",
								["kind"] = BonusKind(battleBonusObject),
								["label"] = battleBonusObject.previewText,
								["data"] = BuildBonusSummary(battleBonusObject, i)
							}
						}
					});
				}
			}
			return jArray;
		}

		private static JObject BuildInfluenceChoiceGroup(string groupId, string kind, BattleBonusObject bonus, List<Influence> influences, int bonusIndex)
		{
			return new JObject
			{
				["groupId"] = groupId,
				["kind"] = kind,
				["label"] = bonus.previewText,
				["required"] = false,
				["minSelect"] = 0,
				["maxSelect"] = 1,
				["data"] = BuildBonusSummary(bonus, bonusIndex),
				["options"] = new JArray(from entry in (influences ?? new List<Influence>()).Select((Influence influence, int index) => new { influence, index })
					where entry.influence != null
					select new JObject
					{
						["optionId"] = $"{kind}.{entry.index}",
						["kind"] = kind,
						["label"] = LabelForObject(entry.influence),
						["data"] = new JObject
						{
							["bonusIndex"] = bonusIndex,
							["influenceIndex"] = entry.index,
							["label"] = LabelForObject(entry.influence)
						}
					})
			};
		}

		private static JObject BuildBonusSummary(BattleBonusObject bonus, int index)
		{
			JObject jObject = new JObject
			{
				["bonusIndex"] = index,
				["type"] = bonus.GetType().Name,
				["previewText"] = bonus.previewText,
				["view"] = bonus.view,
				["triggered"] = bonus.triggered,
				["isChoose"] = bonus.isChoose
			};
			if (!(bonus is CoinBonus coinBonus))
			{
				if (!(bonus is CrystalBonus crystalBonus))
				{
					if (!(bonus is GemBonus gemBonus))
					{
						if (bonus is SelectGetItemBonus selectGetItemBonus)
						{
							jObject["minSelect"] = selectGetItemBonus.minSelectCount;
							jObject["maxSelect"] = SelectGetItemBonus.maxSelectCount;
						}
					}
					else
					{
						jObject["amount"] = gemBonus.totalCount;
						jObject["gemConfigs"] = JObject.FromObject(gemBonus.gemConfigs ?? new Dictionary<int, int>());
					}
				}
				else
				{
					jObject["amount"] = crystalBonus.count;
				}
			}
			else
			{
				jObject["amount"] = coinBonus.count;
			}
			return jObject;
		}

		private static string BonusKind(BattleBonusObject bonus)
		{
			if (!(bonus is CoinBonus))
			{
				if (!(bonus is CrystalBonus))
				{
					if (!(bonus is GemBonus))
					{
						if (!(bonus is SelectGetItemBonus))
						{
							if (!(bonus is AddInfluenceBonus))
							{
								if (bonus is AddBossInfluenceBonus)
								{
									return "bossInfluence";
								}
								return bonus.GetType().Name;
							}
							return "influence";
						}
						return "item";
					}
					return "gem";
				}
				return "crystal";
			}
			return "coin";
		}

		private void ResolveReward(AiGameSessionController controller, JObject request)
		{
			JObject jObject = (request["payload"]?["choices"] as JObject) ?? new JObject();
			List<BattleBonusObject> bonuses = Bonus.bonuses ?? new List<BattleBonusObject>();
			Dictionary<string, RewardChoiceGroupPlan> dictionary = BuildChoiceGroupPlans(controller, bonuses);
			foreach (JProperty item in jObject.Properties())
			{
				if (!dictionary.ContainsKey(item.Name))
				{
					throw new InvalidOperationException("Unknown reward choice group `" + item.Name + "`.");
				}
			}
			foreach (RewardChoiceGroupPlan value in dictionary.Values)
			{
				JToken jToken = jObject[value.GroupId];
				if (jToken == null || jToken.Type == JTokenType.Null || string.IsNullOrWhiteSpace(jToken.Value<string>()))
				{
					if (value.Required)
					{
						throw new InvalidOperationException("Reward choice group `" + value.GroupId + "` requires a selection.");
					}
				}
				else
				{
					value.Validate(jToken.Value<string>());
				}
			}
			controller.ApplyAutomaticBonuses(this);
			foreach (RewardChoiceGroupPlan value2 in dictionary.Values)
			{
				JToken jToken2 = jObject[value2.GroupId];
				if (jToken2 == null || jToken2.Type == JTokenType.Null || string.IsNullOrWhiteSpace(jToken2.Value<string>()))
				{
					if (value2.Required)
					{
						throw new InvalidOperationException("Reward choice group `" + value2.GroupId + "` requires a selection.");
					}
					value2.Skip();
				}
				else
				{
					value2.Apply(jToken2.Value<string>());
				}
			}
			controller.FinishBonusIfComplete(this);
			controller.currentSignature = null;
		}

		private Dictionary<string, RewardChoiceGroupPlan> BuildChoiceGroupPlans(AiGameSessionController controller, List<BattleBonusObject> bonuses)
		{
			Dictionary<string, RewardChoiceGroupPlan> dictionary = new Dictionary<string, RewardChoiceGroupPlan>();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < bonuses.Count; i++)
			{
				BattleBonusObject bonus = bonuses[i];
				if (bonus == null || bonus.view || !bonus.isChoose)
				{
					continue;
				}
				SelectGetItemBonus itemBonus = bonus as SelectGetItemBonus;
				if (itemBonus != null)
				{
					string groupId = ((num == 0) ? "cardReward" : $"cardReward.{num}");
					num++;
					HashSet<string> validOptions = new HashSet<string>(from id in (itemBonus.allItems ?? new List<Item>()).Select((Item item, int index) => (item != null) ? $"card.{index}" : null)
						where id != null
						select id);
					dictionary[groupId] = new RewardChoiceGroupPlan(groupId, itemBonus.minSelectCount > 0, validOptions, delegate(string optionId)
					{
						if (string.IsNullOrWhiteSpace(optionId) || !optionId.StartsWith("card.", StringComparison.Ordinal))
						{
							throw new InvalidOperationException("Invalid option `" + optionId + "` for reward choice group `" + groupId + "`.");
						}
						if (!int.TryParse(optionId.Substring("card.".Length), out var result) || result < 0 || result >= (itemBonus.allItems?.Count ?? 0))
						{
							throw new InvalidOperationException("Invalid option `" + optionId + "` for reward choice group `" + groupId + "`.");
						}
						itemBonus.view = true;
						itemBonus.selectItems = new List<Item> { itemBonus.allItems[result] };
						itemBonus.TriggerBonus(controller.applicationState.runningGameState, delegate
						{
							controller.OnBonusGot(this);
						});
					}, delegate
					{
						itemBonus.view = true;
					});
					continue;
				}
				if (bonus is AddInfluenceBonus addInfluenceBonus)
				{
					string text = ((num2 == 0) ? "influenceReward" : $"influenceReward.{num2}");
					num2++;
					dictionary[text] = BuildInfluenceChoicePlan(controller, text, addInfluenceBonus, addInfluenceBonus.selectableInfluence, "influence");
					continue;
				}
				if (bonus is AddBossInfluenceBonus addBossInfluenceBonus)
				{
					string text2 = ((num3 == 0) ? "bossInfluenceReward" : $"bossInfluenceReward.{num3}");
					num3++;
					dictionary[text2] = BuildInfluenceChoicePlan(controller, text2, addBossInfluenceBonus, addBossInfluenceBonus.selectableInfluence, "bossInfluence");
					continue;
				}
				string genericGroupId = ((num4 == 0) ? "choiceReward" : $"choiceReward.{num4}");
				num4++;
				dictionary[genericGroupId] = new RewardChoiceGroupPlan(genericGroupId, required: false, new string[1] { "trigger" }, delegate(string optionId)
				{
					if (!string.Equals(optionId, "trigger", StringComparison.Ordinal))
					{
						throw new InvalidOperationException("Invalid option `" + optionId + "` for reward choice group `" + genericGroupId + "`.");
					}
					bonus.view = true;
					bonus.TriggerBonus(controller.applicationState.runningGameState, delegate
					{
						controller.OnBonusGot(this);
					});
				}, delegate
				{
					bonus.view = true;
				});
			}
			return dictionary;
		}

		private RewardChoiceGroupPlan BuildInfluenceChoicePlan(AiGameSessionController controller, string groupId, BattleBonusObject bonus, List<Influence> influences, string optionPrefix)
		{
			HashSet<string> validOptions = new HashSet<string>(from id in (influences ?? new List<Influence>()).Select((Influence influence, int index) => (influence != null) ? $"{optionPrefix}.{index}" : null)
				where id != null
				select id);
			return new RewardChoiceGroupPlan(groupId, required: false, validOptions, delegate(string optionId)
			{
				string text = optionPrefix + ".";
				if (string.IsNullOrWhiteSpace(optionId) || !optionId.StartsWith(text, StringComparison.Ordinal))
				{
					throw new InvalidOperationException("Invalid option `" + optionId + "` for reward choice group `" + groupId + "`.");
				}
				if (!int.TryParse(optionId.Substring(text.Length), out var result) || result < 0 || result >= (influences?.Count ?? 0))
				{
					throw new InvalidOperationException("Invalid option `" + optionId + "` for reward choice group `" + groupId + "`.");
				}
				if (!(bonus is AddInfluenceBonus addInfluenceBonus))
				{
					if (bonus is AddBossInfluenceBonus addBossInfluenceBonus)
					{
						addBossInfluenceBonus.view = true;
						addBossInfluenceBonus.selectedInfluence = influences[result];
						addBossInfluenceBonus.TriggerBonus(controller.applicationState.runningGameState, delegate
						{
							controller.OnBonusGot(this);
						});
					}
				}
				else
				{
					addInfluenceBonus.view = true;
					addInfluenceBonus.selectedInfluence = influences[result];
					addInfluenceBonus.TriggerBonus(controller.applicationState.runningGameState, delegate
					{
						controller.OnBonusGot(this);
					});
				}
			}, delegate
			{
				bonus.view = true;
			});
		}

		private void BuildInfluenceBonusActions(AiGameSessionController controller, AddInfluenceBonus bonus, int bonusIndex, string type)
		{
			List<Influence> influences = bonus.selectableInfluence ?? new List<Influence>();
			for (int i = 0; i < influences.Count; i++)
			{
				int influenceIndex = i;
				Influence value = influences[i];
				controller.AddAction($"bonus.{bonusIndex}.influence.{i}", type, LabelForObject(value), new JObject
				{
					["bonusIndex"] = bonusIndex,
					["influenceIndex"] = i,
					["label"] = LabelForObject(value)
				}, delegate
				{
					controller.ApplyAutomaticBonuses(this);
					bonus.view = true;
					bonus.selectedInfluence = influences[influenceIndex];
					bonus.TriggerBonus(controller.applicationState.runningGameState, delegate
					{
						controller.OnBonusGot(this);
					});
				});
			}
		}

		private void BuildBossInfluenceBonusActions(AiGameSessionController controller, AddBossInfluenceBonus bonus, int bonusIndex)
		{
			List<Influence> influences = bonus.selectableInfluence ?? new List<Influence>();
			for (int i = 0; i < influences.Count; i++)
			{
				int influenceIndex = i;
				Influence value = influences[i];
				controller.AddAction($"bonus.{bonusIndex}.bossInfluence.{i}", "chooseBossInfluence", LabelForObject(value), new JObject
				{
					["bonusIndex"] = bonusIndex,
					["influenceIndex"] = i,
					["label"] = LabelForObject(value)
				}, delegate
				{
					controller.ApplyAutomaticBonuses(this);
					bonus.view = true;
					bonus.selectedInfluence = influences[influenceIndex];
					bonus.TriggerBonus(controller.applicationState.runningGameState, delegate
					{
						controller.OnBonusGot(this);
					});
				});
			}
		}

		private static string LabelForObject(object value)
		{
			if (!(value is Item { ItemNameWithUpgrade: var itemNameWithUpgrade }))
			{
				if (!(value is GameSkill { Name: var name }))
				{
					if (!(value is Influence influence))
					{
						if (!(value is Rune rune))
						{
							if (value is Enchant enchant)
							{
								return enchant.def?.Name;
							}
							return value?.ToString() ?? "(null)";
						}
						return rune.def?.Name;
					}
					return influence.def?.InfluenceName;
				}
				return name;
			}
			return itemNameWithUpgrade;
		}
	}

	private readonly ApplicationStateInfo applicationState;

	private readonly Dictionary<string, AiLegalAction> currentLegalActions = new Dictionary<string, AiLegalAction>();

	private readonly HashSet<string> appliedActionIdsInCurrentDecision = new HashSet<string>();

	private readonly List<string> trace = new List<string>();

	private AiPendingDecision pendingDecision;

	private string currentSignature;

	private int repeatedActionGuardSeq;

	private int lastAutoContinuedCount;

	private string lastAutoContinuationDetail;

	public string SessionId { get; private set; }

	public int DecisionSeq { get; private set; }

	public string LastError { get; private set; }

	public bool Active => !string.IsNullOrEmpty(SessionId);

	public bool HasPendingDecision => pendingDecision != null;

	public AiGameSessionController(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
	}

	public JObject CreateSession(JObject request)
	{
		if (!TryAbortPendingDecisionForLifecycle("CreateSession", out var error))
		{
			return error;
		}
		SessionId = request.Value<string>("sessionId");
		if (string.IsNullOrEmpty(SessionId))
		{
			SessionId = Guid.NewGuid().ToString("N");
		}
		LastError = null;
		trace.Clear();
		pendingDecision = null;
		currentLegalActions.Clear();
		appliedActionIdsInCurrentDecision.Clear();
		currentSignature = null;
		DecisionSeq = 0;
		repeatedActionGuardSeq = 0;
		lastAutoContinuedCount = 0;
		lastAutoContinuationDetail = null;
		applicationState.aiSessionMode = true;
		GameAutomationHooks.SetHandler(this);
		if (request.Value<bool?>("startNewGame") == true)
		{
			StartNewGame(request);
		}
		return Observe();
	}

	public JObject CloseSession()
	{
		if (!TryAbortPendingDecisionForLifecycle("CloseSession", out var error))
		{
			return error;
		}
		pendingDecision = null;
		currentLegalActions.Clear();
		appliedActionIdsInCurrentDecision.Clear();
		currentSignature = null;
		lastAutoContinuedCount = 0;
		lastAutoContinuationDetail = null;
		applicationState.aiSessionMode = false;
		GameAutomationHooks.ClearHandler(this);
		string sessionId = SessionId;
		SessionId = null;
		return new JObject
		{
			["sessionId"] = sessionId,
			["closed"] = true
		};
	}

	private bool TryAbortPendingDecisionForLifecycle(string operation, out JObject error)
	{
		error = null;
		if (pendingDecision == null)
		{
			return true;
		}
		try
		{
			if (pendingDecision.TryAbort(out var code, out var message))
			{
				return true;
			}
			error = Error(code, operation + " cannot continue: " + message);
		}
		catch (Exception ex)
		{
			LastError = ex.ToString();
			error = Error("PendingDecisionAbortFailed", operation + " cannot continue: " + ex.Message);
		}
		error["sessionId"] = SessionId;
		if (string.Equals(operation, "CloseSession", StringComparison.Ordinal))
		{
			error["closed"] = false;
		}
		return false;
	}

	public JObject Observe()
	{
		return BuildObservation();
	}

	public JObject Act(JObject request)
	{
		string text = request.Value<string>("actionId");
		int? num = request.Value<int?>("decisionSeq");
		if (!Active)
		{
			return Error("NoActiveSession", "No active AI session.");
		}
		BuildObservation();
		if (!num.HasValue || num.Value != DecisionSeq)
		{
			return Error("StaleDecision", string.Format("Expected decisionSeq {0}, got {1}.", DecisionSeq, num?.ToString() ?? "null"));
		}
		if (string.IsNullOrEmpty(text) || !currentLegalActions.TryGetValue(text, out var value))
		{
			return Error("IllegalAction", "Unknown or illegal actionId `" + text + "`.");
		}
		if (repeatedActionGuardSeq != DecisionSeq)
		{
			appliedActionIdsInCurrentDecision.Clear();
			repeatedActionGuardSeq = DecisionSeq;
		}
		if (appliedActionIdsInCurrentDecision.Contains(text))
		{
			return Error("RepeatedAction", $"Action `{text}` was already applied for decisionSeq {DecisionSeq}; observe or step until a new decision before retrying it.");
		}
		try
		{
			value.Apply(request);
			appliedActionIdsInCurrentDecision.Add(text);
			trace.Add($"{DecisionSeq}:{text}");
			if (pendingDecision != null && pendingDecision.Completed)
			{
				pendingDecision = null;
			}
			return BuildObservation();
		}
		catch (Exception ex)
		{
			LastError = ex.ToString();
			return Error("ActionFailed", ex.Message);
		}
	}

	public JObject StepUntilDecision(int maxActionSteps = 200)
	{
		GameStateInfo runningGameState = applicationState.runningGameState;
		lastAutoContinuedCount = 0;
		lastAutoContinuationDetail = null;
		if (runningGameState != null)
		{
			for (int i = 0; i < maxActionSteps; i++)
			{
				if (HasPendingDecision)
				{
					break;
				}
				if (runningGameState.gameEnded)
				{
					break;
				}
				if (TryResolveBlockingContinuation(runningGameState, out var detail))
				{
					lastAutoContinuedCount++;
					lastAutoContinuationDetail = detail;
					continue;
				}
				if (runningGameState.actionManager != null && !runningGameState.actionManager.isEmpty)
				{
					runningGameState.actionManager.Update();
					continue;
				}
				if (!IsAtImplicitDecision(runningGameState))
				{
				}
				break;
			}
		}
		return BuildObservation();
	}

	public bool TryHandleSelectItem(GameStateInfo gameState, SelectItemInfo info)
	{
		if (!CanHandleAutomation(gameState))
		{
			return false;
		}
		if (info.output == null)
		{
			info.output = new List<Item>();
		}
		pendingDecision = AiPendingDecision.SelectItems(info);
		currentSignature = null;
		return true;
	}

	public bool TryHandleElementSelect<T>(GameStateInfo gameState, ElementSelectPanel<T>.SelectInfo info) where T : class
	{
		if (!CanHandleAutomation(gameState))
		{
			return false;
		}
		if (info.output == null)
		{
			info.output = new List<T>();
		}
		pendingDecision = AiPendingDecision.SelectElements(info);
		currentSignature = null;
		return true;
	}

	public bool TryHandleBonus(GameStateInfo gameState, BattleBonus bonus, bool endEncounterAfterBonus, bool isBattle)
	{
		if (!CanHandleAutomation(gameState) || bonus == null)
		{
			return false;
		}
		pendingDecision = AiPendingDecision.CreateBonus(bonus, endEncounterAfterBonus, isBattle);
		if (pendingDecision.Completed)
		{
			pendingDecision = null;
		}
		currentSignature = null;
		return true;
	}

	private bool CanHandleAutomation(GameStateInfo gameState)
	{
		if (Active && gameState != null)
		{
			return gameState.applicationState == applicationState;
		}
		return false;
	}

	private void StartNewGame(JObject request)
	{
		NewGameConfigData newGameConfigData = NewGameConfigData.DefaultStartGameConfig(applicationState);
		string characterId = request.Value<string>("characterId");
		if (!string.IsNullOrEmpty(characterId))
		{
			PlayerCharacterDef playerCharacterDef = applicationState.assetCatalog.characterDefCatalog.ValidAssets.FirstOrDefault((PlayerCharacterDef c) => c.FullID == characterId);
			if (playerCharacterDef != null)
			{
				newGameConfigData.currentCharacter = playerCharacterDef;
			}
		}
		string contractId = request.Value<string>("contractId");
		if (!string.IsNullOrEmpty(contractId))
		{
			ContractDef contractDef = applicationState.assetCatalog.contractDefCatalog.ValidAssets.FirstOrDefault((ContractDef c) => c.FullID == contractId);
			if (contractDef != null)
			{
				newGameConfigData.contract = contractDef.GenerateContract(applicationState.assetCatalog, newGameConfigData.contract?.difficultyLevel ?? DifficultyLevel.Normal);
			}
		}
		string value = request.Value<string>("difficulty");
		if (!string.IsNullOrEmpty(value) && Enum.TryParse<DifficultyLevel>(value, ignoreCase: true, out var result) && newGameConfigData.contract != null)
		{
			newGameConfigData.contract.difficultyLevel = result;
		}
		newGameConfigData.startGameInfo = NewGameConfigData.GenerateLocalPrepareInfo(applicationState);
		uint? num = request.Value<uint?>("seed");
		if (num.HasValue)
		{
			newGameConfigData.startGameInfo.gameRandomSeed = num.Value;
		}
		Level newGameLevel = newGameConfigData.contract?.GetInitLevel(applicationState.assetCatalog) ?? Level.GetStageLevels(1, applicationState.assetCatalog).FirstOrDefault();
		Bootstrap.instance.Load(BootStrapLoadData.ToStartNewGame(newGameConfigData, Bootstrap.instance.CurrentAccountID, newGameLevel));
	}

	private JObject BuildObservation()
	{
		currentLegalActions.Clear();
		GameStateInfo runningGameState = applicationState.runningGameState;
		string text = "loading";
		JObject jObject = new JObject();
		if (!Active)
		{
			text = "inactive";
		}
		else if (LastError != null)
		{
			text = "error";
			jObject["message"] = LastError;
		}
		else if (runningGameState == null || !runningGameState.inited || !runningGameState.loaded)
		{
			text = "loading";
		}
		else if (runningGameState.gameEnded)
		{
			text = "gameOver";
		}
		else if (pendingDecision == null)
		{
			text = (BuildPanelDecision(runningGameState, jObject) ? "select" : (BuildBattleDecision(runningGameState, jObject) ? "battle" : (BuildWorldEventDecision(runningGameState, jObject) ? "worldEvent" : ((!BuildMapDecision(runningGameState, jObject)) ? "waiting" : "map"))));
		}
		else
		{
			text = pendingDecision.Phase;
			pendingDecision.BuildLegalActions(this, jObject);
		}
		string text2 = text + "|" + string.Join(",", currentLegalActions.Keys) + "|" + pendingDecision?.Signature;
		if (text2 != currentSignature)
		{
			DecisionSeq++;
			currentSignature = text2;
			appliedActionIdsInCurrentDecision.Clear();
			repeatedActionGuardSeq = DecisionSeq;
		}
		JObject jObject2 = (string.Equals(text, "waiting", StringComparison.Ordinal) ? BuildWaitingInfo(runningGameState) : null);
		return new JObject
		{
			["sessionId"] = SessionId,
			["decisionSeq"] = DecisionSeq,
			["phase"] = text,
			["aiSessionMode"] = applicationState.aiSessionMode,
			["game"] = BuildGameSummary(runningGameState),
			["data"] = jObject,
			["waiting"] = jObject2,
			["legalActions"] = new JArray(currentLegalActions.Values.Select((AiLegalAction a) => a.ToJson())),
			["auxiliaryActions"] = BuildAuxiliaryActions(jObject2),
			["traceCount"] = trace.Count
		};
	}

	private bool TryResolveBlockingContinuation(GameStateInfo gameState, out string detail)
	{
		detail = null;
		MultiInfoDisplayPanel multiInfoDisplayPanel = gameState?.UIManager?.multiInfoDisplayPanel;
		if (multiInfoDisplayPanel == null || !multiInfoDisplayPanel.gameObject.activeInHierarchy)
		{
			return false;
		}
		detail = "Auto-closed " + multiInfoDisplayPanel.gameObject.name + ".";
		multiInfoDisplayPanel.Unset();
		return true;
	}

	private JObject BuildWaitingInfo(GameStateInfo gameState)
	{
		bool flag = gameState?.actionManager != null && !gameState.actionManager.isEmpty;
		string blockingUiName = GetBlockingUiName(gameState);
		string text;
		string text2;
		if (!string.IsNullOrEmpty(blockingUiName))
		{
			text = "blockingUi";
			text2 = blockingUiName + " is visible.";
		}
		else if (flag)
		{
			text = "actionQueueBusy";
			text2 = "Action queue is still resolving.";
		}
		else if (gameState != null && gameState.battleState?.isInBattle == true)
		{
			text = "battleTransition";
			text2 = "Battle is active but no player decision is currently available.";
		}
		else if (gameState?.dungeonMapManager?.currentDungeonMap != null)
		{
			text = "noDecision";
			text2 = "Map is loaded but no selectable decision is currently available.";
		}
		else
		{
			text = "unknown";
			text2 = "No AI decision is currently available.";
		}
		if (!string.IsNullOrEmpty(lastAutoContinuationDetail))
		{
			text2 = text2 + " " + lastAutoContinuationDetail;
		}
		return new JObject
		{
			["reason"] = text,
			["detail"] = text2,
			["actionQueueBusy"] = flag,
			["blockingUi"] = blockingUiName,
			["autoContinuedCount"] = lastAutoContinuedCount
		};
	}

	private static string GetBlockingUiName(GameStateInfo gameState)
	{
		ActivatePotentialTalentPanel activatePotentialTalentPanel = gameState?.UIManager?.activatePotentialTalentPanel;
		if (activatePotentialTalentPanel != null && activatePotentialTalentPanel.gameObject.activeInHierarchy)
		{
			return activatePotentialTalentPanel.gameObject.name;
		}
		SelectSkillActivatePanel selectSkillActivatePanel = gameState?.UIManager?.selectSkillPanel;
		if (selectSkillActivatePanel != null && selectSkillActivatePanel.gameObject.activeInHierarchy)
		{
			return selectSkillActivatePanel.gameObject.name;
		}
		MintageRuneRoomEventPanel mintageRuneRoomEventPanel = gameState?.UIManager?.worldEventPanel_MintageRune;
		if (mintageRuneRoomEventPanel != null && mintageRuneRoomEventPanel.gameObject.activeInHierarchy && mintageRuneRoomEventPanel.IsAiMintagePanelActive)
		{
			return mintageRuneRoomEventPanel.mintageRunePanel.gameObject.name;
		}
		UpgradeRoomEventPanel upgradeRoomEventPanel = gameState?.UIManager?.worldEventPanel_UpgradeRoom;
		if (upgradeRoomEventPanel != null && upgradeRoomEventPanel.gameObject.activeInHierarchy && upgradeRoomEventPanel.IsAiUpgradePanelActive)
		{
			return upgradeRoomEventPanel.upgradePanel.gameObject.name;
		}
		MultiInfoDisplayPanel multiInfoDisplayPanel = gameState?.UIManager?.multiInfoDisplayPanel;
		if (multiInfoDisplayPanel != null && multiInfoDisplayPanel.gameObject.activeInHierarchy)
		{
			return multiInfoDisplayPanel.gameObject.name;
		}
		return null;
	}

	private static JArray BuildAuxiliaryActions(JObject waiting)
	{
		JArray jArray = new JArray();
		if (waiting == null)
		{
			return jArray;
		}
		string text = waiting.Value<string>("blockingUi");
		if (!string.IsNullOrEmpty(text))
		{
			jArray.Add(new JObject
			{
				["actionId"] = "aux.continueBlockingUi",
				["kind"] = "continueBlockingUi",
				["label"] = "Continue " + text,
				["data"] = new JObject { ["blockingUi"] = text }
			});
		}
		jArray.Add(new JObject
		{
			["actionId"] = "aux.inspectRuntimeUi",
			["kind"] = "inspectRuntimeUi",
			["label"] = "Inspect runtime UI",
			["data"] = null
		});
		return jArray;
	}

	private bool IsAtImplicitDecision(GameStateInfo gameState)
	{
		BattleStateInfo battleState = gameState.battleState;
		if ((battleState == null || !battleState.isInBattle || battleState.turnPhase != TurnPhase.PlayerControlPhase) && !HasPanelDecision(gameState) && !HasWorldEventDecision(gameState))
		{
			DungeonMapManager dungeonMapManager = gameState.dungeonMapManager;
			if (dungeonMapManager == null)
			{
				return false;
			}
			return dungeonMapManager.currentDungeonMap?.locations?.Any((PathLocation l) => IsSelectableMapLocation(gameState, l)) == true;
		}
		return true;
	}

	private bool HasPanelDecision(GameStateInfo gameState)
	{
		GameUIManager gameUIManager = gameState?.UIManager;
		if (gameUIManager == null)
		{
			return false;
		}
		ActivatePotentialTalentPanel activatePotentialTalentPanel = gameUIManager.activatePotentialTalentPanel;
		if ((object)activatePotentialTalentPanel == null || !activatePotentialTalentPanel.IsAiDecisionActive)
		{
			SelectSkillActivatePanel selectSkillPanel = gameUIManager.selectSkillPanel;
			if ((object)selectSkillPanel == null || !selectSkillPanel.IsAiDecisionActive)
			{
				MintageRuneRoomEventPanel worldEventPanel_MintageRune = gameUIManager.worldEventPanel_MintageRune;
				if ((object)worldEventPanel_MintageRune == null || !worldEventPanel_MintageRune.CanOpenMintageForAiSession())
				{
					MintageRuneRoomEventPanel worldEventPanel_MintageRune2 = gameUIManager.worldEventPanel_MintageRune;
					if ((object)worldEventPanel_MintageRune2 == null || !worldEventPanel_MintageRune2.IsAiMintagePanelActive)
					{
						UpgradeRoomEventPanel worldEventPanel_UpgradeRoom = gameUIManager.worldEventPanel_UpgradeRoom;
						if ((object)worldEventPanel_UpgradeRoom == null || !worldEventPanel_UpgradeRoom.CanOpenCardUpgradeForAiSession())
						{
							return gameUIManager.worldEventPanel_UpgradeRoom?.IsAiUpgradePanelActive ?? false;
						}
					}
				}
			}
		}
		return true;
	}

	private bool HasWorldEventDecision(GameStateInfo gameState)
	{
		WorldEventDisplayInfo worldEventDisplayInfo = gameState.currentEncounter?.triggerEvent?.currentDisplayInfo;
		if (worldEventDisplayInfo == null)
		{
			return false;
		}
		if (!worldEventDisplayInfo.choices.OrEmptyIfNull().Any((WorldEventChoice c) => c != null && (c.executeAbleChecker?.Invoke() ?? true)) && !worldEventDisplayInfo.showLeaveButton)
		{
			return gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.PrepareForExcape);
		}
		return true;
	}

	private bool BuildPanelDecision(GameStateInfo gameState, JObject data)
	{
		if (!BuildPotentialTalentPanelDecision(gameState, data) && !BuildSelectSkillPanelDecision(gameState, data) && !BuildMintageRunePanelDecision(gameState, data))
		{
			return BuildUpgradeRoomPanelDecision(gameState, data);
		}
		return true;
	}

	private bool BuildPotentialTalentPanelDecision(GameStateInfo gameState, JObject data)
	{
		ActivatePotentialTalentPanel panel = gameState.UIManager?.activatePotentialTalentPanel;
		ActivatePotentialTalentPanel activatePotentialTalentPanel = panel;
		if ((object)activatePotentialTalentPanel == null || !activatePotentialTalentPanel.IsAiDecisionActive)
		{
			return false;
		}
		data["kind"] = "potentialTalent";
		List<TalentDef> talents = panel.GetAiActivateAbleTalents();
		data["talents"] = new JArray(talents.Select((TalentDef talent, int num2) => new JObject
		{
			["index"] = num2,
			["id"] = talent.FullID,
			["name"] = talent.GetDisplayName(applicationState, gameState.playerState?.character),
			["type"] = talent.talentType.ToString()
		}));
		for (int num = 0; num < talents.Count; num++)
		{
			int index = num;
			TalentDef talentDef = talents[num];
			AddAction($"select.potentialTalent.{num}", "selectPotentialTalent", talentDef.GetDisplayName(applicationState, gameState.playerState?.character), new JObject
			{
				["talentIndex"] = num,
				["talentId"] = talentDef.FullID,
				["name"] = talentDef.GetDisplayName(applicationState, gameState.playerState?.character)
			}, delegate
			{
				panel.ConfirmActivateTalent(talents[index]);
			});
		}
		AddAction("ui.close.activatePotentialTalent", "closeBlockingUi", "Close potential talent panel", new JObject { ["panel"] = panel.gameObject.name }, panel.CancelForAiSession);
		return true;
	}

	private bool BuildSelectSkillPanelDecision(GameStateInfo gameState, JObject data)
	{
		SelectSkillActivatePanel panel = gameState.UIManager?.selectSkillPanel;
		SelectSkillActivatePanel selectSkillActivatePanel = panel;
		if ((object)selectSkillActivatePanel == null || !selectSkillActivatePanel.IsAiDecisionActive)
		{
			return false;
		}
		List<SelectSkillActivatePanel.AiActivateSkillCandidate> aiActivateSkillCandidates = panel.GetAiActivateSkillCandidates();
		data["kind"] = "activateSkill";
		data["candidates"] = new JArray(aiActivateSkillCandidates.Select((SelectSkillActivatePanel.AiActivateSkillCandidate candidate) => new JObject
		{
			["index"] = candidate.Index,
			["skill"] = BuildSkill(candidate.Skill),
			["targetSlotIndex"] = candidate.TargetSlot?.displayIndex,
			["replaces"] = BuildSkill(candidate.TargetSlot?.gameSkill)
		}));
		foreach (SelectSkillActivatePanel.AiActivateSkillCandidate item in aiActivateSkillCandidates)
		{
			SelectSkillActivatePanel.AiActivateSkillCandidate captured = item;
			string label = ((item.TargetSlot?.skill == null) ? ("Activate " + item.Skill?.NameWithQualityAndUpgrade) : ("Swap " + item.TargetSlot.gameSkill.NameWithQualityAndUpgrade + " -> " + item.Skill?.NameWithQualityAndUpgrade));
			AddAction($"select.skillActivate.{item.Index}", "activateSkill", label, new JObject
			{
				["candidateIndex"] = item.Index,
				["skill"] = BuildSkill(item.Skill),
				["targetSlotIndex"] = item.TargetSlot?.displayIndex,
				["replaces"] = BuildSkill(item.TargetSlot?.gameSkill)
			}, delegate
			{
				if (!panel.TryActivateSkillForAiSession(captured))
				{
					throw new InvalidOperationException("Skill activation candidate is no longer valid.");
				}
			});
		}
		if (panel.CanCancelForAiSession)
		{
			AddAction("ui.close.selectSkillActivate", "closeBlockingUi", "Close skill activation panel", new JObject { ["panel"] = panel.gameObject.name }, panel.CancelForAiSession);
		}
		return true;
	}

	private bool BuildMintageRunePanelDecision(GameStateInfo gameState, JObject data)
	{
		MintageRuneRoomEventPanel panel = gameState.UIManager?.worldEventPanel_MintageRune;
		if (panel == null || !panel.gameObject.activeInHierarchy)
		{
			return false;
		}
		data["kind"] = "mintageRune";
		if (panel.CanOpenMintageForAiSession())
		{
			AddAction("mintage.open", "openMintageRune", "Open mintage rune panel", null, panel.OpenMintagePanelForAiSession);
			return true;
		}
		if (!panel.IsAiMintagePanelActive)
		{
			return false;
		}
		List<MintageRunePanel_Event.AiMintageCandidate> aiMintageCandidates = panel.mintageRunePanel.GetAiMintageCandidates();
		data["runes"] = BuildRunes(panel.mintageRunePanel.GetRunes());
		data["candidates"] = new JArray(aiMintageCandidates.Select((MintageRunePanel_Event.AiMintageCandidate candidate) => new JObject
		{
			["index"] = candidate.Index,
			["item"] = BuildItem(candidate.Item, candidate.Item.owner?.GetItemIndex(candidate.Item) ?? (-1)),
			["reason"] = candidate.Reason.ToString(),
			["forceMintage"] = candidate.ForceMintage
		}));
		foreach (MintageRunePanel_Event.AiMintageCandidate item in aiMintageCandidates)
		{
			MintageRunePanel_Event.AiMintageCandidate captured = item;
			AddAction($"mintage.apply.{item.Index}", "applyMintageRune", "Mintage " + item.Item.ItemNameWithUpgrade, new JObject
			{
				["candidateIndex"] = item.Index,
				["item"] = BuildItem(item.Item, item.Item.owner?.GetItemIndex(item.Item) ?? (-1)),
				["runes"] = BuildRunes(panel.mintageRunePanel.GetRunes()),
				["reason"] = item.Reason.ToString(),
				["forceMintage"] = item.ForceMintage
			}, delegate
			{
				if (!panel.mintageRunePanel.TryMintageForAiSession(captured))
				{
					throw new InvalidOperationException("Mintage candidate is no longer valid.");
				}
			});
		}
		AddAction("ui.close.mintageRune", "closeBlockingUi", "Close mintage rune panel", new JObject { ["panel"] = panel.mintageRunePanel.gameObject.name }, panel.mintageRunePanel.CloseForAiSession);
		return true;
	}

	private bool BuildUpgradeRoomPanelDecision(GameStateInfo gameState, JObject data)
	{
		UpgradeRoomEventPanel panel = gameState.UIManager?.worldEventPanel_UpgradeRoom;
		if (panel == null || !panel.gameObject.activeInHierarchy)
		{
			return false;
		}
		data["kind"] = "upgradeRoom";
		if (panel.CanOpenCardUpgradeForAiSession())
		{
			AddAction("upgrade.openCardUpgrade", "openCardUpgrade", "Open card upgrade panel", null, panel.OpenCardUpgradeForAiSession);
			return true;
		}
		if (!panel.IsAiUpgradePanelActive)
		{
			return false;
		}
		List<UpgradePanel_Event.AiUpgradeCandidate> aiUpgradeCandidates = panel.upgradePanel.GetAiUpgradeCandidates();
		data["candidates"] = new JArray(aiUpgradeCandidates.Select((UpgradePanel_Event.AiUpgradeCandidate candidate) => new JObject
		{
			["index"] = candidate.Index,
			["item"] = BuildItem(candidate.Item, candidate.Item.owner?.GetItemIndex(candidate.Item) ?? (-1)),
			["crystalCost"] = candidate.CrystalCost,
			["healthCost"] = candidate.HealthCost
		}));
		foreach (UpgradePanel_Event.AiUpgradeCandidate item in aiUpgradeCandidates)
		{
			UpgradePanel_Event.AiUpgradeCandidate captured = item;
			AddAction($"upgrade.card.{item.Index}", "upgradeCard", "Upgrade " + item.Item.ItemNameWithUpgrade, new JObject
			{
				["candidateIndex"] = item.Index,
				["item"] = BuildItem(item.Item, item.Item.owner?.GetItemIndex(item.Item) ?? (-1)),
				["crystalCost"] = item.CrystalCost,
				["healthCost"] = item.HealthCost
			}, delegate
			{
				if (!panel.upgradePanel.TryUpgradeForAiSession(captured))
				{
					throw new InvalidOperationException("Upgrade candidate is no longer valid.");
				}
			});
		}
		AddAction("ui.close.upgradeRoom", "closeBlockingUi", "Close upgrade panel", new JObject { ["panel"] = panel.upgradePanel.gameObject.name }, panel.upgradePanel.CloseForAiSession);
		return true;
	}

	private bool BuildBattleDecision(GameStateInfo gameState, JObject data)
	{
		BattleStateInfo battleState = gameState.battleState;
		if (battleState == null || !battleState.isInBattle || battleState.turnPhase != TurnPhase.PlayerControlPhase)
		{
			return false;
		}
		if (!gameState.PlayerControlAble || gameState.actionManager == null || !gameState.actionManager.isEmpty)
		{
			return false;
		}
		data["turn"] = gameState.battleState.turnCount;
		data["player"] = BuildBft(gameState.battleState.player);
		data["enemy"] = BuildBft(gameState.battleState.enemy?.bft);
		data["playerTable"] = BuildItems(gameState.playerState.TableContainer?.GetAllItems());
		data["enemyTable"] = BuildItems(gameState.battleState.EnemyTableContainer?.GetAllItems());
		List<Item> list = gameState.playerState.TableContainer?.GetAllItems() ?? new List<Item>();
		List<Item> second = gameState.battleState.EnemyTableContainer?.GetAllItems() ?? new List<Item>();
		for (int i = 0; i < list.Count; i++)
		{
			Item item = list[i];
			if (item == null || !item.IsCooldownCastAble(gameState))
			{
				continue;
			}
			if (!item.spellNeedsTarget)
			{
				AddAction($"battle.cast.{i}", "castItem", "Cast " + item.ItemNameWithUpgrade, new JObject { ["itemIndex"] = i }, delegate
				{
					gameState.UIManager.playerItemArea.controlInfo.EnqueueSpellRequest(new PlayerControlInfo.SpellRequest
					{
						spellItem = item
					});
				});
				continue;
			}
			foreach (Item target in from t in list.Concat(second)
				where t != null
				select t)
			{
				if (SpellCastInfo.SpellCastAble(gameState, item, target, inBattleDisplay: true, out var _))
				{
					int num = target.owner?.GetItemIndex(target) ?? (-1);
					BattleFieldTransform ownerBft = target.OwnerBft;
					string text = ((ownerBft != null && ownerBft.isEnemy) ? "enemy" : "player");
					AddAction($"battle.cast.{i}.{text}.{num}", "castItem", "Cast " + item.ItemNameWithUpgrade + " -> " + target.ItemNameWithUpgrade, new JObject
					{
						["itemIndex"] = i,
						["targetSide"] = text,
						["targetIndex"] = num
					}, delegate
					{
						gameState.UIManager.playerItemArea.controlInfo.EnqueueSpellRequest(new PlayerControlInfo.SpellRequest
						{
							spellItem = item,
							targetItem = target
						});
					});
				}
			}
		}
		AddAction("battle.endTurn", "endTurn", "End player turn", null, delegate
		{
			gameState.battleState.endPlayerTurnFlag = true;
		});
		return true;
	}

	private bool BuildWorldEventDecision(GameStateInfo gameState, JObject data)
	{
		WorldEvent worldEvent = gameState.currentEncounter?.triggerEvent;
		WorldEventDisplayInfo worldEventDisplayInfo = worldEvent?.currentDisplayInfo;
		if (worldEvent == null || worldEventDisplayInfo == null || worldEvent.finishing || worldEvent.finished)
		{
			return false;
		}
		if (gameState.actionManager == null || !gameState.actionManager.isEmpty)
		{
			return false;
		}
		worldEvent.UpdateWorldEventDisplayInfo();
		data["eventId"] = worldEvent.FullID;
		data["title"] = worldEventDisplayInfo.title;
		data["description"] = worldEventDisplayInfo.description;
		bool showLeaveButton = worldEventDisplayInfo.showLeaveButton;
		bool variableFlag = gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.PrepareForExcape);
		for (int i = 0; i < worldEventDisplayInfo.choices.Count; i++)
		{
			WorldEventChoice choice = worldEventDisplayInfo.choices[i];
			if (choice == null)
			{
				continue;
			}
			Func<bool> executeAbleChecker = choice.executeAbleChecker;
			if (executeAbleChecker == null || executeAbleChecker())
			{
				int index = i;
				AddAction($"worldEvent.choice.{i}", "chooseWorldEvent", choice.buttonText, new JObject
				{
					["choiceIndex"] = i,
					["description"] = choice.descriptionText
				}, delegate
				{
					ExecuteWorldEventChoice(gameState, worldEvent, choice, index);
				});
			}
		}
		if (showLeaveButton | variableFlag)
		{
			AddAction("worldEvent.leave", "leaveWorldEvent", "Leave world event", new JObject
			{
				["eventId"] = worldEvent.FullID,
				["reason"] = (showLeaveButton ? "showLeaveButton" : "PrepareForExcape"),
				["showLeaveButton"] = showLeaveButton,
				["prepareForExcape"] = variableFlag
			}, worldEvent.FinishedWorldEvent);
		}
		return currentLegalActions.Count > 0;
	}

	private static void ExecuteWorldEventChoice(GameStateInfo gameState, WorldEvent worldEvent, WorldEventChoice choice, int index)
	{
		worldEvent.lastSelectChoiceIndex = index;
		worldEvent.lastSelectedChoice = choice;
		WorldEventPanel panel = gameState.UIManager.currentEventPanel;
		if (panel != null)
		{
			panel.selected = null;
		}
		choice.executeAction?.Invoke();
		if (!(panel != null))
		{
			return;
		}
		panel.AfterChoiceConfirm(choice);
		if (!choice.stayAfterChoose)
		{
			panel.HideDescription();
		}
		gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			if (panel != null)
			{
				panel.SetDirty();
			}
		}));
		panel.SetDirty();
	}

	private bool BuildMapDecision(GameStateInfo gameState, JObject data)
	{
		DungeonMap dungeonMap = gameState.dungeonMapManager?.currentDungeonMap;
		if (dungeonMap == null || gameState.actionManager == null || !gameState.actionManager.isEmpty)
		{
			return false;
		}
		data["currentGlobalProgress"] = gameState.dungeonMapManager.currentGlobalProgress;
		data["currentLocation"] = BuildLocation(gameState.dungeonMapManager.currentLocation);
		List<PathLocation> list = dungeonMap.locations?.Where((PathLocation l) => IsSelectableMapLocation(gameState, l)).ToList() ?? new List<PathLocation>();
		foreach (PathLocation loc in list)
		{
			int num = dungeonMap.locations.IndexOf(loc);
			AddAction($"map.enter.{num}", "chooseMapNode", loc.GetLocationDisplayName(), new JObject
			{
				["locationIndex"] = num,
				["location"] = BuildLocation(loc)
			}, delegate
			{
				gameState.actionManager.EnqueueAction(new EnterLocationAction(loc));
			});
		}
		return list.Count > 0;
	}

	private bool IsSelectableMapLocation(GameStateInfo gameState, PathLocation location)
	{
		if (location == null)
		{
			return false;
		}
		DungeonMap currentDungeonMap = gameState.dungeonMapManager.currentDungeonMap;
		if (gameState.dungeonMapManager.currentLocation != null && !gameState.dungeonMapManager.currentLocation.finished)
		{
			return false;
		}
		if (!location.reachable || location.finished)
		{
			return false;
		}
		if (!gameState.dungeonMapManager.CanJump(location))
		{
			return false;
		}
		if (currentDungeonMap.currentLocation != location)
		{
			return location.selectable;
		}
		return false;
	}

	private void ApplyAutomaticBonuses(AiPendingDecision decision)
	{
		if (decision?.Bonus == null)
		{
			return;
		}
		foreach (BattleBonusObject item in decision.Bonus.bonuses.OrEmptyIfNull())
		{
			if (item != null && !item.view && !item.isChoose)
			{
				item.view = true;
				item.TriggerBonus(applicationState.runningGameState, delegate
				{
					OnBonusGot(decision);
				});
			}
		}
		FinishBonusIfComplete(decision);
	}

	private void OnBonusGot(AiPendingDecision decision)
	{
		FinishBonusIfComplete(decision);
		currentSignature = null;
	}

	private void FinishBonusIfComplete(AiPendingDecision decision)
	{
		if (decision != null && decision.Bonus != null && !decision.Bonus.bonuses.OrEmptyIfNull().Any((BattleBonusObject b) => b != null && !b.view && b.isChoose))
		{
			FinishBonusSelection(decision);
		}
	}

	private void FinishBonusSelection(AiPendingDecision decision)
	{
		if (!decision.Completed)
		{
			GameStateInfo runningGameState = applicationState.runningGameState;
			runningGameState.gameEventManager.TriggerFinishBonus(decision.Bonus);
			if (decision.EndEncounterAfterBonus)
			{
				runningGameState.actionManager.EnqueueAction(new EndCurrentEncounterAction(finish: true));
			}
			decision.Completed = true;
		}
	}

	internal void AddAction(string id, string type, string label, JObject payload, Action apply)
	{
		AddAction(id, type, label, payload, null, delegate
		{
			apply?.Invoke();
		});
	}

	internal void AddAction(string id, string type, string label, JObject payload, JObject payloadSchema, Action<JObject> apply)
	{
		currentLegalActions[id] = new AiLegalAction(id, type, label, payload, payloadSchema, apply);
	}

	private JObject BuildGameSummary(GameStateInfo gameState)
	{
		if (gameState == null)
		{
			return null;
		}
		return new JObject
		{
			["gameGuid"] = gameState.GameGUID,
			["loaded"] = gameState.loaded,
			["ended"] = gameState.gameEnded,
			["stage"] = gameState.contractState?.stage,
			["globalProgress"] = gameState.dungeonMapManager?.currentGlobalProgress,
			["localProgress"] = gameState.dungeonMapManager?.currentLocalProgress,
			["inBattle"] = gameState.battleState?.isInBattle,
			["turnPhase"] = gameState.battleState?.turnPhase.ToString(),
			["player"] = BuildBft(gameState.playerState?.bft)
		};
	}

	private static JObject BuildBft(BattleFieldTransform bft)
	{
		if (bft == null)
		{
			return null;
		}
		return new JObject
		{
			["health"] = (int)bft.health,
			["maxHealth"] = (int)bft.maxHealth,
			["tempHealth"] = (int)bft.tempHealth,
			["slotCount"] = bft.tableContainer?.GetFinalFixedSlotCount(),
			["tableCount"] = bft.tableContainer?.ItemsCount
		};
	}

	private static JArray BuildItems(IEnumerable<Item> items)
	{
		JArray jArray = new JArray();
		if (items == null)
		{
			return jArray;
		}
		int num = 0;
		foreach (Item item in items)
		{
			jArray.Add(BuildItem(item, num));
			num++;
		}
		return jArray;
	}

	private static JObject BuildItem(Item item, int index)
	{
		if (item == null)
		{
			return null;
		}
		JObject obj = new JObject
		{
			["index"] = index,
			["id"] = item.def?.FullID,
			["name"] = item.ItemNameWithUpgrade,
			["counter"] = (int)item.counter,
			["maxCounter"] = (int)item.maxCounter,
			["cooldown"] = (int)item.cooldown,
			["needsTarget"] = item.spellNeedsTarget
		};
		BattleFieldTransform ownerBft = item.OwnerBft;
		obj["owner"] = ((ownerBft != null && ownerBft.isEnemy) ? "enemy" : "player");
		return obj;
	}

	private static JObject BuildSkill(GameSkill skill)
	{
		if (skill == null)
		{
			return null;
		}
		return new JObject
		{
			["id"] = skill.skillDef?.FullID,
			["name"] = skill.NameWithQualityAndUpgrade,
			["activateCost"] = skill.ActivateCost,
			["activateCostType"] = skill.ActivateCostType.ToString(),
			["replaceToActivate"] = skill.ReplaceToActivate
		};
	}

	private static JArray BuildRunes(IEnumerable<Rune> runes)
	{
		JArray jArray = new JArray();
		if (runes == null)
		{
			return jArray;
		}
		foreach (Rune rune in runes)
		{
			if (rune != null)
			{
				jArray.Add(new JObject
				{
					["id"] = rune.def?.FullID,
					["name"] = rune.GetNameWithValue(),
					["value"] = rune.value,
					["stacking"] = rune.def?.stacking
				});
			}
		}
		return jArray;
	}

	private static JObject BuildLocation(PathLocation location)
	{
		if (location == null)
		{
			return null;
		}
		return new JObject
		{
			["name"] = location.GetLocationDisplayName(),
			["type"] = location.type.ToString(),
			["variantType"] = location.variantType.ToString(),
			["globalProgress"] = location.globalProgress,
			["localProgress"] = location.localProgress,
			["finished"] = location.finished,
			["reachable"] = location.reachable
		};
	}

	internal static JObject Error(string code, string message)
	{
		return new JObject
		{
			["ok"] = false,
			["error"] = new JObject
			{
				["code"] = code,
				["message"] = message
			}
		};
	}
}
