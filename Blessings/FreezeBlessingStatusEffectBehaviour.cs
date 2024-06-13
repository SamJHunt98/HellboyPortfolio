using System;

using UnityEngine;

namespace UATitle.Game
{
    [Serializable]
	public class FreezeBlessingStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
		[SerializeField] float m_StackAmount;

        private FreezeBlessingStatusEffectBehaviour m_Template;

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);
            if(StatusEffect != null && StatusEffect.Behaviour != null)
            {
                m_Template = StatusEffect.Behaviour as FreezeBlessingStatusEffectBehaviour;
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
                        FreezeDebuffStatusEffectBehaviour frozen = targetStatusEffects.GetActiveStatusEffect(StatusEffectTypeFreeze.Ref) as FreezeDebuffStatusEffectBehaviour;
                        if(data.blessingInventory != null && frozen == null && !data.blessingInventory.IsSlotOnCooldown(data.blessingSlot, true))
                        {
                            frozen = targetStatusEffects.AddStatusEffectByType(StatusEffectTypeFreeze.Ref, data.source) as FreezeDebuffStatusEffectBehaviour;
                        }

                        if(data.blessingInventory != null && frozen != null && !data.blessingInventory.IsSlotOnCooldown(data.blessingSlot, true))
                        {
                            if(frozen.AddStack(Blessing.Level, m_Template.m_StackAmount))
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
			if(context.DamageData.AssociatedBlessingSlot_ != null && context.DamageData.AssociatedBlessingSlot_ == Blessing.Slot)
            {
                if(context.DamageTargetGO != null && context.DamageTargetGO != context.DamageSourceGO)
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
