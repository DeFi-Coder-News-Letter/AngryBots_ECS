using System;
using Unity.Entities;

[Serializable]
public struct PlayerTag : IComponentData 
{
    public int playerIdx;
    public int playerId;
}