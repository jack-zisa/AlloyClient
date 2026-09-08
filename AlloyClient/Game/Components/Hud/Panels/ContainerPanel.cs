using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game.Components.Hud.Inventory;
using AlloyClient.Game.Objects;

namespace AlloyClient.Game.Components.Hud.Panels;

public class ContainerPanel : Panel {
    private InventoryGrid _grid;

    public ContainerPanel(Entity entity, bool oneWay) {
        _grid = new InventoryGrid(entity, 0, oneWay, false);
        AddChild(_grid);
    }

    public void AddItem(ItemDesc item) {
        foreach (var tile in _grid.GetItemTiles()) {
            if (tile.ItemDesc != null)
                continue;
            tile.SetItem(item);
            break;
        }
    }
    
}