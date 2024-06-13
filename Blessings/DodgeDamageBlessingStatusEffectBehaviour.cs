using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace UATitle.Game
{
    [Serializable]
	public class DodgeDamageBlessingStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[SerializeField] float m_StackAmount;

		private DodgeDamageBlessingStatusEffectBehaviour m_Template;

        public override void OnInitialise(InitialisationData data)
        {
			base.OnInitialise(data);
            if(StatusEffect != null && StatusEffect.Behaviour != null)
            {
				m_Template = StatusEffect.Behaviour as DodgeDamageBlessingStatusEffectBehaviour;
			}
		}

		public override void OnTriggered(TriggerData data)
		{
            if(data.blessingSlot != null && data.blessingSlot == Blessing.Slot)
            {
                if(data.target != null)
                {
					StatusEffects targetStatusEffects = data.target.GetComponent<StatusEffects>();
                    if(targetStatusEffects != null)
                    {
						DodgeDamageDebuffStatusEffectBehaviour clumsy = targetStatusEffects.GetActiveStatusEffect(StatusEffectTypeDodgeDamage.Ref) as DodgeDamageDebuffStatusEffectBehaviour;
                        if(data.blessingInventory != null && clumsy == null && !data.blessingInventory.IsSlotOnCooldown(data.blessingSlot, true))
                        {
							clumsy = targetStatusEffects.AddStatusEffectByType(StatusEffectTypeDodgeDamage.Ref, data.source) as DodgeDamageDebuffStatusEffectBehaviour;
						}

                        if(data.blessingInventory != null && clumsy != null && !data.blessingInventory.IsSlotOnCooldown(data.blessingSlot, true))
                        {
							if(clumsy.AddStack(Blessing.Level, m_Template.m_StackAmount))
							{
								data.blessingInventory.SetSlotCooldown(data.blessingSlot, ApplicationCooldown);
							}
							++targetStatusEffects.StatusListChangeIndex;
						}
					}
				}
            }
			base.OnTriggered(data);
		}

		public override void OnDamageGiven(ref Health.AppliedDamage damage, Health.DamageContext context, Blessings blessings)
		{
            if (context.DamageData.AssociatedBlessingSlot_ != null && context.DamageData.AssociatedBlessingSlot_ == Blessing.Slot)
			{
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
