using UnityEngine;

namespace IMVR.BodyArcade.Neuromania
{
    internal class FollowCameraY : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _target;
        [SerializeField] private float _smoothSpeed = 5f;

        #endregion

        #region Update

        private void Update()
        {
            if (_camera == null || _target == null)
                return;

            Vector3 position = _target.transform.position;
            position.y = Mathf.Lerp(position.y, _camera.transform.position.y, _smoothSpeed * Time.deltaTime);
            _target.transform.position = position;
        }

        #endregion
    }
}