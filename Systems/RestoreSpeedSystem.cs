using System.Diagnostics.CodeAnalysis;
using Colossal.Entities;
using Game.Common;
using Game.Net;
using JetBrains.Annotations;
using SpeedLimitEditor.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpeedLimitEditor.Systems;

[UsedImplicitly]
public partial class RestoreSpeedSystem : SystemBase
{
	private EntityQuery entitiesToRestoreQuery;
	protected override void OnCreate()
	{
		base.OnCreate();
		this.entitiesToRestoreQuery = GetEntityQuery(new EntityQueryDesc()
		{
			All = new[]
			{
				ComponentType.ReadOnly<CustomSpeed>()
			},
			Any = new[]
			{
				ComponentType.ReadOnly<Updated>(),
			}
		});
	}
	protected override void OnUpdate()
	{
		JobHandle jobHandle = new RestoreSpeedJob
		{
			EntityType = EntityManager.GetEntityTypeHandle(),
			EntityManager = EntityManager,
		}.ScheduleParallel(this.entitiesToRestoreQuery, Dependency);
		Dependency = jobHandle;
	}
	[BurstCompile]
	private struct RestoreSpeedJob : IJobChunk
	{
		[ReadOnly]
		public EntityTypeHandle EntityType;
		public EntityManager EntityManager;
		[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach", Justification = "This is a burst method, so it's better with for loops.")]
		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var array = chunk.GetNativeArray(this.EntityType);
			for(int i = 0; i < array.Length; i++)
			{
				var entity = array[i];
				if (this.EntityManager.TryGetComponent(entity, out CustomSpeed speed))
					SetSpeed(entity, speed.m_Value);
			}
		}

		[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach", Justification = "This is a burst method, so it's better with for loops.")]
		private void SetSpeed(Entity entity, float speed)
		{
			DynamicBuffer<SubLane> subLanes;
			if (this.EntityManager.TryGetBuffer(entity, false, out subLanes))
				for(int i = 0; i < subLanes.Length; i++)
				{
					var subLane = subLanes[i];
					SetSpeedSubLane(ref subLane, speed);
					subLanes[i] = subLane;
				}
			if (this.EntityManager.TryGetComponent(entity, out Edge edge))
			{
				if (this.EntityManager.TryGetBuffer(edge.m_Start, false, out subLanes))
					for (int i = 0; i < subLanes.Length; i++)
					{
						var subLane = subLanes[i];
						SetSpeedSubLane(ref subLane, speed);
						subLanes[i] = subLane;
					}
				if (this.EntityManager.TryGetBuffer(edge.m_End, false, out subLanes))
					for (int i = 0; i < subLanes.Length; i++)
					{
						var subLane = subLanes[i];
						SetSpeedSubLane(ref subLane, speed);
						subLanes[i] = subLane;
					}
			}
		}

		private void SetSpeedSubLane(ref SubLane subLane, float speed)
		{
			// TODO: ensure that we ignore building connections, but not other unsafe lanes.
			var ignoreFlags = CarLaneFlags.Unsafe | CarLaneFlags.SideConnection;
			if (this.EntityManager.TryGetComponent(subLane.m_SubLane, out CarLane carLane) && ((carLane.m_Flags & ignoreFlags) != ignoreFlags))
			{
				carLane.m_DefaultSpeedLimit = speed;
				carLane.m_SpeedLimit = speed;
				this.EntityManager.SetComponentData(subLane.m_SubLane, carLane);
			}
			if (this.EntityManager.TryGetComponent(subLane.m_SubLane, out TrackLane trackLane))
			{
				trackLane.m_SpeedLimit = speed;
				this.EntityManager.SetComponentData(subLane.m_SubLane, trackLane);
			}
		}
	}
}
