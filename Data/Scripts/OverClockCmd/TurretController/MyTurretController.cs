using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game;
using System.Text;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), true, "TurretCoreSeimAI", "SmallTurretCoreSeimAI")]
    public class MyTurretController : MyTargetPredictDevice
    {
        IMyMotorStator rbase;
        List<IMyMotorStator> reval;

        protected override void FunctionAddOn()
        {
            base.FunctionAddOn();
            SetupInfos += (IMyTerminalBlock Me, StringBuilder Context) =>
            {
                if (Me.EntityId != this.Me.EntityId) return;
                if (Me != null)
                    Context.Append($"{Me.DefinitionDisplayNameText}\n\r");
                if (CurrentGrid == null)
                    Context.Append($"Grid Config Lost\n\r");
                if (GridTerminalSystem == null)
                    Context.Append($"GridTerminalSystem Config Lost\n\r");
                if (TurretRotateManage.IncompleteStructOfTurret)
                    Context.Append($"Missing Rotors\n\r");
                if (rbase == null)
                    Context.Append($"Missing Az Rotor\n\r");
                if (reval == null || reval.Count < 1)
                    Context.Append($"Missing Ev Rotors\n\r");
                if (TurretRotateManage.CanRunning)
                    Context.Append($"Turret Running Disabled\n\r");
                if ((Me.CubeGrid.GetTopMostParent(typeof(IMyCubeGrid)) as IMyCubeGrid) != null)
                    Context.Append($"{(Me.CubeGrid.GetTopMostParent(typeof(IMyCubeGrid)) as IMyCubeGrid).CustomName}\n\r");
            };
        }
        protected override void CheckSystem()
        {

            //var set = Utils.GetCurrentEntityBlocks(Me, block => (block as IMyMotorStator) != null || (block as IMyUserControllableGun) != null);
            //var r_set = set.FindAll(block => (block as IMyMotorStator) != null).ConvertAll(block => block as IMyMotorStator);
            //var turret_grid = Me.CubeGrid;
            //rbase = r_set.Find((IMyMotorStator rotor) => (rotor.TopGrid != null && (rotor.TopGrid == turret_grid || rotor.CubeGrid == turret_grid)) && (rotor.CustomName.Contains("TurretAz") || rotor.CustomName.Contains("turretaz")));
            //reval = r_set.FindAll((IMyMotorStator rotor) => (rotor.CustomName.Contains("TurretEv") || rotor.CustomName.Contains("turretev")) && (rotor.TopGrid != null && rotor.CubeGrid == turret_grid));
            rbase = Utils.GetT(GridTerminalSystem,(IMyMotorStator rotor) => (rotor.TopGrid != null && (rotor.TopGrid == CurrentGrid || rotor.CubeGrid == CurrentGrid)) && (rotor.CustomName.Contains("TurretAz") || rotor.CustomName.Contains("turretaz")));
            reval = Utils.GetTs(GridTerminalSystem,(IMyMotorStator rotor) => (rotor.CustomName.Contains("TurretEv") || rotor.CustomName.Contains("turretev")) && (rotor.TopGrid != null && rotor.CubeGrid == CurrentGrid));
            //var w_set = set.FindAll(block => (block as IMyUserControllableGun) != null).ConvertAll(block => block as IMyUserControllableGun);
            TurretRotateManage.SetupRotors(rbase, reval);
            HashSet<IMyCubeGrid> GunGrids = new HashSet<IMyCubeGrid>();
            foreach (var turret_gun in TurretRotateManage.turret_guns) { if (turret_gun.TopGrid == null) continue; GunGrids.Add(turret_gun.TopGrid); }
            //WeaponManager.SetWeaponsHook(w_set.FindAll((IMyUserControllableGun weapon) => (!(weapon is IMyLargeTurretBase) && GunGrids.Contains(weapon.CubeGrid))));
            WeaponManager.SetWeaponsHook(Utils.GetTs(GridTerminalSystem,(IMyUserControllableGun weapon) => (!(weapon is IMyLargeTurretBase) && GunGrids.Contains(weapon.CubeGrid))));
            TurretRotateManage.SetupGuns(WeaponManager.Weapons.ConvertAll(gun => gun as IMyTerminalBlock));
            InitFinished = TurretRotateManage.CanRunning;
        }
        protected override void Running()
        {
            base.Running();
            TurretRotateManage.Running(Direction);
        }
        private bool Turret_Base_Filter(IMyTerminalBlock rotor)
        {
            if (rotor == null) return false;
            if (!(rotor is IMyMotorStator)) return false;
            if ((rotor as IMyMotorStator).TopGrid == null) return false;
            if ((rotor as IMyMotorStator).TopGrid != CurrentGrid && (rotor as IMyMotorStator).CubeGrid != CurrentGrid) return false;
            if (rotor.CustomName.Contains("TurretAz")) return false;
            return true;
        }
        private bool Turret_Gun_Filter(IMyTerminalBlock rotor)
        {
            if (rotor == null) return false;
            if (!(rotor is IMyMotorStator)) return false;
            if ((rotor as IMyMotorStator).TopGrid == null) return false;
            if ((rotor as IMyMotorStator).TopGrid != CurrentGrid) return false;
            if (rotor.CustomName.Contains("TurretEv")) return false;
            return true;
        }
        private void GetTurretRotors()
        {
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            //CurrentGrid.GetBlocks(blocks,block=>block is IMyMotorStator && Turret_Base_Filter())
        }

        protected MyTurretRotateManage TurretRotateManage { get; } = new MyTurretRotateManage(30, 30, 1);
        #region 纯属性    
        public static bool TurretControllerEnabled(IMyTerminalBlock Me)
        {
            return GetLogic<MyTurretController>(Me) != null;
        }
        public static void Set_Max_Az_Speed(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTurretController>(Me)?.TurretRotateManage.Set_Max_Az_Speed(value);
        }
        public static void Set_Max_Ev_Speed(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTurretController>(Me)?.TurretRotateManage.Set_Max_Ev_Speed(value);
        }
        public static void Set_RotationMult(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTurretController>(Me)?.TurretRotateManage.Set_RotationMult(value);
        }
        #endregion
    }
}