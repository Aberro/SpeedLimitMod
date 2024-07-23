using System;
using System.Collections.Generic;
using System.Linq;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Common;
using Game.Net;
using Game.SceneFlow;
using Game.Settings;
using Game.Tools;
using Game.UI;
using Unity.Entities;
using UnityEngine;
using CarLane = Game.Net.CarLane;
using JetBrains.Annotations;
using SpeedLimitEditor.Utils;
using SpeedLimitEditor.Components;
using CarLaneFlags = Game.Net.CarLaneFlags;

namespace SpeedLimitEditor.Systems;

[UsedImplicitly]
public partial class SpeedLimitEditorUISystem : UISystemBase
{
	private const float InternalToMph = 1.26f;
	private const float MphToInternal = 0.79365085f;
	private const float InternalToKph = 1.8f;
	private const float KphToInternal = 0.5555556f;

	private ToolSystem toolSystem = null!;
	private EntitySelectorToolSystem entitySelectorTool = null!;
	private DefaultToolSystem defaultTool = null!;

	private Entity selectedEntity;
	private bool changingSpeed;
	private NameSystem nameSystem = null!;
	private List<float> speeds = null!; // Helper collection, to avoid allocations

	private ValueBinding<bool> toolActiveBinding = null!;
	public bool ToolActive
	{
		get => this.toolActiveBinding.value;
		set
		{
			if(value == this.toolActiveBinding.value)
				return;
			this.toolActiveBinding.Update(value);
			this.toolSystem.activeTool = this.toolActiveBinding.value ? this.entitySelectorTool : this.defaultTool;
			this.toolSystem.selected = Entity.Null;
		}
	}

	private InterfaceSettings.UnitSystem units;
	private GetterValueBinding<string> unitsBinding = null!;
	public InterfaceSettings.UnitSystem Units
	{
		get => this.units;
		set
		{
			if(this.units == value)
				return;
			this.units = value;
			this.unitsBinding.Update();
		}
	}

	private float averageSpeed = -1;
	private GetterValueBinding<float> averageSpeedBinding = null!;
	public float AverageSpeed
	{
		get => Mathf.Round(this.units == InterfaceSettings.UnitSystem.Metric ? this.averageSpeed * InternalToKph : this.averageSpeed * InternalToMph);
		set
		{
			var convertedValue = this.units == InterfaceSettings.UnitSystem.Metric ? value * KphToInternal : value * MphToInternal;
			if (Math.Abs(this.averageSpeed - convertedValue) < 1)
				return;
			this.averageSpeed = convertedValue;
			this.averageSpeedBinding.Update();
		}
	}

