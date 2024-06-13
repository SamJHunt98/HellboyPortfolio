using System;
using System.Collections.Generic;
using System.Text;

using Sirenix.OdinInspector;

using UABase.Core;
using UABase.Effects;

using UATitle.Game;
using UATitle.Util;

using UberAudio;

using UnityEngine;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UABase.Debugging;
#endif //DEVELOPMENT_BUILD || UNITY_EDITOR

/// <summary>
/// Blessing loadout
/// </summary>
namespace UATitle.Game
{
	[Serializable]
	public class ActiveBlessingData
	{
		public BlessingSlot Slot;
		public Blessing Blessing;
		public int Level;
		public StatusEffectBehaviour ActiveStatusEffect;
		public float SlotCooldown;
		public int RemainingGraceFrames = 0;
	}

	[Serializable]
	public class BlessingSaveData
	{
		public string SlotNameTag;
		public string BlessingNameTag;
		public int Level;
	}

	public delegate void BlessingSlotEvent(BlessingSlot slot);

	public class Blessings : MonoBehaviour
	{
		[SerializeField] BlessingLibrary m_BlessingLibrary;
		[SerializeField] BlessingSlotLibrary m_SlotLibrary;

		public BlessingSlotEvent OnSlotTriggered;
		public BlessingSlotEvent OnSlotCooldownComplete;

		private List<ActiveBlessingData> m_RuntimeActiveBlessings = new List<ActiveBlessingData>(6);

		private StatusEffects m_StatusEffects;

		[Tooltip("Grace frames are the amount of frames that need to pass before the cooldown prevents the blessing from being applied again. Used to allow AOE attacks to apply blessing to every target hit rather than just the first")]
		[SerializeField] int m_CooldownGraceFrames = 15;

		[SerializeField, FoldoutGroup("Achievements")]
		private GameProgressFlagData m_FlagToSetWhenBlessingsAreEquippedInAllSlotsNorn = null;
		[SerializeField, FoldoutGroup("Achievements")]
		private GameProgressFlagData m_FlagToSetWhenBlessingsAreMaxedInAllSlotsNorn = null;

		[SerializeField, FoldoutGroup("Achievements")]
		private GameProgressFlagData m_FlagToSetWhenBlessingsAreEquippedInAllSlotsBoss = null;
		[SerializeField, FoldoutGroup("Achievements")]
		private GameProgressFlagData m_FlagToSetWhenBlessingsAreMaxedInAllSlotsBoss = null;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
		public Blessing TestBlessing;
		public BlessingSlot TestSlot;

		[Button]
		public void TestAddBlessing()
		{
			AddBlessing(TestSlot, TestBlessing);
		}

		[Button]
		public void TestClearBlessings()
		{
			ClearBlessings();
		}
#endif

		private void Awake()
		{
			m_StatusEffects = GetComponent<StatusEffects>();
		}

		private void OnDisable()
		{
			ClearBlessings();
		}
		public Blessing GetBlessingInSlot(BlessingSlot slot)
		{
			ActiveBlessingData data = m_RuntimeActiveBlessings.Find(abd => abd.Slot == slot);
			return (data != null) ? data.Blessing : null;
		}

		public ActiveBlessingData GetActiveBlessingDataInSlot(BlessingSlot slot)
		{
			ActiveBlessingData data = m_RuntimeActiveBlessings.Find(abd => abd.Slot == slot);
			return data;
		}

		public int GetLevelInSlot(BlessingSlot slot)
		{
			ActiveBlessingData data = m_RuntimeActiveBlessings.Find(abd => abd.Slot == slot);
			return (data != null) ? data.Level : 0;
		}

		public void AddBlessing(BlessingSlot slot, Blessing blessing)
		{
			ActiveBlessingData data = m_RuntimeActiveBlessings.Find(abd => abd.Slot == slot);
			if (data != null)
			{
				// Already have a blessing in this slot, check blessing type and replace or increase level appropriately
				if (data.Blessing == blessing)
				{
					++data.Level;
					data.ActiveStatusEffect.OnStatusLevelIncreased();

					BlessingEffectMapping mapping = blessing.Effects.Find(bem => bem.Slot == slot);
					if (GameProgressFlagManager.InstanceCheck())
					{
						if (mapping.FlagSetWhenChosen != null)
						{
							GameProgressFlagManager.Instance.SetFlagsFromData(mapping.FlagSetWhenChosen);
						}
						if (data.Level == mapping.MaxLevel && mapping.FlagSetWhenMaxed != null)
						{
							GameProgressFlagManager.Instance.SetFlagsFromData(mapping.FlagSetWhenMaxed);
						}
					}
				}
				else
				{
					RemoveBlessingInternal(data);
					AddBlessingInternal(slot, blessing, 1);
				}
			}
			else
			{
				AddBlessingInternal(slot, blessing, 1);
			}

			CheckForBlessingRelatedAchievements();
		}

