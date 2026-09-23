using System;
using System.Collections.Generic;
using System.Reflection;

public static class EMULATORS
{
    public static Emulator DOLPHIN = new PhoneEmulator(new[] {CONSOLETYPE.WII, CONSOLETYPE.GAMECUBE}, "Dolphin", "org.dolphinemu.dolphinemu", "org.dolphinemu.dolphinemu.ui.main.MainActivity", "AutoStartFile");
    
    public static Emulator CITRA_CANARY = new PhoneEmulator(new[] {CONSOLETYPE.N3DS}, "Citra", "org.citra.citra_emu.canary", "org.citra.citra_emu.activities.EmulationActivity");
    public static Emulator CITRA = new PhoneEmulator(new[] {CONSOLETYPE.N3DS}, "Citra", "org.citra.citra_emu", "org.citra.citra_emu.activities.EmulationActivity");

    public static Emulator MELONDS = new PhoneEmulator(new[] {CONSOLETYPE.NDS}, "MelonDS", "me.magnum.melonds", "me.magnum.melonds.ui.emulator.EmulatorActivity", "uri");
    public static Emulator DRASTIC = new PhoneEmulator(new[] {CONSOLETYPE.NDS}, "DraStic", "com.dsemu.drastic", "com.dsemu.drastic.DraSticActivity");

    //public static Emulator SUYU_EA = new PhoneEmulator(new[] {CONSOLETYPE.SWITCH}, "suyu", "org.yuzu.yuzu_emu.ea", "org.yuzu.yuzu_emu.activities.EmulationActivity");
    public static Emulator SUYU = new PhoneEmulator(new[] {CONSOLETYPE.SWITCH}, "suyu", "org.suyu.suyu_emu", "org.suyu.suyu_emu.activities.EmulationActivity");
    public static Emulator YUZU_EA = new PhoneEmulator(new[] {CONSOLETYPE.SWITCH}, "Yuzu", "org.yuzu.yuzu_emu.ea", "org.yuzu.yuzu_emu.activities.EmulationActivity");
    public static Emulator YUZU = new PhoneEmulator(new[] {CONSOLETYPE.SWITCH}, "Yuzu", "org.yuzu.yuzu_emu", "org.yuzu.yuzu_emu.activities.EmulationActivity");

    public static Emulator MYBOY = new PhoneEmulator(new[] {CONSOLETYPE.GBA}, "My Boy!", "com.fastemulator.gba", "com.fastemulator.gba.EmulatorActivity");
    public static Emulator MYBOY_FREE = new PhoneEmulator(new[] {CONSOLETYPE.GBA}, "My Boy!", "com.fastemulator.gbafree", "com.fastemulator.gba.EmulatorActivity");
    public static Emulator MYOLDBOY = new PhoneEmulator(new[] {CONSOLETYPE.GB,CONSOLETYPE.GBC}, "My OldBoy!", "com.fastemulator.gbc", "com.fastemulator.gbc.EmulatorActivity");
    public static Emulator MYOLDBOY_FREE = new PhoneEmulator(new[] {CONSOLETYPE.GB,CONSOLETYPE.GBC}, "My OldBoy!", "com.fastemulator.gbcfree", "com.fastemulator.gbc.EmulatorActivity");
    // ignores the game
    // public static Emulator PIZZABOYGBA_PRO = new PhoneEmulator(new[] {CONSOLETYPE.GBA}, "Pizza Boy GBA", "it.dbtecno.pizzaboygbapro", "it.dbtecno.pizzaboygba.MainActivity", "rom_uri");
    // public static Emulator PIZZABOYGBA = new PhoneEmulator(new[] {CONSOLETYPE.GBA}, "Pizza Boy GBA", "it.dbtecno.pizzaboygba", "it.dbtecno.pizzaboygba.MainActivity", "rom_uri");
    // public static Emulator PIZZABOYCLASSIC_PRO = new PhoneEmulator(new[] {CONSOLETYPE.GBA}, "Pizza Boy", "it.dbtecno.pizzaboypro", "it.dbtecno.pizzaboy.MainActivity", "rom_uri");
    // public static Emulator PIZZABOYCLASSIC = new PhoneEmulator(new[] {CONSOLETYPE.GBA}, "Pizza Boy", "it.dbtecno.pizzaboy", "it.dbtecno.pizzaboy.MainActivity", "rom_uri");
    
