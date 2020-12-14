using VRage;
using VRage.Game.Components;
namespace SuperBlocks.Controller
{
    public class InformationDeviceBase : MyGameLogicComponent
    {
        WAN_Network.CommunicationType ComType;
        public virtual bool CanCommunicate => false;
        public virtual void SetMessage(byte[] message, long EntityID) { }
        public virtual MyTuple<byte[], long> GetMessage() { return new MyTuple<byte[], long>(new byte[1] { 0 }, 0); }
    }
}
