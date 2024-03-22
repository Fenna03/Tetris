using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Music : MonoBehaviour
{
    private AudioSource _audioSource;
    public TMP_Text textMeshProOn;
    public TMP_Text textMeshProOff;


    private void Awake()
    {
       // DontDestroyOnLoad(transform.gameObject);
        _audioSource = GetComponent<AudioSource>();
        sfxButtonState(true);
    }
    public void PlayMusic()
    {
        if (_audioSource.isPlaying) return;
        _audioSource.Play();
        _audioSource.volume = 100;
    }

    public void StopMusic()
    {
        _audioSource.Stop();
        _audioSource.volume = 0;
    }

    public void sfxButtonState(bool sfxState)
    {
        if( sfxState)
        {
            textMeshProOn.color = Color.white;
            textMeshProOff.color = Color.gray;
        }
        else
        {
            textMeshProOn.color = Color.gray;
            textMeshProOff.color = Color.white;
        }
    }


}