    // doesn't work, kicks you right out
    //public static Emulator JOHNGBAC = new PhoneEmulator(new[] {CONSOLETYPE.GBA, CONSOLETYPE.GB, CONSOLETYPE.GBC}, "John GBAC", "com.johnemulators.johngbac", "com.johnemulators.activity.GameActivity");
    public static Emulator GBCOID = new PhoneEmulator(new[] {CONSOLETYPE.GB, CONSOLETYPE.GBC}, "GBCoid", "com.androidemu.gbc", "com.androidemu.gbc.EmulatorActivity");
    
    public static Emulator MUPEN64_PRO = new PhoneEmulator(new[] {CONSOLETYPE.N64,CONSOLETYPE.N64DD}, "Mupen64Plus", "org.mupen64plusae.v3.fzurita.pro", "paulscode.android.mupen64plusae.SplashActivity");
    public static Emulator MUPEN64 = new PhoneEmulator(new[] {CONSOLETYPE.N64,CONSOLETYPE.N64DD}, "Mupen64Plus", "org.mupen64plusae.v3.fzurita", "paulscode.android.mupen64plusae.SplashActivity");
    public static Emulator MUPEN64_OG = new PhoneEmulator(new[] {CONSOLETYPE.N64,CONSOLETYPE.N64DD}, "Mupen64PlusAE", "paulscode.android.mupen64plusae", "paulscode.android.mupen64plusae.SplashActivity");
    
    public static Emulator SNES9X = new PhoneEmulator(new[] {CONSOLETYPE.SNES}, "Snes9x", "com.explusalpha.Snes9xPlus", "com.imagine.BaseActivity");
    public static Emulator SUPERRETRO16 = new PhoneEmulator(new[] {CONSOLETYPE.SNES}, "SuperRetro16", "com.bubblezapgames.supergnes", "com.bubblezapgames.supergnes.IntentFilterActivity");
    public static Emulator SUPERRETRO16_LITE = new PhoneEmulator(new[] {CONSOLETYPE.SNES}, "SuperRetro16", "com.bubblezapgames.supergnes_lite", "com.bubblezapgames.supergnes.IntentFilterActivity");
    public static Emulator NESOID = new PhoneEmulator(new[] {CONSOLETYPE.NES, CONSOLETYPE.FAMICON}, "Nesoid", "com.androidemu.nes", "com.androidemu.nes.EmulatorActivity");
    
    public static Emulator GEAROID = new PhoneEmulator(new[] {CONSOLETYPE.MASTERSYSTEMMARK3,CONSOLETYPE.GAMEGEAR}, "Gearoid", "com.androidemu.gg", "com.androidemu.gg.EmulatorActivity");
    public static Emulator GENSOID = new PhoneEmulator(new[] {CONSOLETYPE.MEGADRIVEGENESIS}, "Gensoid", "com.androidemu.gens", "com.androidemu.gens.EmulatorActivity");
    public static Emulator MASTERGEAR = new PhoneEmulator(new[] {CONSOLETYPE.GAMEGEAR}, "MasterGear", "com.fms.mg", "com.fms.emulib.MainActivity");
    // reported not working
    // public static Emulator REDREAM = new PhoneEmulator(new[] {CONSOLETYPE.DREAMCAST}, "redream", "io.recompiled.redream", "io.recompiled.redream.MainActivity");
    public static Emulator REICAST = new PhoneEmulator(new[] {CONSOLETYPE.DREAMCAST}, "Reicast", "com.reicast.emulator", "com.reicast.emulator.MainActivity");
    public static Emulator FLYCAST = new PhoneEmulator(new[] {CONSOLETYPE.DREAMCAST,CONSOLETYPE.SEGANAOMI,CONSOLETYPE.SEGANAOMI2,CONSOLETYPE.ATOMISWAVE}, "Flycast", "com.flycast.emulator", "com.reicast.emulator.MainActivity");
    public static Emulator UOYABAUSE_PRO = new PhoneEmulator(new[] {CONSOLETYPE.SEGASATURN}, "uoYabause", "org.uoyabause.android.pro", "org.uoyabause.android.Yabause", "org.uoyabause.android.FileNameEx");
    public static Emulator UOYABAUSE = new PhoneEmulator(new[] {CONSOLETYPE.SEGASATURN}, "uoYabause", "org.uoyabause.android", "org.uoyabause.android.Yabause", "org.uoyabause.android.FileNameEx");
    public static Emulator YABASANSHIRO2_PRO = new PhoneEmulator(new[] {CONSOLETYPE.SEGASATURN}, "YabaSanshiro 2", "org.devmiyax.yabasanshioro2.pro", "org.uoyabause.android.Yabause", "org.uoyabause.android.FileNameEx");
    public static Emulator YABASANSHIRO2 = new PhoneEmulator(new[] {CONSOLETYPE.SEGASATURN}, "YabaSanshiro 2", "org.devmiyax.yabasanshioro2", "org.uoyabause.android.Yabause", "org.uoyabause.android.FileNameEx");

