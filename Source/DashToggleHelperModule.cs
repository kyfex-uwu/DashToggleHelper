using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.DashToggleHelper;

public class DashToggleHelperModule : EverestModule {
    public static int lastDashes = -1;
    private static bool moreDashelineLoaded;

    public static readonly Dictionary<int, Color> dashColors = new();

    static DashToggleHelperModule() {
        dashColors.Add(0, Player.UsedHairColor);
        dashColors.Add(1, Player.NormalHairColor);
        dashColors.Add(2, Player.TwoDashesHairColor);
    }

    public DashToggleHelperModule() {
        Instance = this;
        //Logger.SetLogLevel("DashToggleBlockModule", LogLevel.Verbose);

        EverestModuleMetadata moreDasheline = new() {
            Name = "MoreDasheline",
            Version = new Version(1, 7, 1)
        };
        moreDashelineLoaded = Everest.Loader.DependencyLoaded(moreDasheline);
    }

    public static DashToggleHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(DashToggleHelperModuleSettings);

    public static DashToggleHelperModuleSettings Settings => (DashToggleHelperModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(DashToggleHelperModuleSession);

    public static DashToggleHelperModuleSession Session => (DashToggleHelperModuleSession)Instance._Session;

    public static Color getColor(int color) {
        if (moreDashelineLoaded) return MoreDashelineIntegration.getColor(color);
        return dashColors.ContainsKey(color) ? dashColors[color] : Color.White;
    }

    private static void FindInGroupOverride(On.Celeste.CassetteBlock.orig_FindInGroup orig, CassetteBlock self,
        CassetteBlock block) {
        if (self is DashToggleBlock) {
            var dashToggleBlock = (DashToggleBlock)self;
            dashToggleBlock.FindInGroupOverride(block);
        } else {
            orig.Invoke(self, block);
        }
    }

    private static bool CheckForSameOverride(On.Celeste.CassetteBlock.orig_CheckForSame orig, CassetteBlock self,
        float x, float y) {
        if (self is DashToggleBlock) {
            var dashToggleBlock = (DashToggleBlock)self;
            return dashToggleBlock.CheckForSameOverride(x, y);
        }

        return orig.Invoke(self, x, y);
    }

    private static void SetImageOverride(On.Celeste.CassetteBlock.orig_SetImage orig, CassetteBlock self, float x,
        float y, int tx, int ty) {
        if (self is DashToggleBlock) {
            var dashToggleBlock = (DashToggleBlock)self;
            dashToggleBlock.SetImageOverride(x, y, tx, ty);
        } else {
            orig.Invoke(self, x, y, tx, ty);
        }
    }

    private static void ShiftSizeOverride(On.Celeste.CassetteBlock.orig_ShiftSize orig, CassetteBlock self, int amt) {
        if (self is DashToggleBlock) orig.Invoke(self, amt * 2);
        else orig.Invoke(self, amt);
    }

    private static CrystalColor DTSpinnerImage(CrystalStaticSpinner spinner) {
        if (spinner is DashToggleStaticSpinner) return CrystalColor.Rainbow;
        return spinner.color;
    }

    private static Color DTSpinnerColor(CrystalStaticSpinner spinner) {
        if (spinner is DashToggleStaticSpinner) return getColor(((DashToggleStaticSpinner)spinner).Dashes);
        return Color.White;
    }

    private static bool isDTSpinner(CrystalStaticSpinner spinner) {
        return spinner is DashToggleStaticSpinner;
    }

    public static void AddIfSameSpinner(CrystalStaticSpinner entity, Vector2 pos, CrystalStaticSpinner self) {
        if (self is DashToggleStaticSpinner == entity is DashToggleStaticSpinner &&
            !(!(self is DashToggleStaticSpinner) ||
              ((DashToggleStaticSpinner)self).Dashes == ((DashToggleStaticSpinner)entity).Dashes))
            self.AddSprite(pos);
    }

    private static void CreateSpritesOverride(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld<CrystalStaticSpinner>("color"));
        cursor.Remove();
        cursor.EmitDelegate(DTSpinnerImage);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchCall<Color>("get_White"));
        cursor.Remove();
        cursor.EmitLdarg0();
        cursor.EmitDelegate(DTSpinnerColor);

        ILLabel dest = null;
        cursor.GotoNext(MoveType.After, instr => {
            var toReturn = instr.MatchBgeUn(out var maybeDest);
            if (maybeDest != default) dest = maybeDest;
            return toReturn;
        });

        cursor.EmitLdloc(4);
        cursor.EmitDelegate(isDTSpinner);
        cursor.EmitLdarg0();
        cursor.EmitDelegate(isDTSpinner);
        cursor.EmitBneUn(dest);
    }

    private static void tintIfDTSpinner(CrystalStaticSpinner spinner, Image image) {
        if (spinner is DashToggleStaticSpinner) image.Color = getColor((spinner as DashToggleStaticSpinner).Dashes);
    }

    private static void AddSpriteOverride(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld<CrystalStaticSpinner>("color"));
        cursor.Remove();
        cursor.EmitDelegate(DTSpinnerImage);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<CrystalStaticSpinner>("filler"));
        cursor.EmitLdarg0();
        cursor.EmitLdloc1();
        cursor.EmitDelegate(tintIfDTSpinner);
    }

    private static void CreateOffSprites(On.Celeste.CrystalStaticSpinner.orig_CreateSprites orig,
        CrystalStaticSpinner self) {
        var expanded = self is DashToggleStaticSpinner ? self.expanded : true;
        orig.Invoke(self);
        if (!expanded)
            ((DashToggleStaticSpinner)self).CreateOffSprites();
    }

    private static void CheckDashUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        if (self.Dashes != lastDashes) lastDashes = self.Dashes;
        orig.Invoke(self);
    }

    public override void Load() {
        On.Celeste.CassetteBlock.FindInGroup += FindInGroupOverride;
        On.Celeste.CassetteBlock.CheckForSame += CheckForSameOverride;
        On.Celeste.CassetteBlock.SetImage += SetImageOverride;
        On.Celeste.CassetteBlock.ShiftSize += ShiftSizeOverride;
        IL.Celeste.CrystalStaticSpinner.CreateSprites += CreateSpritesOverride;
        IL.Celeste.CrystalStaticSpinner.AddSprite += AddSpriteOverride;
        On.Celeste.CrystalStaticSpinner.CreateSprites += CreateOffSprites;

        On.Celeste.Player.Update += CheckDashUpdate;
    }

    public override void Unload() {
        On.Celeste.CassetteBlock.FindInGroup -= FindInGroupOverride;
        On.Celeste.CassetteBlock.CheckForSame -= CheckForSameOverride;
        On.Celeste.CassetteBlock.SetImage -= SetImageOverride;
        On.Celeste.CassetteBlock.ShiftSize -= ShiftSizeOverride;
        IL.Celeste.CrystalStaticSpinner.CreateSprites -= CreateSpritesOverride;
        IL.Celeste.CrystalStaticSpinner.AddSprite -= AddSpriteOverride;
        On.Celeste.CrystalStaticSpinner.CreateSprites -= CreateOffSprites;

        On.Celeste.Player.Update -= CheckDashUpdate;
    }
}