using OpenTK.Mathematics;

namespace AlloyClient.Game.Objects.Util;

public static class EntityUtils {
    public static bool FindClosestInteractableInRadius(Vector2 position, float radius, out Entity interactable) {
        interactable = null;

        var radiusSq = radius * radius;
        var closestDistanceSq = radiusSq;

        foreach (var cell in Map.GetCellsInRadius(position, radius)) {
            if (!Map.InteractiveObjects.TryGetValue(cell, out var entities))
                continue;

            foreach (var entity in entities.Values) {
                if (entity == Map.LocalPlayer)
                    continue;

                Vector2.DistanceSquared(in position, in entity.Position, out var distanceSq);

                if (distanceSq >= closestDistanceSq)
                    continue;

                closestDistanceSq = distanceSq;
                interactable = entity;
            }
        }

        return interactable != null;
    }
    
    public static Entity GetClosestPlayer(Vector2 position, float radius) {
        Player closest = null;

        var radiusSq = radius * radius;
        var closestDistanceSq = radiusSq;

        foreach (var cell in Map.GetCellsInRadius(position, radius)) {
            if (!Map.Players.TryGetValue(cell, out var players))
                continue;

            foreach (var player in players.Values) {
                Vector2.DistanceSquared(in position, in player.Position, out var distanceSq);

                if (distanceSq >= closestDistanceSq)
                    continue;

                closestDistanceSq = distanceSq;
                closest = player;
            }
        }

        return closest;
    }
    
    public static Entity GetClosestEnemy(Vector2 position, float radius) {
        Entity entity = null;

        var radiusSq = radius * radius;
        var closestDistanceSq = radiusSq;

        foreach (var cell in Map.GetCellsInRadius(position, radius)) {
            if (!Map.Entities.TryGetValue(cell, out var entities))
                continue;

            foreach (var en in entities.Values) {
                if (!en.Properties.IsEnemy)
                    continue;
                
                Vector2.DistanceSquared(in position, in en.Position, out var distanceSq);

                if (distanceSq >= closestDistanceSq)
                    continue;

                closestDistanceSq = distanceSq;
                entity = en;
            }
        }

        return entity;
    }

    public static bool IsCharacter(Entity entity) => entity.Properties.IsEnemy || entity.Properties.IsAlly || entity.Properties.IsPlayer || entity.Properties.Class == "Character";
}