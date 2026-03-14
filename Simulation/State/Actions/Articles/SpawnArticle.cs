using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Articles/Spawn Article")]
    public unsafe partial class SpawnArticle : HNSFStateAction
    {
        public AssetRef<EntityPrototype> projectilePrototypeRef;
        public FPVector2 offset;
        public bool articleOnSameTeam = true;
        public bool setSameTargetAsOwner;
        public bool useSameStats = true;
        public StateActionTargetContext targetContext = new StateActionTargetContext();

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            targetContext.callingEntity = entity;
            var positionEntityRef = HNSFStateHelper.GetStateTargetEntity(frame, ref targetContext);
            if (positionEntityRef == EntityRef.None
                || !frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform)) return false;
            actionTargetContext.callingEntity = entity;
            var actionTarget = HNSFStateHelper.GetStateTargetEntity(frame, ref actionTargetContext);
            if (actionTarget == EntityRef.None) return false;

            return DoAction(frame, actionTarget, transform);
        }

        public bool DoAction(Frame frame, EntityRef ownerEntityRef, Transform2D* transformOrigin)
        {
            if (projectilePrototypeRef == default
                || !frame.Unsafe.TryGetPointer<ArticlesOwner>(ownerEntityRef, out var articlesOwner)
                || !frame.TryFindAsset<EntityPrototype>(projectilePrototypeRef.Id, out var projectilePrototype))
                return false;

            var articleEntityRef = articlesOwner->SpawnArticle(frame, ownerEntityRef, projectilePrototypeRef);

            if (frame.Unsafe.TryGetPointer<Transform2D>(articleEntityRef, out var articleTransform))
            {
                articleTransform->Position = transformOrigin->Position;
                articleTransform->Rotation = transformOrigin->Rotation;
                
                if (frame.Unsafe.TryGetPointer<FacingDirection>(articleEntityRef,
                        out var articleFacingDirection)
                    && frame.Unsafe.TryGetPointer<FacingDirection>(ownerEntityRef,
                        out var ownerFacingDirection))
                {
                    articleFacingDirection->isFacingRight = ownerFacingDirection->isFacingRight;
                    articleTransform->Position += articleFacingDirection->TransformDirection(offset);
                }
            }
            
            if (articleOnSameTeam
                && frame.Unsafe.TryGetPointer<CombatTeam>(articleEntityRef, out var articleTeam)
                && frame.Unsafe.TryGetPointer<CombatTeam>(ownerEntityRef, out var selfTeam))
            {
                articleTeam->value = selfTeam->value;
            }

            if (setSameTargetAsOwner
                && frame.Unsafe.TryGetPointer<CombatTargeter>(articleEntityRef, out var articleTargeter)
                && frame.Unsafe.TryGetPointer<CombatTargeter>(ownerEntityRef, out var selfTargeter))
            {
                articleTargeter->softTarget = selfTargeter->targetEntity;
                articleTargeter->targetEntity = selfTargeter->targetEntity;
            }

            if (useSameStats)
            {
                ConfigureArticleStats(frame, ownerEntityRef, articleEntityRef);
            }

            return false;
        }

        partial void ConfigureArticleStats(Frame frame, EntityRef articleOwner, EntityRef article);
        
        public override HNSFStateAction Copy()
        {
            return CopyTo(new SpawnArticle());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SpawnArticle;
            t.projectilePrototypeRef = projectilePrototypeRef;
            t.offset = offset;
            t.articleOnSameTeam = articleOnSameTeam;
            t.setSameTargetAsOwner = setSameTargetAsOwner;
            t.useSameStats = useSameStats;
            t.targetContext = targetContext;
            return base.CopyTo(target);
        }
    }
}