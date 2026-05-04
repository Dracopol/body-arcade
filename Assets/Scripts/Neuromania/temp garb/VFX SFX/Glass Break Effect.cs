using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.VFXSFX
{
    internal class GlassBreakEffect : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject _glass;
        [SerializeField] private ParticleSystem _breakParticles;
        [SerializeField] private AudioSource _breakSound;

        #endregion

        #region Internal

        internal void Play()
        {
            if (_glass != null)
                _glass.SetActive(false);

            if (_breakParticles != null)
                _breakParticles.Play();

            if (_breakSound != null)
                _breakSound.Play();
        }

        #endregion
    }
}