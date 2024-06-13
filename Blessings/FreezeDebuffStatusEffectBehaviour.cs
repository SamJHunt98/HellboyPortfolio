using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UABase.Effects;

using UnityEngine;

namespace UATitle.Game
{
	/// <summary>
	/// THIS STATUS EFFECT ESSENTIALLY ONLY CONTROLS WHETHER THE STATUS IS ACTIVE OR NOT - THE FREEZING ITSELF IS NOW DONE VIA AN ACTION STATE
	/// </summary>
	[Serializable]
	public class FreezeDebuffStatusEffectBehaviour : StatusEffectBehaviour
	{
		[Serializable]
		public struct FrozenLevelData
		{
			public int Level;
			public float Duration;
			[Tooltip("The amount of time that has to pass before the explosive effect can be stacked again - stops spamming")]
			public int Cooldown;
		}

		[TableList]
		public List<FrozenLevelData> FrozenData;

		private int m_Level = 0;
		int m_Cooldown = 6;
		private float m_FrozenTime;
		private FreezeDebuffStatusEffectBehaviour m_Template;

		private bool m_EffectIsActive = false;
		private bool m_IsInCooldown = false;

		private Health m_Health = null;

		string m_CooldownEffectID = null;
		[AssetsOnly] public EffectData CooldownEffect = null;

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as FreezeDebuffStatusEffectBehaviour;
			}

			if (m_Owner != null)
			{
				m_Health = m_Owner.GetComponent<Health>();
			}

			m_Stacks = 0;
		}

		public override bool AddStack(int level, float stackAmount)
		{
			int lastStack = (int)m_Stacks;

			if (level > m_Level)
			{
				m_Level = level;
			}

			if (!m_EffectIsActive && !m_IsInCooldown)
			{
				int maxStacks = 1;

				if (m_Stacks < maxStacks)
				{
					m_Stacks += stackAmount;
					Debug.Log("Setting elapsed time back down to 0.1 for our own sake");
					m_ElapsedTime = 0.1f;
					if (m_Template != null && (int)m_Stacks >= maxStacks)
					{
						int index = m_Template.FrozenData.FindIndex(fld => fld.Level == m_Level);
						if (index >= 0)
						{
							m_ElapsedTime = 0.1f;
							m_FrozenTime = m_Template.FrozenData[index].Duration;
							m_Cooldown = m_Template.FrozenData[index].Cooldown;
							Duration = m_FrozenTime + m_Cooldown;
							OnFullyStacked();

							if (m_OwnerActionStateComponent != null)
							{
								//m_OwnerActionStateComponent.RequestPause("FrozenEffect");
								m_EffectIsActive = true;
								m_ElapsedTime = 0;
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		const float k_IgnoreDamageWindow = 0.5f;
		float m_StartHealth = -1.0f;

		protected override bool OnUpdate(float deltaTime)
		{
			if (m_EffectIsActive)
			{
				bool complete = (m_ElapsedTime > m_FrozenTime);

				if (complete)
				{
					RemoveFreezeEffect();
				}
			}
			return base.OnUpdate(deltaTime);
		}

		void RemoveFreezeEffect()
		{
			if (m_OwnerActionStateComponent != null)
			{
				//m_OwnerActionStateComponent.RequestUnPause("FrozenEffect");
				if (m_Template != null)
				{
					m_EffectIsActive = false;
					m_IsInCooldown = true;
					ForceStopRunningEffect(); //turn off the freeze effect on the enemy
					if (m_Template.CooldownEffect != null)
					{
						m_CooldownEffectID = IDGenerator.GenerateUniqueString("FrozenEffect");
						EffectInterface.Play(m_Template.CooldownEffect, m_Owner, m_CooldownEffectID);
					}
					int index = m_Template.FrozenData.FindIndex(fld => fld.Level == m_Level);
					if (index >= 0)
					{
						float cooldown = m_Template.FrozenData[index].Cooldown;
						Duration = cooldown;
					}
				}
			}
			EffectIsActivated = false;
		}

		public override void OnStatusEffectRemoved()
		{
			//Clean up the debuff if the Status Effect ends before the Debuff Duration elapses.
			//This is primarily or when an agent is kileld before the status effect ends.
			if (m_EffectIsActive)
			{
				if (m_OwnerActionStateComponent != null)
				{
					//m_OwnerActionStateComponent.RequestUnPause("FrozenEffect");
				}
				m_EffectIsActive = false;
			}

			if ((int)m_Stacks > 0)
			{
				if (m_Template != null && m_Template.CooldownEffect != null && m_CooldownEffectID != null)
				{
					EffectInterface.Stop(m_CooldownEffectID);
					m_CooldownEffectID = null;
				}
				base.OnStatusEffectRemoved();
			}
		}

		public override void OnImpactReceived(ref Health.AppliedDamage damage, Health.DamageContext context)
		{
			if (m_EffectIsActive && m_ElapsedTime > k_IgnoreDamageWindow)
			{
				RemoveFreezeEffect();
			}
			base.OnImpactReceived(ref damage, context);
		}
	}
}
