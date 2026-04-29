using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace IMVR.BodyArcade.Neuromania.Input
{
    internal class BurgerButton : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private InputActionReference _menuButton;

        [SerializeField] private string _sceneName;

        #endregion

        #region Setup

        private void OnEnable()
        {
            if (_menuButton == null) return;
            _menuButton.action.Enable();
            _menuButton.action.performed += HandleMenuButtonPressed;
        }

        private void OnDisable()
        {
            if (_menuButton == null) return;
            _menuButton.action.performed -= HandleMenuButtonPressed;
            _menuButton.action.Disable();
        }

        #endregion

        #region Event Handlers

        private void HandleMenuButtonPressed(InputAction.CallbackContext ctx)
        {
            SwitchScene(_sceneName);
        }

        #endregion

        #region Private

        public void SwitchScene(string _sceneName)
        {
            SceneManager.LoadScene(_sceneName);
        }

        #endregion
    }
}