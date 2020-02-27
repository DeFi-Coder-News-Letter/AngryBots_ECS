using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	[Header("Movement")]
	public float speed = 2f;

	[Header("Life Settings")]
	public float enemyHealth = 1f;

	Rigidbody rigidBody;


	void Start()
	{
		rigidBody = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (Settings.AnyPlayerAlive())
		{
			var playerPositions = Settings.PlayerPositions;
			var minDirPlayer = playerPositions[0];
			var minDist = 0.0f;
			float3 pos = transform.position;
			for (int i = 1; i < playerPositions.Length; ++i)
			{
				var dirPlayer = playerPositions[i] - pos;
				var dist = math.dot(dirPlayer, dirPlayer);
				if (dist < minDist)
				{
					minDist = dist;
					minDirPlayer = dirPlayer;
				}
			}

			Vector3 heading = minDirPlayer;
			heading.y = 0f;
			transform.rotation = Quaternion.LookRotation(heading);
		}

		Vector3 movement = transform.forward * speed * Time.deltaTime;
		rigidBody.MovePosition(transform.position + movement);
	}

	//Enemy Collision
	void OnTriggerEnter(Collider theCollider)
	{
		if (!theCollider.CompareTag("Bullet"))
			return;

		enemyHealth--;

		if(enemyHealth <= 0)
		{
			Destroy(gameObject);
			BulletImpactPool.PlayBulletImpact(transform.position);
		}
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		manager.AddComponent(entity, typeof(EnemyTag));
		manager.AddComponent(entity, typeof(MoveForward));

		MoveSpeed moveSpeed = new MoveSpeed { Value = speed };
		manager.AddComponentData(entity, moveSpeed);

		Health health = new Health { Value = enemyHealth };
		manager.AddComponentData(entity, health);
	}
}
