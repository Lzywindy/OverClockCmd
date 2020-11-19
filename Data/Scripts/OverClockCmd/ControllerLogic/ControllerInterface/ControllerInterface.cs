namespace SuperBlocks
{
    public interface IHeilController
    {
        bool HoverMode { get; set; }
        float MaxiumHoverSpeed { get; set; }
    }
    public interface IPlaneController
    {
        bool EnabledCuriser { get; set; }
        float MaxiumFlightSpeed { get; set; }
    }
    public interface IWingModeController
    {
        bool HasWings { get; set; }
        bool EnabledAirBrake { get; }
    }
    public interface IPoseParamAdjust
    {
        float LocationSensetive { get; set; }
        float MaxReactions_AngleV { get; set; }
        float SafetyStage { get; set; }
    }
    public interface ICtrlDevCtrl
    {
        bool EnabledThrusters { get; set; }
        bool EnabledGyros { get; set; }
    }
    public interface IPlanetVehicle
    {
        float MaximumCruiseSpeed { get; set; }
    }
    public interface ILandVehicle
    {
        bool IsTank { get; set; }
    }
    public interface ISeaVehicle
    {
        bool IsSubmarine { get; set; }
    }
}
