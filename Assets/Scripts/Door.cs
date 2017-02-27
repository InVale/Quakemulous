using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DG.Tweening;

public class Door : NetworkBehaviour {

	public float DoorOpenTime = 5;
	public float DoorOpenSpeed = 0.25f;
	public Transform LeftDoor;
	public Transform RightDoor;

	public void OpenDoor () {
		CmdOpen();
	}

	[Command]
	void CmdOpen () {
		RpcOpen();
		Open();
	}

	[ClientRpc]
	void RpcOpen () {
		Open();
	}

	void Open () {
		LeftDoor.DOKill();
		LeftDoor.DOScaleX(0, DoorOpenSpeed);
		RightDoor.DOKill();
		RightDoor.DOScaleX(0, DoorOpenSpeed);

		DOVirtual.DelayedCall(DoorOpenTime, () =>
			{
				Close();
			});
	}

	void Close () {
		LeftDoor.DOKill();
		LeftDoor.DOScaleX(1, DoorOpenSpeed);
		RightDoor.DOKill();
		RightDoor.DOScaleX(1, DoorOpenSpeed);
	}
}
