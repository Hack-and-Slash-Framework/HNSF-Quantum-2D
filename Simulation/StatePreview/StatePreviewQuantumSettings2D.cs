using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class StatePreviewQuantumSettings2D : StatePreviewQuantumSettingsBase
    {
#if QUANTUM_UNITY
        [Header("Preview Actor")]
#endif
        public FPVector2 attackerGroundStartPosition;
        public FPVector2 attackerAerialStartPosition;
        public FPVector2 attackerGroundStartingVelocity;
        public FPVector2 attackerAerialStartingVelocity;
        public bool attackerGroundStartingFlip;
        public bool attackerAerialStartingFlip;
        public ActorInputButtonType attackerInputButtons;
        public FPVector2 attackerInputMovement;
        
#if QUANTUM_UNITY
        [Header("Helping Actor")]
#endif
        public FPVector2 defenderGroundStartPosition;
        public FPVector2 defenderAerialStartPosition;
        public FPVector2 defenderGroundStartingVelocity;
        public FPVector2 defenderAerialStartingVelocity;
        public bool defenderGroundStartingFlip = true;
        public bool defenderAerialStartingFlip = true;
        public ActorInputButtonType defenderInputButtons;
        public FPVector2 defenderInputMovement;
        
        public virtual FPVector2 AttackerGetStartPosition(bool isAerial = false)
        {
            return isAerial ? attackerAerialStartPosition : attackerGroundStartPosition;
        }

        public virtual FPVector2 DefenderGetStartPosition(bool isAerial = false)
        {
            return isAerial ? defenderAerialStartPosition : defenderGroundStartPosition;
        }

        public virtual FPVector2 AttackerGetVelocity(bool isAerial = false)
        {
            return isAerial ? attackerAerialStartingVelocity : attackerGroundStartingVelocity;
        }

        public virtual FPVector2 DefenderGetVelocity(bool isAerial = false)
        {
            return isAerial ? defenderAerialStartingVelocity : defenderGroundStartingVelocity;
        }

        public virtual bool AttackerGetFlipDirection(bool isAerial = false)
        {
            return isAerial ? attackerAerialStartingFlip : attackerGroundStartingFlip;
        }

        public virtual bool DefenderGetFlipDirection(bool isAerial = false)
        {
            return isAerial ? defenderAerialStartingFlip : defenderGroundStartingFlip;
        }
    }
}