		public void AddBlessingFromSave(BlessingSaveData saveData)
		{
			Blessing blessing = null;
			BlessingSlot slot = null;

			for (int i=0; i<m_BlessingLibrary.Count; ++i)
			{
				if (m_BlessingLibrary[i].DisplayNameTag == saveData.BlessingNameTag)
				{
					blessing = m_BlessingLibrary[i];
					break;
				}
			}

			for (int i=0; i<m_SlotLibrary.Count; ++i)
			{
				if (m_SlotLibrary[i].DisplayNameTag == saveData.SlotNameTag)
				{
					slot = m_SlotLibrary[i];
				}
			}

			if (blessing != null)
			{
				AddBlessingInternal(slot, blessing, saveData.Level);
			}
		}

		private void AddBlessingInternal(BlessingSlot slot, Blessing blessing, int level)
		{
			ActiveBlessingData data = new ActiveBlessingData();
			data.Blessing = blessing;
			data.Slot = slot;
			data.Level = level;

			// Add status effect (if it has one for the slot in question)
			if (m_StatusEffects != null)
			{
				BlessingEffectMapping mapping = blessing.Effects.Find(bem => bem.Slot == slot);
				if (mapping != null)
				{
					BlessingStatusEffectBehaviour.BlessingInitialisationData initData = new BlessingStatusEffectBehaviour.BlessingInitialisationData();
					initData.BlessingData = data;
					data.ActiveStatusEffect = m_StatusEffects.AddStatusEffect(mapping.Effect, null, initData);

					if (GameProgressFlagManager.InstanceCheck() && mapping.FlagSetWhenChosen != null)
					{
						GameProgressFlagManager.Instance.SetFlagsFromData(mapping.FlagSetWhenChosen);
					}
				}
			}
			m_RuntimeActiveBlessings.Add(data);
			//HUD element needs to be updated the lastest so that when the HUD calls GetBlessingInSlot this blessing is there
			SetSlotCooldown(slot, 0);
			if (slot.HudIconVariable != null)
			{
				slot.HudIconVariable.Value = blessing.Source.BlessingPipIcon;
			}

			//load audio for blessing
			if(blessing.AudioBankData == null)
				return;

			foreach (var bank in blessing.AudioBankData.BanksToMount)
			{
				if (blessing.AudioBankData.Async)
				{
					AudioManager.Instance.LoadEventBankAsync(bank.Name, bank.Bank);
				}
				else
				{
					AudioManager.Instance.LoadEventBank(bank.Name, bank.Bank);
				}
			}
		}

		private void RemoveBlessingInternal(ActiveBlessingData data)
		{
			// Clear up any status effect
			if (m_StatusEffects != null)
			{
				if (data.ActiveStatusEffect != null)
				{
					m_StatusEffects.RemoveStatusEffect(data.ActiveStatusEffect);
				}
			}
			if (data.Slot.HudIconVariable != null)
			{
				data.Slot.HudIconVariable.Value = null;
			}
			SetSlotCooldown(data.Slot, 0);
			m_RuntimeActiveBlessings.Remove(data);

			//unload audio for blessing
			if(data.Blessing.AudioBankData == null)
				return;

			foreach (var bank in data.Blessing.AudioBankData.BanksToMount)
			{
				AudioManager.Instance.UnloadEventBank(bank.Name, bank.Bank);
			}
		}

		public void ClearBlessings()
		{
			List<ActiveBlessingData> blessingsToRemove = new List<ActiveBlessingData>(m_RuntimeActiveBlessings);
			foreach (var blessing in blessingsToRemove)
			{
				RemoveBlessingInternal(blessing);
			}
		}

		public List<PrincipalCharacter> GetActivePrincipalCharacters()
		{
			List<PrincipalCharacter> activeCharacters = new List<PrincipalCharacter>();
			for (int i = 0; i < m_RuntimeActiveBlessings.Count; i++)
			{
				PrincipalCharacter character = m_RuntimeActiveBlessings[i].Blessing.Source;
				if (!activeCharacters.Contains(character))
				{
					activeCharacters.Add(character);
				}
			}
			return activeCharacters;
		}

		public List<ActiveBlessingData> GetActiveBlessingDatas()
		{
			return m_RuntimeActiveBlessings;
		}

