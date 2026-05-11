using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace IMVR.BodyArcade.NeuromaniaV1.Input
{
    internal class BurgerButton : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private string _sceneName;

        #endregion

        #region Fields

        private InputAction _menuAction;

        #endregion

        #region Setup

        private void Awake()
        {
            _menuAction = new InputAction(
                name: "MenuButton",
                type: InputActionType.Button,
                binding: "<XRController>{RightHand}/secondaryButton");
        }

        private void OnEnable()
        {
            _menuAction.performed += HandleMenuButtonPressed;
            _menuAction.Enable();
        }

        private void OnDisable()
        {
            _menuAction.performed -= HandleMenuButtonPressed;
            _menuAction.Disable();
        }

        private void OnDestroy()
        {
            _menuAction.Dispose();
        }

        #endregion

        #region Event Handlers

        private void HandleMenuButtonPressed(InputAction.CallbackContext ctx)
        {
            SwitchScene(_sceneName);
        }

        #endregion

        #region Private

        public void SwitchScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        #endregion
    }
}