    public static Emulator IRATAJAGUAR = new PhoneEmulator(new[] {CONSOLETYPE.ATARIJAGUAR}, "IrataJaguar", "ru.vastness.altmer.iratajaguar", "ru.vastness.altmer.iratajaguar.MainActivity", "rom");
    public static Emulator ATAROID = new PhoneEmulator(new[] {CONSOLETYPE.ATARI2600}, "Ataroid", "com.androidemu.atari", "com.androidemu.atari.EmulatorActivity");
    public static Emulator REAL3DO = new PhoneEmulator(new[] {CONSOLETYPE.THE3DO}, "Real3DOPlayer", "ru.vastness.altmer.real3doplayer", "ru.vastness.altmer.real3doplayer.MainActivity", "cd");
    public static Emulator COLEMPLUS = new PhoneEmulator(new[] {CONSOLETYPE.COLECOVISION}, "ColEm+", "com.fms.colem.deluxe", "com.fms.emulib.MainActivity");
    public static Emulator SPECCY_DELUXE = new PhoneEmulator(new[] {CONSOLETYPE.SINCLAIRZXSPECTRUM}, "Speccy", "com.fms.speccy.deluxe", "com.fms.emulib.MainActivity", "cd");
    public static Emulator SPECCY = new PhoneEmulator(new[] {CONSOLETYPE.SINCLAIRZXSPECTRUM}, "Speccy", "com.fms.speccy", "com.fms.emulib.MainActivity", "cd");
    public static Emulator FMSX_DELUXE = new PhoneEmulator(new[] {CONSOLETYPE.MSX,CONSOLETYPE.MSX2}, "fMSX", "com.fms.fmsx.deluxe", "com.fms.emulib.MainActivity", "cd");
    public static Emulator FMSX = new PhoneEmulator(new[] {CONSOLETYPE.MSX,CONSOLETYPE.MSX2}, "fMSX", "com.fms.fmsx", "com.fms.emulib.MainActivity", "cd");
    public static Emulator UAE4DROID = new PhoneEmulator(new[] {CONSOLETYPE.COMMAMIGA}, "UAE4Droid", "org.ab.uae", "org.ab.uae.StartActivity", "v");

    public static Emulator PPSSPP_GOLD = new PhoneEmulator(new[] {CONSOLETYPE.PSP}, "PPSSPP", "org.ppsspp.ppssppgold", "org.ppsspp.ppsspp.PpssppActivity");
    public static Emulator PPSSPP = new PhoneEmulator(new[] {CONSOLETYPE.PSP}, "PPSSPP", "org.ppsspp.ppsspp", "org.ppsspp.ppsspp.PpssppActivity");
    // reported not working
    // public static Emulator AETHERSX2 = new PhoneEmulator(new[] {CONSOLETYPE.PS2}, "AetherSX2", "xyz.aethersx2.android", "xyz.aethersx2.android.EmulationActivity", "bootPath");
    public static Emulator PLAY = new PhoneEmulator(new[] {CONSOLETYPE.PS2}, "Play!", "com.virtualapplications.play", "com.virtualapplications.play.ExternalEmulatorLauncher");
    public static Emulator EPSXE = new PhoneEmulator(new[] {CONSOLETYPE.PSX}, "ePSXe", "com.epsxe.ePSXe", "com.epsxe.ePSXe.ePSXe", "com.epsxe.ePSXe.isoName");
    public static Emulator FPSENG = new PhoneEmulator(new[] {CONSOLETYPE.PSX}, "FPseNG", "com.emulator.fpse64", "com.emulator.fpse64.Main");
    public static Emulator FPSE = new PhoneEmulator(new[] {CONSOLETYPE.PSX}, "FPse", "com.emulator.fpse", "com.emulator.fpse.Main");
    public static Emulator DUCKSTATION = new PhoneEmulator(new[] {CONSOLETYPE.PSX}, "DuckStation", "com.github.stenzek.duckstation", "com.github.stenzek.duckstation.EmulationActivity", "bootPath");

