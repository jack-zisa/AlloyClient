using AlloyClient.Game;

namespace AlloyClient.Networking.Packets.Incoming;

public class ConditionEffect : IncomingPacket<ConditionEffect> {
    private string Effect;
    private bool Apply;
    
    public override PacketId PacketId => PacketId.TradeAccepted;

    public override void Reset() {
        Effect = "Nothing";
        Apply = false;
    }

    public override void Read(ref SpanReader reader) {
        Effect = reader.ReadUTF();
        Apply = reader.ReadBoolean();
    }

    public override void Handle() {
        if (Apply) Map.LocalPlayer.AddConditionEffect(Game.ConditionEffect.FromName(Effect));
        else Map.LocalPlayer.RemoveConditionEffect(Game.ConditionEffect.FromName(Effect));
    }

    public override string ToString() {
        return $"Effect: {Effect}, Apply: {Apply}";
    }
}