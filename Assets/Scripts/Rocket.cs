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

	void OnTriggerEnter (Collider collider) {

		if (collider.tag == "Door") {
			collider.transform.parent.parent.parent.GetComponent<Door>().OpenDoor();

			Destroy(transform.gameObject);
		}
		else if (collider.gameObject != ID){
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
							closestPoint = closestPoint - Contact.position;

							float _knockbackForce  = Knockback * ((ExplosionRadius - closestPoint.magnitude) / ExplosionRadius);
							Vector3 _imprimedKnockback = _knockbackForce * _originToPoint.normalized;
							float _appliedDamage = Damage * ((ExplosionRadius - closestPoint.magnitude) / ExplosionRadius);
							_sphereHit[i].gameObject.GetComponent<CCC>().TakeKnockback(_imprimedKnockback, _appliedDamage);
						}
					}
				}
			}

			Destroy(transform.gameObject);
		}
	}
}
