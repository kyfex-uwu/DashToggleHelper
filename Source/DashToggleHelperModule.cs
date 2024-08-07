using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using static System.Diagnostics.Activity;

public class DashToggleHelperModule : EverestModule
{
	public static int lastDashes = -1;

	public static DashToggleHelperModule Instance { get; private set; }

	public override Type SettingsType => typeof(DashToggleHelperModuleSettings);

	public static DashToggleHelperModuleSettings Settings => (DashToggleHelperModuleSettings)Instance._Settings;

	public override Type SessionType => typeof(DashToggleHelperModuleSession);

	public static DashToggleHelperModuleSession Session => (DashToggleHelperModuleSession)Instance._Session;

	public DashToggleHelperModule()
	{
		Instance = this;
		//Logger.SetLogLevel("DashToggleBlockModule", LogLevel.Verbose);
	}

    private static Color[] dashColors = new Color[]{
        Calc.HexToColor("44B7FF"),
        Calc.HexToColor("f03232"),
        Calc.HexToColor("ff6def"),
        Calc.HexToColor("008000"),
        Calc.HexToColor("ffff00"),
        Calc.HexToColor("ff00ff")
    };
	public static Color getColor(int color) {
		return dashColors[color%dashColors.Length];
	}

    private void FindInGroupOverride(On.Celeste.CassetteBlock.orig_FindInGroup orig, CassetteBlock self, CassetteBlock block)
	{
		if (self is DashToggleBlock)
		{
			DashToggleBlock dashToggleBlock = (DashToggleBlock)self;
			dashToggleBlock.FindInGroupOverride(block);
		}
		else
		{
			orig.Invoke(self, block);
		}
	}

	private bool CheckForSameOverride(On.Celeste.CassetteBlock.orig_CheckForSame orig, CassetteBlock self, float x, float y)
	{
		if (self is DashToggleBlock)
		{
			DashToggleBlock dashToggleBlock = (DashToggleBlock)self;
			return dashToggleBlock.CheckForSameOverride(x, y);
		}
		return orig.Invoke(self, x, y);
	}

	private void SetImageOverride(On.Celeste.CassetteBlock.orig_SetImage orig, CassetteBlock self, float x, float y, int tx, int ty)
	{
		if (self is DashToggleBlock)
		{
			DashToggleBlock dashToggleBlock = (DashToggleBlock)self;
			dashToggleBlock.SetImageOverride(x, y, tx, ty);
		}
		else
		{
			orig.Invoke(self, x, y, tx, ty);
		}
	}

	private void ShiftSizeOverride(On.Celeste.CassetteBlock.orig_ShiftSize orig, CassetteBlock self, int amt)
	{
		if (self is DashToggleBlock)
		{
			orig.Invoke(self, amt * 2);
		}
		else
		{
			orig.Invoke(self, amt);
		}
	}

	private static CrystalColor DTSpinnerImage(CrystalStaticSpinner spinner) {
        if (spinner is DashToggleStaticSpinner) return CrystalColor.Rainbow;
        return DynamicData.For(spinner).Get<CrystalColor>("color");
    }
    private static Color DTSpinnerColor(CrystalStaticSpinner spinner) {
        if (spinner is DashToggleStaticSpinner) return getColor(((DashToggleStaticSpinner)spinner).Dashes);
        return Color.White;
    }
    private static bool isDTSpinner(CrystalStaticSpinner spinner) {
		return spinner is DashToggleStaticSpinner;
    }
	public static void AddIfSameSpinner(CrystalStaticSpinner entity, Vector2 pos, CrystalStaticSpinner self) {
		if ((self is DashToggleStaticSpinner) == (entity is DashToggleStaticSpinner)&&
				!(!(self is DashToggleStaticSpinner) || ((DashToggleStaticSpinner)self).Dashes == ((DashToggleStaticSpinner)entity).Dashes)) {
			self.AddSprite(pos);
		}
    }
    private void CreateSpritesOverride(ILContext il) {
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
        if (spinner is DashToggleStaticSpinner) {
            image.Color = getColor((spinner as DashToggleStaticSpinner).Dashes);
        }
    }
    private void AddSpriteOverride(ILContext il) {
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
    private void CreateOffSprites(On.Celeste.CrystalStaticSpinner.orig_CreateSprites orig, CrystalStaticSpinner self) {
		var expanded = self is DashToggleStaticSpinner ? DynamicData.For(self).Get<bool>("expanded") : true;
		orig.Invoke(self);
		if (!expanded)
			((DashToggleStaticSpinner)self).CreateOffSprites();
	}

    private void CheckDashUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        if (self.Dashes != lastDashes) {
            lastDashes = self.Dashes;
        }
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
