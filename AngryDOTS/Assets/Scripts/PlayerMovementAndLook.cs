using Bolt;
using UnityEngine;

public class PlayerMovementAndLook : Bolt.EntityBehaviour<IMainPlayerState>
{
	[Header("Camera")]
	public Camera mainCamera;

	[Header("Movement")]
	public float speed = 4.5f;
	public LayerMask whatIsGround;

	[Header("Life Settings")]
	public float playerHealth = 1f;

	[Header("Animation")]
	public Animator playerAnimator;

	[Header("Player Identity")]
	public int idx;
	
	public float fireRate = .1f;

	Rigidbody playerRigidbody;

 
	float timer;
	PlayerShooting playerShooting;


	public bool IsDead 
	{
		get{ return isDead;}
	}

	private bool isDead;

	void Awake()
	{
		playerRigidbody = GetComponent<Rigidbody>();
		playerShooting = GetComponent<PlayerShooting>();
		mainCamera = Camera.main;
	}
	public override void Attached()
	{
		// This couples the Transform property of the State with the GameObject Transform
		state.SetTransforms(state.Transform, transform);
		state.SetAnimator(GetComponent<Animator>());
		//state.AddCallback("LookingAt", () => TurnThePlayer(state.LookingAt));

		state.Animator.SetLayerWeight(0, 1);
		state.Animator.SetLayerWeight(1, 1);

		state.OnFire = () =>
		{
			playerShooting.Fire();
		};
	}

	//Only runs in owner of the game object
	public override void SimulateController()
	{
		if (isDead)
			return;

		//Arrow Key Input
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		Vector3 inputDirection = new Vector3(h, 0, v);

		//Camera Direction
		var cameraForward = mainCamera.transform.forward;
		var cameraRight = mainCamera.transform.right;

		cameraForward.y = 0f;
		cameraRight.y = 0f;

		//Try not to use var for roadshows or learning code
		Vector3 desiredDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;

		Vector3 moveVector = desiredDirection;
		Vector3 lookAtVector = Vector3.zero;

		timer += Time.deltaTime;
		bool fire = false;
		if (Input.GetButton("Fire1") && timer >= fireRate && !IsDead)
		{

			fire = true;
			timer = 0f;
		}
		
		if (Application.isFocused) // workaround to prevent all the players to look in the same dir when playin in the same pc
		{
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, whatIsGround))
			{
				Vector3 playerToMouse = hit.point - transform.position;
				playerToMouse.y = 0f;
				lookAtVector = playerToMouse.normalized;
			}
		}

		IMainPlayerCommandInput input = MainPlayerCommand.Create();

		input.MoveVector = moveVector;
		input.Fire = fire;
		input.LookAtVector = lookAtVector;

		entity.QueueInput(input);

	}

	public override void ExecuteCommand(Command command, bool resetState)
	{
		MainPlayerCommand cmd = command as MainPlayerCommand;
		if(resetState)
		{
			playerRigidbody.position = cmd.Result.Position;
			playerRigidbody.velocity = cmd.Result.Velocity ;

		}
		else
		{
			MoveThePlayer(cmd.Input.MoveVector);
			TurnThePlayer(cmd.Input.LookAtVector);

			//cmd.Result.Position = transform.position;
			cmd.Result.Position = playerRigidbody.position;
			cmd.Result.Velocity = playerRigidbody.velocity;

			if (cmd.IsFirstExecution)
			{
				if (cmd.Input.Fire)
				{
					playerShooting.Fire();
					// Notify third party that this player has fired
					state.Fire();
				}
				AnimateThePlayer(cmd.Input.MoveVector);			
			}
		}
	}


	void MoveThePlayer(Vector3 desiredDirection)
	{
		Vector3 movement = new Vector3(desiredDirection.x, 0f, desiredDirection.z);
		movement = movement.normalized * speed * Time.deltaTime;

		playerRigidbody.MovePosition(transform.position + movement);

	}

	void TurnThePlayer(Vector3 arg)
	{
		Quaternion newRotation = Quaternion.LookRotation(arg);
		playerRigidbody.MoveRotation(newRotation);
	}

	void AnimateThePlayer(Vector3 desiredDirection)
	{
		if(!playerAnimator)
			return;

		Vector3 movement = new Vector3(desiredDirection.x, 0f, desiredDirection.z);
		float forw = Vector3.Dot(movement, transform.forward);
		float stra = Vector3.Dot(movement, transform.right);

		playerAnimator.SetFloat("Forward", forw);
		playerAnimator.SetFloat("Strafe", stra);
	}

	//Player Collision
	void OnTriggerEnter(Collider theCollider)
	{
		if (!theCollider.CompareTag("Enemy"))
			return;

		playerHealth--;

		if(playerHealth <= 0)
		{
			Settings.PlayerDied(idx);
		}
	}

	public void PlayerDied()
	{
		if (isDead)
			return;

		isDead = true;

		playerAnimator.SetTrigger("Died");
		playerRigidbody.isKinematic = true;
		GetComponent<Collider>().enabled = false;
	}
}
