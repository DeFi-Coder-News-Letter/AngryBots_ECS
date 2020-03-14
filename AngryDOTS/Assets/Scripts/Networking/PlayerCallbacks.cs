using UnityEngine;

[BoltGlobalBehaviour("ECS Shooter")]
public class PlayerCallbacks : Bolt.GlobalEventListener
{
    public override void SceneLoadLocalDone(string map)
    {

    }

    public override void ControlOfEntityGained(BoltEntity entity)
    {
        // this tells the player camera to look at the entity we are controlling
        var brain = Camera.main.gameObject.GetComponent<Cinemachine.CinemachineBrain>();
        brain.ActiveVirtualCamera.Follow = entity.gameObject.transform;
    }
}