using UnityEngine;

public class MainPlayerObject
{
    public BoltEntity character;
    public BoltConnection connection;

    public bool IsServer
    {
        get { return connection == null; }
    }

    public bool IsClient
    {
        get { return connection != null; }
    }

    public void Spawn()
    {
        if (!character)
        {
            character = BoltNetwork.Instantiate(BoltPrefabs.Player, RandomPosition(), Quaternion.identity);

            if (IsServer)
            {
                character.TakeControl();
            }
            else
            {
                character.AssignControl(connection);
            }
        }

        // teleport entity to a random spawn position
        character.transform.position = RandomPosition();
    }

    Vector3 RandomPosition()
    {
        float x = Random.Range(-5f, +5f);
        float z = Random.Range(-5f, +5f);
        return new Vector3(x, 10f, z);
    }

}