using UnityEngine;

namespace IMVR.BodyArcade.NeuromaniaV1
{
    internal class FollowCamera : MonoBehaviour
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
            position.x = Mathf.Lerp(position.x, _camera.transform.position.x, _smoothSpeed * Time.deltaTime);
            position.y = Mathf.Lerp(position.y, _camera.transform.position.y, _smoothSpeed * Time.deltaTime);
            position.z = Mathf.Lerp(position.z, _camera.transform.position.z, _smoothSpeed * Time.deltaTime);
            _target.transform.position = position;
        }

        #endregion
    }
}