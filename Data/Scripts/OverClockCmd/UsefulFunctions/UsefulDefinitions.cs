using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using SuperBlocks.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
namespace SuperBlocks
{
    public struct EventVariable<T> : IEquatable<T>
    {
        public T Value { get { return data; } set { if (data.Equals(value)) return; data = value; DoVariableChanged?.Invoke(); } }
        private T data;
        public Action DoVariableChanged;
        public bool Equals(T other) { return Value.Equals(other); }
    }
    public static partial class Utils
    {
        public class FireWeaponManage
        {
            public bool UnabledFire => Common.IsNullCollection(CurrentWeapons);
            public List<IMyTerminalBlock> CurrentWeapons { get; private set; }
            private Queue<IMyTerminalBlock> Weapons2Fire { get; } = new Queue<IMyTerminalBlock>();
            int firegap;
            int count = 0;
            public void LoadCurrentWeapons(IEnumerable<IMyTerminalBlock> Weapons, float RPM) => LoadCurrentWeapons(Weapons, (int)(3600 / RPM));
            public void LoadCurrentWeapons(IEnumerable<IMyTerminalBlock> Weapons, int firegap)
            {
                if (Common.IsNullCollection(Weapons)) return;
                CurrentWeapons = new List<IMyTerminalBlock>();
                CurrentWeapons.AddRange(Weapons);
                Weapons2Fire.Clear();
                foreach (var CurrentWeapon in CurrentWeapons)
                    Weapons2Fire.Enqueue(CurrentWeapon);
                this.firegap = firegap;
            }
            public void ResetFireRPM(float RPM) => firegap = (int)(3600 / RPM);
            public void SetFire(bool Fire = false)
            {
                if (Common.IsNullCollection(CurrentWeapons)) return;
                if (BasicInfoService.WeaponInfos.ContainsKey(CurrentWeapons.FirstOrDefault()?.BlockDefinition.SubtypeId ?? ""))
                {
                    if (firegap <= 0)
                    {
                        foreach (var weapon in CurrentWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, Fire);
                        Weapons2Fire.Clear(); count = 0;
                        return;
                    }
                    if (!Common.IsNullCollection(Weapons2Fire)) return;
                    if (Common.IsNullCollection(Weapons2Fire) && CurrentWeapons.All(MyWeaponAndTurretApi.CanFire))
                    {
                        foreach (var weapon in CurrentWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, false);
                        foreach (var CurrentWeapon in CurrentWeapons)
                            Weapons2Fire.Enqueue(CurrentWeapon);
                        count = 0;
                        return;
                    }
                }
            }
            public void RunningAutoFire(bool CanFire = false)
            {
                if (Common.IsNullCollection(CurrentWeapons)) return;
                if (BasicInfoService.WeaponInfos.ContainsKey(CurrentWeapons.FirstOrDefault()?.BlockDefinition.SubtypeId ?? ""))
                {
                    if (!CanFire) { foreach (var weapon in CurrentWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, false); return; }
                    if (Weapons2Fire.Count < 1) return;
                    if (!MyWeaponAndTurretApi.CanFire(Weapons2Fire.Peek())) return;
                    if (count > 0) { count--; return; }
                    MyWeaponAndTurretApi.FireWeapon(Weapons2Fire.Dequeue(), CanFire);
                    count = firegap;
                }
            }
        }
        public class CycleStructure<T>
        {
            private List<T> Container;
            private uint currentindex;
            public int Count => Container?.Count ?? 0;
            public void LoadDatas(IEnumerable<T> Datas)
            {
                if (Common.IsNullCollection(Datas)) return;
                Container = new List<T>();
                Container.AddRange(Datas);
                currentindex = 0;
            }
            public void Increase()
            {
                if (Common.IsNullCollection(Container)) return;
                currentindex++;
                currentindex = (uint)(currentindex % (ulong)Container.Count);
            }
            public void Decrease()
            {
                if (Common.IsNullCollection(Container)) return;
                currentindex--;
                currentindex = (uint)Math.Max(Math.Min(currentindex, (ulong)Container.Count), 0);
            }
            public T GetCurrentSelect()
            {
                if (Common.IsNullCollection(Container)) return default(T);
                return Container[(int)currentindex];
            }
        }

        public const string HoverEngineNM = "Hover";
        public const string WheelsGroupNM = @"Wheels";
        public const string ACDoorsGroupNM = @"ACDoors";
        public const string BrakeNM = @"Brake";
        public const string BackwardNM = @"Backward";
        public const string MotorOverrideId = @"Propulsion override";
        public const string SteerOverrideId = @"Steer override";
        public const string VehicleControllerConfigID = @"VehicleController";
        public const string OverclockedID = @"Overclocked";
    }
    public delegate void MyActionRef<T>(ref T value);
    public enum ControllerRole : int { None, Aeroplane, Helicopter, VTOL, SpaceShip, SeaShip, Submarine, TrackVehicle, WheelVehicle, HoverVehicle }
    public struct Direction6Values
    {
        public float Forward;
        public float Backward;
        public float Left;
        public float Right;
        public float Up;
        public float Down;
        public Direction6Values(float Forward, float Backward, float Left, float Right, float Up, float Down)
        {
            this.Forward = Forward; this.Backward = Backward; this.Left = Left;
            this.Right = Right; this.Up = Up; this.Down = Down;
        }
        public Direction6Values(MyTuple<float, float, float, float, float, float> Values)
        {
            Forward = Values.Item1; Backward = Values.Item2; Left = Values.Item3;
            Right = Values.Item4; Up = Values.Item5; Down = Values.Item6;
        }
        public Direction6Values(float[] Values)
        {
            Forward = Backward = Left = Right = Up = Down = 0;
            if (Utils.Common.IsNullCollection(Values)) return;
            for (int index = 0; index < Values.Length; index++)
            {
                switch (index)
                {
                    case 0: Forward = Values[index]; break;
                    case 1: Backward = Values[index]; break;
                    case 2: Left = Values[index]; break;
                    case 3: Right = Values[index]; break;
                    case 4: Up = Values[index]; break;
                    case 5: Down = Values[index]; break;
                    default: break;
                }
            }
        }
        public static Direction6Values operator +(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward + b.Forward,
                Backward = a.Backward + b.Backward,
                Left = a.Left + b.Left,
                Right = a.Right + b.Right,
                Up = a.Up + b.Up,
                Down = a.Down + b.Down
            };
        }
        public static Direction6Values operator -(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward - b.Forward,
                Backward = a.Backward - b.Backward,
                Left = a.Left - b.Left,
                Right = a.Right - b.Right,
                Up = a.Up - b.Up,
                Down = a.Down - b.Down
            };
        }
        public static Direction6Values operator *(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward * b.Forward,
                Backward = a.Backward * b.Backward,
                Left = a.Left * b.Left,
                Right = a.Right * b.Right,
                Up = a.Up * b.Up,
                Down = a.Down * b.Down
            };
        }
        public static Direction6Values operator /(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward / b.Forward,
                Backward = a.Backward / b.Backward,
                Left = a.Left / b.Left,
                Right = a.Right / b.Right,
                Up = a.Up / b.Up,
                Down = a.Down / b.Down
            };
        }
    }
}