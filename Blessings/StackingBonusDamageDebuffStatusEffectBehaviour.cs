using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UABase.Effects;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class
	StackingBonusDamageDebuffStatusEffectBehaviour : StatusEffectBehaviour
	{
		[Serializable]
		public struct WoundLevelData
		{
			public int Level;
			public int ExtraDamagePerStack;
			public int MaxStacks;
			[Tooltip("Factor by which duration is extended (per stack)")]
			public AnimationCurve DurationExtension;
		}

		[TableList]
		public List<WoundLevelData> WoundingData;

		private int m_Level = 0;
		private StackingBonusDamageDebuffStatusEffectBehaviour m_Template;

		[AssetsOnly] public EffectData AddStackEffect       = null;
		private string m_AddStackEffectID = null;

		[SerializeField] float m_EffectDuration = 15;
		[Tooltip("Amount of frames we want between applications of the blessing dealing bonus damage - stops shotgun cheese")]
		[SerializeField] int m_InternalCooldownFrames = 5;
		int m_RemainingCooldownFrames = 0;
		bool m_IsInCooldown = false;
		public override void OnInitialise(StatusEffectBehaviour.InitialisationData initData)
		{
			base.OnInitialise(initData);

			if (StatusEffect != null && StatusEffect.Behaviour != null)
			{
				m_Template = StatusEffect.Behaviour as StackingBonusDamageDebuffStatusEffectBehaviour;
			}

			m_Stacks = 0;
			m_Template.m_IsInCooldown = false;
			m_Template.m_RemainingCooldownFrames = 0;
		}

		public override void OnPreImpactModification(ref Health.AppliedDamage damage, Health.DamageContext context, Blessings blessings)
		{
			int dmgPerStack = 2;

			// Check template wound data
			if (m_Template != null && m_Template.WoundingData != null)
			{
				if (!m_Template.m_IsInCooldown)
				{
					int index = m_Template.WoundingData.FindIndex(wld => wld.Level == m_Level);
					if (index >= 0)
					{
						dmgPerStack = m_Template.WoundingData[index].ExtraDamagePerStack;
						damage.Damage += dmgPerStack * (int)m_Stacks;
						damage.Armor += dmgPerStack * (int)m_Stacks;
					}
					m_Template.m_RemainingCooldownFrames = m_Template.m_InternalCooldownFrames;
					m_Template.m_IsInCooldown = true;
				}
			}
		}

		public override bool AddStack(int level, float stackAmount)
		{
			//used to store the previous stack so we know when a new breakpoint has been hit, meaning we can apply the stack effect
			int lastStack = (int)m_Stacks;

			// Use highest level of blessing that has been applied
			if (level > m_Level)
			{
				m_Level = level;
			}

			// Get template data for that level
			int maxStacks = 3;
			AnimationCurve extensionFactor = null;

			if (m_Template != null)
			{
				int index = m_Template.WoundingData.FindIndex(wld => wld.Level == m_Level);
				if (index >= 0)
				{
					maxStacks = m_Template.WoundingData[index].MaxStacks;
					extensionFactor = m_Template.WoundingData[index].DurationExtension;
				}
			}

			// Limit stacks
			if (m_Stacks < maxStacks)
			{
				m_Stacks += stackAmount;

				if (lastStack == 0 && (int)m_Stacks == 0)
				{
					//if not stacked up yet we want to return the timer to the start
					m_ElapsedTime = 0.1f;
				}
				if (m_Template != null && (int)m_Stacks > lastStack)
				{
					if (lastStack == 0)
					{
						OnFullyStacked();
						Duration = m_Template.m_EffectDuration;
						return true;
					}
					else
					{
						m_AddStackEffectID = Guid.NewGuid().ToString();
						EffectInterface.Play(m_Template.AddStackEffect, m_Owner);
					}
				}
			}

			// Extend remaining time (based on number of stacks, and if it will be an increase over existing duration
			if (extensionFactor != null)
			{
				float extension = Duration * extensionFactor.Evaluate((int)m_Stacks);
				m_ElapsedTime = Mathf.Min(m_ElapsedTime, Duration - extension);
			}
			return false;
		}

		public override void OnStatusEffectRemoved()
		{
			if ((int)m_Stacks > 0)
			{
				//Running Effect
				if (m_AddStackEffectID != null)
				{
					EffectInterface.Stop(m_AddStackEffectID);
				}

				base.OnStatusEffectRemoved();
			}
		}

		protected override bool OnUpdate(float deltaTime)
		{
			if (m_Template.m_IsInCooldown)
			{
				m_RemainingCooldownFrames--;
				if (m_RemainingCooldownFrames <= 0)
				{
					m_Template.m_IsInCooldown = false;
				}
			}
			return base.OnUpdate(deltaTime);
		}
	}
}