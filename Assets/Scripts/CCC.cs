using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Rewired;
using DG.Tweening;
using UnityEngine.Networking;
using UnityEditor;

public class CCC : NetworkBehaviour
{
	[Header("Camera Settings")]
	[Tooltip("Sensibilité de la caméra (lorsqu'on la contrôle).")]
	public float CameraSpeed = 1f;

	[Header("Mouvement")]
	[Tooltip("Vitesse frontal du joueur. Devrait être la vitesse la plus rapide du joueur au sol.")]
	public float FrontSpeed = 5f;
	[Tooltip("Vitesse en recul du joueur.")]
	public float BackSpeed = 5f;
	[Tooltip("Vitesse latéral du joueur.")]
	public float SideSpeed = 3f;
	public float WallGravityDistance = 0.1f;
	public float TurnSpeed = 0.1f;

	[Header("Jump")]
	[Tooltip("Force du saut minimum (input le plus court).")]
	public float JumpMinimumForce = 6f;
	[Tooltip("Force supplémentaire maximal en plus de la force minimal (force du saut minimum + input le plus long).")]
	public float JumpAddedForce = 11f;
	[Tooltip("Durée de l'input de Jump le plus long (il faut maintenir le bouton Jump pendant cette durée pour avoir la hauteur de saut maximal).")]
	public float JumpButtonPressDuration = 0.5f;
	[Tooltip("Durée de la Fenêtre de buffer du saut (au cas où le joueur réappuie sur saut juste avant d'atterir).")]
	public float JumpBuffer = 0.05f;
	[Tooltip("Euh... Bah la Gravité quoi.")]
	public float Gravity = 19.81f;

	[Header("Air Control")]
	[Tooltip("Force du Air Control, vitesse à laquelle on agis sur sa trajectoire en l'air. Si trop élevée," +
		"on peut immédiatement dépassé la limite et ainsi n'avoir pas d'effet. Si trop basse, bah ya juste pas d'effet.")]
	public float AirControlPower = 0.25f;
	[Tooltip("Limite de combien on peut agir sur sa trajectoire en l'air (sans faire les abus avec la Caméra). Plus elle est élévée et plus le air contrôle est permissif et inversement.")]
	public float AirControlLimit = 1f;

	[Header("Health")]
	public float HealthTotal = 150;
	[HideInInspector]
	public float Health;
	public float KnockbackOnGround = 5;
	public Material HitMaterial;
	public float HitVisualDuration = 0.25f;

	[Header("Stuff")]
	[Tooltip("Radius de la sphère qui check si le Player est au sol.")]
	public float GroundCheckRadius = 0.1f;
	[Tooltip("Limite de l'angle Y minimale de la caméra lorsque le joueur la contrôle.")]
	[Range(0f, 90f)]
	public float BottomAngleLimit = 70f;
	[Tooltip("Limite de l'angle Y maximale de la caméra lorsque le joueur la contrôle.")]
	[Range(0f, 90f)]
	public float TopAngleLimit = 90f;

	[Header("Prog Stuff")]
	public LayerMask Ground;
	public Transform PlayerTiltZ;
	public Transform PlayerRotY;
	public Transform CamRotX;
	public Transform GroundCheck;
	public Transform Camera;

	[Header("Status")]
	[Tooltip("Le Player est-il au sol ?")]
	public bool _isGrounded = false;
	[Tooltip("Le Player est-il en train de Jump (maximum jusqu'à la Durée de Button Press) ?")]
	public bool _isJumping = false;
	public float CurrentSpeed = 0;

	bool _canJump = true;
	bool _turning = false;

	Rigidbody _body;
	Player _player;
	Renderer _render;

	Material _myMaterial;
	Vector3 _speed;
	Vector3 _velocity2D = Vector3.zero;
	float _velocityGravity = 0;
	Vector3 _knockbackVelocity;

	float _knockbackCooldown = 0;
	float _jumpBuffer = -1;
	float _jumpTime;
	float _yRotation = 0f;
	float _xRotation = 0f;

	int HitResetCount = 0;

