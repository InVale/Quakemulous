using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Rewired;
using DG.Tweening;

public class Interactions : NetworkBehaviour {

	public float RocketFireSpeed = 1;
	public float RocketSpeed = 10;
	public float RocketExplosionRadius = 1;
	public float RocketKnockback = 10;
	public float RocketDammage = 100;

	public GameObject RocketPrefab;
	public Transform RocketSpawn;

	bool _canFireRocket = true;

	Player _player;

	void Start()
	{
		if (isLocalPlayer) {
			_player = ReInput.players.GetPlayer(0);
		}
	}

	// Update is called once per frame
	void Update () {
		if (isLocalPlayer) {

			if (_canFireRocket && _player.GetButtonDown("Fire")) {
				CmdFire(RocketSpawn.position, RocketSpawn.rotation);
				_canFireRocket = false;
				DOVirtual.DelayedCall(RocketFireSpeed, () =>
					{
						_canFireRocket = true;
					});
			}

		}
	}

	[Command]
	void CmdFire(Vector3 myPosition, Quaternion myRotation)
	{
		var _rocket = (GameObject)Instantiate(RocketPrefab, myPosition, myRotation);
		_rocket.transform.GetComponent<Rigidbody>().velocity = _rocket.transform.forward * RocketSpeed;
		_rocket.transform.rotation *= Quaternion.Euler(90, 0, 0);
		_rocket.transform.GetComponent<Rocket>().ExplosionRadius = RocketExplosionRadius;
		_rocket.transform.GetComponent<Rocket>().Knockback = RocketKnockback;
		_rocket.transform.GetComponent<Rocket>().Dammage = RocketDammage;
		_rocket.transform.GetComponent<Rocket>().ID = gameObject;

		NetworkServer.Spawn(_rocket);
	}
}
