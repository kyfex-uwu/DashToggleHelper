using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Image = Monocle.Image;

namespace Celeste.Mod.DashToggleHelper;

[Tracked(true)]
[CustomEntity("DashToggleHelper/DashToggleStaticSpinner")]
internal class DashToggleStaticSpinner : CrystalStaticSpinner {
    public readonly int Dashes;
    private readonly int ID;
    private readonly string prefix;
    private readonly List<Image> offImages = new();

    private readonly List<Image> onImages = new();

    public DashToggleStaticSpinner(Vector2 position, bool attachToSolid, int dashes, int id, string prefix) :
        base(position, attachToSolid, CrystalColor.Purple) {
        Dashes = dashes;
        ID = id;
        this.prefix = prefix;
    }

    public DashToggleStaticSpinner(EntityData data, Vector2 offset) :
        this(data.Position + offset, data.Bool("attachToSolid"),
            data.Int("dashes"), data.ID, data.String("prefix", "objects/DashToggleHelper/dashtogglestaticspinner/")) { }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        Collidable = false;
    }

    public override void Update() {
        base.Update();

        if (DashToggleHelperModule.lastDashes != Dashes) {
            Collidable = false;
            Depth = 8900;
            if (filler != null) filler.Depth = 8901;
            if (border != null) border.Depth = 8902;

            foreach (var i in offImages) i.Visible = true;
            foreach (var i in onImages) i.Visible = false;
        }
        else {
            Depth = -8500;
            if (filler != null) filler.Depth = -8499;
            if (border != null) border.Depth = -8498;
            foreach (var i in offImages) i.Visible = false;
            foreach (var i in onImages) i.Visible = true;
        }
    }

    public void CreateOffSprites() {
        foreach (var maybe in Scene.Tracker.GetEntities<DashToggleStaticSpinner>())
            if (maybe is DashToggleStaticSpinner entity)
                if (entity.Dashes == Dashes &&
                    entity.ID > ID &&
                    entity.AttachToSolid == AttachToSolid &&
                    (entity.Position - Position).LengthSquared() < 576f)
                    AddSprite((Position + entity.Position) / 2f - Position);

        border.drawing[1] = filler;

        foreach (var c in Components) {
            if (!(c is Image)) continue;
            onImages.Add(c as Image);
        }

        if (filler != null)
            foreach (var c in filler) {
                if (!(c is Image)) continue;
                (c as Image).Color = DashToggleHelperModule.getColor(Dashes);
                onImages.Add(c as Image);
            }

        Calc.PushRandom(randomSeed);
        var atlasSubtextures = GFX.Game.GetAtlasSubtextures(prefix + "fg");
        var mTexture = Calc.Random.Choose(atlasSubtextures);
        var newColor = DashToggleHelperModule.getColor(Dashes);

        if (!SolidCheck(new Vector2(X - 4f, Y - 4f))) {
            var toAdd = new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!SolidCheck(new Vector2(X + 4f, Y - 4f))) {
            var toAdd = new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!SolidCheck(new Vector2(X + 4f, Y + 4f))) {
            var toAdd = new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        if (!SolidCheck(new Vector2(X - 4f, Y + 4f))) {
            var toAdd = new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f).SetColor(newColor);
            offImages.Add(toAdd);
            Add(toAdd);
        }

        foreach (var maybe in Scene.Tracker.GetEntities<DashToggleStaticSpinner>())
            if (maybe is DashToggleStaticSpinner entity)
                if (entity.Dashes == Dashes &&
                    entity.ID > ID &&
                    entity.AttachToSolid == AttachToSolid &&
                    (entity.Position - Position).LengthSquared() < 576f) {
                    if (filler == null) {
                        filler = new Entity(Position);
                        Scene.Add(filler);
                        filler.Depth = Depth + 1;
                    }

                    var atlasSubtextures2 = GFX.Game.GetAtlasSubtextures(prefix + "bg");
                    var newOffs = (Position + entity.Position) / 2f - Position;
                    var image = new Image(Calc.Random.Choose(atlasSubtextures2));
                    image.Position = newOffs;
                    image.Rotation = Calc.Random.Choose(0, 1, 2, 3) * ((float)Math.PI / 2f);
                    image.CenterOrigin();
                    image.SetColor(newColor);
                    offImages.Add(image);

                    filler.Add(image);
                }

        Calc.PopRandom();
    }
}