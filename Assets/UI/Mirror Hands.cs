using UnityEngine;
using IMVR.BodyArcade.UI;

namespace IMVR.CorpusVR.Core
{
    internal class MirrorHands : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private GameObject _left;
        [SerializeField] private GameObject _right;
        [SerializeField] private GameObject _copyRight;
        [SerializeField] private GameObject _copyLeft;

        #endregion

        #region Fields

        private MirrorMode _mirrorMode;

        #endregion

        #region Properties

        internal GameObject ActiveLeft => _mirrorMode == MirrorMode.MirrorLeft
            ? _copyLeft.transform.GetChild(0).gameObject
            : _left.transform.GetChild(0).gameObject;

        internal GameObject ActiveRight => _mirrorMode == MirrorMode.MirrorRight
            ? _copyRight.transform.GetChild(0).gameObject
            : _right.transform.GetChild(0).gameObject;

        #endregion

        #region Setup

        private void Start()
        {
            _mirrorMode = SessionConfig.Instance != null
                ? SessionConfig.Instance.MirrorMode
                : MirrorMode.Unmirrored;

            ApplyMirrorMode();
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_mirrorMode == MirrorMode.MirrorRight)
                CopyTransform(_right, _copyRight);
            else if (_mirrorMode == MirrorMode.MirrorLeft)
                CopyTransform(_left, _copyLeft);
        }

        #endregion

        #region Private

        private void ApplyMirrorMode()
        {
            switch (_mirrorMode)
            {
                case MirrorMode.Unmirrored:
                    _left.SetActive(true);
                    _right.SetActive(true);
                    _copyLeft.SetActive(false);
                    _copyRight.SetActive(false);
                    break;

                case MirrorMode.MirrorRight:
                    _left.SetActive(false);
                    _right.SetActive(true);
                    _copyLeft.SetActive(false);
                    _copyRight.SetActive(true);
                    break;

                case MirrorMode.MirrorLeft:
                    _left.SetActive(true);
                    _right.SetActive(false);
                    _copyLeft.SetActive(true);
                    _copyRight.SetActive(false);
                    break;
            }
        }

        private void CopyTransform(GameObject source, GameObject target)
        {
            if (source == null || target == null) return;

            Vector3 mirroredPosition = source.transform.position;
            mirroredPosition.x *= -1;
            target.transform.position = mirroredPosition;

            Vector3 mirroredRotation = source.transform.eulerAngles;
            mirroredRotation.y *= -1;
            target.transform.eulerAngles = mirroredRotation;
        }

        #endregion
    }
}