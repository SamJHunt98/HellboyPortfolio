using System;

using UABase.Core;
using UABase.Effects;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class StatusEffectBehaviour
	{

		public struct TriggerData
		{
			public Health.DamageContext damageContext;
			public BlessingSlot blessingSlot;
			public GameObject target;
			public GameObject source;
			public Blessings blessingInventory;
		}
		public class InitialisationData
		{ }

		// Cached values
		protected StatusEffect m_Effect;
		public StatusEffect StatusEffect { get { return m_Effect; } }

		protected GameObject m_Owner;
		protected GameObject m_Source;
		protected AIAgent m_OwnerAIAgent;
		protected ActionStateComponent m_OwnerActionStateComponent;

		protected float m_ElapsedTime;
		public float ElapsedTime { get { return m_ElapsedTime; } }

		public float Duration { get; set; }
		public float ApplicationCooldown { get; set; }

		protected bool CanReturnDuration = false;
		protected float m_Stacks = -1;

		protected bool EffectIsActivated = false;
		public int StacksInt { get { return (int)m_Stacks; } }
		public float StacksFloat { get { return m_Stacks; } }

		private string m_RunningEffectID;
		private GameObject m_SpawnedObject;

		public void Initialise(StatusEffect effect, GameObject owner, GameObject source, InitialisationData data)
		{
			m_Effect = effect;
			m_Owner = owner;
			m_Source = source;

			if (m_Owner != null)
			{
				m_OwnerAIAgent = m_Owner.GetComponent<AIAgent>();
				m_OwnerActionStateComponent = m_Owner.GetComponent<ActionStateComponent>();
			}

			m_ElapsedTime = 0.0f;

			Duration = effect.Duration;
			ApplicationCooldown = effect.ApplicationCooldown;

			OnInitialise(data);
		}

		public virtual void OnInitialise(InitialisationData data)
		{

		}

		public virtual void OnStatusEffectAdded()
		{
			EffectIsActivated = false;
			if (!StatusEffect.EffectOnFullyStacked)
			{
				// Start Effect
				if (StatusEffect.StartEffect != null)
				{
					EffectInterface.Play(StatusEffect.StartEffect, m_Owner);
				}

				// Running Effect
				if (StatusEffect.RunningEffect != null)
				{
					m_RunningEffectID = Guid.NewGuid().ToString();
					EffectInterface.Play(StatusEffect.RunningEffect, m_Owner, m_RunningEffectID);
				}
			}

			// AI & Actionstate Message
			if (StatusEffect.AIEventStart.Length > 0 && m_OwnerAIAgent != null)
			{
				m_OwnerAIAgent.SendEvent(StatusEffect.AIEventStart);
			}
			if (StatusEffect.ActionStateStart.Length > 0 && m_OwnerActionStateComponent != null)
			{
				m_OwnerActionStateComponent.SetStateByName(StatusEffect.ActionStateStart);
			}
			if (StatusEffect.ControllerAction.Value != AgentControllerAction.None && m_OwnerActionStateComponent != null)
			{
				m_OwnerActionStateComponent.AgentController.SetAction(StatusEffect.ControllerAction.Value, true, true);
			}

			//Spawn Prefabs
			if (StatusEffect.SpawnPrefab != null)
			{
				m_SpawnedObject = UtilsCreate.Create(
						StatusEffect.SpawnPrefab,
						m_Owner.transform.position,
						m_Owner.transform.rotation,
						m_Owner.transform,
						null,
						true);
			}
		}

		public virtual void OnStatusEffectRemoved()
		{
			EffectIsActivated = false;
			//Running Effect
			if (StatusEffect.RunningEffect != null)
			{
				EffectInterface.Stop(m_RunningEffectID);
			}

			//End Effect
			if (StatusEffect.EndEffect != null)
			{
				EffectInterface.Play(StatusEffect.EndEffect, m_Owner);
			}

			//AI & Actionstate Message
			if (StatusEffect.AIEventEnd.Length > 0 && m_OwnerAIAgent != null)
			{
				m_OwnerAIAgent.SendEvent(StatusEffect.AIEventEnd);
			}
			if (StatusEffect.ActionStateEnd.Length > 0 && m_OwnerActionStateComponent != null)
			{
				m_OwnerActionStateComponent.SetStateByName(StatusEffect.ActionStateEnd);
			}
			if (StatusEffect.ControllerAction.Value != AgentControllerAction.None)
			{
				m_OwnerActionStateComponent.AgentController.SetAction(StatusEffect.ControllerAction.Value, false);
			}

			// Disable spawned object
			if (m_SpawnedObject != null)
			{
				m_SpawnedObject.SetActive(false);
				m_SpawnedObject = null;
			}
		}

		public bool Update(float deltaTime)
		{
			m_ElapsedTime += deltaTime;

			bool shouldRemove = OnUpdate(deltaTime);

			if (!shouldRemove && m_Effect.DurationMode == StatusEffectDurationMode.Timed)
			{
				if (m_ElapsedTime >= Duration)
				{
					shouldRemove = true;
				}
			}

			return shouldRemove;
		}

		protected virtual bool OnUpdate(float deltaTime)
		{
			return false;
		}

		// Action states
		public virtual bool BlocksActionState(ActionState state)
		{
			return false;
		}

		public virtual void OnActionStateExit(ActionState oldState)
		{

		}

		public virtual void OnActionStateEnter(ActionState newState)
		{

		}

		// Combat overrides
		public virtual void OnDamageGiven(ref Health.AppliedDamage damage, Health.DamageContext context, Blessings blessings = null)
		{

		}
		public virtual void OnAttackMissed()
		{

		}

		public virtual void OnPreImpactModification(ref Health.AppliedDamage damage, Health.DamageContext context, Blessings blessings = null)
		{

		}

		public virtual void OnImpactReceived(ref Health.AppliedDamage damage, Health.DamageContext context)
		{

		}

		public virtual void OnDamageReceived(ref Health.AppliedDamage damage, Health.DamageContext context)
		{

		}

		public virtual void ApplyStatusEffectToTarget(GameObject target)
		{

		}

		// Targeting
		public virtual bool CanBeTargeted(GameObject targeter)
		{
			return true;
		}

		public virtual void GetEffectiveFactionWhenConsideringTargets(ref Faction faction)
		{

		}

		public virtual void GetEffectiveFactionAsTarget(ref Faction faction)
		{

		}

		public virtual bool AddStack(int level, float stackAmount)
		{
			return false;
		}

		public virtual void OnTriggered(TriggerData data)
		{
		}

		public virtual void OnStatusLevelIncreased()
		{

		}

		public virtual void OnFullyStacked()
		{
			StatusEffects effects = m_Owner.GetComponent<StatusEffects>();
			if (effects != null)
			{
				effects.ActivateNamedEffectDelegates(StatusEffect);
			}
			CanReturnDuration = true;

			EffectIsActivated = true;
			if (StatusEffect.EffectOnFullyStacked)
			{
				// Start Effect
				if (StatusEffect.StartEffect != null)
				{
					EffectInterface.Play(StatusEffect.StartEffect, m_Owner);
				}

				// Running Effect
				if (StatusEffect.RunningEffect != null)
				{
					m_RunningEffectID = Guid.NewGuid().ToString();
					EffectInterface.Play(StatusEffect.RunningEffect, m_Owner, m_RunningEffectID);
				}
			}
		}

		public Sprite GetIconSprite()
		{
			return m_Effect.Icon.Value;
		}

		public Color GetAssociatedColor()
		{
			return m_Effect.AssociatedColor;
		}

		public bool GetCanReturnDuration()
		{
			//used so that we can see whether the duration bar on the UI should be active or not
			return CanReturnDuration;
		}

		public bool GetEffectIsActivated()
		{
			return EffectIsActivated;
		}
		public float GetElapsedDurationPercentage()
		{
			float remaining = (ElapsedTime / Duration) * 100; //progress bars use percentage
			return 100 - remaining;
		}

		public virtual void DelayedInitialise()
		{

		}

		public void ForceStopRunningEffect()
		{
			if (StatusEffect.RunningEffect != null)
			{
				EffectInterface.Stop(m_RunningEffectID);
			}
		}
	}
}