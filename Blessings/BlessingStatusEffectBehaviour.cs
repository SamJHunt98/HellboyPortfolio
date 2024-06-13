using System;

using UnityEngine;

namespace UATitle.Game
{
	[Serializable]
	public class BlessingStatusEffectBehaviour : StatusEffectBehaviour
	{
		public class BlessingInitialisationData : StatusEffectBehaviour.InitialisationData
		{
			public ActiveBlessingData BlessingData;
		}

		public ActiveBlessingData Blessing { get; private set; }

		public override void OnInitialise(InitialisationData data)
		{
			base.OnInitialise(data);

			BlessingInitialisationData blessingInitData = data as BlessingInitialisationData;
			Blessing = blessingInitData.BlessingData;
		}
	}
}