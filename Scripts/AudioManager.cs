using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {

    [SerializeField]
    AudioClip playerJump = null;
    [SerializeField]
    AudioClip playerDash = null;
    [SerializeField]
    AudioClip playerDeath = null;
    [SerializeField]
    AudioClip mainMusic = null;

    AudioSource effectSource;
    AudioSource musicSource;
    Player player;

    void Start() {
        effectSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        player = GameObject.Find("Player").GetComponent("Player") as Player;
        player.jumpEvent += OnPlayerJump;
        player.dashEvent += OnPlayerDash;
        player.deathEvent += OnPlayerDeath;

        PlayMusic(mainMusic);
    }

    void PlayEffect(AudioClip clip) {
        effectSource.clip = clip;
        effectSource.Play();
    }

    void PlayMusic(AudioClip clip) {
		musicSource.clip = clip;
		musicSource.Play();
	}

    void OnPlayerJump() {
        PlayEffect(playerJump);
    }

    void OnPlayerDash() {
        PlayEffect(playerDash);
    }

    void OnPlayerDeath() {
        PlayEffect(playerDeath);
    }
}
