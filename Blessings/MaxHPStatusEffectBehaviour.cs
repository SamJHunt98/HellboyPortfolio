using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class MaxHPStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[Serializable]
		public struct MaxHPLevelData
		{
			public int Level;
			public int ExtraHealth;
		}

		[TableList]
		public List<MaxHPLevelData> HealthData;

		private Health m_Health;
		private int m_AddedHealth = 0;

		private MaxHPStatusEffectBehaviour m_Template;

		public override void OnInitialise(StatusEffectBehaviour.InitialisationData initData)
		{
			base.OnInitialise(initData);

			if (m_Owner != null)
			{
				m_Health = m_Owner.GetComponent<Health>();
			}

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as MaxHPStatusEffectBehaviour;
			}
		}

		public override void OnStatusEffectAdded()
		{
			base.OnStatusEffectAdded();

			if (m_Health != null && m_Template != null)
			{
				int index = m_Template.HealthData.FindIndex(d => d.Level == Blessing.Level);
				if (index >= 0)
				{
					m_AddedHealth = m_Template.HealthData[index].ExtraHealth;
					m_Health.HealthStatModifier.ModifyStatDelta(m_AddedHealth);
				}
			}
		}

		public override void OnStatusEffectRemoved()
		{
			if (m_Health != null)
			{
				m_Health.HealthStatModifier.ModifyStatDelta(-m_AddedHealth);
				m_AddedHealth = 0;
			}

			base.OnStatusEffectRemoved();
		}

		public override void OnStatusLevelIncreased()
		{
			//removes previous upgrade and replaces it with the one for this level
			OnStatusEffectRemoved();
			OnStatusEffectAdded();
		}
	}
}