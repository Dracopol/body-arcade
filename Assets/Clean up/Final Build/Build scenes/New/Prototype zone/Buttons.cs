using UnityEngine;
using UnityEngine.Splines;

public class Buttons : MonoBehaviour
{
    [Header("Btn")]
    [SerializeField] private GameObject redHouse;
    [SerializeField] private GameObject yellowHouse;
    [SerializeField] private GameObject blueHouse;

    [SerializeField] private GameObject redHouse2;
    [SerializeField] private GameObject yellowHouse2;
    [SerializeField] private GameObject blueHouse2;

    [Header("Sounds")]
    [SerializeField] private AudioClip redSound;
    [SerializeField] private AudioClip yellowSound;
    [SerializeField] private AudioClip blueSound;

    [Header("Spline Spawning")]
    [SerializeField] private GameObject redPrefab;
    [SerializeField] private GameObject yellowPrefab;
    [SerializeField] private GameObject bluePrefab;

    [SerializeField] private SplineContainer targetSpline1;
    [SerializeField] private SplineContainer targetSpline2;

    private Renderer _redRenderer;
    private Renderer _yellowRenderer;
    private Renderer _blueRenderer;
    private Color _reddefaultColor;
    private Color _yellowdefaultColor;
    private Color _bluedefaultColor;

    private Renderer _redRenderer2;
    private Renderer _yellowRenderer2;
    private Renderer _blueRenderer2;
    private Color _reddefaultColor2;
    private Color _yellowdefaultColor2;
    private Color _bluedefaultColor2;

    private void Awake()
    {
        _redRenderer = redHouse.GetComponent<Renderer>();
        _reddefaultColor = _redRenderer.material.color;

        _yellowRenderer = yellowHouse.GetComponent<Renderer>();
        _yellowdefaultColor = _yellowRenderer.material.color;

        _blueRenderer = blueHouse.GetComponent<Renderer>();
        _bluedefaultColor = _blueRenderer.material.color;

        
        _redRenderer2 = redHouse2.GetComponent<Renderer>();
        _reddefaultColor2 = _redRenderer2.material.color;

        _yellowRenderer2 = yellowHouse2.GetComponent<Renderer>();
        _yellowdefaultColor2 = _yellowRenderer2.material.color;

        _blueRenderer2 = blueHouse2.GetComponent<Renderer>();
        _bluedefaultColor2 = _blueRenderer2.material.color;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnAndAnimate(yellowPrefab, 1);
        }
    }

    private void SpawnAndAnimate(GameObject animatedPrefab, int spline)
    {
        SplineContainer targetSpline = spline == 1 ? targetSpline1 : targetSpline2;

        Vector3 spawnPos = targetSpline.EvaluatePosition(0f);
        GameObject instance = Instantiate(animatedPrefab, spawnPos, Quaternion.identity);

        SplineAnimate splineAnimate = instance.GetComponent<SplineAnimate>();
        if (splineAnimate == null)
        {
            Debug.LogWarning("Buttons: Prefab is missing a SplineAnimate component.");
            Destroy(instance);
            return;
        }

        splineAnimate.Container = targetSpline;
        splineAnimate.Loop = SplineAnimate.LoopMode.Once; 
        splineAnimate.Completed += () => Destroy(instance); 
        splineAnimate.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Red"))
        {
            _redRenderer.material.color = Color.green;
            SpawnAndAnimate(redPrefab, 1);
            AudioSource.PlayClipAtPoint(redSound, redHouse.transform.position);
        }
        if (other.CompareTag("Yellow"))
        {
            _yellowRenderer.material.color = Color.green;
            SpawnAndAnimate(yellowPrefab, 1);
            AudioSource.PlayClipAtPoint(yellowSound, yellowHouse.transform.position);
        }
        if (other.CompareTag("Blue"))
        {
            _blueRenderer.material.color = Color.green;
            SpawnAndAnimate(bluePrefab, 1);
            AudioSource.PlayClipAtPoint(blueSound, blueHouse.transform.position);
        }

        if (other.CompareTag("Red2"))
        {
            _redRenderer2.material.color = Color.green;
            SpawnAndAnimate(redPrefab, 2);
            AudioSource.PlayClipAtPoint(redSound, redHouse.transform.position);
        }
        if (other.CompareTag("Yellow2"))
        {
            _yellowRenderer2.material.color = Color.green;
            SpawnAndAnimate(yellowPrefab, 2);
            AudioSource.PlayClipAtPoint(yellowSound, yellowHouse.transform.position);
        }
        if (other.CompareTag("Blue2"))
        {
            _blueRenderer2.material.color = Color.green;
            SpawnAndAnimate(bluePrefab, 2);
            AudioSource.PlayClipAtPoint(blueSound, blueHouse.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Red"))
        {
            _redRenderer.material.color = _reddefaultColor;
        }
        if (other.CompareTag("Yellow"))
        {
            _yellowRenderer.material.color = _yellowdefaultColor;
        }
        if (other.CompareTag("Blue"))
        {
            _blueRenderer.material.color = _bluedefaultColor;
        }
        if (other.CompareTag("Red2"))
        {
            _redRenderer2.material.color = _reddefaultColor2;
        }
        if (other.CompareTag("Yellow2"))
        {
            _yellowRenderer2.material.color = _yellowdefaultColor2;
        }
        if (other.CompareTag("Blue2"))
        {
            _blueRenderer2.material.color = _bluedefaultColor2;
        }
    }
}