	// Use this for initialization
	void Start()
	{
		_render = GetComponent <Renderer> ();
		_myMaterial = _render.material;

		if (isLocalPlayer) {
			Health = HealthTotal;
			_player = ReInput.players.GetPlayer(0);
			_body = GetComponent<Rigidbody>();

			Cursor.lockState = CursorLockMode.None;
			Cursor.lockState = CursorLockMode.Locked;
			_yRotation = _body.rotation.eulerAngles.y;

			Camera = GameObject.FindGameObjectWithTag ("MainCamera").transform;
			Camera.parent = CamRotX;
			Camera.localPosition = Vector3.zero;
			Camera.localEulerAngles = Vector3.zero;
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (isLocalPlayer) {

			if (Input.GetKeyDown (KeyCode.KeypadPlus)) {
				CameraSpeed += 0.5f;
				if (CameraSpeed == 10) {
					CameraSpeed = 9.5f;
				}
			}
			else if (Input.GetKeyDown (KeyCode.KeypadMinus)) {
				CameraSpeed -= 0.5f;
				if (CameraSpeed == 0) {
					CameraSpeed = 0.5f;
				}
			}

			//Checking Air/Ground State & GRAVITY---------------
			if (!_isGrounded) {
				_isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, Ground);
				_canJump = false;

				if (_isGrounded) {
					_velocityGravity = 0;
					_canJump = true;
				}
				else {
					_velocityGravity -= Gravity * Time.deltaTime;
				}
			}
			else {
				_isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, Ground);
			}

			if (_isGrounded && (_knockbackCooldown <= 0)) {
				_knockbackVelocity = Vector3.zero;
			}


			//ROTATION-------------------------------------------

			//we store the input used for rotation
			if (true) {
				float rotx;
				float roty;

				rotx = _player.GetAxis("Look Horizontal") * CameraSpeed * 0.1f;
				roty = -_player.GetAxis("Look Vertical") * CameraSpeed * 0.1f;

				//we store the rotation along Y axis
				//because physics functions have to be called in FixedUpdate
				//but inputs have to be processed in Update
				_yRotation += rotx * Mathf.Rad2Deg * Time.deltaTime;
				Vector3 rot = PlayerRotY.localEulerAngles;
				rot.y = _yRotation;
				PlayerRotY.localEulerAngles = rot;

				//since we don't use the rigidbody to rotate the camera along the local X axis
				//we can directly modify the transform
				//note also that the camera has no collider attached to it that could interfere with the rigidbody
				_xRotation += roty * Time.deltaTime * Mathf.Rad2Deg;
				_xRotation = Mathf.Clamp(_xRotation, -TopAngleLimit, BottomAngleLimit);
				rot = CamRotX.localEulerAngles;
				rot.x = _xRotation;
				CamRotX.localEulerAngles = rot;
			}

			//MOVEMENT-----------------------------------------------
			if (_isGrounded) {

				Vector3 vertical = PlayerRotY.localRotation * Vector3.forward * _player.GetAxisRaw ("Move Vertical");
				Vector3 horizontal = PlayerRotY.localRotation * Vector3.right * _player.GetAxisRaw ("Move Horizontal");

				float bufferHigherSpeed; 
				if (_player.GetAxisRaw ("Move Vertical") >= 0) {
					_speed = vertical * FrontSpeed + horizontal * SideSpeed;
					bufferHigherSpeed = FrontSpeed;
				}
				else {
					_speed = vertical * BackSpeed + horizontal * SideSpeed;
					bufferHigherSpeed = BackSpeed;
				}

				if (_speed.magnitude > bufferHigherSpeed) {
					_speed = _speed.normalized * bufferHigherSpeed;
				}

				_velocity2D = _speed;
			}
			//AirControl------------------------------------------------------------
			else {
				_speed = PlayerRotY.localRotation * Vector3.forward * _player.GetAxisRaw ("Move Vertical") +
					PlayerRotY.localRotation * Vector3.right * _player.GetAxisRaw ("Move Horizontal");
				if (_speed.magnitude > 1) {
					_speed.Normalize ();
				}
				_speed *= AirControlPower * Time.deltaTime * 60;

				Vector3 oldVelocity = _velocity2D;

				if (Vector3.Angle (_velocity2D, _speed) <= 90) {
					float _proj = ((_velocity2D.x * _speed.x) + (_velocity2D.z * _speed.z)) / ((_speed.x * _speed.x) + (_speed.z * _speed.z));
					_proj *= new Vector2 (_speed.x, _speed.z).magnitude;
					if (_proj < AirControlLimit) {
						_velocity2D += _speed;
					}
				}
				else {
					_velocity2D += _speed;
				}

				Vector3 newVelocity = _velocity2D;

				if (newVelocity.magnitude >= oldVelocity.magnitude) {
					newVelocity = newVelocity.normalized * _velocity2D.magnitude;
				}

				_velocity2D = newVelocity;
			}



			//GRAVITY BOOT-------------------------------------------------------
			RaycastHit hit;
			if (!_turning && !_isGrounded) {

				Vector3 vertical = PlayerRotY.localRotation * transform.forward * _player.GetAxisRaw ("Move Vertical");
				Vector3 horizontal = PlayerRotY.localRotation * transform.right * _player.GetAxisRaw ("Move Horizontal");
				Vector3 directionalInput = vertical + horizontal;

				if ((_body.velocity.magnitude <= 0.1) || ((_body.velocity - transform.up * _velocityGravity).magnitude <= 0.1)) {
					


					if (Physics.Raycast(transform.position, directionalInput.normalized, out hit, WallGravityDistance * 10, Ground)) {
						if (hit.normal != transform.up) {
							_turning = true;
							transform.DOKill();
							transform.DORotateQuaternion (Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation, TurnSpeed);
							DOVirtual.DelayedCall(TurnSpeed, () =>
								{
									_turning = false;
								});
						}
					}
				}
				else {
					if (Physics.Raycast(transform.position, _body.velocity.normalized, out hit, WallGravityDistance * (2.5f + _body.velocity.magnitude), Ground)) {
						if (hit.normal != transform.up) {
							_turning = true;
							transform.DOKill();
							transform.DORotateQuaternion (Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation, TurnSpeed);
							DOVirtual.DelayedCall(TurnSpeed, () =>
								{
									_turning = false;
								});
						}
					}
				}

			}

			//JUMP--------------------------------------------------
			if (_player.GetButtonDown ("Jump") && !_isJumping) {
					
				if (_canJump) {
					_jumpBuffer = -1;
					_isJumping = true;
					_velocityGravity = JumpMinimumForce;
					_canJump = false;
					_jumpTime = JumpButtonPressDuration;
				}
				else {
					_jumpBuffer = JumpBuffer;
				}
			}
			else if (_jumpBuffer >= 0) {

				if (!_isJumping && _canJump) {
					_jumpBuffer = -1;
					_isJumping = true;
					_velocityGravity = JumpMinimumForce;
					_canJump = false;
					_jumpTime = JumpButtonPressDuration;
				}
				else {
					_jumpBuffer -= Time.deltaTime;
				}
			}
			else if (_player.GetButton ("Jump") && _isJumping)
			{
				if (_jumpTime <= Time.deltaTime) {
					_isJumping = false;
					_velocityGravity += _jumpTime * JumpAddedForce;
				}
				else {
					_jumpTime -= Time.deltaTime;
					_velocityGravity += Time.deltaTime * JumpAddedForce;
				}
			}
			else {
				_isJumping = false;
			}
				
			//TESTING STUFF-----------------------------------------
			if (Input.GetKey(KeyCode.T)) {

			}
		}
	}

	void FixedUpdate()
	{
		if (isLocalPlayer) {

			_knockbackCooldown --;

			//MOUVEMENT & JUMP & GRAVITY-----------------------------
			Vector3 newSpeed = (transform.right * _velocity2D.x + transform.up * _velocityGravity + transform.forward * _velocity2D.z) + _knockbackVelocity;
			CurrentSpeed = newSpeed.magnitude;
			_body.velocity = newSpeed;	
		}
	}

	void OnCollisionEnter (Collision collision) {
		if (Ground == (Ground | (1 << collision.collider.gameObject.layer))) {
			if (_knockbackCooldown != KnockbackOnGround) {
				_knockbackCooldown = 0;
				_knockbackVelocity = Vector3.zero;
			}
		}
	}

	void OnCollisionStay (Collision collision) {

		/*
		if (collision.collider.tag == "Platform") 
		{
			Collider[] _sphereHit = Physics.OverlapSphere (GroundCheck.position, GroundCheckRadius);
			if (_sphereHit.Length > 0) 
			{
				for (int i = 0; i < _sphereHit.Length; i++)
				{
					if (_sphereHit[i].transform != transform.parent && _sphereHit[i].tag == "Platform") 
					{
						transform.parent = _sphereHit[i].transform.parent.parent.transform;
						i = _sphereHit.Length;
					}
				}
			}
		}
		*/
	}

	void OnCollisionExit (Collision collision) {

		/*
		if (collision.collider.tag == "Platform") {
			transform.parent = null;
		}
		*/
	}

	void OnTriggerEnter (Collider collider) {

	}

	public void TakeKnockback (Vector3 Knockback, float damage) {

		if (isLocalPlayer) {
			_knockbackCooldown = KnockbackOnGround;
			_knockbackVelocity = Knockback;
			Health -= damage;
		}
		else {
			if (HitMaterial != null) {
				_render.material = HitMaterial;
				HitResetCount++;
				DOVirtual.DelayedCall(HitVisualDuration, () =>
					{
						HitResetCount--;
						if (HitResetCount == 0) {
							_render.material = _myMaterial;
						}
					});
			}
		}

	}
}
