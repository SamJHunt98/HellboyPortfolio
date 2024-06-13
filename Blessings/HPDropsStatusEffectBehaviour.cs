using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UABase.Core;

namespace UATitle.Game
{
    [Serializable]
	public class HPDropsStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
        [Serializable]
        public struct HPDropsLevelData
        {
			public int Level;
			[Tooltip("Percentage chance of health prefabs dropping: e.g 0.3 would mean a 30% chance")]
			[MinValue(0), MaxValue(1.0f)]public float AddedFactor; //will be added onto the default factor of 0
			[Tooltip("1 pip = 4 health")]
			public int HealingAmount;
		}

		[TableList]
		public List<HPDropsLevelData> HealthData;

		[SerializeField] FloatVariable m_DropChance;
		[SerializeField] IntVariable m_HealingAmount;
		private float m_AddedFactor;

		private HPDropsStatusEffectBehaviour m_Template;

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);
            
            if(StatusEffect != null && StatusEffect.Behaviour != null)
            {
				m_Template = StatusEffect.Behaviour as HPDropsStatusEffectBehaviour;
			}
		}

		public override void OnStatusEffectAdded()
		{
			base.OnStatusEffectAdded();
            if(m_Template != null)
            {
				int index = m_Template.HealthData.FindIndex(d => d.Level == Blessing.Level);
                if(index >= 0)
                {
					m_AddedFactor = m_Template.HealthData[index].AddedFactor;
                    if(m_Template.m_DropChance != null)
                    {
						m_Template.m_DropChance.Value += m_AddedFactor;
					}
					if(m_Template.m_HealingAmount != null)
					{
						m_Template.m_HealingAmount.Value = m_Template.HealthData[index].HealingAmount;
					}
				}
			}
		}

		public override void OnStatusEffectRemoved()
		{
            if(m_Template.m_DropChance != null)
            {
				m_Template.m_DropChance.Value -= m_AddedFactor;
				m_AddedFactor = 0;
			}
			base.OnStatusEffectRemoved();
		}

        public override void OnStatusLevelIncreased()
		{
			OnStatusEffectRemoved();
			OnStatusEffectAdded();
		}
	}
}