using IMVR.BodyArcade.Neuromania.Punching;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.VFXSFX
{
    internal class PunchEffects : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private PunchDetector _detector;
        [SerializeField] private ParticleSystem _gloveFlame;
        [SerializeField] private AudioSource _wooshAudio;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _detector.OnPunchStart += HandlePunchStart;
            _detector.OnPunchEnd += HandlePunchEnd;
        }

        private void OnDisable()
        {
            _detector.OnPunchStart -= HandlePunchStart;
            _detector.OnPunchEnd -= HandlePunchEnd;
        }

        #endregion

        #region Event Handlers

        private void HandlePunchStart()
        {
            _gloveFlame.Play();
            _wooshAudio.Play();
        }

        private void HandlePunchEnd()
        {
            _gloveFlame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        #endregion
    }
}