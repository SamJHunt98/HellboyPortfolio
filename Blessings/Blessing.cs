using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UABase.Core;

using UnityEngine;
using UtilStrings;

namespace UATitle.Game
{
	[System.Serializable]
	public class BlessingEffectMapping
	{
		public BlessingSlot Slot;
		public StatusEffect Effect;
		public int MaxLevel;
		public GameProgressFlagData FlagSetWhenChosen = null;
		public GameProgressFlagData FlagSetWhenMaxed = null;
	}

	[System.Serializable]
	public class BlessingDescriptionToLevel
	{
		public InspectorEnum<StringKey> DisplayDescriptionKey;
		public int Level;
	}
	[CreateAssetMenu(
		fileName = "Blessing_NewBlessing",
		menuName = "UA/Blessings/Create Blessing")]
	public class Blessing : ScriptableObject
	{
#if UNITY_EDITOR
		[Multiline]
		public string DeveloperDescription = "";
#endif
		public PrincipalCharacter Source;
		public string DisplayNameTag; //name that will be displayed on the GUI for this blessing
		public string DisplayDescriptionTag; //description that will be displayed

		public InspectorEnum<StringKey> DisplayNameKey;
		public InspectorEnum<StringKey> DisplayDescriptionKey;
		public List<BlessingDescriptionToLevel> LevelSpecificDisplayDescriptionKeys;
		public bool Passive;
		[ShowIf("Passive")]
		public ItemType RequiredItem = null;
		[TableList]
		public List<BlessingEffectMapping> Effects;
		public AudioBankDataObject AudioBankData;

		[Header("POST GAME ONLY")]
		[Tooltip("ONLY USE FOR POST GAME BLESSINGS - WILL OVERRIDE THE ICON SHOWN IN SCREEN")]
		public Sprite OverrideBlessingIcon = null;
		public List<GameProgressFlagData> LevelCompleteProgressFlags;

		public StringKey GetDescriptionKeyAtLevel(int level)
		{
			int index = LevelSpecificDisplayDescriptionKeys.FindIndex(d => d.Level == level);
			if (index >= 0)
			{
				return LevelSpecificDisplayDescriptionKeys[index].DisplayDescriptionKey.Value;
			}
			return DisplayDescriptionKey.Value;
		}
	}
}