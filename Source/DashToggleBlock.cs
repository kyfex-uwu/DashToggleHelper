using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DashToggleHelper;

[Tracked]
[CustomEntity("DashToggleHelper/DashToggleBlock")]
internal class DashToggleBlock : CassetteBlock {
    private readonly string prefix;
    private bool initialized;

    public DashToggleBlock(Vector2 position, EntityID id, float width, float height, int dashes, string prefix)
        : base(position, id, width, height, dashes, -1f) {
        color = DashToggleHelperModule.getColor(Index);
        this.prefix = prefix;
    }

    public DashToggleBlock(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, id, data.Width, data.Height, data.Int("dashes"),
            data.String("prefix", "objects/DashToggleHelper/dashtoggleblock/")) { }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        ShiftSize(1);
    }

    public override void Update() {
        base.Update();
        if (!initialized) {
            var entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                Activated = entity.Dashes == Index;
                initialized = true;
            }
        }

        if (Index == DashToggleHelperModule.lastDashes) Activated = true;
        else Activated = false;
    }

    public void FindInGroupOverride(CassetteBlock block) {
        foreach (DashToggleBlock entity in Scene.Tracker.GetEntities<DashToggleBlock>()) {
            if (entity != this && entity != block && entity.Index == Index &&
                (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2,
                    (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1,
                    (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity)) {
                group.Add(entity);
                FindInGroupOverride(entity);
                entity.group = this.group;
            }
        }
    }

    public bool CheckForSameOverride(float x, float y) {
        foreach (Entity maybe in Scene.Tracker.GetEntities<DashToggleBlock>()) {
            if (maybe is CassetteBlock entity) {
                if (entity.Index == Index && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
                    return true;
            }
        }

        return false;
    }

    public void SetImageOverride(float x, float y, int tx, int ty) {
        var atlasSubtextures = GFX.Game.GetAtlasSubtextures(prefix + "Inactive");
        pressed.Add(CreateImage(x, y, tx, ty, atlasSubtextures[Index % atlasSubtextures.Count]));
        solid.Add(CreateImage(x, y, tx, ty, GFX.Game[prefix + "Active"]));
    }
}