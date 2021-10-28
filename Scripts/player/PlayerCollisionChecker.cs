using UnityEngine;
using System.Collections;

public class PlayerCollisionChecker : RaycastController {

	public LayerMask collisionMask;
	public float maxSlopeAngle = 60;

	public CollisionData CollisionData {get; private set;}
	[HideInInspector]
	public Vector2 PlayerInput {get; private set;}
	[HideInInspector]
	public int FaceDirection;

	public override void Start() {
		base.Start ();
		this.CollisionData = new CollisionData();
		this.FaceDirection = 1;
	}

	public void Move(Vector2 moveAmount, bool standingOnPlatform) {
		Move (moveAmount, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false) {
		UpdateRaycastOrigins ();

		CollisionData.Reset ();
		CollisionData.MoveAmountOld = moveAmount;
		PlayerInput = input;

		if (moveAmount.y < 0) {
			DescendSlope(ref moveAmount);
		}

		if (moveAmount.x != 0) {
			this.FaceDirection = (int)Mathf.Sign(moveAmount.x);
		}

		HorizontalCollisions (ref moveAmount);

		if (moveAmount.y != 0) {
			VerticalCollisions (ref moveAmount);
		}

		transform.Translate (moveAmount);

		if (standingOnPlatform) {
			CollisionData.Below = true;
		}
	}

	void HorizontalCollisions(ref Vector2 moveAmount) {
		float directionX = this.FaceDirection;
		float rayLength = Mathf.Abs (moveAmount.x) + skinWidth;

		if (Mathf.Abs(moveAmount.x) < skinWidth) {
			rayLength = 2*skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX,Color.red);

			if (hit) {
				if (hit.distance == 0 || hit.collider.tag == "Through") {
					continue;
				}

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxSlopeAngle) {
					if (CollisionData.IsDescendingSlope) {
						CollisionData.IsDescendingSlope = false;
						moveAmount = CollisionData.MoveAmountOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != CollisionData.PreviousSlopeAngle) {
						distanceToSlopeStart = hit.distance-skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
					moveAmount.x += distanceToSlopeStart * directionX;
				}

				if (!CollisionData.IsClimbingSlope || slopeAngle > maxSlopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (CollisionData.IsClimbingSlope) {
						moveAmount.y = Mathf.Tan(CollisionData.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
					}

					CollisionData.Left = directionX == -1;
					CollisionData.Right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector2 moveAmount) {
		float directionX = this.FaceDirection;
		float directionY = Mathf.Sign (moveAmount.y);
		float rayLength = Mathf.Abs (moveAmount.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i ++) {

			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY,Color.red);

			if (hit) {
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) {
						continue;
					}
					if (CollisionData.IsFallingThroughPlatform) {
						continue;
					}
					if (PlayerInput.y == -1) {
						CollisionData.IsFallingThroughPlatform = true;
						Invoke("ResetFallingThroughPlatform",.5f);
						continue;
					}
				}

				moveAmount.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (CollisionData.IsClimbingSlope) {
					moveAmount.x = moveAmount.y / Mathf.Tan(CollisionData.SlopeAngle * Mathf.Deg2Rad) * directionX;
				}

				CollisionData.Below = directionY == -1;
				CollisionData.Above = directionY == 1;
			}
		}

		if (CollisionData.IsClimbingSlope) {
			rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != CollisionData.SlopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					CollisionData.SlopeAngle = slopeAngle;
					CollisionData.SlopeNormal = hit.normal;
				}
			}
		}
	}

	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) {
		float moveDistance = Mathf.Abs (moveAmount.x);
		float climbmoveAmountY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (moveAmount.y <= climbmoveAmountY) {
			moveAmount.y = climbmoveAmountY;
			moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
			CollisionData.Below = true;
			CollisionData.IsClimbingSlope = true;
			CollisionData.SlopeAngle = slopeAngle;
			CollisionData.SlopeNormal = slopeNormal;
		}
	}

	void DescendSlope(ref Vector2 moveAmount) {

		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast (raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast (raycastOrigins.bottomRight, Vector2.down, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);
		if (maxSlopeHitLeft ^ maxSlopeHitRight) {
			SlideDownSlope (maxSlopeHitLeft, ref moveAmount);
			SlideDownSlope (maxSlopeHitRight, ref moveAmount);
		}

		if (!CollisionData.IsSlidingDownSlope) {
			float directionX = Mathf.Sign (moveAmount.x);
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
					if (Mathf.Sign (hit.normal.x) == directionX) {
						if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (moveAmount.x)) {
							float moveDistance = Mathf.Abs (moveAmount.x);
							float descendmoveAmountY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
							moveAmount.y -= descendmoveAmountY;

							CollisionData.SlopeAngle = slopeAngle;
							CollisionData.IsDescendingSlope = true;
							CollisionData.Below = true;
							CollisionData.SlopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}

	void SlideDownSlope(RaycastHit2D hit, ref Vector2 moveAmount) {
		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle) {
				moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs (moveAmount.y) - hit.distance) / Mathf.Tan (slopeAngle * Mathf.Deg2Rad);

				CollisionData.SlopeAngle = slopeAngle;
				CollisionData.IsSlidingDownSlope = true;
				CollisionData.SlopeNormal = hit.normal;
			}
		}

	}

	void ResetFallingThroughPlatform() {
		CollisionData.IsFallingThroughPlatform = false;
	}		
}