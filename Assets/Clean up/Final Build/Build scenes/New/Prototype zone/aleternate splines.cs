using UnityEngine;
using UnityEngine.Splines;


public class aleternatesplines : MonoBehaviour
{
    [SerializeField] private GameObject xr;
    [SerializeField] private GameObject player;

    private SplineAnimate xrSplineAnimate;

    private SplineAnimate playerSplineAnimate;

    private void Awake()
    {
        xrSplineAnimate = xr.GetComponent<SplineAnimate>();
        playerSplineAnimate = player.GetComponent<SplineAnimate>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            switchToXR();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            switchToPlayer();
    }

    private void switchToPlayer()
    {
        xrSplineAnimate.enabled = false;
        playerSplineAnimate.enabled = true;
        xr.transform.localPosition = Vector3.zero;
        playerSplineAnimate.Play();
    }
    private void switchToXR()
    {
        xrSplineAnimate.enabled = true;
        playerSplineAnimate.enabled = false;
        player.transform.localPosition = Vector3.zero;
        xr.transform.localPosition = Vector3.zero;
        xrSplineAnimate.Play();
    }
}
