using UnityEngine;

namespace NuevaAndinia.Audio
{
    using NuevaAndinia.Core;

    public class PlayerAudioController : MonoBehaviour, IPlayerAudio
    {
        [SerializeField] private AudioSource footstepsAudioSource;
        [SerializeField] private AudioSource landingAudioSource;
        [SerializeField] private AudioClip landingClip;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField, Range(0, 1)] private float volume = 0.5f;

        public void OnFootstep(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.5f)
                PlayFootstep();
                
            
        }

        public void OnLand(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.5f)
                PlayLanding();
                
        }

        public void PlayFootstep()
        {
            if (footstepClips.Length == 0 || footstepsAudioSource == null) return;

            int index = Random.Range(0, footstepClips.Length);
            footstepsAudioSource.PlayOneShot(footstepClips[index], volume);
        }

        public void PlayLanding()
        {
            if (landingClip == null || landingAudioSource == null) return;

            landingAudioSource.PlayOneShot(landingClip, volume);
        }
    }
}
