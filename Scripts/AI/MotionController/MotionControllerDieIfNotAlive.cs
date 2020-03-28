﻿using com.ootii.Actors.AnimationControllers;
using com.ootii.Actors.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NeoFPS.AI.OotiiMotionController
{
    /// <summary>
    /// Checks to see if the Neo FPS character is alive. If it is do nothing otherwise play the
    /// death animation and destroy the object.
    /// </summary>
    [CreateAssetMenu(fileName = "DieIfNotAlive", menuName = "NeoFPS/AI/IsAlive")]
    public class MotionControllerDieIfNotAlive : AIBehaviour
    {
        [SerializeField, Tooltip("The time to delay before destroying the object.")]
        float m_DestroyDelay = 5;

        IHealthManager m_HealthManager;
        MotionController m_MotionController;

        internal override bool Init(GameObject owner)
        {
            // REFACTOR: this could also do this by subscribing to the health manager's onIsAliveChanged event, which is probably a more robust approach?
            m_IsActive = base.Init(owner);
            m_HealthManager = owner.GetComponent<IHealthManager>();
            m_IsActive &= m_HealthManager != null;
            Debug.Assert(m_IsActive, owner + " has an DieIfNotAlive behaviour but no IHealthManager component.");

            m_MotionController = m_Owner.GetComponent<MotionController>();
            m_IsActive &= m_MotionController != null;
            Debug.Assert(m_IsActive, owner + " has an DieIfNotAlive behaviour but no motionController component.");

            return m_IsActive;
        }

        internal override string Tick()
        {
            if (m_HealthManager.isAlive)
            {
                return "Agent is alive.";
            }

            NavMeshAgent agent = m_Owner.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
            }

            CombatMessage message = CombatMessage.Allocate();
            if (message != null)
            {
                message.ID = 1108; // DEATH
                message.StyleIndex = -1; // Random
                message.Defender = m_Owner;

                m_MotionController.SendMessage(message);
            }

            Destroy(m_Owner, m_DestroyDelay);

            StopAllBehaviourControllers();

            return "";
        }

        /// <summary>
        /// Disable all Behaviour Controllers so that no more behaviours will be
        /// triggered.
        /// </summary>
        private void StopAllBehaviourControllers()
        {
            BasicAIController[] controllers = m_Owner.GetComponents<BasicAIController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].m_IsActive = false;
            }
        }
    }
}