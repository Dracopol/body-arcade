using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using DG.Tweening;

public class ButtonAction : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable interactableButton;
    [SerializeField] private GameObject buttonMesh;
    [SerializeField] private GameObject activatorMesh;

    private Renderer buttonRender;
    private Renderer activatorRender;
    private bool isActive;
    private bool isInCooldown = false;
    [SerializeField] private float cooldownDuration = 0.5f;

    void Start()
    {
        isActive = false;
        buttonRender = buttonMesh.GetComponent<Renderer>();
        activatorRender = activatorMesh.GetComponent<Renderer>();
    }

    public void OnButtonPressed(SelectEnterEventArgs args)
    {
        // Buffer Zone
        if (isInCooldown)
            return;
        isInCooldown = true;

        StartCoroutine(CooldownTimer());

        isActive = !isActive;

        if (isActive)
        {
            buttonRender.material.EnableKeyword("_EMISSION");
            activatorRender.material.EnableKeyword("_EMISSION");

            Color targetButtonColor = Color.white * 2f;
            Color targetActivatorColor = Color.yellow * 2f;

            buttonRender.material.DOColor(targetButtonColor, "_EmissionColor", 0.5f);
            activatorRender.material.DOColor(targetActivatorColor, "_EmissionColor", 0.5f);
        }
        else
        {
            Debug.Log("Deactivated!");

            Color offColor = Color.black;
            buttonRender.material.DOColor(offColor, "_EmissionColor", 0.5f);
            activatorRender.material.DOColor(offColor, "_EmissionColor", 0.5f);
        }
    }

    public void OnButtonReleased(SelectExitEventArgs args)
    {
        Debug.Log("Released!");
    }

    private System.Collections.IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(cooldownDuration);
        isInCooldown = false;
    }
}