		public bool IsSlotOnCooldown(BlessingSlot slot, bool useGraceFrames = false)
		{
			ActiveBlessingData data = m_RuntimeActiveBlessings.Find(abd => abd.Slot == slot);
			if (data != null)
			{
				if (useGraceFrames)
				{
					if (data.RemainingGraceFrames > 0)
					{
						return false;
					}
				}
				return data.SlotCooldown > 0;
			}
			else
			{
				Debug.LogWarning("Blessing in slot " + slot.name + " not found");
			}
			return true; //returning true as a failsafe since it will mean in the event of a null value the blessing will act as if it cannot be applied due to cooldown
		}

		public void Update()
		{
			for (int i = 0; i < m_RuntimeActiveBlessings.Count; i++)
			{
				//update all the slot cooldown values so that they tick back down to 0 - 0 means they can be used, higher than that means they are locked
				if (m_RuntimeActiveBlessings[i].SlotCooldown > 0)
				{
					if (m_RuntimeActiveBlessings[i].RemainingGraceFrames > 0)
					{
						m_RuntimeActiveBlessings[i].RemainingGraceFrames--;
					}
					m_RuntimeActiveBlessings[i].SlotCooldown = Mathf.Clamp(m_RuntimeActiveBlessings[i].SlotCooldown -= Time.deltaTime, 0, float.MaxValue);

					if (m_RuntimeActiveBlessings[i].SlotCooldown <= 0.0f && OnSlotCooldownComplete != null)
					{
						OnSlotCooldownComplete(m_RuntimeActiveBlessings[i].Slot);
					}
				}
			}
		}

		public void SetSlotCooldown(BlessingSlot slot, float cooldown)
		{
			ActiveBlessingData data = m_RuntimeActiveBlessings.Find(abd => abd.Slot == slot);
			if (data != null)
			{
				if (data.RemainingGraceFrames == 0)
				{
					//don't want to set the cooldown during the grace period
					data.SlotCooldown = cooldown;
					data.RemainingGraceFrames = cooldown > 0 ? m_CooldownGraceFrames : 0;
					if (OnSlotTriggered != null && cooldown > 0.0f)
					{
						OnSlotTriggered(slot);
					}
				}
			}
			else
			{
				Debug.LogWarning("Blessing in slot " + slot.name + " not found");
			}
		}

		private void CheckForBlessingRelatedAchievements()
		{
			const int k_BossSlotCount = 3;
			const int k_NornSlotCount = 3;

			List<BlessingSlot> slotsWhereBossBlessingsAreEquipped = new List<BlessingSlot>();
			List<PrincipalCharacter> nornsWhosBlessingsAreEquipped = new List<PrincipalCharacter>();

			bool allBossBlessingsAreMaxLevel = true;
			bool allNornBlessingsAreMaxLevel = true;

			for (int b = 0; b < m_RuntimeActiveBlessings.Count; ++b)
			{
				ActiveBlessingData activeBlessingData = m_RuntimeActiveBlessings[b];
				BlessingEffectMapping blessingEffectMapping = activeBlessingData.Blessing.Effects.Find(effect => effect.Slot == activeBlessingData.Slot);

				if (activeBlessingData.Blessing.Source.IsBoss)
				{
					if (!slotsWhereBossBlessingsAreEquipped.Contains(blessingEffectMapping.Slot))
					{
						slotsWhereBossBlessingsAreEquipped.Add(blessingEffectMapping.Slot);
						allBossBlessingsAreMaxLevel = allBossBlessingsAreMaxLevel && activeBlessingData.Level == blessingEffectMapping.MaxLevel;
					}
				}
				else
				{
					if (!nornsWhosBlessingsAreEquipped.Contains(activeBlessingData.Blessing.Source))
					{
						nornsWhosBlessingsAreEquipped.Add(activeBlessingData.Blessing.Source);
						allNornBlessingsAreMaxLevel = allNornBlessingsAreMaxLevel && activeBlessingData.Level == blessingEffectMapping.MaxLevel;
					}
				}
			}

			if (slotsWhereBossBlessingsAreEquipped.Count == k_BossSlotCount)
			{
				GameProgressFlagManager.Instance.SetFlagsFromData(m_FlagToSetWhenBlessingsAreEquippedInAllSlotsBoss);

				if (allBossBlessingsAreMaxLevel)
				{
					GameProgressFlagManager.Instance.SetFlagsFromData(m_FlagToSetWhenBlessingsAreMaxedInAllSlotsBoss);
				}
			}

			if (nornsWhosBlessingsAreEquipped.Count == k_NornSlotCount)
			{
				GameProgressFlagManager.Instance.SetFlagsFromData(m_FlagToSetWhenBlessingsAreEquippedInAllSlotsNorn);

				if (allNornBlessingsAreMaxLevel)
				{
					GameProgressFlagManager.Instance.SetFlagsFromData(m_FlagToSetWhenBlessingsAreMaxedInAllSlotsNorn);
				}
			}
		}
	}
}