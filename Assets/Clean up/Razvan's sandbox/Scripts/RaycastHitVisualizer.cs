using UnityEngine;
using System.Collections;

public class RaycastHitVisualizer : MonoBehaviour
{
    [Header("Line Renderer (optional)")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineDuration = 0.05f;

    [Header("Tracer (optional)")]
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private float tracerSpeed = 100f;

    // Called by RaycastHitDetector
    public void ShowRay(Vector3 start, Vector3 end)
    {
        if (lineRenderer != null)
            StartCoroutine(ShowLine(start, end));

        if (tracerPrefab != null)
            SpawnTracer(start, end);
    }

    private IEnumerator ShowLine(Vector3 start, Vector3 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        yield return new WaitForSeconds(lineDuration);

        lineRenderer.enabled = false;
    }

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = Instantiate(tracerPrefab, start, Quaternion.identity);
        StartCoroutine(MoveTracer(tracer, end));
    }

    private IEnumerator MoveTracer(GameObject tracer, Vector3 end)
    {
        while (tracer != null && Vector3.Distance(tracer.transform.position, end) > 0.1f)
        {
            tracer.transform.position = Vector3.MoveTowards(
                tracer.transform.position,
                end,
                tracerSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (tracer != null) Destroy(tracer);
    }
}
