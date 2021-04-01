using System;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public class MyEventParameter<T> where T : IEquatable<T>
        {
            public Action OnValueChanged = () => { };
            private T value;
            public MyEventParameter() { }
            public MyEventParameter(T value) { this.value = value; }
            public T Value { get { return value; } set { if (!this.value.Equals(value)) { this.value = value; OnValueChanged?.Invoke(); } } }
            public override string ToString()
            {
                return value.ToString();
            }
        }
        public class MyEventParameter_Bool
        {
            public Action OnValueChanged = () => { };
            private volatile bool value;
            public MyEventParameter_Bool() { }
            public MyEventParameter_Bool(bool value) { this.value = value; }
            public bool Value { get { return value; } set { if (this.value != value) { this.value = value; OnValueChanged?.Invoke(); } } }
            public override string ToString()
            {
                return value.ToString();
            }
        }
        public class MyEventParameter_Float
        {
            public Action OnValueChanged = () => { };
            private volatile float value;
            public MyEventParameter_Float() { }
            public MyEventParameter_Float(float value) { this.value = value; }
            public float Value { get { return value; } set { if (this.value != value) { this.value = value; OnValueChanged?.Invoke(); } } }
            public override string ToString()
            {
                return value.ToString();
            }
        }
        public class MyEventParameter_Int
        {
            public Action OnValueChanged = () => { };
            private volatile int value;
            public MyEventParameter_Int() { }
            public MyEventParameter_Int(int value) { this.value = value; }
            public int Value { get { return value; } set { if (this.value != value) { this.value = value; OnValueChanged?.Invoke(); } } }
            public override string ToString()
            {
                return value.ToString();
            }
        }
        public class MyEventParameter_UInt
        {
            public Action OnValueChanged = () => { };
            private volatile uint value;
            public MyEventParameter_UInt() { }
            public MyEventParameter_UInt(uint value) { this.value = value; }
            public uint Value { get { return value; } set { if (this.value != value) { this.value = value; OnValueChanged?.Invoke(); } } }
            public override string ToString()
            {
                return value.ToString();
            }
        }
        public class MyEventParameter_String
        {
            public Action OnValueChanged = () => { };
            private volatile string value;
            public MyEventParameter_String() { }
            public MyEventParameter_String(string value) { this.value = value; }
            public string Value { get { return value; } set { if (this.value != value) { this.value = value; OnValueChanged?.Invoke(); } } }
            public override string ToString()
            {
                return value.ToString();
            }
        }
        public class MyEventParameter_ControlRole
        {
            public Action OnValueChanged = () => { };
            private volatile ControllerRole value;
            public MyEventParameter_ControlRole() { }
            public MyEventParameter_ControlRole(ControllerRole value) { this.value = value; }
            public ControllerRole Value { get { return value; } set { if (this.value != value) { this.value = value; OnValueChanged?.Invoke(); } } }
            public override string ToString()
            {
                return value.ToString();
            }
        }
    }
}
