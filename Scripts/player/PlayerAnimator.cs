using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerCollisionChecker), typeof (Animator))]
public class PlayerAnimator : MonoBehaviour {

    Animator animator;
	SpriteRenderer spriteRenderer;

    public GameObject groundedFlipAnim;

    void Start() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateAnimation(Vector3 velocity, PlayerCollisionChecker playerCollisionChecker) {
		if(Mathf.Abs(velocity.x) > 0.1f){
			animator.SetBool("isRunning", true);
		} else {
			animator.SetBool("isRunning", false);
		}

		animator.SetFloat("verticalVelocity", velocity.y);

        bool shouldFlipX = (playerCollisionChecker.FaceDirection == -1);

        if(spriteRenderer.flipX != shouldFlipX) {
            spriteRenderer.flipX = shouldFlipX;

            if(playerCollisionChecker.CollisionData.Below){
               PlayGroundedFlipAnim(playerCollisionChecker.FaceDirection);
            }
        }
    }

    void PlayGroundedFlipAnim(int faceDirection) {
        Vector3 position = this.transform.position;
		position.x -= 0.7f * faceDirection;
		position.y -= 0.55f;
        Quaternion rotation = faceDirection == -1 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
		Instantiate(groundedFlipAnim, position, rotation);
    }
}
