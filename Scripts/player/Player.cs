using UnityEngine;
using System.Collections;
using Cinemachine;

[RequireComponent (typeof (PlayerCollisionChecker), typeof (PlayerAnimator), typeof (PlayerInput))]
public class Player : MonoBehaviour {

	[SerializeField]
	float moveSpeed = 8;
	[SerializeField]
	float wallSlideSpeedMax = 3;
	[SerializeField]
	float dashDistance = 5;
	[SerializeField]
	float dashTime = .2f;
	[SerializeField]
	float wallStickTime = .25f;
	[SerializeField]
	float deathDelay = 2.0f;
	[SerializeField]
	float maxJumpHeight = 4;
	[SerializeField]
	float minJumpHeight = 1;
	[SerializeField]
	float timeToJumpApex = .4f;
	[SerializeField]
	Vector2 wallJumpClimb = new Vector2(6, 24);
	[SerializeField]
	Vector2 wallJump = new Vector2(16, 24);
	[SerializeField]
	float accelAirborne = .2f;
	[SerializeField]
	float accelGrounded = .1f;

	float timeToWallUnstick;
	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	float velocityXSmoothing;

	bool isDashing;
	bool isWallSliding;
	bool isDead;

	Cinemachine.CinemachineImpulseSource impulseSource;
	
	Vector2 directionalInput;
	Vector3 velocity;

	PlayerCollisionChecker playerCollisionChecker;
	PlayerAnimator animator;
	PlayerInput playerInput;

	public delegate void PlayerDelegate();
	public event PlayerDelegate jumpEvent;
	public event PlayerDelegate dashEvent;
	public event PlayerDelegate deathEvent;

	public delegate void GateDelegate(Gate gate);
	public event GateDelegate gateEvent;

	void Start() {
		playerCollisionChecker = GetComponent<PlayerCollisionChecker> ();
		animator = GetComponent<PlayerAnimator>();

		playerInput = GetComponent<PlayerInput> ();
		playerInput.directionalInputEvent += OnDirectionalInputUpdate;
		playerInput.jumpPressedEvent += OnJumpPressed;
		playerInput.jumpReleasedEvent += OnJumpReleased;
		playerInput.dashPressedEvent += OnDashPressed;

		impulseSource = GetComponent<Cinemachine.CinemachineImpulseSource>();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}

	void Update() {
		CalculateVelocity ();
		isWallSliding = (playerCollisionChecker.CollisionData.Left || playerCollisionChecker.CollisionData.Right) && !playerCollisionChecker.CollisionData.Below && velocity.y < 0;

		if (isWallSliding) {
			HandleWallSliding ();
		}

		HandleMove();
		animator.UpdateAnimation(velocity, playerCollisionChecker);
	}

	void OnDirectionalInputUpdate (Vector2 input) {
		if(!isDead) {
			directionalInput = input;
		}
	}

	void OnJumpPressed() {
		if(!isDead) {
			if (isWallSliding) {
				HandleWallJump();
				FireEvent(jumpEvent);
			} else if (playerCollisionChecker.CollisionData.Below) {
				HandleJump();
				FireEvent(jumpEvent);
			}
		}
	}

	void OnJumpReleased() {
		if(!isDead) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}
	}

	void OnDashPressed() {
		//todo particles + doublejump + 1 dash/ground
		if (!isDashing && !isDead) {
			StartCoroutine(HandleDash());
			FireEvent(dashEvent);
		}
	}

	void OnDeath() {
		isDead = true;
		directionalInput = new Vector2();
		Invoke("ResetDeath", deathDelay);
		FireEvent(deathEvent);
	}

	void OnTriggerEnter2D(Collider2D other) {
		Debug.Log(transform.position.x-other.gameObject.transform.position.x);
		if(other.tag == "Trap") {
			OnDeath();
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		//FIXME bug si player passe pas gate et fait marche arriere
		if(other.tag == "Gate" && gateEvent != null) {
			gateEvent(other.gameObject.GetComponent<GateTrigger>().Gate);
		}
	}

	void ResetDeath() {
		isDead = false;
	}

	IEnumerator HandleDash() {
		isDashing = true;
		Vector2 normalizedInput = directionalInput.normalized;
		float dashVelocity = dashDistance / dashTime;

		if(!isWallSliding){
			velocity.x = (normalizedInput == Vector2.zero) ? dashVelocity * playerCollisionChecker.FaceDirection : normalizedInput.x * dashVelocity;
		} else {
			velocity.x = dashVelocity * -playerCollisionChecker.FaceDirection;
		}
		
		velocity.y = normalizedInput.y * dashVelocity;
		
		impulseSource.GenerateImpulse(new Vector3(10, 10));
		animator.UpdateAnimation(velocity, playerCollisionChecker);

		yield return new WaitForSeconds(dashTime);
		velocity.x = playerCollisionChecker.FaceDirection;
		velocity.y = 0;
		isDashing = false;
	}
	
	void HandleWallSliding() {
		int wallDirX = (playerCollisionChecker.CollisionData.Left) ? -1 : 1;
		
		if (velocity.y < -wallSlideSpeedMax) {
			velocity.y = -wallSlideSpeedMax;
		}

		if (timeToWallUnstick > 0) {
			velocityXSmoothing = 0;
			velocity.x = 0;

			if (directionalInput.x != wallDirX && directionalInput.x != 0) {
				timeToWallUnstick -= Time.deltaTime;
			} else {
				timeToWallUnstick = wallStickTime;
			}
		} else {
			timeToWallUnstick = wallStickTime;
		}
	}

	void HandleWallJump() {
		int wallDirX = (playerCollisionChecker.CollisionData.Left) ? -1 : 1;

		if (Mathf.Sign(wallDirX) == Mathf.Sign(directionalInput.x) && directionalInput.x != 0) { // wall climb
			velocity.x = -wallDirX * wallJumpClimb.x;
			velocity.y = wallJumpClimb.y;
		} else { // wall jump
			velocity.x = -wallDirX * wallJump.x;
			velocity.y = wallJump.y;
			playerCollisionChecker.FaceDirection *= -1;
		}
	}

	void HandleJump() {
		if (playerCollisionChecker.CollisionData.IsSlidingDownSlope) { // jump from sliding slope
			velocity.y = maxJumpVelocity * playerCollisionChecker.CollisionData.SlopeNormal.y;
			velocity.x = maxJumpVelocity * playerCollisionChecker.CollisionData.SlopeNormal.x;
		} else { // base jump
			velocity.y = maxJumpVelocity;
		}
	}

	void HandleMove() {
		playerCollisionChecker.Move (velocity * Time.deltaTime, directionalInput);
		UpdateVerticalVelocityAfterMove();
	}

	void CalculateVelocity() {
		if(!isDashing){
			float targetVelocityX = directionalInput.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (playerCollisionChecker.CollisionData.Below)?accelGrounded:accelAirborne);
			velocity.y += gravity * Time.deltaTime;
		}
	}

	void UpdateVerticalVelocityAfterMove() {
		if (playerCollisionChecker.CollisionData.Above || playerCollisionChecker.CollisionData.Below) {
			if (playerCollisionChecker.CollisionData.IsSlidingDownSlope) { 
				velocity.y += playerCollisionChecker.CollisionData.SlopeNormal.y * -gravity * Time.deltaTime; // big angle = fast fall
			} else {
				velocity.y = 0; // is grounded or cannot go through smthinh
			}
		}
	}

	void FireEvent(PlayerDelegate playerEvent) {
		if(playerEvent != null){
			playerEvent();
		}
	}
}