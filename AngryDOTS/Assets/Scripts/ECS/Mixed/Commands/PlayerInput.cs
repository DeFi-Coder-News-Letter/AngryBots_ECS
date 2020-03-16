using Unity.NetCode;
using Unity.Networking.Transport;

public struct PlayerInput : ICommandData<PlayerInput>
{
    public uint Tick => tick;
    public uint tick;
    public float horizontal;
    public float vertical;
    public float mousePosX;
    public float mousePosZ;
    public int fire;

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        horizontal = reader.ReadFloat();
        vertical = reader.ReadFloat();
        mousePosX = reader.ReadFloat();
        mousePosZ = reader.ReadFloat();
        fire = reader.ReadInt();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteFloat(horizontal);
        writer.WriteFloat(vertical);
        writer.WriteFloat(mousePosX);
        writer.WriteFloat(mousePosZ);
        writer.WriteInt(fire);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, PlayerInput baseline,
        NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    public void Serialize(ref DataStreamWriter writer, PlayerInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }
}

public class PlayerSendCommandSystem : CommandSendSystem<PlayerInput>
{
}

public class PlayerReceiveCommandSystem : CommandReceiveSystem<PlayerInput>
{
}