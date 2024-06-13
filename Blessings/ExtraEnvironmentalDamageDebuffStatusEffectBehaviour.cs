using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UABase.Effects;
using UABase.Core;

using UnityEngine;
using UABase.Util;
using UATitle.Util;

namespace UATitle.Game
{
    [Serializable]
	public class ExtraEnvironmentalDamageDebuffStatusEffectBehaviour : StatusEffectBehaviour
	{
        [Serializable]
        public struct EnvLevelData
        {
			public int Level;
			public int ExtraDamage;
		}

		[TableList]
		public List<EnvLevelData> LevelData;

		private int m_Level = 0;

		[SerializeField] private float m_Duration = 5;
		private ExtraEnvironmentalDamageDebuffStatusEffectBehaviour m_Template;
		[SerializeField] DamageInstanceType m_EnvironmentalDamageType;

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);

            if(StatusEffect != null && StatusEffect.Behaviour != null)
            {
				m_Template = StatusEffect.Behaviour as ExtraEnvironmentalDamageDebuffStatusEffectBehaviour;
			}

			m_Stacks = 0;
		}

		public override bool AddStack(int level, float stackAmount)
		{
			int lastStack = (int)m_Stacks;
			int maxStacks = 1;

            if(level > m_Level)
            {
				m_Level = level;
			}

            if(m_Stacks < maxStacks)
            {
				m_Stacks += stackAmount;
				m_ElapsedTime = 0.1f;
                if ((int)m_Stacks > lastStack)
                {
                    if (m_Template != null)
                    {
						OnFullyStacked();
						m_ElapsedTime = 0;
						Duration = m_Template.m_Duration;
						return true;
					}
                }
			}
			return false;
		}

		public override void OnPreImpactModification(ref Health.AppliedDamage damage, Health.DamageContext context, Blessings blessings)
		{
            if(m_Template.m_EnvironmentalDamageType != null && context.DamageData.AssociatedDamageType_ == m_Template.m_EnvironmentalDamageType)
            {
                if(m_Template != null)
                {
                    if(m_Template.LevelData != null)
                    {
						int index = m_Template.LevelData.FindIndex(ld => ld.Level == m_Level);
                        if(index >= 0)
                        {
							damage.Damage += m_Template.LevelData[index].ExtraDamage;
							damage.Armor += m_Template.LevelData[index].ExtraDamage;
						}
					}
                }
            }
			base.OnDamageReceived(ref damage, context);
		}

		public override void OnStatusEffectRemoved()
		{
			if((int)m_Stacks > 0)
			{
				base.OnStatusEffectRemoved();
			}
		}
	}
}
