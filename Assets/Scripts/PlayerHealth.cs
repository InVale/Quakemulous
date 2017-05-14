using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool>{}

public class PlayerHealth : NetworkBehaviour {

	[Header("Health")]
	public float HealthTotal = 150;
	[SyncVar]
	public float Health;
	[SyncVar]
	public int Kill;
	[SyncVar]
	public int Death;
	public float RespawnTime;
	public ToggleEvent onToggleAlive;

	HUDHandler _myHUD;

	// Use this for initialization
	void Start () {
		Health = HealthTotal;

		if (isLocalPlayer) {
			GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
			if(canvas == null) {
				Debug.LogError("There should be a Canvas Prefab in the scene !");
			}
			else {
				_myHUD = canvas.GetComponent<HUDHandler>();
			}
		}
	}

	void Update () {
		
		if (isLocalPlayer) {
			if (_myHUD != null) {
				_myHUD.HealthData(HealthTotal, Health);
				_myHUD.ScoreData(Kill, Death);
			}
		}
	}

	public void UpdateHealth (float value, GameObject id) {
		Health += value;
		if (Health <= 0) {
			Health = 0;
			if (id != null) {
				id.GetComponent<PlayerHealth>().Kill ++;
			}
			Death ++;
			onToggleAlive.Invoke(false);
			RpcDead();
			Invoke ("Respawn", RespawnTime);
		}
	}

	[ClientRpc]
	void RpcDead () {
		onToggleAlive.Invoke(false);
		if (isLocalPlayer) {
			Transform spawn = NetworkManager.singleton.GetStartPosition();
			transform.position = spawn.position;
			transform.rotation = spawn.rotation;
		}
	}

	void Respawn () {
		Health = HealthTotal;
		onToggleAlive.Invoke(true);
		RpcRespawn();
	}

	[ClientRpc]
	void RpcRespawn () {
		onToggleAlive.Invoke(true);
	}
}
