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
			float3 pos = transform.position;
			var playerPositions = Settings.PlayerPositions;
			int minIdx = 0;
			var dir = playerPositions[0] - pos;
			var minDist = math.dot(dir, dir);
			for (int i = 1; i < playerPositions.Length; ++i)
			{
				dir = playerPositions[i] - pos;
				var dist = math.dot(dir, dir);
				if (dist < minDist)
				{
					minDist = dist;
					minIdx = i;
				}
			}

			Vector3 heading = playerPositions[minIdx] - pos;
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