	private ValueBinding<string> roadNameBinding = null!;
	public string RoadName
	{
		get => this.roadNameBinding.value;
		set => this.roadNameBinding.Update(value);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		AddBinding(this.toolActiveBinding = new ValueBinding<bool>(nameof(SpeedLimitEditor), Bindings.SpeedLimitToolActive, false));
		AddBinding(this.unitsBinding = new GetterValueBinding<string>(nameof(SpeedLimitEditor), Bindings.UnitSystem, () => this.units == InterfaceSettings.UnitSystem.Metric ? "kph" : "mph"));
		AddBinding(this.averageSpeedBinding = new GetterValueBinding<float>(nameof(SpeedLimitEditor), Bindings.AverageSpeed, () => AverageSpeed));
		AddBinding(this.roadNameBinding = new ValueBinding<string>(nameof(SpeedLimitEditor), Bindings.Name, ""));
		AddBinding(new TriggerBinding<float>(nameof(SpeedLimitEditor), Bindings.SetSpeedLimit, HandleSpeedLimitChange));
		AddBinding(new TriggerBinding(nameof(SpeedLimitEditor), Bindings.SelectTool, HandleSelectTool));
		AddBinding(new TriggerBinding(nameof(SpeedLimitEditor), Bindings.Reset, HandleReset));

		this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
		this.entitySelectorTool = World.GetOrCreateSystemManaged<EntitySelectorToolSystem>();
		this.defaultTool = World.GetOrCreateSystemManaged<DefaultToolSystem>();
		this.nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
		if (this.entitySelectorTool is null || this.toolSystem is null || this.defaultTool is null || this.nameSystem is null)
			throw new NullReferenceException("One or more systems are null");
		this.selectedEntity = Entity.Null;
		this.units = GameManager.instance.settings.userInterface.unitSystem;

		this.speeds = new List<float>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (this.changingSpeed)
			return;
		Units = GameManager.instance.settings.userInterface.unitSystem;

		// selected entity changed
		if(this.selectedEntity.Index != this.toolSystem.selected.Index || this.selectedEntity.Version != this.toolSystem.selected.Version)
		{
			if (ToolActive && this.toolSystem.activeTool != this.entitySelectorTool)
				this.toolSystem.activeTool = this.entitySelectorTool;
			if (this.toolSystem.selected == Entity.Null)
			{
				// no entity selected, reset values
				this.selectedEntity = Entity.Null;
				RoadName = "";
				AverageSpeed = -1f;
				return;
			}

			// new entity selected, update values
			var entity = this.toolSystem.selected;
			this.averageSpeed = GetAverageSpeed(entity);
			this.averageSpeedBinding.Update();
			if (AverageSpeed > 0)
			{
				this.selectedEntity = this.toolSystem.selected;
				RoadName = this.nameSystem.GetRenderedLabelName(this.toolSystem.selected);
			}
			else
			{
				this.selectedEntity = Entity.Null;
				RoadName = "";
				AverageSpeed = -1f;
			}
		}
		else if (ToolActive && this.toolSystem.activeTool != this.entitySelectorTool)
		{
			ToolActive = false;
		}
	}

	private float GetAverageSpeed(Entity entity)
	{
		void GatherSpeed(ref SubLane subLane)
		{
			var ignoreFlags = CarLaneFlags.Unsafe | CarLaneFlags.SideConnection;
			if (EntityManager.TryGetComponent(subLane.m_SubLane, out CarLane carLane) && ((carLane.m_Flags & ignoreFlags) != ignoreFlags))
				this.speeds.Add(carLane.m_SpeedLimit);
			if(EntityManager.TryGetComponent(subLane.m_SubLane, out TrackLane trackLane))
				this.speeds.Add(trackLane.m_SpeedLimit);
		}

		this.speeds.Clear();
		try
		{
			DynamicBuffer<SubLane> subLanes;
			if (EntityManager.TryGetBuffer(entity, false, out subLanes))
				subLanes.ForEach(subLane => GatherSpeed(ref subLane));
			if (this.speeds.Any())
				return this.speeds.Average();
			return -1;
		}
		finally
		{
			this.speeds.Clear();
		}
	}

	private void HandleSpeedLimitChange(float suggestedSpeed)
	{
		if (this.selectedEntity == Entity.Null || suggestedSpeed <= 0 || Math.Abs(suggestedSpeed - AverageSpeed) < 0.5f)
			return;
		this.changingSpeed = true;
		AverageSpeed = suggestedSpeed; // Use this to convert units.
		if (EntityManager.TryGetComponent(this.selectedEntity, out Temp temp))
		{
			EntityManager.AddComponent<CustomSpeed>(temp.m_Original);
			EntityManager.SetComponentData(temp.m_Original, new CustomSpeed { m_Value = this.averageSpeed });
			EntityManager.AddComponent<Updated>(temp.m_Original);
		}
		else
		{
			EntityManager.AddComponent<CustomSpeed>(this.selectedEntity);
			EntityManager.SetComponentData(this.selectedEntity, new CustomSpeed { m_Value = this.averageSpeed });
			EntityManager.AddComponent<Updated>(this.selectedEntity);
		}
		this.changingSpeed = false;
	}

	private void HandleSelectTool() => ToolActive = !ToolActive;

	private void HandleReset()
	{
		if(this.selectedEntity == Entity.Null)
			return;
		EntityManager.RemoveComponent<CustomSpeed>(this.selectedEntity);
		EntityManager.AddComponent<Updated>(this.selectedEntity);
	}
}