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
	public class ReturnDamageDebuffEffectBehaviour : StatusEffectBehaviour
	{
		[Serializable]
		public struct PunishmentLevelData
		{
			public int Level;
			public float DamagePercentage;
		}

		[TableList]
		public List<PunishmentLevelData> PunishmentData;

		struct DamageInstance
		{
			public int DamageAmount;
			public float TimeApplied;
		}
		List<DamageInstance> DamageToApply = new List<DamageInstance>();

		[SerializeField] float DamageDelay = 0.5f;
		private int m_Level = 0;
		private bool m_EffectStacked = false;
		private ReturnDamageDebuffEffectBehaviour m_Template;

		[SerializeField] float m_EffectDuration = 5;

		[SerializeField] GameObject m_DamagerPrefab = null;

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as ReturnDamageDebuffEffectBehaviour;
			}

			m_Stacks = 0;
		}

		public override void OnDamageGiven(ref Health.AppliedDamage damage, Health.DamageContext context, Blessings blessings)
		{
			//will take a portion of this damage and deal it to myself - will create a damager object with damage data that has
			//zero for knockback stun etc - unless we want it to?
			if (m_Template != null && m_EffectStacked)
			{
				int index = m_Template.PunishmentData.FindIndex(pld => pld.Level == m_Level);
				if (index >= 0)
				{
					if (context.DamageTargetGO == m_Source)
					{
						int returnedDamage = (int)(damage.Damage * m_Template.PunishmentData[index].DamagePercentage);
						if (returnedDamage <= 0 && damage.Damage > 0)
						{
							//we always want at least 1 damage to be returned if the original attack did damage
							returnedDamage = Mathf.Clamp(returnedDamage, 1, int.MaxValue);
						}
						// damage.Damage -= returnedDamage;
						// damage.Armor -= returnedDamage;
						DamageInstance instance = new DamageInstance{DamageAmount = returnedDamage, TimeApplied = m_ElapsedTime};
						DamageToApply.Add(instance);
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

			if (m_Stacks < maxStacks)
			{
				m_Stacks += stackAmount;
				m_ElapsedTime = 0.1f;
				if ((int)m_Stacks >= maxStacks)
				{
					if (m_Template != null)
					{
						m_ElapsedTime = 0f;
						OnFullyStacked();
						Duration = m_Template.m_EffectDuration;
						m_EffectStacked = true;
						return true;
					}
				}
			}
			return false;
		}

		protected override bool OnUpdate(float deltaTime)
		{
			for (int i = DamageToApply.Count - 1; i >= 0; i--)
			{
				if (m_ElapsedTime - DamageToApply[i].TimeApplied >= DamageDelay)
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
								damageData.DamageRef.Value = DamageToApply[i].DamageAmount;
								GameObject reflect = UtilsCreate.Create(
										m_Template.m_DamagerPrefab,
										m_Owner.transform.position + Vector3.up * 2, //hitbox location
										m_Owner.transform.rotation,
										m_Owner.transform,
										true);
							}
							DamageToApply.Remove(DamageToApply[i]);
						}
					}
				}
			}
			return base.OnUpdate(deltaTime);
		}

		public override void OnStatusEffectRemoved()
		{
			//only want it to do this if it is actually stacked upon being removed (avoids bugs with removing effects that haven't been played on time out)
			if (m_EffectStacked)
			{
				for (int i = DamageToApply.Count - 1; i >= 0; i--)
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
								damageData.DamageRef.Value = DamageToApply[i].DamageAmount;
								GameObject reflect = UtilsCreate.Create(
										m_Template.m_DamagerPrefab,
										m_Owner.transform.position + Vector3.up * 2, //hitbox location
										m_Owner.transform.rotation,
										m_Owner.transform,
										true);
							}
							DamageToApply.Remove(DamageToApply[i]);
						}
					}
				}

				base.OnStatusEffectRemoved();
			}
		}
	}
}

