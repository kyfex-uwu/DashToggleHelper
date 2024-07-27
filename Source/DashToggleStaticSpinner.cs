using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Image = Monocle.Image;

[Tracked(true), CustomEntity("DashToggleHelper/DashToggleStaticSpinner")]
class DashToggleStaticSpinner : CrystalStaticSpinner {
    public readonly int Dashes;
    private DynamicData parentData;
    public DashToggleStaticSpinner(Vector2 position, bool attachToSolid, int dashes, int id) :
        base(position, attachToSolid, CrystalColor.Purple) {
        this.Dashes = dashes;
        parentData = DynamicData.For(this);
        parentData.Set("ID", id);
    }
    public DashToggleStaticSpinner(EntityData data, Vector2 offset) :
        this(data.Position + offset, data.Bool("attachToSolid"), data.Int("dashes"), data.ID) {}


    public override void Awake(Scene scene) {
        base.Awake(scene);
        this.Collidable = false;
    }

    public override void Update() {
        base.Update();

        var filler = parentData.Get<Entity>("filler");
        var border = parentData.Get<Entity>("border");
        if (DashToggleHelperModule.lastDashes != this.Dashes) {
            this.Collidable = false;
            this.Depth = 8900;
            if (filler != null) {
                filler.Depth = 8901;
            }
            if (border != null) {
                border.Depth = 8902;
            }

            foreach(Image i in this.offImages) {
                i.Visible = true;
            }
            foreach (Image i in this.onImages) {
                i.Visible = false;
            }
        } else {
            this.Depth = -8500;
            if (filler != null) {
                filler.Depth = -8499;
            }
            if (border != null) {
                border.Depth = -8498;
            }
            foreach (Image i in this.offImages) {
                i.Visible = false;
            }
            foreach (Image i in this.onImages) {
                i.Visible = true;
            }
        }
    }

    private List<Image> onImages = new List<Image>();
    private List<Image> offImages = new List<Image>();
    public void CreateOffSprites() {
        foreach (DashToggleStaticSpinner entity in base.Scene.Tracker.GetEntities<DashToggleStaticSpinner>()) {
            var data = DynamicData.For(entity);
            if (entity.Dashes == this.Dashes &&
                    data.Get<int>("ID") > parentData.Get<int>("ID") &&
                    entity.AttachToSolid == AttachToSolid &&
                    (entity.Position - Position).LengthSquared() < 576f) {
                parentData.Invoke("AddSprite", (Position + entity.Position) / 2f - Position);
            }
        }
        DynamicData.For(parentData.Get<Entity>("border")).Get<Entity[]>("drawing")[1] = parentData.Get<Entity>("filler");

        foreach (Component c in this.Components) {
            if (!(c is Image)) continue;
            this.onImages.Add(c as Image);
        }
        var filler = parentData.Get<Entity>("filler");
        if (filler != null) {
            foreach (Component c in filler) {
                if (!(c is Image)) continue;
                (c as Image).Color = DashToggleHelperModule.getColor(this.Dashes);
                this.onImages.Add(c as Image);
            }
        }

        Calc.PushRandom(parentData.Get<int>("randomSeed"));
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/DashToggleHelper/dashtogglestaticspinner/fg");
        MTexture mTexture = Calc.Random.Choose(atlasSubtextures);
        Color color = DashToggleHelperModule.getColor(this.Dashes);

        if (!parentData.Invoke<bool>("SolidCheck", new Vector2(base.X - 4f, base.Y - 4f))) {
            var toAdd = new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f).SetColor(color);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!parentData.Invoke<bool>("SolidCheck", new Vector2(base.X + 4f, base.Y - 4f))) {
            var toAdd=new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f).SetColor(color);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!parentData.Invoke<bool>("SolidCheck", new Vector2(base.X + 4f, base.Y + 4f))) {
            var toAdd=new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f).SetColor(color);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!parentData.Invoke<bool>("SolidCheck", new Vector2(base.X - 4f, base.Y + 4f))) {
            var toAdd=new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f).SetColor(color);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        foreach (DashToggleStaticSpinner entity in base.Scene.Tracker.GetEntities<DashToggleStaticSpinner>()) {
            if (entity.Dashes==this.Dashes &&
                    entity.parentData.Get<int>("ID") > parentData.Get<int>("ID") && 
                    entity.AttachToSolid == AttachToSolid && 
                    (entity.Position - Position).LengthSquared() < 576f) {
                var filler2 = parentData.Get<Entity>("filler");
                if (filler2 == null) {
                    filler2 = new Entity(Position);
                    parentData.Set("filler", filler2);
                    base.Scene.Add(filler2);
                    filler2.Depth = base.Depth + 1;
                }

                List<MTexture> atlasSubtextures2 = GFX.Game.GetAtlasSubtextures("objects/DashToggleHelper/dashtogglestaticspinner/bg");
                var offset = (Position + entity.Position) / 2f - Position;
                Image image = new Image(Calc.Random.Choose(atlasSubtextures2));
                image.Position = offset;
                image.Rotation = (float)Calc.Random.Choose(0, 1, 2, 3) * ((float)Math.PI / 2f);
                image.CenterOrigin();
                image.SetColor(color);
                this.offImages.Add(image);

                filler.Add(image);
            }
        }

        Calc.PopRandom();
    }
}
