using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Rocket : NetworkBehaviour {

	public Transform Contact;
	public LayerMask HardObjects;

	[SyncVar]
	public float ExplosionRadius = 1;
	[SyncVar]
	public float Knockback = 1;
	[SyncVar]
	public float Damage = 1;
	[SyncVar]
	public GameObject ID;

	void OnCollisionEnter (Collision Collision) {

		if (Collision.collider.tag == "Door") {
			Collision.collider.transform.parent.parent.parent.GetComponent<Door>().OpenDoor();
		}
		else {
			Collider[] _sphereHit = Physics.OverlapSphere (Contact.position, ExplosionRadius);
			if (_sphereHit.Length > 0) 
			{
				for (int i = 0; i < _sphereHit.Length; i++)
				{
					if (_sphereHit[i].tag == "Player" && _sphereHit[i].gameObject != ID) 
					{
						Vector3 _originToPoint = _sphereHit[i].transform.position - Contact.position;
						RaycastHit Hit;

						if (!Physics.Raycast(Contact.position, _originToPoint, out Hit, _originToPoint.magnitude, HardObjects)) {

							Vector3 closestPoint = _sphereHit [i].ClosestPointOnBounds (Contact.position);

							float _knockbackForce  = Knockback * ((ExplosionRadius - closestPoint.magnitude) / ExplosionRadius);
							Debug.Log (ExplosionRadius - closestPoint.magnitude);
							Vector3 _imprimedKnockback = _knockbackForce * closestPoint.normalized;
							float _appliedDamage = Damage * ((ExplosionRadius - closestPoint.magnitude) / ExplosionRadius);
							_sphereHit[i].gameObject.GetComponent<CCC>().TakeKnockback(_imprimedKnockback, _appliedDamage);
						}
					}
				}
			}
		}

		Destroy(transform.gameObject);
	}
}
