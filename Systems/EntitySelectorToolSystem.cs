
using Game.Areas;
using Game.Common;
using Game.Net;
using Game.Notifications;
using Game.Prefabs;
using Game.Routes;
using Game.Tools;
using Unity.Mathematics;
using NotImplementedException = System.NotImplementedException;

namespace SpeedLimitEditor.Systems;

public partial class EntitySelectorToolSystem : DefaultToolSystem
{
	public override string toolID => "EntitySelectorTool";
	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override void InitializeRaycast()
	{
		this.m_ToolRaycastSystem.raycastFlags = RaycastFlags.SubElements | RaycastFlags.Cargo | RaycastFlags.Passenger | RaycastFlags.EditorContainers;
		this.m_ToolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Overground | CollisionMask.Underground;
		this.m_ToolRaycastSystem.typeMask = TypeMask.Net;
		this.m_ToolRaycastSystem.netLayerMask = Layer.Road | Layer.PublicTransportRoad | Layer.TrainTrack | Layer.TramTrack | Layer.SubwayTrack;
		this.m_ToolRaycastSystem.areaTypeMask = AreaTypeMask.None;
		this.m_ToolRaycastSystem.routeType = RouteType.None;
		this.m_ToolRaycastSystem.transportType = TransportType.None;
		this.m_ToolRaycastSystem.iconLayerMask = IconLayerMask.None;
		this.m_ToolRaycastSystem.utilityTypeMask = UtilityTypes.None;
		this.m_ToolRaycastSystem.rayOffset = new float3();
	}
}
