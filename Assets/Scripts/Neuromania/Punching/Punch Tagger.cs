using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Punching
{
    internal class GloveTagger : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private PunchDetector _detector;
        [SerializeField] private Collider _tagTarget;

        #endregion

        #region Fields

        private string _defaultTag;

        #endregion

        #region Setup

        private void Awake()
        {
            _defaultTag = _tagTarget.tag;
        }

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

        private void HandlePunchStart() => _tagTarget.tag = Tags.Glove;
        private void HandlePunchEnd() => _tagTarget.tag = _defaultTag;

        #endregion
    }
}