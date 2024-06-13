using System;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class StackingBonusDamageBlessingStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[SerializeField]float m_StackAmount;

		private StackingBonusDamageBlessingStatusEffectBehaviour m_Template;

		public override void OnInitialise(StatusEffectBehaviour.InitialisationData data)
		{
			base.OnInitialise(data);
			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as StackingBonusDamageBlessingStatusEffectBehaviour;
			}
		}

		public override void OnTriggered(TriggerData data)
		{
			if (data.blessingSlot != null && data.blessingSlot == Blessing.Slot)
			{
				// Add or refresh Wound debuff on target
				if (data.target != null)
				{
					StatusEffects targetStatusEffects = data.target.GetComponent<StatusEffects>();
					if (targetStatusEffects != null)
					{
						// Check for existing Wound effect
						StackingBonusDamageDebuffStatusEffectBehaviour wounded = targetStatusEffects.GetActiveStatusEffect(StatusEffectTypeStackingBonusDamage.Ref) as StackingBonusDamageDebuffStatusEffectBehaviour;
						if (data.blessingInventory != null && wounded == null && !data.blessingInventory.IsSlotOnCooldown(data.blessingSlot, true))
						{
							wounded = targetStatusEffects.AddStatusEffectByType(StatusEffectTypeStackingBonusDamage.Ref, data.source) as StackingBonusDamageDebuffStatusEffectBehaviour;
						}

						if (data.blessingInventory != null && wounded != null && !data.blessingInventory.IsSlotOnCooldown(data.blessingSlot, true))
						{
							if(wounded.AddStack(Blessing.Level, m_Template.m_StackAmount))
							{
								data.blessingInventory.SetSlotCooldown(data.blessingSlot, ApplicationCooldown);
							}
							++targetStatusEffects.StatusListChangeIndex;
						}
					}
				}
			}
		}
		public override void OnDamageGiven(ref Health.AppliedDamage damage, Health.DamageContext context, Blessings blessings)
		{
			if (context.DamageData.AssociatedBlessingSlot_ != null && context.DamageData.AssociatedBlessingSlot_ == Blessing.Slot)
			{
				// Add or refresh Wound debuff on target
				if (context.DamageTargetGO != null && context.DamageTargetGO != context.DamageSourceGO)
				{
					TriggerData data = new TriggerData()
					{
						damageContext = context,
						target = context.DamageTargetGO,
						blessingSlot = context.DamageData.AssociatedBlessingSlot_,
						source = m_Owner,
						blessingInventory = blessings
				};
					OnTriggered(data);
				}
			}
		}
	}
}