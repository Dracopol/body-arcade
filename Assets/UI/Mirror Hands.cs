using UnityEngine;
using IMVR.BodyArcade.UI;

internal class MirrorHands : MonoBehaviour
{
    [SerializeField] private GameObject _left;
    [SerializeField] private GameObject _right;
    [SerializeField] private GameObject _copyRight;
    [SerializeField] private GameObject _copyLeft;

    private MirrorMode _mirrorMode;

    private void Start()
    {
        _mirrorMode = SessionConfig.Instance != null
            ? SessionConfig.Instance.MirrorMode
            : MirrorMode.Unmirrored;

        Debug.Log($"[MirrorHands] SessionConfig exists: {SessionConfig.Instance != null}");
        Debug.Log($"[MirrorHands] MirrorMode: {_mirrorMode}");
        Debug.Log($"[MirrorHands] Left: {_left?.name} | Right: {_right?.name} | CopyLeft: {_copyLeft?.name} | CopyRight: {_copyRight?.name}");

        ApplyMirrorMode();

        Debug.Log($"[MirrorHands] After Apply — Left active: {_left?.activeSelf} | Right active: {_right?.activeSelf} | CopyLeft active: {_copyLeft?.activeSelf} | CopyRight active: {_copyRight?.activeSelf}");
    }

    private void Update()
    {
        if (_mirrorMode == MirrorMode.MirrorRight)
            CopyTransform(_right, _copyRight);
        else if (_mirrorMode == MirrorMode.MirrorLeft)
            CopyTransform(_left, _copyLeft);
    }

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
}