using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace UATitle.Game
{
	public class CharmCooldownStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[Serializable]
		public struct CharmCooldownLevelData
		{
			public int Level;
			[Tooltip("Value between 0 and 1 representing the percentage cooldown reduction - e.g 0.2 means 20% lower cooldown")]
			[MinValue(0), MaxValue(1)]public float CooldownFactor;
		}

		[TableList]
		public List<CharmCooldownLevelData> CooldownData;

		private float m_ReducedCooldown = 0;

		private CharmCooldownStatusEffectBehaviour m_Template;

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);
			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as CharmCooldownStatusEffectBehaviour;
			}
		}

		public override void OnStatusEffectAdded()
		{
			base.OnStatusEffectAdded();
			if (m_Template != null)
			{
				int index = m_Template.CooldownData.FindIndex(d => d.Level == Blessing.Level);
				if (index >= 0)
				{
					if (AbilityCooldownFactorRef.Ref != null)
					{
						m_ReducedCooldown = m_Template.CooldownData[index].CooldownFactor;
						AbilityCooldownFactorRef.Ref.Value -= m_Template.CooldownData[index].CooldownFactor;
						if (m_Owner != null)
						{
							Inventory inv = Inventory.Get(m_Owner);
							if (inv.GetLoadoutTotal(LoadoutType.Ability) != 0)
							{
								inv.GetAbility(LoadoutType.Ability).UpdateCooldownMax();
							}
						}
					}
				}
			}
		}

		public override void OnStatusEffectRemoved()
		{
			if (AbilityCooldownFactorRef.Ref != null)
			{
				AbilityCooldownFactorRef.Ref.Value += m_ReducedCooldown;
				if (m_Owner != null)
				{
					Inventory inv = Inventory.Get(m_Owner);
					if (inv.GetLoadoutTotal(LoadoutType.Ability) != 0)
					{
						inv.GetAbility(LoadoutType.Ability).UpdateCooldownMax();
					}
				}
			}
			base.OnStatusEffectRemoved();
		}
	}
}

