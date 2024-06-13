using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class MaxBulletCapacityStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[Serializable]
		public struct MaxBulletCapacityLevelData
		{
			public int Level;
			public int ExtraRounds;
			public float ReloadTimeReduction;
		}

		[TableList]
		public List<MaxBulletCapacityLevelData> AmmoData;

		[SerializeField] OnInventoryChangedGameEvent m_ChangedEvent;
		private Firearm m_Firearm;

		private Inventory m_Inventory;
		private int m_AddedRounds = 0;
		private float m_ReducedCooldownMultiplier = 0;

		private MaxBulletCapacityStatusEffectBehaviour m_Template;

		public override void OnInitialise(StatusEffectBehaviour.InitialisationData initData)
		{
			base.OnInitialise(initData);

			if (m_Owner != null)
			{
				m_Inventory = Inventory.Get(m_Owner);
				if (m_Inventory != null)
				{
					m_Firearm = m_Inventory.GetFirearm(LoadoutType.Firearm);
				}			
			}

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as MaxBulletCapacityStatusEffectBehaviour;
			}
		}

		public override void OnStatusEffectAdded()
		{
			base.OnStatusEffectAdded();

			if (m_Firearm != null && m_Template != null)
			{
				m_Template.m_ChangedEvent.RegisterDelegate(OnInventoryChanged);
				int index = m_Template.AmmoData.FindIndex(d => d.Level == Blessing.Level);
				if (index >= 0)
				{
					m_AddedRounds = m_Template.AmmoData[index].ExtraRounds;
					m_ReducedCooldownMultiplier = m_Template.AmmoData[index].ReloadTimeReduction;
					//m_Firearm.m_Data.ClipSize += m_AddedRounds;
					m_Firearm.SetAddionalBullets(m_AddedRounds);
					m_Firearm.AdjustAmmoCount(m_AddedRounds);
					m_Firearm.AdjustReloadMultiplierByMultiplication(m_ReducedCooldownMultiplier);
				}
			}
		}

		public override void OnStatusEffectRemoved()
		{
			if (m_Firearm != null)
			{
				//m_Firearm.m_Data.ClipSize -= m_AddedRounds;
				m_Firearm.SetAddionalBullets(0);
				m_Firearm.AdjustAmmoCount(-m_AddedRounds);
				m_Firearm.AdjustReloadMultiplierByMultiplication(1.0f / m_ReducedCooldownMultiplier);
				m_AddedRounds = 0;
				m_ReducedCooldownMultiplier = 0;
			}
			if (m_Template != null)
			{
				m_Template.m_ChangedEvent.UnregisterDelegate(OnInventoryChanged);
			}
			base.OnStatusEffectRemoved();
		}

		public override void OnStatusLevelIncreased()
		{
			//removes previous upgrade and replaces it with the one for this level
			OnStatusEffectRemoved();
			OnStatusEffectAdded();
		}

		void OnInventoryChanged(InventoryChangedEventData data)
		{
			//will be null first time this is called since the firearm is removed, and then be the new firearm once its added
			Firearm currentFirearm = m_Inventory.GetFirearm(LoadoutType.Firearm);
			if (m_Template != null)
			{

				if (currentFirearm != null && m_Firearm != null)
				{
					if (currentFirearm == m_Firearm)
					{
						//the firearm has not changed! Early out....
						return;
					}
				}

				//remove the bullets from the firearm that's being discarded - only for the post game randomiser
				if (currentFirearm == null && m_Firearm != null)
				{
					int index = m_Template.AmmoData.FindIndex(d => d.Level == Blessing.Level);
					if (index >= 0)
					{
						m_AddedRounds = m_Template.AmmoData[index].ExtraRounds;
						m_ReducedCooldownMultiplier = m_Template.AmmoData[index].ReloadTimeReduction;
						//m_Firearm.m_Data.ClipSize -= m_AddedRounds;
						m_Firearm.SetAddionalBullets(0);
						m_Firearm.AdjustAmmoCount(-m_AddedRounds);
						m_Firearm.AdjustReloadMultiplierByMultiplication(1.0f / m_ReducedCooldownMultiplier);
					}
				}

				//add bullets to newly added firearm
				m_Firearm = currentFirearm;
				if (m_Firearm != null)
				{
					int index = m_Template.AmmoData.FindIndex(d => d.Level == Blessing.Level);
					if (index >= 0)
					{
						m_AddedRounds = m_Template.AmmoData[index].ExtraRounds;
						m_ReducedCooldownMultiplier = m_Template.AmmoData[index].ReloadTimeReduction;
						//m_Firearm.m_Data.ClipSize += m_AddedRounds;
						m_Firearm.SetAddionalBullets(m_AddedRounds);
						m_Firearm.AdjustAmmoCount(m_AddedRounds);
						m_Firearm.AdjustReloadMultiplierByMultiplication(m_ReducedCooldownMultiplier);
					}
				}
			}
		}
	}
}