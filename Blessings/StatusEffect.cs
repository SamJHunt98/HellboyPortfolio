using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UABase.Core;
using UABase.Effects;

using UnityEngine;

namespace UATitle.Game
{
	public enum StatusEffectDurationMode
	{
		Continual,
		Timed,
		Custom
	}

	[Serializable]
	[CreateAssetMenu(fileName = "SE_NewStatusEffect", menuName = "UA/Status Effects/Create Status Effect", order = 100)]
	public class StatusEffect : ScriptableObject
	{
		[Header("Type")]
		[Tooltip("Type of the status effect")]
		public StatusEffectType EffectType;

		[Header("Behaviour")]
		[Tooltip("Allow override of behaviour for this status effect. If null will use default.")]
		[SerializeReference]
		public StatusEffectBehaviour Behaviour;

		[Tooltip("Is this status effect seen as a good or bad thing to have affecting you")]
		public bool Positive = false;

		// Duration
		public StatusEffectDurationMode DurationMode;
		[ShowIf("DurationMode", StatusEffectDurationMode.Timed)]
		public float Duration;

		public float ApplicationCooldown;
		// Display
		[Header("Display")]
		public string NameTag;
		public string DescriptionTag;
		public SpriteVariable Icon;
		public Color AssociatedColor = Color.white;
		public InspectorEnum<UtilStrings.StringKey> NameStringKey = new InspectorEnum<UtilStrings.StringKey>(UtilStrings.StringKey.SK_NONE);
		public InspectorEnum<UtilStrings.StringKey> SubStringKey = new InspectorEnum<UtilStrings.StringKey>(UtilStrings.StringKey.SK_NONE);

		[Header("Additional Spawning")]
		[AssetsOnly] public GameObject SpawnPrefab = null;

		[Header("Effects")]
		[Tooltip("This will cause the start and running effects to only begin playing once the effect has fully stacked, otherwise it will begin immediately. Useful for status effects applied on-hit, as enemies do not suffer their effects until multiple stacks have been applied.")]
		[SerializeField] public bool EffectOnFullyStacked = false;
		[AssetsOnly] public EffectData StartEffect      = null;
		[AssetsOnly] public EffectData RunningEffect    = null;
		[AssetsOnly] public EffectData EndEffect        = null;

		[Header("AI")]
		public string AIEventStart                      = "";
		public string AIEventEnd                        = "";

		[Header("ActionState")]
		public string ActionStateStart                  = "";
		public string ActionStateEnd                    = "";
		public InspectorEnum<AgentControllerAction> ControllerAction = new InspectorEnum<AgentControllerAction>(AgentControllerAction.None);
	}
}