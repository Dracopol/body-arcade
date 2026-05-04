using UnityEngine;

namespace IMVR.BodyArcade.Neuromania
{
    internal class GloveFlameEffect : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private ParticleSystem _gloveFlame;
        [SerializeField] private AudioSource _wooshAudio;
        [SerializeField] private GameObject _tagTarget;
        [SerializeField] private float _minimumSpeed = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _forwardRatioThreshold = 0.7f;

        #endregion

        #region Fields

        private const string GloveTag = "Glove";
        private const string DefaultTag = "Untagged";

        private Vector3 _previousPosition;
        private bool _isPlaying;

        #endregion

        #region Setup

        private void Start()
        {
            _previousPosition = transform.position;
        }

        #endregion

        #region Update

        private void Update()
        {
            Vector3 worldDelta = transform.position - _previousPosition;
            _previousPosition = transform.position;

            float speed = worldDelta.magnitude / Time.deltaTime;
            float forwardVelocity = Vector3.Dot(worldDelta / Time.deltaTime, transform.forward);
            float forwardRatio = speed > 0.01f ? forwardVelocity / speed : 0f;

            bool isPunching = speed >= _minimumSpeed && forwardRatio >= _forwardRatioThreshold;

            if (isPunching && !_isPlaying)
            {
                Debug.Log($"Punch detected! Speed: {speed:F2}, Forward Ratio: {forwardRatio:F2}");
                _gloveFlame.Play();
                _wooshAudio.Play();
                _tagTarget.tag = GloveTag;
                _isPlaying = true;
            }
            else if (!isPunching && _isPlaying)
            {
                Debug.Log($"Punch ended. Speed: {speed:F2}, Forward Ratio: {forwardRatio:F2}");
                _gloveFlame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _tagTarget.tag = DefaultTag;
                _isPlaying = false;
            }
        }

        #endregion
    }
}