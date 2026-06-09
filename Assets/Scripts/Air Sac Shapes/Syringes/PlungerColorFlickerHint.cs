using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Syringes
{
    // Idle "interact with me" hint: ping-pongs the plunger renderer's color between two
    // colors while active, restores the base color when stopped. MonoBehaviour so it can
    // live on the plunger GO and drive itself via Update.
    internal class PlungerColorFlickerHint : MonoBehaviour, IPlungerHint
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Renderer _renderer;

        [Header("Behaviour")]
        [SerializeField] private Color _colorA = Color.black;
        [SerializeField] private Color _colorB = Color.cyan;
        [SerializeField, Min(0.01f)] private float _speed = 3f;

        #endregion

        #region Fields

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock _block;
        private bool  _active;
        private float _phase;

        #endregion

        #region Setup

        private void Awake()
        {
            _block = new MaterialPropertyBlock();

            // Emission must be enabled on the instanced material for _EmissionColor to render.
            if (_renderer != null)
                foreach (Material m in _renderer.materials)
                    m.EnableKeyword("_EMISSION");
        }

        #endregion

        #region Update

        private void Update()
        {
            if (!_active || _renderer == null) return;

            _phase += Time.deltaTime * _speed;
            float t = Mathf.PingPong(_phase, 1f);
            Apply(Color.Lerp(_colorA, _colorB, t));
        }

        #endregion

        #region IPlungerHint

        public void Begin()
        {
            if (_active) return;
            _active = true;
            _phase  = 0f;
        }

        public void End()
        {
            if (!_active) return;
            _active = false;
            Apply(_colorA);   // restore base look
        }

        #endregion

        #region Private

        private void Apply(Color color)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_block);
            _block.SetColor(EmissionColorId, color);
            _renderer.SetPropertyBlock(_block);
        }

        #endregion
    }
}
