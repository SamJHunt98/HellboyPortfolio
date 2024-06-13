using System;
using System.Collections.Generic;
using System.Text;

using Sirenix.OdinInspector;

using UABase.Core;
using UABase.Effects;

using UATitle.Game;
using UATitle.Util;

using UnityEngine;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UABase.Debugging;
#endif //DEVELOPMENT_BUILD || UNITY_EDITOR

/// <summary>
/// Allows this object to be effected by status effects
/// </summary>
namespace UATitle.Game
{
	public class StatusEffects : StaticDictionaryBase<StatusEffects>
	{
		[Tooltip("Set of status effects that can be applied and referenced by status effect type on this character")]
		public StatusEffectList ValidEffects;

		[HideInInspector]
		List<StatusEffectBehaviour> ActiveStatusEffects = new List<StatusEffectBehaviour>();

		List<StatusEffectBehaviour> LateInitialiseStatusEffects = new List<StatusEffectBehaviour>();

		public int StatusListChangeIndex = 0; //used as a comparison point to check whether the ActiveStatusEffects list has changed - incremented whenever something is added or removed

        //used to add text to the screen to show when a status has been applied
		public delegate void OnNamedEffectAdded(UtilStrings.StringKey name, UtilStrings.StringKey substring);
		OnNamedEffectAdded m_EffectDelegates = null;

		Blessings m_BlessingInventory = null;
		Health m_Health = null;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
		public StatusEffect TestStatusEffect;

		[Button]
		public void TestAddStatusEffect()
		{
			AddStatusEffect(TestStatusEffect, null, null);
		}

		[Button]
		public void TestRemoveStatusEffects()
		{
			ClearEverything();
		}
#endif

		[Button]
		public void AddStun()
		{
			AddStatusEffectByType(StatusEffectTypeStun.Ref, null);
		}

		[Button]
		public void RemoveStun()
		{
			var stun = GetActiveStatusEffect(StatusEffectTypeStun.Ref);
			if (stun != null)
			{
				RemoveStatusEffect(stun);
			}
		}

		// [Button]
		// public void AddBleed()
		// {
		//     ApplyStatusEffect(StatusEffectType.Bleed, 10);
		// }

		// [Button]
		// public void AddFreeze()
		// {
		//     ApplyStatusEffect(StatusEffectType.Freeze, 10);
		// }

		// [Button]
		// public void AddPlague()
		// {
		//     ApplyStatusEffect(StatusEffectType.Plague, 10);
		// }

		// [Button]
		// public void AddFire()
		// {
		//     ApplyStatusEffect(StatusEffectType.Fire, 10);
		// }

		private void Awake()
		{
			m_Health = GetComponent<Health>();
			m_BlessingInventory = GetComponent<Blessings>();
		}

		private void OnEnable()
		{
			if (m_Health != null)
			{
				m_Health.AddImpactDelegate(OnImpactReceived);
				m_Health.AddHurtDelegate(OnDamageReceived);
				m_Health.AddKilledDelegate(NotifyKilled);
				m_Health.AddPreImpactModifyDamageDelegate(OnPreImpactModification);
			}

			BaseAdd();
		}

		private void OnDisable()
		{
			if (m_Health != null)
			{
				m_Health.RemoveImpactDelegate(OnImpactReceived);
				m_Health.RemoveHurtDelegate(OnDamageReceived);
				m_Health.RemoveKilledDelegate(NotifyKilled);
				m_Health.RemovePreImpactModifyDamageDelegate(OnPreImpactModification);
			}

			ClearEverything();
			BaseRemove();
		}

		public void NotifyKilled(ref Health.AppliedDamage appliedDamage, Health.DamageContext damageContext)
		{
			ClearEverything();
		}

		public StatusEffectBehaviour AddStatusEffectByType(StatusEffectType statusEffectType, GameObject source)
		{
			StatusEffectBehaviour behaviour = null;
			int effectIndex = ValidEffects.Value.FindIndex(se => se.EffectType == statusEffectType);
			if (effectIndex != -1)
			{
				behaviour = AddStatusEffect(ValidEffects[effectIndex], source, null);
			}
			return behaviour;
		}

		public StatusEffectBehaviour AddStatusEffect(StatusEffect statusEffect, GameObject source, StatusEffectBehaviour.InitialisationData initData)
		{
			StatusEffectBehaviour addedBehaviour = null;
			StatusEffectBehaviour definedBehaviour = statusEffect.Behaviour;
			if (definedBehaviour != null)
			{
				addedBehaviour = (StatusEffectBehaviour)Activator.CreateInstance(definedBehaviour.GetType());
			}
			else
			{
				addedBehaviour = new StatusEffectBehaviour();
			}

			if (addedBehaviour != null)
			{
				addedBehaviour.Initialise(statusEffect, gameObject, source, initData);
				ActiveStatusEffects.Add(addedBehaviour);
				addedBehaviour.OnStatusEffectAdded();
				LateInitialiseStatusEffects.Add(addedBehaviour);
			}
			++StatusListChangeIndex;
			return addedBehaviour;
		}

		public IEnumerable<StatusEffectBehaviour> GetActiveStatusEffects()
		{
			return ActiveStatusEffects;
		}

		public bool HasActiveStatusEffect(StatusEffectType effectType)
		{
			int index = ActiveStatusEffects.FindIndex(se => se.StatusEffect.EffectType == effectType);
			return index >= 0;
		}

		public bool HasActivatedStatusEffectOfType(StatusEffectType effectType)
		{
			StatusEffectBehaviour behaviour = GetActiveStatusEffect(effectType);
			if (behaviour != null)
			{
				return behaviour.GetEffectIsActivated();
			}
			return false;
		}
		public StatusEffectBehaviour GetActiveStatusEffect(StatusEffectType effectType)
		{
			return ActiveStatusEffects.Find(se => se.StatusEffect.EffectType == effectType);
		}

