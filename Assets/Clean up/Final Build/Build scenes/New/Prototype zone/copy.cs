using UnityEngine;

public class copy : MonoBehaviour
{
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    [SerializeField] private GameObject copyRight;

    void Update()
    {
        CopyTransform(right, copyRight);
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            left.SetActive(true);
            copyRight.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            left.SetActive(false);
            copyRight.SetActive(true);
        }
    }

    private void CopyTransform(GameObject source, GameObject target)
    {
        if (source == null || target == null) return;

        // Mirror position on the X axis
        Vector3 mirroredPosition = source.transform.position;
        mirroredPosition.x *= -1;
        target.transform.position = mirroredPosition;

        // Copy rotation but mirror on the Y axis
        Vector3 mirroredRotation = source.transform.eulerAngles;
        mirroredRotation.y *= -1;
        target.transform.eulerAngles = mirroredRotation;
    }
}