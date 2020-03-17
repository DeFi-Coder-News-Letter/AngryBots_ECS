using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ECSSyncObject : Bolt.EntityBehaviour<IEelsState>
{
    public Vector3[] EelTransforms = new Vector3[20];

    public int Count;

    static ECSSyncObject instance;
    static public ECSSyncObject Instance { get { return instance; } }

    public EnemySpawner Spawner;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        for (int i = 0; i < 20; ++i)
            ECSSyncObject.Instance.EelTransforms[i] = new Vector3();

        Spawner = GameObject.Find("Enemy Spawner").GetComponent<EnemySpawner>(); 
    }

    public void FixedUpdate()
    {
        if (!GetComponent<BoltEntity>().HasControl)
        {
            for (int i = Count; i < state.ValidElements; ++i)
            {
                Spawner.Spawn();
            }

            Count = state.ValidElements;
            for (int i = 0; i < Count; ++i)
            {
                EelTransforms[i] = state.Positions[i];
            }
        }
    }

    // Only called by server
    public override void SimulateController()
    {
        state.ValidElements = Count;
        for (int i = 0; i < state.ValidElements; ++i)
        {
            state.Positions[i] = EelTransforms[i];
        }
        
    }
    // When servers sends new positions
}
