using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace UATitle.Game
{
    [Serializable]
	public class TempCurrencyStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[Serializable]
        public struct TempCurrencyLevelData
        {
			public int Level;
			public float AddedFactor; //will be added onto the default factor of 1: e.g a value of 0.5 will give a 1.5x reward
		}

		[TableList]
		public List<TempCurrencyLevelData> FortuneData;

		private Purse m_Purse;
		private float m_AddedFactor;

		private TempCurrencyStatusEffectBehaviour m_Template;

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);
            if(m_Owner != null)
            {
				m_Purse = m_Owner.GetComponentInChildren<Purse>();
			}
            if(StatusEffect != null && StatusEffect.Behaviour != null)
            {
				m_Template = StatusEffect.Behaviour as TempCurrencyStatusEffectBehaviour;
			}
		}

		public override void OnStatusEffectAdded()
		{
			base.OnStatusEffectAdded();

            if(m_Purse != null && m_Template != null)
            {
				int index = m_Template.FortuneData.FindIndex(d => d.Level == Blessing.Level);
                if(index >= 0)
                {
					m_AddedFactor = m_Template.FortuneData[index].AddedFactor;
					m_Purse.TempCurrencyMultiplier.Value += m_AddedFactor;
				}
			}
		}

		public override void OnStatusEffectRemoved()
		{
            if(m_Purse != null)
            {
				m_Purse.TempCurrencyMultiplier.Value -= m_AddedFactor;
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
