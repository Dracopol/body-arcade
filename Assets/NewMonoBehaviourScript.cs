using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.UIElements;

namespace IMVR.BodyArcade.UI
{
    internal class UIInteractionDebugger : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;
        [SerializeField] private List<NearFarInteractor> _interactors;

        private void Update()
        {
            foreach (var interactor in _interactors)
            {
                if (interactor == null) continue;

                // Log ray direction and position
                Debug.DrawRay(
                    interactor.transform.position,
                    interactor.transform.forward * 5f,
                    Color.red
                );

                // Log what the interactor is hovering
                var hover = interactor.interactablesHovered;
                if (hover.Count > 0)
                    Debug.Log($"[DEBUG] {interactor.name} hovering: {hover[0]}");

                // Check if ray hits anything on UI layer
                var ray = new Ray(interactor.transform.position, interactor.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("UI")))
                    Debug.Log($"[DEBUG] Physics raycast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                else
                    Debug.Log($"[DEBUG] {interactor.name} raycast hit NOTHING on UI layer");
            }

            // Log UI Toolkit root element status
            if (_document != null && _document.rootVisualElement != null)
            {
                var root = _document.rootVisualElement;
                Debug.Log($"[DEBUG] UI root size: {root.layout.width}x{root.layout.height} | visible: {root.visible} | display: {root.resolvedStyle.display}");
            }
        }
    }
}