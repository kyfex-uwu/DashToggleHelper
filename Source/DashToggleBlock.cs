using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DashToggleHelper;

[Tracked(true), CustomEntity("DashToggleHelper/DashToggleBlock")]
class DashToggleBlock : CassetteBlock
{
	private bool initialized = false;
	private readonly string prefix;

	public DashToggleBlock(Vector2 position, EntityID id, float width, float height, int dashes, string prefix)
		: base(position, id, width, height, dashes, -1f) {
		this.color = DashToggleHelperModule.getColor(Index);
		this.prefix = prefix;
	}

	public DashToggleBlock(EntityData data, Vector2 offset, EntityID id)
		: this(data.Position + offset, id, data.Width, data.Height, data.Int("dashes"),
			data.String("prefix","objects/DashToggleHelper/dashtoggleblock/"))
	{
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		this.ShiftSize(1);
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
			if (entity != this && entity != block && entity.Index == this.Index && !this.group.Contains(entity) && 
			    (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))))
			{
				FindInGroupOverride(entity);
				this.group.Add(entity);
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
		List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(this.prefix+"Inactive");
		this.pressed.Add(this.CreateImage(x,y,tx,ty,atlasSubtextures[Index % atlasSubtextures.Count]));
		this.solid.Add(this.CreateImage(x,y,tx,ty,GFX.Game[this.prefix+"Active"]));
	}
}
