using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Rewired;
using DG.Tweening;
using System;
using System.IO;

public class Interactions : NetworkBehaviour {

	[Serializable]
	public class ProjectilLevel {
		public float minSpeedValue = 0;
		public float FireSpeed = 1;
		public float TravelSpeed = 10;
		public float ExplosionRadius = 3;
		public float Knockback = 25;
		public float Damage = 100;
	}

	public ProjectilLevel[] RocketLevel = new ProjectilLevel[1];

	public GameObject RocketPrefab;
	public Transform RocketSpawn;

	bool _canFireRocket = true;

	Player _player;
	CCC _ccc;

	void Start()
	{
		if (isLocalPlayer) {
			_player = ReInput.players.GetPlayer(0);
			_ccc = gameObject.GetComponent <CCC> ();
		}
	}

	// Update is called once per frame
	void Update () {
		if (isLocalPlayer) {

			if (_canFireRocket && _player.GetButtonDown("Fire")) {

				int currentLevel = 0;
				float myCurrentSpeed = _ccc.CurrentSpeed;
				for (int i = 0; i < RocketLevel.Length; i++) {
					if (myCurrentSpeed >= RocketLevel[i].minSpeedValue) {
						currentLevel = i;
					}
					else {
						break;
					}
				}

				CmdFire(RocketSpawn.position, RocketSpawn.rotation, currentLevel);
				_canFireRocket = false;
				DOVirtual.DelayedCall(RocketLevel[currentLevel].FireSpeed, () =>
					{
						_canFireRocket = true;
					});
			}

		}
	}

	[Command]
	void CmdFire(Vector3 myPosition, Quaternion myRotation, int myLevel)
	{
		var _rocket = (GameObject)Instantiate(RocketPrefab, myPosition, myRotation);
		_rocket.transform.GetComponent<Rigidbody>().velocity = _rocket.transform.forward * RocketLevel[myLevel].TravelSpeed;
		_rocket.transform.rotation *= Quaternion.Euler(90, 0, 0);
		_rocket.transform.GetComponent<Rocket>().ExplosionRadius = RocketLevel[myLevel].ExplosionRadius;
		_rocket.transform.GetComponent<Rocket>().Knockback = RocketLevel[myLevel].Knockback;
		_rocket.transform.GetComponent<Rocket>().Damage = RocketLevel[myLevel].Damage;
		_rocket.transform.GetComponent<Rocket>().ID = gameObject;

		NetworkServer.Spawn(_rocket);
	}
}
