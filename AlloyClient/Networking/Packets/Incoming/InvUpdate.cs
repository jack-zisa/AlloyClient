using System;
using AlloyClient.Assets.Libraries;
using AlloyClient.Game;

namespace AlloyClient.Networking.Packets.Incoming;

public class InvUpdate : IncomingPacket<InvUpdate> {
    private int Slot;
    private ushort ItemType;
    
    public override PacketId PacketId => PacketId.TradeAccepted;

    public override void Reset() {
        Slot = -1;
        ItemType = 0;
    }

    public override void Read(ref SpanReader reader) {
        Slot = reader.ReadInt32();
        ItemType = reader.ReadUInt16();
    }

    public override void Handle() {
        Map.LocalPlayer.Equipment[Slot] = ObjectLibrary.TypeToItem[ItemType];
        Map.LocalPlayer.InventoryUpdate.Dispatch(Slot);
    }

    public override string ToString() {
        return $"Slot: {Slot}, ItemType: {ItemType}";
    }
}