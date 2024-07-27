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

	private static object CSO_func1(CrystalStaticSpinner spinner) {
        if (spinner is DashToggleStaticSpinner) return CrystalColor.Rainbow;
        return DynamicData.For(spinner).Get<CrystalColor>("color");
    }
    private static object CSO_func2(CrystalStaticSpinner spinner) {
        if (spinner is DashToggleStaticSpinner) return getColor(((DashToggleStaticSpinner)spinner).Dashes);
        return Color.White;
    }
    private void CreateSpritesOverride(ILContext il) {
		var cursor = new ILCursor(il);

		cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld<CrystalStaticSpinner>("color"));
		cursor.Remove();
		cursor.EmitDelegate(CSO_func1);

		cursor.GotoNext(MoveType.Before, instr => instr.MatchCall<Color>("get_White"));
		cursor.Remove();
		cursor.Emit(OpCodes.Ldarg_0);
		cursor.EmitDelegate(CSO_func2);
    }
    private static object ASO_func1(CrystalStaticSpinner spinner) {
        if (spinner is DashToggleStaticSpinner) return CrystalColor.Rainbow;
        return DynamicData.For(spinner).Get<CrystalColor>("color");
    }
    private static void ASO_func2(CrystalStaticSpinner spinner, Image image) {
        if (spinner is DashToggleStaticSpinner) {
            image.Color = getColor((spinner as DashToggleStaticSpinner).Dashes);
        }
    }
    private void AddSpriteOverride(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld<CrystalStaticSpinner>("color"));
        cursor.Remove();
        cursor.EmitDelegate(ASO_func1);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdarg(0),
			instr => instr.MatchLdfld<CrystalStaticSpinner>("filler"));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.EmitDelegate(ASO_func2);
    }
    private void CreateOffSprites(On.Celeste.CrystalStaticSpinner.orig_CreateSprites orig, CrystalStaticSpinner self) {
		var expanded = self is DashToggleStaticSpinner ? DynamicData.For(self).Get<bool>("expanded") : true;
		orig.Invoke(self);
		if (!expanded)
			((DashToggleStaticSpinner)self).CreateOffSprites();
	}

    private void CheckDashUpdate(On.Celeste.Level.orig_UpdateTime orig, Level self) {
        Player entity = self.Tracker.GetEntity<Player>();
        if (entity == null) {
            lastDashes = -1;
        }else if (entity.Dashes != lastDashes) {
            lastDashes = entity.Dashes;
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

        On.Celeste.Level.UpdateTime += CheckDashUpdate;
	}

    public override void Unload() {
        On.Celeste.CassetteBlock.FindInGroup -= FindInGroupOverride;
        On.Celeste.CassetteBlock.CheckForSame -= CheckForSameOverride;
        On.Celeste.CassetteBlock.SetImage -= SetImageOverride;
        On.Celeste.CassetteBlock.ShiftSize -= ShiftSizeOverride;
        IL.Celeste.CrystalStaticSpinner.CreateSprites -= CreateSpritesOverride;
        IL.Celeste.CrystalStaticSpinner.AddSprite -= AddSpriteOverride;
        On.Celeste.CrystalStaticSpinner.CreateSprites -= CreateOffSprites;

        On.Celeste.Level.UpdateTime -= CheckDashUpdate;
    }
}
