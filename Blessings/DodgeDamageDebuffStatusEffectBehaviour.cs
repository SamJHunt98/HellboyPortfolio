using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UABase.Effects;

using UnityEngine;
using UABase.Core;
using UABase.Util;
using UATitle.Util;

namespace UATitle.Game
{
	[Serializable]
	public class DodgeDamageDebuffStatusEffectBehaviour : StatusEffectBehaviour
	{
		[Serializable]
		public struct ClumsyLevelData
		{
			public int Level;
			public int DamageAmount;
			public float Duration;
		}

		[TableList]
		public List<ClumsyLevelData> ClumsyData;

		private int m_Level = 0;
		private bool m_EffectStacked = false;
		private DodgeDamageDebuffStatusEffectBehaviour m_Template;

		[SerializeField] float m_MaxDistance = 3;

		[SerializeField] GameObject m_DamagerPrefab = null;


		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as DodgeDamageDebuffStatusEffectBehaviour;
			}

			m_Stacks = 0;
		}

		public override void OnAttackMissed()
		{
			if (m_Template != null && m_EffectStacked)
			{
				//don't want it to be used if you're far out of range - should focus on dodging
				if (Vector3.Distance(m_Owner.transform.position, m_Source.transform.position) < m_Template.m_MaxDistance)
				{
					int index = m_Template.ClumsyData.FindIndex(wld => wld.Level == m_Level);
					if (index >= 0)
					{
						if (m_Template.m_DamagerPrefab != null)
						{
							Damager damager = m_Template.m_DamagerPrefab.GetComponentInChildren<Damager>();
							if (damager != null)
							{
								DamageData damageData = damager.GetDamageData();
								Locomotion loc = m_Owner.GetComponent<Locomotion>();
								Vector3 forward = Vector3.zero;
								if (loc != null)
								{
									forward = loc.V3Forward;
								}

								if (damageData != null)
								{
									damageData.DamageRef.Value = m_Template.ClumsyData[index].DamageAmount;
									GameObject reflect = UtilsCreate.Create(
										m_Template.m_DamagerPrefab,
										m_Owner.transform.position + Vector3.up * 2, //hitbox location
										m_Owner.transform.rotation,
										m_Owner.transform,
										true);
								}
							}
						}
					}
				}
			}
		}

		public override bool AddStack(int level, float stackAmount)
		{
			int lastStack = (int)m_Stacks;

			if (level > m_Level)
			{
				m_Level = level;
			}

			int maxStacks = 1;

			if ((int)m_Stacks < maxStacks)
			{
				m_Stacks += stackAmount;
				m_ElapsedTime = 0.1f;
				if (m_Stacks >= maxStacks)
				{
					if (m_Template != null)
					{
						m_ElapsedTime = 0f;
						int index = m_Template.ClumsyData.FindIndex(wld => wld.Level == m_Level);
						if(index >= 0)
						{
							Duration = m_Template.ClumsyData[index].Duration;
						}
						OnFullyStacked();
						m_EffectStacked = true;
						return true;
					}
				}
			}
			return false;
		}

		public override void OnStatusEffectRemoved()
		{
			if(m_EffectStacked)
			{
				base.OnStatusEffectRemoved();
			}
		}
	}
}