    // Robert Broglia's emulators (they're all very standardized and work great!):
    public static Emulator NESEMU = new PhoneEmulator(new[] {CONSOLETYPE.NES}, "NES.emu", "com.explusalpha.NesEmu", "com.imagine.BaseActivity");
    public static Emulator GBAEMU = new PhoneEmulator(new[] {CONSOLETYPE.GBA}, "GBA.emu", "com.explusalpha.GbaEmu", "com.imagine.BaseActivity");
    public static Emulator MDEMU = new PhoneEmulator(new[] {CONSOLETYPE.MEGADRIVEGENESIS, CONSOLETYPE.SEGAMEGACD, CONSOLETYPE.MASTERSYSTEMMARK3}, "MD.emu", "com.explusalpha.MdEmu", "com.imagine.BaseActivity");
    public static Emulator A2600EMU = new PhoneEmulator(new[] {CONSOLETYPE.ATARI2600}, "2600.emu", "com.explusalpha.A2600Emu", "com.imagine.BaseActivity");
    public static Emulator GBCEMU = new PhoneEmulator(new[] {CONSOLETYPE.GB,CONSOLETYPE.GBC}, "GBC.emu", "com.explusalpha.GbcEmu", "com.imagine.BaseActivity");
    public static Emulator LYNXEMU = new PhoneEmulator(new[] {CONSOLETYPE.ATARILYNX}, "Lynx.emu", "com.explusalpha.LynxEmu", "com.imagine.BaseActivity");
    public static Emulator SWANEMU = new PhoneEmulator(new[] {CONSOLETYPE.WONDERSWAN,CONSOLETYPE.WONDERSWANCOLOR}, "Swan.emu", "com.explusalpha.SwanEmu", "com.imagine.BaseActivity");
    public static Emulator MSXEMU = new PhoneEmulator(new[] {CONSOLETYPE.MSX,CONSOLETYPE.MSX2,CONSOLETYPE.COLECOVISION}, "MSX.emu", "com.explusalpha.MsxEmu", "com.imagine.BaseActivity");
    public static Emulator NGPEMU = new PhoneEmulator(new[] {CONSOLETYPE.NEOGEOPOCKET,CONSOLETYPE.NEOGEOPOCKETCOLOR}, "NGP.emu", "com.explusalpha.NgpEmu", "com.imagine.BaseActivity");
    public static Emulator C64EMU = new PhoneEmulator(new[] {CONSOLETYPE.COMM64}, "C64.emu", "com.explusalpha.C64Emu", "com.imagine.BaseActivity");
    public static Emulator PCEEMU = new PhoneEmulator(new[] {CONSOLETYPE.TURBOGRAFX16, CONSOLETYPE.SUPERGRAFX, CONSOLETYPE.TURBOGRAFXCD}, "PCE.emu", "com.explusalpha.PceEmu", "com.imagine.BaseActivity");
    public static Emulator NEOEMU = new PhoneEmulator(new[] {CONSOLETYPE.NEOGEO}, "NEO.emu", "com.explusalpha.NeoEmu", "com.imagine.BaseActivity");

