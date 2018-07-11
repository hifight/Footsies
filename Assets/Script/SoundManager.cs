using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{

    public class SoundManager : Singleton<SoundManager>
    {

        public GameObject seSourceObject1;
        public GameObject seSourceObject2;
        public GameObject bgmSourceObject;

        [Range(0.0f, 1.0f)]
        public float masterVolume = 1f;

        private AudioSource seSource1;
        private AudioSource seSource2;
        private AudioSource bgmSource;

        private float defaultBGMVolume;
        public bool isBGMOn { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            seSource1 = seSourceObject1.GetComponent<AudioSource>();
            seSource2 = seSourceObject2.GetComponent<AudioSource>();
            bgmSource = bgmSourceObject.GetComponent<AudioSource>();
            defaultBGMVolume = bgmSource.volume;
            isBGMOn = true;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public bool toggleBGM()
        {
            if (isBGMOn)
            {
                bgmSource.volume = 0;
                isBGMOn = false;
            }
            else
            {
                bgmSource.volume = defaultBGMVolume;
                isBGMOn = true;
            }

            return isBGMOn;
        }


        public void playSE(AudioClip clip)
        {
            seSource1.clip = clip;
            seSource1.panStereo = 0;
            seSource1.Play();
        }

        public void playFighterSE(AudioClip clip, bool isPlayerOne, float posX)
        {
            var audioSource = seSource1;
            if (!isPlayerOne)
            {
                audioSource = seSource2;
            }

            audioSource.clip = clip;
            audioSource.panStereo = posX / 5;
            audioSource.Play();
        }
    }

}