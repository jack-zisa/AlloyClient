using System;
using AlloyClient.Assets.Libraries;
using AlloyClient.Display;
using AlloyClient.Game;
using Org.BouncyCastle.Utilities;

namespace AlloyClient.Networking.Packets.Incoming;

public class InvUpdate : IncomingPacket<InvUpdate> {
    private int[] Slots;
    private int[] ItemTypes;
    
    public override PacketId PacketId => PacketId.InvUpdate;

    public override void Reset() {
        Slots = null;
        ItemTypes = null;
    }

    public override void Read(ref SpanReader reader) {
        int length = reader.ReadInt32();
        Slots = new int[length];
        ItemTypes = new int[length];
        
        for (int i = 0; i < length; ++i) {
            Slots[i] = reader.ReadInt32();
            ItemTypes[i] = reader.ReadUInt16();
        }
    }

    public override void Handle() {
        if (Slots.Length != ItemTypes.Length)
            return;
        
        for (int i = 0; i < Slots.Length; ++i) {
            int slot = Slots[i];
            int itemType = ItemTypes[i];

            if (itemType == -1) {
                Map.LocalPlayer.Equipment[slot] = null;

                if (slot < 4 && ScreenManager.GetScreen() is GameScreen gameScreen) {
                    gameScreen.GetHud().GetHotbarInventory().GetItemTiles()[slot].SetItem(null);
                }
            } else {
                Map.LocalPlayer.Equipment[slot] = ObjectLibrary.TypeToItem[(ushort) itemType];

                if (slot < 4 && ScreenManager.GetScreen() is GameScreen gameScreen) {
                    gameScreen.GetHud().GetHotbarInventory().GetItemTiles()[slot].SetItem(ObjectLibrary.TypeToItem[(ushort) itemType]);
                }
            }
            
            Map.LocalPlayer.InventoryUpdate.Dispatch(slot);
        }

        /*if (Slots.ContainsAnyInRange(0, 3)) {
            Map.LocalPlayer.UpdateItemStatBonuses();
        }*/
    }

    public override string ToString() {
        return $"Slots: {Slots}, ItemTypes: {ItemTypes}";
    }
}