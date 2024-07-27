using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

[Tracked(true), CustomEntity("DashToggleHelper/DashToggleBlock")]
class DashToggleBlock : CassetteBlock
{
	private DynamicData parentData;

	private bool initialized = false;

	public DashToggleBlock(Vector2 position, EntityID id, float width, float height, int dashes)
		: base(position, id, width, height, dashes, -1f)
	{
		parentData = DynamicData.For(this);
		parentData.Set("color", DashToggleHelperModule.getColor(Index));
	}

	public DashToggleBlock(EntityData data, Vector2 offset, EntityID id)
		: this(data.Position + offset, id, data.Width, data.Height, data.Int("dashes"))
	{
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		parentData.Invoke("ShiftSize", 1);
	}

	public override void Update()
	{
		base.Update();
		if (!initialized)
		{
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (entity != null)
			{
				Activated = entity.Dashes == Index;
				initialized = true;
			}
		}
        if (this.Index == DashToggleHelperModule.lastDashes) {
            this.Activated = true;
        } else {
            this.Activated = false;
        }
    }

	public void FindInGroupOverride(CassetteBlock block)
	{
		foreach (DashToggleBlock entity in base.Scene.Tracker.GetEntities<DashToggleBlock>())
		{
			List<CassetteBlock> list = parentData.Get<List<CassetteBlock>>("group");
			if (entity != this && entity != block && entity.Index == Index && !list.Contains(entity) && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))))
			{
				list.Add(entity);
				FindInGroupOverride(entity);
				entity.parentData.Set("group", list);
			}
		}
	}

	public bool CheckForSameOverride(float x, float y)
	{
		foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<DashToggleBlock>())
		{
			if (entity.Index == Index && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
			{
				return true;
			}
		}
		return false;
	}

	public void SetImageOverride(float x, float y, int tx, int ty)
	{
		List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/DashToggleHelper/dashtoggleblock/Inactive");
		parentData.Get<List<Image>>("pressed").Add((Image)parentData.Invoke("CreateImage", x, y, tx, ty, atlasSubtextures[Index % atlasSubtextures.Count]));
		parentData.Get<List<Image>>("solid").Add((Image)parentData.Invoke("CreateImage", x, y, tx, ty, GFX.Game["objects/DashToggleHelper/dashtoggleblock/Active"]));
	}
}
