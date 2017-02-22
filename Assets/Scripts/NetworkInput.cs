using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using DG.Tweening;

[NetworkSettings (sendInterval = .03f, channel = 0)]
public class NetworkInput : NetworkBehaviour
{
	[SyncVar]
	public Vector3 realPosition = Vector3.zero;
	[SyncVar]
	public Vector3 realVelocity;
	[SyncVar]
	public Vector3 realRotation;
	//		private float lastTime;

	public float alpha;

	void Update ()
	{

		//		alpha = 0.1f / (Time.time - lastTime) / .03f;

		if (isLocalPlayer) {
			realPosition = transform.position;
			realVelocity = GetComponent<Rigidbody> ().velocity;
			CmdSync (transform.position, GetComponent<Rigidbody> ().velocity, transform.eulerAngles);
		} else {

			transform.DOKill ();
			transform.DOMove (realPosition, 0.1f, false);
			transform.DORotate (realRotation, 0.1f);
			GetComponent<Rigidbody> ().velocity = realVelocity;	
		}
	}

	[Command]
	void CmdSync (Vector3 position, Vector3 velocity, Vector3 rotation)
	{
		realPosition = position;
		realVelocity = velocity;
		realRotation = rotation;
	}
}