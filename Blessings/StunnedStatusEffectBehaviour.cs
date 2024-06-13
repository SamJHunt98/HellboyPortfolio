using System;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class StunnedStatusEffectBehaviour : StatusEffectBehaviour
	{
		private StunnedStatusEffectBehaviour m_Template;
		private Health m_Health;
		public override void OnInitialise(StatusEffectBehaviour.InitialisationData initData)
		{
			base.OnInitialise(initData);

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as StunnedStatusEffectBehaviour;
				if (m_Template != null)
				{
					m_Template.m_Health = m_Owner.GetComponent<Health>();
				}
			}
		}

		public override void OnStatusEffectAdded()
		{
			if (m_Template != null && m_Template.m_Health != null)
			{
				m_Template.m_Health.SetCanAdjustArmour(false);
			}
			base.OnStatusEffectAdded();
		}
		[SerializeField] float m_MaxRemainingTimeAfterHit = 1.0f;
		[Tooltip("The time between stun being applied and the player being able to knock them out of it - stops multi-hit effects from instantly breaking them out")]
		[SerializeField] float m_OnHitTimeReductionGracePeriod = 0.5f;

		public override void OnImpactReceived(ref Health.AppliedDamage damage, Health.DamageContext context)
		{
			base.OnImpactReceived(ref damage, context);
			if (damage.Damage > 0)
			{
				if (m_ElapsedTime > m_Template.m_OnHitTimeReductionGracePeriod)
				{
					//Wickedy wack
					if (m_ElapsedTime < Duration - m_Template.m_MaxRemainingTimeAfterHit)
					{
						m_ElapsedTime = Duration - m_Template.m_MaxRemainingTimeAfterHit;
					}
				}
			}
		}

		public override void OnStatusEffectRemoved()
		{
			if (m_Template != null && m_Template.m_Health != null)
			{
				m_Template.m_Health.SetCanAdjustArmour(true);
			}
			base.OnStatusEffectRemoved();
		}
	}
}