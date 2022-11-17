using System;
using UnityEngine;
using Rand = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CoreLib
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioRandom : MonoBehaviour
    {
        public AudioClip[] clips;

        [MinMaxRange(0, 1)]
        public Vector2 volumeRange = new Vector2(0.9f, 1);

        [MinMaxRange(-3, 3)]
        public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

        private AudioSource _source;


        private void Awake()
        {
            this._source = GetComponent<AudioSource>();
        }

        public void Play()
        {
            var clip = clips[Rand.Range(0, clips.Length)];
            var volume = Rand.Range(volumeRange.x, volumeRange.y);
            var pitch = Rand.Range(pitchRange.x, pitchRange.y);
            _source.loop = false;
            _source.pitch = pitch;
            _source.PlayOneShot(clip, volume);
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(AudioRandom))]
    public class AudioRandomEditor  : Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying && GUILayout.Button("Play"))
            {
                var arand = target as AudioRandom;
                arand.Play();
                
            }
            base.OnInspectorGUI();
        }
        
    }
    #endif
}