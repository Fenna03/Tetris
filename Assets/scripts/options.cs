using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class options : MonoBehaviour
{
    public bool pause = true;
    //public byte gameMode;
    //everything for volume and music
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }
}
