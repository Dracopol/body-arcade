using System.Collections.Generic;
using UnityEngine;

namespace IMVR.BodyArcade.UI
{
    internal class SessionConfigAirSacShapes : MonoBehaviour
    {
        internal static SessionConfigAirSacShapes Instance { get; private set; }

        internal MirrorMode MirrorMode;
        internal List<GameObject> SelectedShapes = new();

        [SerializeField] private List<GameObject> _leftHandShapes = new();
        [SerializeField] private List<GameObject> _rightHandShapes = new();
        [SerializeField] private List<GameObject> _bothHandsShapes = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        internal void ApplyHandMode(HandMode mode)
        {
            SelectedShapes = mode switch
            {
                HandMode.LeftOnly => _leftHandShapes,
                HandMode.RightOnly => _rightHandShapes,
                HandMode.BothSimultaneously => _bothHandsShapes,
                _ => _bothHandsShapes
            };
        }
    }
}