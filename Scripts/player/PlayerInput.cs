using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {

	public delegate void PlayerAxisInputDelegate(Vector2 input);	
	public delegate void PlayerKeyInputDelegate();

	public event PlayerAxisInputDelegate directionalInputEvent;
	public event PlayerKeyInputDelegate jumpPressedEvent;
	public event PlayerKeyInputDelegate jumpReleasedEvent;
	public event PlayerKeyInputDelegate dashPressedEvent;

	void Update () {
		if(directionalInputEvent != null) {
			directionalInputEvent(new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")));
		}

		if (Input.GetKeyDown (KeyCode.JoystickButton0) && jumpPressedEvent != null){
			jumpPressedEvent();
		}
		
		if (Input.GetKeyUp (KeyCode.JoystickButton0) && jumpReleasedEvent != null){
			jumpReleasedEvent();
		}

		if (Input.GetKeyDown (KeyCode.JoystickButton1) && dashPressedEvent != null){
			dashPressedEvent();
		}
	}
}