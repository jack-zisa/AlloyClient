using AlloyClient.Game;

namespace AlloyClient.Networking.Packets.Incoming;

public class ConditionEffect : IncomingPacket<ConditionEffect> {
    private string Effect;
    
    public override PacketId PacketId => PacketId.TradeAccepted;

    public override void Reset() {
        Effect = "Nothing";
    }

    public override void Read(ref SpanReader reader) {
        Effect = reader.ReadUTF();
    }

    public override void Handle() {
        Map.LocalPlayer.EffectBuckets.AddConditionEffect(Game.ConditionEffect.FromName(Effect));
    }

    public override string ToString() {
        return $"Effect: {Effect}";
    }
}