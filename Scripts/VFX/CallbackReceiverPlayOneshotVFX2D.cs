using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public unsafe class CallbackReceiverPlayOneshotVFX2D
{
    protected List<IDisposable> _disposableCallbacks = new List<IDisposable>();

    protected Dictionary<EventKey, VisualEffectBase> _unconfirmedVisualEffects = new();

    public QuantumEntityViewUpdater viewUpdater;

    public Dictionary<VisualEffectEntry, ObjectPool<VisualEffectBase>> visualEffectPools = new();
    
    public virtual void Initialize()
    {
        _disposableCallbacks.Add(QuantumCallback.SubscribeManual((CallbackEventCanceled c) => WhenEventCanceled(c)));
        _disposableCallbacks.Add(QuantumCallback.SubscribeManual((CallbackEventConfirmed c) => WhenEventConfirmed(c)));
        _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventPlayVisualEffectAtLocation2D e) => PlayEffectEvent(e)));
        
        _disposableCallbacks.Add(QuantumCallback.SubscribeManual((CallbackEventCanceled c) => WhenStopEventCanceled(c)));
        _disposableCallbacks.Add(QuantumCallback.SubscribeManual((CallbackEventConfirmed c) => WhenStopEventConfirmed(c)));
        _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventStopVisualEffect e) => StopEffectEvent(e)));
    }

    public virtual void Breakdown()
    {
        for (int i = 0; i < _disposableCallbacks.Count; i++)
        {
            _disposableCallbacks[i].Dispose();
        }
        _disposableCallbacks.Clear();
    }
    
    private void WhenEventConfirmed(CallbackEventConfirmed callback)
    {
        _unconfirmedVisualEffects.Remove(callback.EventKey);
    }

    private void WhenEventCanceled(CallbackEventCanceled callback)
    {
        if (!_unconfirmedVisualEffects.ContainsKey(callback.EventKey)) return;
        _unconfirmedVisualEffects[callback.EventKey]?.DestroyEffect();
        _unconfirmedVisualEffects.Remove(callback.EventKey);
    }

    protected virtual void PlayEffectEvent(EventPlayVisualEffectAtLocation2D callback)
    {
        if (callback.visualEffectRef == default) return;
        
        if (viewUpdater == null) viewUpdater = GameObject.FindAnyObjectByType<QuantumEntityViewUpdater>();
        EventKey key = (EventKey)callback;

        var g = callback.Game;
        
        var veAsset = g.Frames.Predicted.FindAsset<VisualEffectEntry>(callback.visualEffectRef.Id);
        var parentEntity = viewUpdater.GetView(callback.parent);

        var basePosition = callback.position.ToUnityVector3();
        if (callback.flipped) basePosition.x *= -1;

        var ve = GetPooledEffect(veAsset);
        ve.transform.position = callback.position.ToUnityVector3();
        ve.transform.eulerAngles = Vector3.zero;
        
        if (!ve.TryGetComponent<VisualEffectBase>(out var veB)) return;
        veB.entryAsset = veAsset;
        _unconfirmedVisualEffects.Add(key, veB);
        
        if (parentEntity)
        {
            var fvp = parentEntity.GetComponent<FighterVisualPositioner>();
            GameObject parentBone = null;
            if (callback.parentBoneTag.IsValid && fvp)
            {
                parentBone = fvp.GetBone(callback.parentBoneTag);
            }

            if (parentBone == null) parentBone = parentEntity.gameObject;
            
            var effectPosition = callback.positionAsOffset
                ? parentBone.transform.position + parentBone.transform.TransformVector(basePosition)
                : basePosition;

            if (callback.atClosestBodyPosition && fvp != null)
            {
                effectPosition = fvp.GetClosestVisualPositionByYNoXZ(callback.sourcePosition.ToUnityVector3());
            }
            
            var rot = 0;
            if (veAsset.flipType == VisualEffectEntry.FlipType.Rotation && callback.flipped) rot = -180;
            ve.transform.SetPositionAndRotation(effectPosition, Quaternion.Euler(new Vector3(0, rot, 0)));
            if(veAsset.flipType == VisualEffectEntry.FlipType.Scale && callback.flipped) ve.transform.localScale = new Vector3(ve.transform.localScale.x * -1, ve.transform.localScale.y, ve.transform.localScale.z);
            
            if (callback.setRotationToForceDir &&
                g.Frames.Predicted.Unsafe.TryGetPointer<BattleActorPhysics>(callback.parent, out var cphy))
            {
                ve.transform.rotation = Quaternion.LookRotation(Vector3.forward, cphy->force.ToUnityVector3().normalized);
                var angles = ve.transform.eulerAngles;
                angles.z += 90;
                ve.transform.eulerAngles = angles;
            }
            
            ve.transform.position = effectPosition;
            
            if (callback.parented)
            {
                ve.transform.SetParent(parentBone.transform, true);
            }

            var parentEntityVFXManager = parentEntity.GetComponent<FighterVFXManager>();
            if (parentEntityVFXManager)
            {
                parentEntityVFXManager.Play(veAsset, veB);
                return;
            }
        }

        veB.Play();
    }
    
    private void WhenStopEventConfirmed(CallbackEventConfirmed callback)
    {
    }

    private void WhenStopEventCanceled(CallbackEventCanceled callback)
    {
    }
    
    protected virtual void StopEffectEvent(EventStopVisualEffect callback)
    {
        if (viewUpdater == null) viewUpdater = GameObject.FindAnyObjectByType<QuantumEntityViewUpdater>();
        EventKey key = (EventKey)callback;
        
        var g = callback.Game;
        var parentEntity = viewUpdater.GetView(callback.parent);

        if (parentEntity)
        {
            var parentEntityVFXManager = parentEntity.GetComponent<FighterVFXManager>();
            if (parentEntityVFXManager)
            {
                if (callback.stopAllInstances)
                {
                    parentEntityVFXManager.StopAllEffectsOfType(callback.effectToStop, callback.destroyAllParticles);
                }
                else
                {
                    parentEntityVFXManager.StopEffect(callback.effectToStop, callback.offset,
                        callback.destroyAllParticles, callback.unparent);
                }
            }
        }
    }

    protected virtual GameObject GetPooledEffect(VisualEffectEntry entryAsset)
    {
        if (entryAsset == null) return null;
        InitializePool(entryAsset);
        return visualEffectPools[entryAsset].Get().gameObject;
    }

     protected virtual void InitializePool(VisualEffectEntry entryAsset)
    {
        if (visualEffectPools.ContainsKey(entryAsset)) return;
        visualEffectPools.Add(entryAsset, new ObjectPool<VisualEffectBase>(
            createFunc: () => GameObject.Instantiate(entryAsset.visualEffect).GetComponent<VisualEffectBase>(),
            actionOnGet: (ve) => ReinitializeVisualEffect(entryAsset, ve),
            actionOnRelease: ReleaseVisualEffect,
            actionOnDestroy: (ve) =>
            {
                if (ve == null) return;
                GameObject.Destroy(ve.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 50
        ));
    }
    
    protected virtual void ReinitializeVisualEffect(VisualEffectEntry entryAsset,  VisualEffectBase ve)
    {
        ve.sourcePool = visualEffectPools[entryAsset];
        ve.Reinitialize();
        ve.gameObject.SetActive(true);
    }
    
    protected virtual void ReleaseVisualEffect(VisualEffectBase ve)
    {
        ve.transform.SetParent(null, false);
        ve.transform.localScale = new Vector3(1, 1, 1);
        ve.transform.eulerAngles = Vector3.zero;
        ve.Stop(true);
        ve.gameObject.SetActive(false);
        ve.sourcePool = null;
    }
}

