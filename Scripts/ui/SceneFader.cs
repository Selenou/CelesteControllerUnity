using UnityEngine;

public class SceneFader : MonoBehaviour {
	Animator anim;
	Player player;

	void Start() {
		anim = GetComponent<Animator>();
		player = GameObject.Find("Player").GetComponent("Player") as Player;
        player.deathEvent += OnPlayerDeath;
	}

	void OnPlayerDeath(){
		anim.SetTrigger("Fade");
	}
}
