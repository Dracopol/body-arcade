using UnityEngine;
using IMVR.BodyArcade.UiRobot;

namespace IMVR.CorpusVR.Core
{
    internal class MirrorHands : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private GameObject _left;
        [SerializeField] private GameObject _right;
        [SerializeField] private GameObject _copyRight;
        [SerializeField] private GameObject _copyLeft;
        [SerializeField] private Camera _camera;

        [Header("Debug")]
        [SerializeField] private float _debugCameraX;

        #endregion

        #region Fields

        private MirrorMode _mirrorMode;

        #endregion

        #region Properties

        internal GameObject ActiveLeft => _mirrorMode switch
        {
            MirrorMode.MirrorRight => _copyRight.transform.GetChild(0).GetChild(0).gameObject,
            _ => _left.transform.GetChild(0).GetChild(0).gameObject
        };

        internal GameObject ActiveRight => _mirrorMode switch
        {
            MirrorMode.MirrorLeft => _copyLeft.transform.GetChild(0).GetChild(0).gameObject,
            _ => _right.transform.GetChild(0).GetChild(0).gameObject
        };

        #endregion

        #region Setup

        private void Start()
        {
            _mirrorMode = SessionConfigAirSacShapes.Instance != null
                ? SessionConfigAirSacShapes.Instance.MirrorMode
                : MirrorMode.Unmirrored;

            ApplyMirrorMode();
        }

        #endregion

        #region Update

        private void Update()
        {
            // if (_camera != null)
            //     Debug.Log($"[MirrorHands] Camera X: {_camera.transform.position.x:F3} | Right X: {_right?.transform.position.x:F3} | CopyRight X: {_copyRight?.transform.position.x:F3}");

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

            float cameraX = _camera != null ? _camera.transform.position.x : 0f;

            Vector3 mirroredPosition = source.transform.position;
            mirroredPosition.x = 2f * cameraX - source.transform.position.x;
            target.transform.position = mirroredPosition;

            Quaternion q = source.transform.rotation;
            target.transform.rotation = new Quaternion(q.x, -q.y, -q.z, q.w);
        }

        #endregion
    }
}