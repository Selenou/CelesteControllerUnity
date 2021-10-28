using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component {

	private static T instance;

	public static T Instance {
		get {
			if ( instance == null ) {
				instance = FindObjectOfType<T> ();
				if ( instance == null ) {
					GameObject obj = new GameObject ();
					obj.name = typeof ( T ).Name;
					instance = obj.AddComponent<T> ();
				}
			}
			return instance;
		}
	}

	protected virtual void Awake () {
		if ( instance == null ) {
			instance = this as T;
			DontDestroyOnLoad ( gameObject );
		}
		else {
			Destroy ( gameObject );
		}
	}
}