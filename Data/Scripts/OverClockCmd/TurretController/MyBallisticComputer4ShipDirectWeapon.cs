using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
namespace SuperBlocks.Controller
{
    /// <summary>
    /// 这个类主要用于战斗机武器、战舰舰艏炮武器的弹道计算
    /// </summary>
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "BallisticComputer4ShipDirectWeapon", "SmallBallisticComputer4ShipDirectWeapon")]
    public sealed class MyBallisticComputer4ShipDirectWeapon : MyTargetPredictDevice
    {
        protected override void CheckSystem()
        {
            base.CheckSystem();
            if (CurrentGrid == null) return;
            WeaponManager.SetWeaponsHook(Utils.GetTs(GridTerminalSystem, (IMyUserControllableGun weapon) => !(weapon is IMyLargeTurretBase)));
        }
        protected override void Running()
        {
            bool hastarget = CanApprochTarget;
            if (!EnabledRunning) return;
            if (!hastarget) V_project = null;
        }
    }
}