    // GENERAL FRONTENDS:
    // if it doesn't work try =   com.swordfish.lemuroid.app.mobile.feature.main.MainActivity
    /*public static Emulator LEMUROID = new PhoneEmulator(new[] {CONSOLETYPE.ATARI2600, CONSOLETYPE.ATARI7800, CONSOLETYPE.ATARILYNX, CONSOLETYPE.NES, CONSOLETYPE.SNES, CONSOLETYPE.GB, CONSOLETYPE.GBC, CONSOLETYPE.GBA,
        CONSOLETYPE.MEGADRIVEGENESIS, CONSOLETYPE.SEGAMEGACD, CONSOLETYPE.MASTERSYSTEMMARK3, CONSOLETYPE.GAMEGEAR, CONSOLETYPE.N64, CONSOLETYPE.N64DD, CONSOLETYPE.PSX, CONSOLETYPE.PSP, CONSOLETYPE.FBNEOGAMES, CONSOLETYPE.NDS,
        CONSOLETYPE.TURBOGRAFX16, CONSOLETYPE.SUPERGRAFX, CONSOLETYPE.TURBOGRAFXCD, CONSOLETYPE.NEOGEOPOCKET, CONSOLETYPE.NEOGEOPOCKETCOLOR, CONSOLETYPE.WONDERSWAN, CONSOLETYPE.WONDERSWANCOLOR, CONSOLETYPE.N3DS},
        "Lemuroid", "com.swordfish.lemuroid", "com.swordfish.lemuroid.app.mobile.feature.main.MainActivity");*/ // doesn't work at all
    // no idea how to get it working either
    //public static Emulator VGBANEXT = new PhoneEmulator(new[] {CONSOLETYPE.GBA,CONSOLETYPE.GBC,CONSOLETYPE.GB,CONSOLETYPE.NES,CONSOLETYPE.FAMICON,CONSOLETYPE.SNES}, "VGBAnext", "com.fsm.emu", null);
    public static Emulator MAMEDROID = new PhoneEmulator(new[] {CONSOLETYPE.MAME}, "MAME", "com.seleuco.mame4droid", "com.seleuco.mame4droid.MAME4droid");
    public static Emulator J2ME = new PhoneEmulator(new[] {CONSOLETYPE.J2ME}, "J2ME", "ru.playsoftware.j2meloader", "ru.playsoftware.j2meloader.MainActivity");
    
    // No SAF for RetroArch: https://github.com/libretro/RetroArch/issues/12181
    /*public static Emulator RETROARCH = new PhoneEmulator(Array.Empty<CONSOLETYPE>(), // retroarch can just play anything I guess lol, but it gave issues, so we let ppl choose manually
        "RetroArch", "com.retroarch", "com.retroarch.browser.retroactivity.RetroActivityFuture", "ROM");
    public static Emulator RETROARCH_64 = new PhoneEmulator(Array.Empty<CONSOLETYPE>(), // retroarch can just play anything I guess lol
        "RetroArch", "com.retroarch.aarch64", "com.retroarch.browser.retroactivity.RetroActivityFuture", "ROM");*/

    // PC-ONLY EMULATORS:
    // No Android activity, we'll just put the ones that require special args in PC here. List of pre-checked emulators
    // DeSmuME, RyujiNX, RPCS3
    public static Emulator DOLPHIN_PC = new PCEmulator(new[] {CONSOLETYPE.WII, CONSOLETYPE.GAMECUBE}, "Dolphin", "Dolphin", "-e {file}");
    public static Emulator CEMU = new PCEmulator(new[] {CONSOLETYPE.WIIU}, "Cemu", "Cemu", "-g {file}");

    private static List<Emulator> AllEmulators = new List<Emulator>();

    public static List<Emulator> GetAllEmulators()
    {
        if (AllEmulators.IsNullOrEmpty())
        {
            foreach(FieldInfo info in typeof(EMULATORS).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    AllEmulators.Add((Emulator) info.GetValue(null));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
        return AllEmulators;
    }
}

public abstract class Emulator
{
    public List<CONSOLETYPE> consoles;
    public string name;
    public string androidPackage;
    public string androidActivity;
    public string androidExtra;
    public bool sendAsPath;
    public string pcArgs;
    public string exeFileName;

    public Emulator(CONSOLETYPE[] consoles, string name, string androidPackage, string androidActivity, string pcArgs = "{file}", string androidExtra = null, bool sendAsPath = true, string exeFileName = null)
    {
        this.consoles = new List<CONSOLETYPE>(consoles);
        this.name = name;
        this.androidPackage = androidPackage;
        this.androidActivity = androidActivity;
        this.androidExtra = androidExtra;
        this.pcArgs = pcArgs;
        this.sendAsPath = sendAsPath;
        this.exeFileName = exeFileName;
    }
}

public class PCEmulator : Emulator
{
    public PCEmulator(CONSOLETYPE[] consoles, string name, string exeFileName, string pcArgs = "{file}") : base(consoles, name, null, null, pcArgs, null, false, exeFileName)
    {
    }
}

public class PhoneEmulator : Emulator
{
    public PhoneEmulator(CONSOLETYPE[] consoles, string name, string androidPackage, string androidActivity, string androidExtra = "Uri", bool sendAsPath = true) : base(consoles, name, androidPackage, androidActivity, "{file}", androidExtra, sendAsPath)
    {
    }
}