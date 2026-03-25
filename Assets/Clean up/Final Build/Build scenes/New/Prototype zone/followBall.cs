using UnityEngine;
using System.Collections.Generic;

public class followBall : MonoBehaviour
{
    [SerializeField] private string followTag = "Yellow";

    private bool isFollowing = false;

    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(followTag))
        {
            SetColor(other, Color.green);
            isFollowing = true;
        }
        else if (!other.CompareTag("Untagged") && !isFollowing)
        {
            SetColor(other, Color.red);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(followTag))
        {
            RestoreColor(other);
            isFollowing = false;
        }
        else if (!other.CompareTag("Untagged"))
        {
            RestoreColor(other);
        }
    }

    private void SetColor(Collider target, Color color)
    {
        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null)
        {
            if (!originalColors.ContainsKey(rend))
                originalColors[rend] = rend.material.color;

            rend.material.color = color;
        }
    }

    private void RestoreColor(Collider target)
    {
        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null && originalColors.ContainsKey(rend))
        {
            rend.material.color = originalColors[rend];
            originalColors.Remove(rend);
        }
    }
}