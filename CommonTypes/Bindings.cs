using Reinforced.Typings.Attributes;

namespace SpeedLimitEditor;

[TsClass(IncludeNamespace = false, AutoExportFields = true)]
public class Bindings
{
	public const string SpeedLimitToolActive = nameof(SpeedLimitToolActive);
	public const string UnitSystem = nameof(UnitSystem);
	public const string AverageSpeed = nameof(AverageSpeed);
	public const string Name = nameof(Name);
	public const string SetSpeedLimit = nameof(SetSpeedLimit);
	public const string SelectTool = nameof(SelectTool);
	public const string Reset = nameof(Reset);
}