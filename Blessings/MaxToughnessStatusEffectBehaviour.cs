using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class MaxToughnessStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[Serializable]
		public struct MaxToughnessLevelData
		{
			public int Level;
			public int ExtraToughness;
		}

		[TableList]
		public List<MaxToughnessLevelData> ToughnessData;

		private Health m_Health;
		private int m_AddedToughness = 0;

		private MaxToughnessStatusEffectBehaviour m_Template;

		public override void OnInitialise(StatusEffectBehaviour.InitialisationData initData)
		{
			base.OnInitialise(initData);

			if (m_Owner != null)
			{
				m_Health = m_Owner.GetComponent<Health>();
			}

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as MaxToughnessStatusEffectBehaviour;
			}
		}

		public override void OnStatusEffectAdded()
		{
			base.OnStatusEffectAdded();

			if (m_Health != null && m_Template != null)
			{
				int index = m_Template.ToughnessData.FindIndex(d => d.Level == Blessing.Level);
				if (index >= 0)
				{
					m_AddedToughness = m_Template.ToughnessData[index].ExtraToughness;
					m_Health.ArmourStatModifier.ModifyStatDelta(m_AddedToughness);
				}
			}
		}

		public override void OnStatusEffectRemoved()
		{
			if (m_Health != null)
			{
				m_Health.ArmourStatModifier.ModifyStatDelta(-m_AddedToughness);
				m_AddedToughness = 0;
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