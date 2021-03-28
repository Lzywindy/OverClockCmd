using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public static class MyPlanetInfoAPI
        {
            public static MyPlanet GetNearestPlanet(Vector3D MyPosition) { return MyGamePruningStructure.GetClosestPlanet(MyPosition); }
            public static Vector3D? GetNearestPlanetPosition(Vector3D MyPosition) { return MyGamePruningStructure.GetClosestPlanet(MyPosition)?.PositionComp?.GetPosition(); }
            public static double? GetSealevel(Vector3D MyPosition)
            {
                var planet = MyGamePruningStructure.GetClosestPlanet(MyPosition);
                if (planet == null || GetCurrentGravity(MyPosition) == Vector3.Zero) return null;
                return (MyPosition - planet.PositionComp.GetPosition()).Length() - planet.AverageRadius;
            }
            public static double? GetSurfaceHight(Vector3D MyPosition)
            {
                var planet = MyGamePruningStructure.GetClosestPlanet(MyPosition);
                if (planet == null || GetCurrentGravity(MyPosition) == Vector3.Zero) return null;
                var position = planet.GetClosestSurfacePointGlobal(ref MyPosition);
                return Vector3D.Distance(position, MyPosition);
            }
            public static Vector3 GetCurrentGravity(Vector3D Position) { float mult; return MyAPIGateway.Physics.CalculateNaturalGravityAt(Position, out mult); }
            public static Vector3 GetCurrentGravity(Vector3D Position, out float mult) { return MyAPIGateway.Physics.CalculateNaturalGravityAt(Position, out mult); }
            public struct NearestPlanetInfo
            {
                public float PlanetRadius { get; private set; }
                public float AtmoAtt { get; private set; }
                public MyPlanet Planet { get; private set; }
                public static NearestPlanetInfo? CtreateNearestPlanetInfo(Vector3D MyPosition)
                {
                    var planet = MyGamePruningStructure.GetClosestPlanet(MyPosition);
                    if (planet == null) return null;
                    return new NearestPlanetInfo()
                    {
                        Planet = planet,
                        PlanetRadius = planet.AverageRadius,
                        AtmoAtt = planet.AtmosphereAltitude
                    };
                }
            }
        }
    }
}
