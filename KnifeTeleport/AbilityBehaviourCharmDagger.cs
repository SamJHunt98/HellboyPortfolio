using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UABase.Core;
using UATitle.Game;
using Sirenix.OdinInspector;
using UABase.Effects;
using UnityEngine.AI;
public sealed class AbilityBehaviourCharmDagger : AbilityBehaviourBase
{

	[SerializeField] GameObject m_DamagerPrefab = null;
	[SerializeField] DamageData m_DamageData = null;
	[SerializeField] float m_ForwardOffsetDistance = 1.0f;
	[SerializeField] float m_MinTeleportThreshold = 3.0f;
	[SerializeField] float m_MaxTeleportThreshold = 20.0f;

	[SerializeField, AssetsOnly] EffectData m_TeleportStartEffect = null;
	[SerializeField, AssetsOnly] EffectData m_TeleportEndEffect = null;
	public override void Activate()
	{
		if (m_DamagerPrefab != null)
		{
			Vector3 position = transform.position;
			Quaternion rotation = m_Owner.transform.rotation;
			bool canDamage = true;
			if (IsTeleportEnabled())
			{
				Locomotion locomotion = m_Owner.GetComponent<Locomotion>();
				if (locomotion != null)
				{
					if (locomotion.TargetTransform != null)
					{
						float distance = Vector3.Distance(m_Owner.transform.position, (locomotion.TargetTransform.position - new Vector3(0,2,0)));
						if (distance >= m_MinTeleportThreshold)
						{
							if (distance <= m_MaxTeleportThreshold)
							{
								//calculate where hellboy should spawn if this teleport is valid
								Vector3 spawnDirection = Vector3.Normalize(m_Owner.transform.position - (locomotion.TargetTransform.position - new Vector3(0,2,0)));
								//set direction Y to 0 so that it doesn't force us into the ground
								spawnDirection = new Vector3(spawnDirection.x, 0, spawnDirection.z);
								Vector3 positionToSpawn = (locomotion.TargetTransform.position - new Vector3(0,2,0)) + m_MinTeleportThreshold * spawnDirection;

								//check whether there are any obstacles in the place where hellboy should spawn
								Collider[] colliders = {};
								LayerMask layerMask = LayerMask.GetMask("Obstacle");
								colliders = Physics.OverlapSphere(positionToSpawn, 1.0f, layerMask);
								if (colliders.Length > 0)
								{
									//if there are things stopping hellboy from teleporting, do 0 damage and refund most of the cooldown
									canDamage = false;
									m_Ability.SetCooldownTo(2);
								}
								else
								{
									NavMeshPath path = new NavMeshPath();
									NavMesh.CalculatePath(m_Owner.transform.position, positionToSpawn, NavMesh.AllAreas, path);
									if (path.status == NavMeshPathStatus.PathComplete)
									{
										//if valid, make sure that any door triggers passed through are told that they've been entered
										NotifyDoorTriggersOfTeleport(positionToSpawn - m_Owner.transform.position, distance, positionToSpawn);
										//if there is a valid navmesh position at the spawn point, he can play the teleport and deal damage
										EffectInterface.Play(m_TeleportStartEffect, m_Owner, m_Owner.transform.position, spawnDirection);
										m_Owner.transform.position = positionToSpawn;
										EffectInterface.Play(m_TeleportEndEffect, m_Owner, m_Owner.transform.position, spawnDirection);
										position = transform.position;
									}
									else
									{
										//if there is no navmesh spot it should also reset the cooldown
										canDamage = false;
										m_Ability.SetCooldownTo(2);
									}
								}
							}
							else
							{
								//also refund cooldown if you are out of range to teleport - but not if the teleport range was 0
								canDamage = false;
								m_Ability.SetCooldownTo(2);
							}
						}
					}
					rotation = Quaternion.LookRotation(locomotion.GetV3Forward(), Vector3.up);
					position += locomotion.GetV3Forward() * m_ForwardOffsetDistance;
				}
			}
			if (canDamage)
			{
				GameObject charmDamage = UtilsCreate.Create(
									m_DamagerPrefab,
									position,
									rotation,
									m_Owner.transform,
									true);

				Damager damager = charmDamage.GetComponentInChildren<Damager>();
				if (damager != null)
				{
					if (m_DamageData != null)
					{
						damager.SetOverrideDamageData(m_DamageData);
					}
					Vector3 damageDirection = UAMath.BuildV3FromYAngle(UAMath.AngleYFromV3(m_Owner.transform.forward) + m_Ability.GetAbilityData().DamageYAngle);
					damager.SetOverrideForward(damageDirection);
				}
				DamageData data = damager.GetDamageData();
				if (data != null)
				{
					data = m_Ability.GetAbilityData().DamageData;
				}
			}
		}
	}

	bool IsTeleportEnabled()
	{
		if (m_MinTeleportThreshold > 0 && m_MaxTeleportThreshold > m_MinTeleportThreshold)
		{
			return true;
		}
		return false;
	}

	void NotifyDoorTriggersOfTeleport(Vector3 rayDirection, float validDistance, Vector3 endPos)
	{
		List<RoomEntranceManager> roomLinks = RoomEntranceManager.GetAll();
		//keep ray off of ground to avoid it missing a trigger
		Vector3 origin = m_Owner.transform.position + new Vector3(0, 1, 0);
		Ray ray = new Ray(origin, rayDirection.normalized);
		for (int i = 0; i < roomLinks.Count; i++)
		{
			roomLinks[i].TryPassRayThroughRoom(ray, validDistance, endPos);
		}
	}
}
