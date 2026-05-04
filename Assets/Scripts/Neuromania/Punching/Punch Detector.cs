using System;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Punching
{
    internal class PunchDetector : MonoBehaviour
    {
        #region Events

        internal event Action OnPunchStart;
        internal event Action OnPunchEnd;

        #endregion

        #region Serialized Fields

        [SerializeField] private float _minimumSpeed = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _forwardRatioThreshold = 0.7f;

        #endregion

        #region Fields

        private Vector3 _previousPosition;
        private bool _isPunching;

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
            Vector3 delta = transform.position - _previousPosition;
            _previousPosition = transform.position;

            float speed = delta.magnitude / Time.deltaTime;
            float forwardRatio = speed > 0.01f ? Vector3.Dot(delta.normalized, transform.forward) : 0f;

            bool punching = speed >= _minimumSpeed && forwardRatio >= _forwardRatioThreshold;

            if (punching && !_isPunching)
            {
                _isPunching = true;
                OnPunchStart?.Invoke();
            }
            else if (!punching && _isPunching)
            {
                _isPunching = false;
                OnPunchEnd?.Invoke();
            }
        }

        #endregion
    }
}