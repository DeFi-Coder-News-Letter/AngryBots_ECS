using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;

//public struct GameState : IComponentData
//public unsafe struct GameState : IBufferElementData
//{
//	public int playersAlive;
//	public int playersCount;
//	//public NativeArray<int> playerIds;
//	public fixed int playerIds[10];
//	//public NativeArray<float3> playerPositions;
//	public fixed float playerPositionsX[10];
//	public fixed float playerPositionsY[10];
//	public fixed float playerPositionsZ[10];
//}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(CollisionSystem))]
public class PlayerTransformUpdateSystem : ComponentSystem
{
	// Query to obtain all the players
	EntityQuery playerGroup;

	protected override void OnCreate()
	{
		playerGroup = GetEntityQuery(ComponentType.ReadOnly<Health>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PlayerComponent>());

		//var state = EntityManager.Instantiate();
		//EntityManager.AddComponentData(state, new GameState { });
		//SetSingleton<GameState>(new GameState { });

		//var entityGameState = EntityManager.CreateEntity();
		//EntityManager.AddBuffer<GameState>(entityGameState);
		//EntityManager.GetBuffer<GameState>()

		//EntityManager.CreateEntity(ComponentType.ReadOnly<GameState>());
		//SetSingleton(new GameState { playersAlive = 0, playersCount  = 0,
		//	playerIds = new NativeArray<int>(10, Allocator.Persistent),
		//	playerPositions = new NativeArray<float3>(10, Allocator.Persistent)
		//});
	}

	protected override void OnUpdate()
	{
		//if (!Settings.AnyPlayerAlive())
		//{
		//	return;
		//}
		//GameState state = GetSingleton<GameState>();

		var healths = playerGroup.ToComponentDataArray<Health>(Allocator.TempJob);
		Settings.PlayersAlive = healths.Length;
		healths.Dispose();

		var playersComp = playerGroup.ToComponentDataArray<PlayerComponent>(Allocator.TempJob);
		Settings.PlayersCount = playersComp.Length;
		playersComp.Dispose();

		//if (state.playerIds != null)
		//{
		//	state.playerIds.Dispose();
		//}
		//if (state.playerPositions != null)
		//{
		//	state.playerPositions.Dispose();
		//}
		//state.playerIds = new NativeArray<int>(state.playersCount, Allocator.Persistent);
		//state.playerPositions = new NativeArray<float3>(state.playersCount, Allocator.Persistent);
		Settings.PlayerPositions.Clear();
		//if (Settings.PlayerPositions.Count != Settings.PlayerPositionsPlayersCount)
		//{
		//	Settings.PlayerPositions.Resize(PlayersCount);
		//}
		int i = 0;
		Entities.WithAll<PlayerTag>().ForEach((ref Translation pos, ref PlayerComponent player) =>
		{
			//state.playerIds[i] = player.playerId;
			//state.playerPositions[i] = pos.Value;
			//Settings.PlayerPositions[i] = pos.Value;
			Settings.PlayerPositions.Add(pos.Value);
			++i;
		});

		//SetSingleton<GameState>(state);

		//Entities.WithAll<PlayerTag>().ForEach((ref Translation pos, ref PlayerTag tag) =>
		//{
		//	pos = new Translation { Value = Settings.GetPlayerPosition(tag.playerIdx) };
		//});
	}
}