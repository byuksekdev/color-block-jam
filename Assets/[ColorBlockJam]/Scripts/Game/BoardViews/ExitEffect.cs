using System;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// The small cubes thrown out where a block leaves through a door. It is one cell wide at scale 1
    /// and is stretched to the width of the block. The object comes from a pool.
    /// </summary>
    public sealed class ExitEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private ParticleSystemRenderer _renderer;

        /// <summary>The last cube has gone and the effect can be taken away.</summary>
        public event Action<ExitEffect> Finished;

        public void Play(int cells, Material material)
        {
            transform.localScale = new Vector3(cells, 1f, 1f);
            _renderer.sharedMaterial = material;
            _particles.Play();
        }

        /// <summary>Removes the cubes at once, before the object goes back to the pool.</summary>
        public void Release()
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>Unity calls this when the particle system has stopped and its stop action is Callback.</summary>
        private void OnParticleSystemStopped()
        {
            Finished?.Invoke(this);
        }
    }
}
