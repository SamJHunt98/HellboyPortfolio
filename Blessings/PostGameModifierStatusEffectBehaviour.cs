using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UABase.Core;
using System;

namespace UATitle.Game
{
    [Serializable]
	public class PostGameModifierStatusEffectBehaviour : BlessingStatusEffectBehaviour
	{
        public BoolVariable VariableToSet = null;

        private PostGameModifierStatusEffectBehaviour m_Template;
        public override void OnInitialise(InitialisationData data)
        {
            base.OnInitialise(data);
            if(StatusEffect != null && StatusEffect.Behaviour != null)
            {
                m_Template = StatusEffect.Behaviour as PostGameModifierStatusEffectBehaviour;
            }
        }

        public override void OnStatusEffectAdded()
        {
            base.OnStatusEffectAdded();

            if(m_Template != null)
            {
                if(m_Template.VariableToSet != null)
                {
                    m_Template.VariableToSet.Value = true;
                }
                else
                {
                    Debug.LogError("Variable to set in added Post Game Modifier is null");
                }
            }
        }

        public override void OnStatusEffectRemoved()
        {
            if(m_Template != null)
            {
                if(m_Template.VariableToSet != null)
                {
                    m_Template.VariableToSet.Value = false;
                }
                else
                {
                    Debug.LogError("Variable to set in removed Post Game Modifier is null");
                }
            }
            base.OnStatusEffectRemoved();
        }
	}
}
