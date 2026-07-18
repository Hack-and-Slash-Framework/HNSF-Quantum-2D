using System.Collections.Generic;
using HnSF.core.state;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.systems
{
    public unsafe partial class CombatDirectHitResolverSystem : CombatHitResolverSystem
    {
        public static List<DirectHitInfo> directHits = new List<DirectHitInfo>();
        
        public override void Update(Frame f)
        {
            for (int i = 0; i < directHits.Count; i++)
            {
                DirectDamage(f, directHits[i]);
            }
            
            directHits.Clear();
        }

        public virtual bool DirectDamage(Frame frame, DirectHitInfo info)
        {
            if (info.releaseDefenderFromThrow)
            {
                CombatHelper.ThrowHelper.ReleaseThrowee(frame, info.attackerEntityRef, info.defenderEntityRef);
            }
            
            var result = AttemptHurtActor(
                frame: frame,
                attackerEntityRef: info.attackerEntityRef,
                defenderEntityRef: info.defenderEntityRef,
                attackerHitbox: new Hitbox()
                {
                    active = true,
                    hitInfoRef = info.hitInfoRef.Id,
                    id = 0,
                    owner = info.attackerEntityRef
                },
                attackerHitboxPos: FPVector2.Zero,
                defenderHurtbox: new Hurtbox()
                {
                    active = true,
                    owner = info.defenderEntityRef,
                    id = 0
                },
                attackerState: info.hitByState,
                attackerStateId: info.hitByStateIdentifier,
                ignoreIfAlreadyTouched: false);
            
            if (info.checkForStateChange)
            {
                HNSFStateHelper.Generic.CheckForStateChange(frame, info.defenderEntityRef);
            }
            return result;
        }
    }
}
