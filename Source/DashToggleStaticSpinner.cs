using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Image = Monocle.Image;

namespace Celeste.Mod.DashToggleHelper;

[Tracked(true), CustomEntity("DashToggleHelper/DashToggleStaticSpinner")]
class DashToggleStaticSpinner : CrystalStaticSpinner {
    public readonly int Dashes;
    private readonly int ID;
    private readonly string prefix;

    public DashToggleStaticSpinner(Vector2 position, bool attachToSolid, int dashes, int id, string prefix) :
        base(position, attachToSolid, CrystalColor.Purple) {
        this.Dashes = dashes;
        this.ID = id;
        this.prefix = prefix;
    }
    public DashToggleStaticSpinner(EntityData data, Vector2 offset) :
        this(data.Position + offset, data.Bool("attachToSolid"), 
            data.Int("dashes"), data.ID, data.String("prefix","objects/DashToggleHelper/dashtogglestaticspinner/")) {}
    
    public override void Awake(Scene scene) {
        base.Awake(scene);
        this.Collidable = false;
    }

    public override void Update() {
        base.Update();

        if (DashToggleHelperModule.lastDashes != this.Dashes) {
            this.Collidable = false;
            this.Depth = 8900;
            if (this.filler != null) {
                this.filler.Depth = 8901;
            }
            if (this.border != null) {
                this.border.Depth = 8902;
            }

            foreach(Image i in this.offImages) {
                i.Visible = true;
            }
            foreach (Image i in this.onImages) {
                i.Visible = false;
            }
        } else {
            this.Depth = -8500;
            if (this.filler != null) {
                this.filler.Depth = -8499;
            }
            if (this.border != null) {
                this.border.Depth = -8498;
            }
            foreach (var i in this.offImages) {
                i.Visible = false;
            }
            foreach (var i in this.onImages) {
                i.Visible = true;
            }
        }
    }

    private List<Image> onImages = new();
    private List<Image> offImages = new();
    public void CreateOffSprites() {
        foreach (DashToggleStaticSpinner entity in base.Scene.Tracker.GetEntities<DashToggleStaticSpinner>()) {
            if (entity.Dashes == this.Dashes &&
                    entity.ID > this.ID &&
                    entity.AttachToSolid == AttachToSolid &&
                    (entity.Position - Position).LengthSquared() < 576f) { 
                        this.AddSprite((Position + entity.Position) / 2f - Position);
            }
        }

        this.border.drawing[1] = this.filler;

        foreach (Component c in this.Components) {
            if (!(c is Image)) continue;
            this.onImages.Add(c as Image);
        }
        if (this.filler != null) {
            foreach (Component c in this.filler) {
                if (!(c is Image)) continue;
                (c as Image).Color = DashToggleHelperModule.getColor(this.Dashes);
                this.onImages.Add(c as Image);
            }
        }

        Calc.PushRandom(this.randomSeed);
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(this.prefix+"fg");
        MTexture mTexture = Calc.Random.Choose(atlasSubtextures);
        Color newColor = DashToggleHelperModule.getColor(this.Dashes);

        if (!this.SolidCheck( new Vector2(base.X - 4f, base.Y - 4f))) {
            var toAdd = new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!this.SolidCheck(new Vector2(base.X + 4f, base.Y - 4f))) {
            var toAdd=new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!this.SolidCheck(new Vector2(base.X + 4f, base.Y + 4f))) {
            var toAdd=new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!this.SolidCheck(new Vector2(base.X - 4f, base.Y + 4f))) {
            var toAdd=new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        foreach (DashToggleStaticSpinner entity in base.Scene.Tracker.GetEntities<DashToggleStaticSpinner>()) {
            if (entity.Dashes==this.Dashes &&
                    entity.ID>this.ID && 
                    entity.AttachToSolid == AttachToSolid && 
                    (entity.Position - Position).LengthSquared() < 576f) {
                if (this.filler == null) {
                    this.filler = new Entity(Position);
                    base.Scene.Add(this.filler);
                    this.filler.Depth = base.Depth + 1;
                }

                List<MTexture> atlasSubtextures2 = GFX.Game.GetAtlasSubtextures(this.prefix+"bg");
                var newOffs = (Position + entity.Position) / 2f - Position;
                Image image = new Image(Calc.Random.Choose(atlasSubtextures2));
                image.Position = newOffs;
                image.Rotation = Calc.Random.Choose(0, 1, 2, 3) * ((float)Math.PI / 2f);
                image.CenterOrigin();
                image.SetColor(newColor);
                this.offImages.Add(image);

                filler.Add(image);
            }
        }

        Calc.PopRandom();
    }
}
