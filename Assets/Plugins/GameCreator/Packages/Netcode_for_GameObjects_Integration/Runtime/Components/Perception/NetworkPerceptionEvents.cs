using System;
using GameCreator.Runtime.Perception;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.Components.Perception
{
    /// <summary>
    /// Global network perception event manager.
    /// Broadcasts perception events that can be listened to by GameCreator visual scripting triggers.
    /// Place this component in your scene (typically on the NetworkManagers GameObject).
    ///
    /// Events:
    /// - Awareness level/stage changes
    /// - Target tracked/untracked
    /// - Noise heard (network-validated)
    /// - Evidence noticed
    /// </summary>
    [AddComponentMenu("Game Creator/Network/Network Perception Events")]
    public class NetworkPerceptionEvents : MonoBehaviour
    {
        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkPerceptionEvents s_Instance;
        public static NetworkPerceptionEvents Instance => s_Instance;

        // EVENTS: Awareness ----------------------------------------------------------------------

        /// <summary>
        /// Fired when a Perception's awareness level changes for any target.
        /// Parameters: (Perception source, GameObject target, float level)
        /// </summary>
        public static event Action<Perception, GameObject, float> EventAwarenessChanged;

        /// <summary>
        /// Fired when a Perception's awareness stage changes for any target.
        /// Parameters: (Perception source, GameObject target, AwareStage stage)
        /// </summary>
        public static event Action<Perception, GameObject, AwareStage> EventAwarenessStageChanged;

        /// <summary>
        /// Fired when a Perception starts tracking a target.
        /// Parameters: (Perception source, GameObject target)
        /// </summary>
        public static event Action<Perception, GameObject> EventTargetTracked;

        /// <summary>
        /// Fired when a Perception stops tracking a target.
        /// Parameters: (Perception source, GameObject target)
        /// </summary>
        public static event Action<Perception, GameObject> EventTargetUntracked;

        // EVENTS: Sensory ------------------------------------------------------------------------

        /// <summary>
        /// Fired when a Perception hears a noise (server-validated).
        /// Parameters: (Perception listener, StimulusNoise noise)
        /// </summary>
        public static event Action<Perception, StimulusNoise> EventNoiseHeard;

        /// <summary>
        /// Fired when a Perception notices evidence.
        /// Parameters: (Perception source, Evidence evidence)
        /// </summary>
        public static event Action<Perception, Evidence> EventEvidenceNoticed;

        // EVENTS: Relay --------------------------------------------------------------------------

        /// <summary>
        /// Fired when awareness is relayed from one Perception to another.
        /// Parameters: (Perception source, Perception receiver)
        /// </summary>
        public static event Action<Perception, Perception> EventAwarenessRelayed;

        /// <summary>
        /// Fired when evidence is relayed from one Perception to another.
        /// Parameters: (Perception source, Perception receiver)
        /// </summary>
        public static event Action<Perception, Perception> EventEvidenceRelayed;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning("[NetworkPerceptionEvents] Duplicate instance detected, destroying");
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            Debug.Log("[NetworkPerceptionEvents] Initialized");
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        // BROADCAST METHODS: Awareness -----------------------------------------------------------

        /// <summary>
        /// Notify all listeners that awareness level changed.
        /// Called by NetworkPerception when server broadcasts awareness changes.
        /// </summary>
        public static void NotifyAwarenessChanged(Perception perception, GameObject target, float level)
        {
            if (perception == null || target == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Awareness changed: {perception.name} -> {target.name} = {level:F2}");
            EventAwarenessChanged?.Invoke(perception, target, level);
        }

        /// <summary>
        /// Notify all listeners that awareness stage changed.
        /// Called by NetworkPerception when server broadcasts stage changes.
        /// </summary>
        public static void NotifyAwarenessStageChanged(Perception perception, GameObject target, AwareStage stage)
        {
            if (perception == null || target == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Stage changed: {perception.name} -> {target.name} = {stage}");
            EventAwarenessStageChanged?.Invoke(perception, target, stage);
        }

        /// <summary>
        /// Notify all listeners that a target is now being tracked.
        /// </summary>
        public static void NotifyTargetTracked(Perception perception, GameObject target)
        {
            if (perception == null || target == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Target tracked: {perception.name} -> {target.name}");
            EventTargetTracked?.Invoke(perception, target);
        }

        /// <summary>
        /// Notify all listeners that a target is no longer being tracked.
        /// </summary>
        public static void NotifyTargetUntracked(Perception perception, GameObject target)
        {
            if (perception == null || target == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Target untracked: {perception.name} -> {target.name}");
            EventTargetUntracked?.Invoke(perception, target);
        }

        // BROADCAST METHODS: Sensory -------------------------------------------------------------

        /// <summary>
        /// Notify all listeners that a noise was heard (server-validated).
        /// </summary>
        public static void NotifyNoiseHeard(Perception perception, StimulusNoise noise)
        {
            if (perception == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Noise heard: {perception.name} tag={noise.Tag ?? "null"}");
            EventNoiseHeard?.Invoke(perception, noise);
        }

        /// <summary>
        /// Notify all listeners that evidence was noticed.
        /// </summary>
        public static void NotifyEvidenceNoticed(Perception perception, Evidence evidence)
        {
            if (perception == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Evidence noticed: {perception.name} -> {evidence?.name ?? "null"}");
            EventEvidenceNoticed?.Invoke(perception, evidence);
        }

        // BROADCAST METHODS: Relay ---------------------------------------------------------------

        /// <summary>
        /// Notify all listeners that awareness was relayed between perceptions.
        /// </summary>
        public static void NotifyAwarenessRelayed(Perception source, Perception receiver)
        {
            if (source == null || receiver == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Awareness relayed: {source.name} -> {receiver.name}");
            EventAwarenessRelayed?.Invoke(source, receiver);
        }

        /// <summary>
        /// Notify all listeners that evidence was relayed between perceptions.
        /// </summary>
        public static void NotifyEvidenceRelayed(Perception source, Perception receiver)
        {
            if (source == null || receiver == null) return;

            Debug.Log($"[NetworkPerceptionEvents] Evidence relayed: {source.name} -> {receiver.name}");
            EventEvidenceRelayed?.Invoke(source, receiver);
        }
    }
}
