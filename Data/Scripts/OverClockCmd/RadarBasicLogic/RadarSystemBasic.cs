using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
namespace SuperBlocks.Controller
{
    public class RadarSystemBasic : MySuperBlockProgram
    {
        protected List<IMyEntity> Entities { get; private set; }
        float DetectedRange;
        protected virtual bool EntityFilter(IMyEntity entity)
        {
            if (CurrentGrid == null || entity == null) return false;
            return Vector3D.Distance(CurrentGrid.GetPosition(), entity.GetPosition()) < DetectedRange;
        }
        public void UpdateEntities(IEnumerable<IMyEntity> entities)
        {
            if (entities == null || CurrentGrid == null) return;
            Entities = new List<IMyEntity>(entities.Where(EntityFilter));
        }
    }
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class RadarSystemService : MySessionComponentBase
    {
        public static HashSet<IMyEntity> Entities { get; } = new HashSet<IMyEntity>();
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            if (!Initialized) return;
            if (!BasicInfoService.SetupComplete) return;
            if (!SetupComplete)
            {
                Init();
                SetupComplete = true;
                return;
            }
            else UpdateInfors();
        }
        private void Init()
        {
        }
        protected sealed override void UnloadData()
        {
        }
        #region 每隔一定时间更新一下所有实体
        private void UpdateInfors()
        {
            if (counts == 0)
            {
                MyAPIGateway.Entities.GetEntities(Entities);
                counts = 100;
            }
            counts--;
        }
        int counts = 100;
        #endregion
    }
}