		public void RemoveStatusEffect(StatusEffectBehaviour seb)
		{
			if (ActiveStatusEffects.Contains(seb))
			{
				ActiveStatusEffects.Remove(seb);
				--StatusListChangeIndex;
				seb.OnStatusEffectRemoved();
			}
		}

		public void OnPreImpactModification(ref Health.AppliedDamage damage, Health.DamageContext context)
		{
			foreach (var effect in ActiveStatusEffects)
			{
				effect.OnPreImpactModification(ref damage, context, m_BlessingInventory);
			}
		}
		public void OnImpactReceived(ref Health.AppliedDamage damage, Health.DamageContext context)
		{
			foreach (var effect in ActiveStatusEffects)
			{
				effect.OnImpactReceived(ref damage, context);
			}
		}

		public void OnDamageGiven(ref Health.AppliedDamage damage, Health.DamageContext context)
		{
			foreach (var effect in ActiveStatusEffects)
			{
				effect.OnDamageGiven(ref damage, context, m_BlessingInventory);
			}
		}

		public void OnDamageReceived(ref Health.AppliedDamage damage, Health.DamageContext context)
		{
			foreach (var effect in ActiveStatusEffects)
			{
				effect.OnDamageReceived(ref damage, context);
			}
		}

		public void OnAttackMissed()
		{
			foreach (var effect in ActiveStatusEffects)
			{
				effect.OnAttackMissed();
			}
		}

		public void TriggerAllEffects(StatusEffectBehaviour.TriggerData data)
		{
			foreach (var effect in ActiveStatusEffects)
			{
				BlessingStatusEffectBehaviour status = effect as BlessingStatusEffectBehaviour;
				if (status != null && status.Blessing.Slot == data.blessingSlot)
				{
					effect.OnTriggered(data);
				}
			}
		}

		public bool IsActionStateBlocked(ActionState state)
		{
			foreach(var effect in ActiveStatusEffects)
			{
				if(effect.BlocksActionState(state))
				{
					return true;
				}
			}
			return false;
		}

		public void OnActionStateEnter(ActionState state)
		{
			foreach (var effect in ActiveStatusEffects)
			{
				effect.OnActionStateEnter(state);
			}
		}

		public void OnActionStateExit(ActionState state)
		{
			foreach (var effect in ActiveStatusEffects)
			{
				effect.OnActionStateExit(state);
			}
		}

		public void ActivateNamedEffectDelegates(StatusEffect effect)
		{
			if(m_EffectDelegates != null)
			{
				m_EffectDelegates(effect.NameStringKey.Value, effect.SubStringKey.Value);
			}
		}
		public void AddNamedEffectDelegate(OnNamedEffectAdded effectDelegate)
		{
			m_EffectDelegates -= effectDelegate;
			m_EffectDelegates += effectDelegate;
		}

		public void RemoveNamedEffectDelegate(OnNamedEffectAdded effectDelegate)
		{
			m_EffectDelegates -= effectDelegate;
		}

		public int GetActivatedStatusEffectCount()
		{
			int activatedEffects = 0;
			for (int i = 0; i < ActiveStatusEffects.Count; i++)
			{
				if (ActiveStatusEffects[i].GetEffectIsActivated())
				{
					activatedEffects++;
				}
			}
			return activatedEffects;
		}

		void Update()
		{
			float deltaTime  = Time.deltaTime;
			for(int i = LateInitialiseStatusEffects.Count - 1; i >= 0; i--)
			{
				LateInitialiseStatusEffects[i].DelayedInitialise();
				LateInitialiseStatusEffects.RemoveAt(i);
			}
			List<StatusEffectBehaviour> effectsToRemove = new List<StatusEffectBehaviour>();
			foreach (var effect in ActiveStatusEffects)
			{
				if (effect.Update(deltaTime))
				{
					effectsToRemove.Add(effect);
				}
			}

			foreach (var effect in effectsToRemove)
			{
				RemoveStatusEffect(effect);
			}

#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (DebugFlags.Instance.GetFlag("Components/StatusEffects"))
			{
				DebugDrawStatusEffects();
			}
#endif //DEVELOPMENT_BUILD || UNITY_EDITOR
		}

		void ClearEverything()
		{
			List<StatusEffectBehaviour> effectsToRemove = new List<StatusEffectBehaviour>(ActiveStatusEffects);
			foreach (var effect in effectsToRemove)
			{
				RemoveStatusEffect(effect);
			}
		}

#if DEVELOPMENT_BUILD || UNITY_EDITOR
		StringBuilder _SB = new StringBuilder();
		void DebugDrawStatusEffects()
		{
			_SB.Length = 0;
			_SB.Append("Status Effects\n");

			for (int a = 0; a < ActiveStatusEffects.Count; a++)
			{
				var effect = ActiveStatusEffects[a];
				if (effect != null && effect.StatusEffect != null)
				{
					_SB.Append(effect.StatusEffect.name);
					_SB.Append(" - ");
					_SB.Append(effect.StatusEffect.EffectType != null ? effect.StatusEffect.EffectType.ToString() : "untyped");
					if (effect.StatusEffect.DurationMode == StatusEffectDurationMode.Timed)
					{
						_SB.Append(": ");
						_SB.Append(effect.StatusEffect.Duration - effect.ElapsedTime);
					}
				}
				else
				{
					_SB.Append("null status effect");
				}
			}
			UtilsDebug.DrawText(
				transform.position + Vector3.up * 1.8f,
				_SB.ToString(),
				Color.cyan);
		}
#endif //DEVELOPMENT_BUILD || UNITY_EDITOR
	}
}