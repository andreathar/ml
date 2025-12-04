using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Emit Network Noise")]
    [Description(
        "Emits a Noise Stimulus that is synchronized across all connected clients. "
            + "NPCs on all machines will hear this noise. Use this instead of 'Emit Noise' for multiplayer games."
    )]
    [Category("Network/Perception/Emit Network Noise")]
    [Image(typeof(IconNoise), ColorTheme.Type.Green)]
    [Parameter("Position", "The world position where the noise is emitted")]
    [Parameter("Radius", "The radius within which the noise can be heard")]
    [Parameter("Tag", "The identifier tag for this noise type (e.g., 'footstep', 'gunshot')")]
    [Parameter("Intensity", "The strength of the noise (0-10)")]
    [Keywords("Network", "Multiplayer", "Sound", "Noise", "Distract", "Alert", "Aural", "Hear")]
    [Serializable]
    public class InstructionNetworkEmitNoise : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetDecimal m_Radius = GetDecimalDecimal.Create(10f);

        [SerializeField]
        private PropertyGetString m_Tag = GetStringId.Create("footstep");

        [SerializeField]
        private PropertyGetDecimal m_Intensity = GetDecimalDecimal.Create(0.5f);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Network Noise [{this.m_Tag}] radius {this.m_Radius}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            // Get parameters
            Vector3 position = this.m_Position.Get(args);
            float radius = (float)this.m_Radius.Get(args);
            string tag = this.m_Tag.Get(args);
            float intensity = (float)this.m_Intensity.Get(args);

            // Validate NetworkNoiseEmitter exists
            if (NetworkNoiseEmitter.Instance == null)
            {
                Debug.LogWarning(
                    "[InstructionNetworkEmitNoise] NetworkNoiseEmitter not found in scene. "
                        + "Please add a NetworkNoiseEmitter component to your NetworkManagers GameObject. "
                        + "Falling back to local-only noise emission."
                );

                // Fallback to local emission for development/testing
                EmitLocalNoise(position, radius, tag, intensity);
                return DefaultResult;
            }

            // Emit through network
            NetworkNoiseEmitter.Instance.EmitNoise(position, radius, tag, intensity);

            return DefaultResult;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        /// <summary>
        /// Fallback local noise emission when NetworkNoiseEmitter is not available.
        /// This allows the instruction to work in single-player or during development.
        /// </summary>
        private void EmitLocalNoise(Vector3 position, float radius, string tag, float intensity)
        {
            var perceptions = new System.Collections.Generic.List<ISpatialHash>();
            SpatialHashPerception.Find(position, radius, perceptions);

            HearManager.Instance?.AddGizmoNoise(position, radius);

            StimulusNoise stimulus = new StimulusNoise(tag, position, radius, intensity);

            foreach (ISpatialHash spatialHash in perceptions)
            {
                Perception perception = spatialHash as Perception;
                if (perception == null)
                    continue;

                SensorHear sensorHear = perception.GetSensor<SensorHear>();
                if (sensorHear == null)
                    continue;

                sensorHear.OnReceiveNoise(stimulus);
            }

            Debug.Log(
                $"[InstructionNetworkEmitNoise] Local fallback: emitted noise at {position}, "
                    + $"radius={radius}, tag={tag}, intensity={intensity}"
            );
        }
    }
}
