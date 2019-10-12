using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [AttributeUsage (AttributeTargets.Class)]
    public class PacketAttribute : Attribute {
        public short packet_code { get; private set; }
        public PacketAttribute (short code) {
            this.packet_code = code;
        }
    }
    public interface IPacket {
        byte[] GetBytes ();
        void ReadFromReader (BanchoPacketReader reader);
    }
    public static class PacketFactory {
        private static Dictionary<short, Type> loadedPacketTypes =
            new Dictionary<short, Type> ();
        private static bool typesLoaded = false;
        private static void LoadPacketTypes () {
            var q = (
                from t in Assembly
                .GetExecutingAssembly ()
                .GetExportedTypes () where t.Namespace == MethodBase
                .GetCurrentMethod ()
                .DeclaringType.Namespace && t
                .GetCustomAttribute (
                    typeof (PacketAttribute)
                ) != null select t
            ).ToList ();
            foreach (var i in q) {
                loadedPacketTypes[(i.GetCustomAttribute (
                    typeof (PacketAttribute)
                ) as PacketAttribute).packet_code] = i;
            }
        }
        public static IPacket CreatePacket (
            short packet_type,
            BanchoPacketReader reader
        ) {
            if (!typesLoaded) { LoadPacketTypes (); typesLoaded = true; }
            if (loadedPacketTypes.ContainsKey (packet_type)) {
                return (IPacket) loadedPacketTypes[packet_type].GetConstructor (
                    new Type[] { typeof (BanchoPacketReader) }
                ).Invoke (new object[] { reader });
            }
            throw new KeyNotFoundException ("Unknown Packet Type");
        }
    }
}