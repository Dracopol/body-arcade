using System;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    // Owns the air sac's end (win/fail) reveal. The green tube fill is the timing source: the
    // spawner calls BeginReveal when the fill starts, Follow(t) each frame as the fill rises
    // (0 → fraction), and FillComplete when it reaches the fraction. The sac colors/scales in
    // lockstep with the fill, then freezes (it does not follow the tube's drain). Punch fires on
    // both outcomes at fill-complete; particles are win-only (their lifetime is registered through
    // the destroy-delay sink so teardown waits for them).
    internal class AirSacEndAnimator : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private AirSacColor _color;
        [SerializeField] private AirSacSize _size;
        [SerializeField] private AirSacProgressParticleVisualizer _particles;

        #endregion

        #region Fields

        private Action<float> _destroyDelaySink;
        private bool _following;

        #endregion

        #region Internal

        // The spawner injects RegisterDestroyDelay so win particles hold teardown until they finish.
        internal void SetDestroyDelaySink(Action<float> sink) => _destroyDelaySink = sink;

        // New shape wired to this (pooled) sac — make sure no stale reveal is left following.
        internal void ResetReveal()
        {
            _following = false;
        }

        // The green tube fill has started rising — begin tracking it.
        internal void BeginReveal()
        {
            _following = true;
        }

        // Drive the sac's color + size from the current tube fill value (0 = sick, 1 = healed).
        // Ignored unless we're in the reveal window, so the sac stays sick during play.
        internal void Follow(float t)
        {
            if (!_following) return;
            if (_color != null) _color.Apply(t);
            if (_size  != null) _size.Apply(t);
        }

        // The fill reached the fraction. Freeze the sac there (don't follow the drain), punch, and
        // on a win spawn the celebration particles.
        internal void FillComplete(bool won)
        {
            _following = false;

            if (_particles != null)
            {
                _particles.Punch();
                if (won) _destroyDelaySink?.Invoke(_particles.SpawnParticles());
            }
        }

        #endregion
    }
}
