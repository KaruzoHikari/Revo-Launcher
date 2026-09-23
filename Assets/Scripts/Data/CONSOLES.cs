using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum CONSOLETYPE
{
    // gametdb consoles:
    UNKNOWN, WII, GAMECUBE, WIIU, NDS, N3DS, SWITCH, PS3, // 7
    
    // libretro consoles:
    AMSTRADCPC, AMSTRADGX4000, ARDUBOY, ATARI2600, ATARI5200, ATARI7800, ATARI8BIT, ATARIJAGUAR, ATARILYNX, // 16
    ATARIST, ATOMISWAVE, WONDERSWAN, WONDERSWANCOLOR, CANNONBALL, CASIOLOOPY, CASIOPV1000, CAVESTORY, CHAILOVE, // 25
    COLECOVISION, COMM64, COMMAMIGA, COMMCD32, COMMCDTV, COMMPLUS4, COMMVIC20, DOOM, DOS, DINOTHAWR, // 35
    ARCADIA2001, ADVENTUREVISION, SUPERCASETTEVISION, FBNEOGAMES, CHANNELF, FLASHBACK, SUPERACAN, VECTREX, // 43
    GAMEPARKGP32, HANDELECTRONICGAME, HARTUNGGAMEMASTER, JUMPNBUMP, LEAPFROG, LOWRESNX, LUTRO, MAME, ODYSSEY2, // 52
    INTELLIVISION, MSX, MSX2, XBOX, XBOX360, MRBOOM, TURBOGRAFX16, TURBOGRAFXCD, SUPERGRAFX, PC8801, PC98, PCFX, // 64
    FAMICON, GB, GBA, GBC, N64, N64DD, NES, POKEMONMINI, SATELLAVIEW, SUFAMITURBO, SNES, VIRTUALBOY, // 76
    PHILLIPSCDI, PHILLIPSVIDEOPAC, QUAKE, QUAKE2, QUAKE3, STUDIO2, RPGMAKER, RICKDANGEROUS, NEOGEO, NEOGEOCD, // 86
    NEOGEOPOCKET, NEOGEOPOCKETCOLOR, SCUMMVM, SEGA32X, DREAMCAST, GAMEGEAR, MASTERSYSTEMMARK3, MEGADRIVEGENESIS, // 94
    SEGAMEGACD, SEGANAOMI, SEGANAOMI2, SEGAPICO, SEGASG1000, SEGASATURN, SHARPX1, SHARPX68000, SINCLAIRZX81, // 103
    SINCLAIRZXSPECTRUM, PSX, PS2, PS4, PSP, PSVITA, TIC80, THE3DO, THOMSONMOTO, TIGERGAME, // 113
    TOMBRAIDER, VTECHCREATIVISION, VTECHVSMILE, VIRCON32, WASM4, WATARASUPERVISION, WOLFENSTEIN3D, // 120
    COMMPET, SPECTRAVIDEO,
    
    // other
    STEAM, J2ME
}

public static class CONSOLES
{
    // GameTDB consoles
    public static Console WII = new(CONSOLETYPE.WII, "Nintendo Wii", "wii", "png",
        BOXTYPE.DVD, Vector3.one, Color.white, true, true,
        new[] { "wbfs", "wbf", "wad" }, "Wii");

    public static Console GAMECUBE = new(CONSOLETYPE.GAMECUBE, "Nintendo GameCube", "wii", "png",
        BOXTYPE.DVD, Vector3.one, new Color(0.09f, 0.09f, 0.09f), true, true,
        new[] { "rvz", "gcz", "gcm", "gcz" }, "Wii");

    public static Console WIIU = new(CONSOLETYPE.WIIU, "Nintendo Wii U", "wiiu", "jpg",
        BOXTYPE.DVD, Vector3.one, new Color(0.0f, 0.6411f, 0.8589f, 1f), true, true,
        new[] { "rpx" }, "WiiU");

    public static Console N3DS = new(CONSOLETYPE.N3DS, "Nintendo 3DS", "3ds", "jpg",
        BOXTYPE.CARTRIDGE, new Vector3(1f, 1f, 0.75f), Color.white, true, true,
        new[] { "3ds", "cia" }, "3DS");

    public static Console NDS = new(CONSOLETYPE.NDS, "Nintendo DS", "ds", "jpg",
        BOXTYPE.CARTRIDGE, Vector3.one, Color.white, false, true,
        new[] { "nds" }, "DS");

    public static Console SWITCH = new(CONSOLETYPE.SWITCH, "Nintendo Switch", "switch", "png",
        BOXTYPE.DVD, new Vector3(0.85f, 0.98f, 0.8f), new Color(1f, 1f, 0.97f, 0.1f), false, true,
        new[] { "nsp", "xci" }, "Switch");

    public static Console PS3 = new(CONSOLETYPE.PS3, "PlayStation 3", "ps3", "jpg",
        BOXTYPE.DVD, new Vector3(1f, 0.895f, 1f), new Color(1f, 1f, 0.97f, 0.1f), false, true,
        new[] { "" }, "PS3");

    // Here are the LibRetro consoles!
    public static Console UNKNOWN = new RetroConsole(CONSOLETYPE.UNKNOWN, "Unknown", new[] { "" }, false);
    public static Console GBA = new RetroConsole(CONSOLETYPE.GBA, "Nintendo - Game Boy Advance", new[] { "gba" });
    public static Console GBC = new RetroConsole(CONSOLETYPE.GBC, "Nintendo - Game Boy Color", new[] { "gbc" });
    public static Console GB = new RetroConsole(CONSOLETYPE.GB, "Nintendo - Game Boy", new[] { "gb" });
    public static Console AMSTRADCPC = new RetroConsole(CONSOLETYPE.AMSTRADCPC, "Amstrad - CPC", new[] { "cdc" });
    public static Console AMSTRADGX4000 = new RetroConsole(CONSOLETYPE.AMSTRADGX4000, "Amstrad - GX4000", new[] { "" });
    public static Console ARDUBOY = new RetroConsole(CONSOLETYPE.ARDUBOY, "Arduboy Inc - Arduboy", new[] { "" });
    public static Console ATARI2600 = new RetroConsole(CONSOLETYPE.ATARI2600, "Atari - 2600", new[] { "a26" });
    public static Console ATARI5200 = new RetroConsole(CONSOLETYPE.ATARI5200, "Atari - 5200", new[] { "a52" });
    public static Console ATARI7800 = new RetroConsole(CONSOLETYPE.ATARI7800, "Atari - 7800", new[] { "a78" });
    public static Console ATARI8BIT = new RetroConsole(CONSOLETYPE.ATARI8BIT, "Atari - 8-bit", new[] { "" });
    public static Console ATARIJAGUAR = new RetroConsole(CONSOLETYPE.ATARIJAGUAR, "Atari - Jaguar", new[] { "" });
    public static Console ATARILYNX = new RetroConsole(CONSOLETYPE.ATARILYNX, "Atari - Lynx", new[] { "lnx" });
    public static Console ATARIST = new RetroConsole(CONSOLETYPE.ATARIST, "Atari - ST", new[] { "" });
    public static Console ATOMISWAVE = new RetroConsole(CONSOLETYPE.ATOMISWAVE, "Atomiswave", new[] { "" });
    public static Console WONDERSWAN = new RetroConsole(CONSOLETYPE.WONDERSWAN, "Bandai - WonderSwan", new[] { "ws" });
    public static Console WONDERSWANCOLOR = new RetroConsole(CONSOLETYPE.WONDERSWANCOLOR, "Bandai - WonderSwan Color", new[] { "wsc" });
    public static Console CANNONBALL = new RetroConsole(CONSOLETYPE.CANNONBALL, "Cannonball", new[] { "" });
    public static Console CASIOLOOPY = new RetroConsole(CONSOLETYPE.CASIOLOOPY, "Casio - Loopy", new[] { "" });
    public static Console CASIOPV1000 = new RetroConsole(CONSOLETYPE.CASIOPV1000, "Casio - PV-1000", new[] { "" });
    public static Console CAVESTORY = new RetroConsole(CONSOLETYPE.CAVESTORY, "Cave Story", new[] { "" });
    public static Console CHAILOVE = new RetroConsole(CONSOLETYPE.CHAILOVE, "ChaiLove", new[] { "" });
    public static Console COLECOVISION = new RetroConsole(CONSOLETYPE.COLECOVISION, "Coleco - ColecoVision", new[] { "col" });
    public static Console COMM64 = new RetroConsole(CONSOLETYPE.COMM64, "Commodore - 64", new[] { "" });
    public static Console COMMAMIGA = new RetroConsole(CONSOLETYPE.COMMAMIGA, "Commodore - Amiga", new[] { "" });
    public static Console COMMCD32 = new RetroConsole(CONSOLETYPE.COMMCD32, "Commodore - CD32", new[] { "" });
    public static Console COMMCDTV = new RetroConsole(CONSOLETYPE.COMMCDTV, "Commodore - CDTV", new[] { "" });
    public static Console COMMPLUS4 = new RetroConsole(CONSOLETYPE.COMMPLUS4, "Commodore - Plus-4", new[] { "" });
    public static Console COMMVIC20 = new RetroConsole(CONSOLETYPE.COMMVIC20, "Commodore - VIC-20", new[] { "" });
    public static Console SPECTRAVIDEO = new RetroConsole(CONSOLETYPE.SPECTRAVIDEO, "Spectravideo - SVI-318 - SVI-328", new[] { "" });
    // No covers:
    // public static Console COMMPET = new RetroConsole(CONSOLETYPE.COMMPET, "Commodore - PET", new[] { "" });
    public static Console DOOM = new RetroConsole(CONSOLETYPE.DOOM, "DOOM", new[] { "" });
    public static Console DOS = new RetroConsole(CONSOLETYPE.DOS, "DOS", new[] { "" });
    public static Console DINOTHAWR = new RetroConsole(CONSOLETYPE.DINOTHAWR, "Dinothawr", new[] { "" });
    public static Console ARCADIA2001 = new RetroConsole(CONSOLETYPE.ARCADIA2001, "Emerson - Arcadia 2001", new[] { "" });
    public static Console ADVENTUREVISION = new RetroConsole(CONSOLETYPE.ADVENTUREVISION, "Entex - Adventure Vision", new[] { "" });
    public static Console SUPERCASETTEVISION = new RetroConsole(CONSOLETYPE.SUPERCASETTEVISION, "Epoch - Super Cassette Vision", new[] { "" });
    public static Console FBNEOGAMES = new RetroConsole(CONSOLETYPE.FBNEOGAMES, "FBNeo - Arcade Games", new[] { "" });
    public static Console CHANNELF = new RetroConsole(CONSOLETYPE.CHANNELF, "Fairchild - Channel F", new[] { "" });
    public static Console FLASHBACK = new RetroConsole(CONSOLETYPE.FLASHBACK, "Flashback", new[] { "" });
    public static Console SUPERACAN = new RetroConsole(CONSOLETYPE.SUPERACAN, "Funtech - Super Acan", new[] { "" });
    public static Console VECTREX = new RetroConsole(CONSOLETYPE.VECTREX, "GCE - Vectrex", new[] { "" });
    public static Console GAMEPARKGP32 = new RetroConsole(CONSOLETYPE.GAMEPARKGP32, "GamePark - GP32", new[] { "" });
    public static Console HANDELECTRONICGAME = new RetroConsole(CONSOLETYPE.HANDELECTRONICGAME, "Handheld Electronic Game", new[] { "" });
    public static Console HARTUNGGAMEMASTER = new RetroConsole(CONSOLETYPE.HARTUNGGAMEMASTER, "Hartung - Game Master", new[] { "" });
    // No covers:
    // public static Console JUMPNBUMP = new RetroConsole(CONSOLETYPE.JUMPNBUMP, "Jump 'n Bump", new[] { "" });
    public static Console LEAPFROG = new RetroConsole(CONSOLETYPE.LEAPFROG, "LeapFrog - Leapster Learning Game System", new[] { "" });
    public static Console LOWRESNX = new RetroConsole(CONSOLETYPE.LOWRESNX, "LowRes NX", new[] { "" });
    // No covers:
    // public static Console LUTRO = new RetroConsole(CONSOLETYPE.LUTRO, "Lutro", new[] { "" });
    public static Console MAME = new RetroConsole(CONSOLETYPE.MAME, "MAME", new[] { "" });
    public static Console ODYSSEY2 = new RetroConsole(CONSOLETYPE.ODYSSEY2, "Magnavox - Odyssey2", new[] { "" });
    public static Console INTELLIVISION = new RetroConsole(CONSOLETYPE.INTELLIVISION, "Mattel - Intellivision", new[] { "" });
    public static Console MSX = new RetroConsole(CONSOLETYPE.MSX, "Microsoft - MSX", new[] { "mx1" });
    public static Console MSX2 = new RetroConsole(CONSOLETYPE.MSX2, "Microsoft - MSX2", new[] { "mx2" });
    public static Console XBOX = new RetroConsole(CONSOLETYPE.XBOX, "Microsoft - Xbox", new[] { "xbe" },
        BOXTYPE.DVD, new Vector3(1f, 1f, 1f), new Color(0.17f, 0.66f, 0.15f, 0.8f));
    public static Console XBOX360 = new RetroConsole(CONSOLETYPE.XBOX360, "Microsoft - Xbox 360", new[] { "xex" }, 
        BOXTYPE.DVD, new Vector3(1f, 1f, 1f), new Color(0.17f, 0.66f, 0.15f, 0.8f));
    public static Console MRBOOM = new RetroConsole(CONSOLETYPE.MRBOOM, "MrBoom", new[] { "" });
    public static Console TURBOGRAFX16 = new RetroConsole(CONSOLETYPE.TURBOGRAFX16, "NEC - PC Engine - TurboGrafx 16", new[] { "pce" });
    public static Console TURBOGRAFXCD = new RetroConsole(CONSOLETYPE.TURBOGRAFXCD, "NEC - PC Engine CD - TurboGrafx-CD", new[] { "" });
    public static Console SUPERGRAFX = new RetroConsole(CONSOLETYPE.SUPERGRAFX, "NEC - PC Engine SuperGrafx", new[] { "sgx" });
    public static Console PC8801 = new RetroConsole(CONSOLETYPE.PC8801, "NEC - PC-8001 - PC-8801", new[] { "" });
    public static Console PC98 = new RetroConsole(CONSOLETYPE.PC98, "NEC - PC-98", new[] { "" });
    public static Console PCFX = new RetroConsole(CONSOLETYPE.PCFX, "NEC - PC-FX", new[] { "" });
    public static Console FAMICON = new RetroConsole(CONSOLETYPE.FAMICON, "Nintendo - Family Computer Disk System", new[] { "fds" });
    public static Console N64 = new RetroConsole(CONSOLETYPE.N64, "Nintendo - Nintendo 64", new[] { "n64","v64","z64" });
    public static Console N64DD = new RetroConsole(CONSOLETYPE.N64DD, "Nintendo - Nintendo 64DD", new[] { "n64","v64","z64","ndd" });
    public static Console NES = new RetroConsole(CONSOLETYPE.NES, "Nintendo - Nintendo Entertainment System", new[] { "nes", "nez", "unf", "unif" });
    public static Console POKEMONMINI = new RetroConsole(CONSOLETYPE.POKEMONMINI, "Nintendo - Pokemon Mini", new[] { "" });
    public static Console SATELLAVIEW = new RetroConsole(CONSOLETYPE.SATELLAVIEW, "Nintendo - Satellaview", new[] { "" });
    public static Console SUFAMITURBO = new RetroConsole(CONSOLETYPE.SUFAMITURBO, "Nintendo - Sufami Turbo", new[] { "" });
    public static Console SNES = new RetroConsole(CONSOLETYPE.SNES, "Nintendo - Super Nintendo Entertainment System", new[] { "smc","sfc" });
    public static Console VIRTUALBOY = new RetroConsole(CONSOLETYPE.VIRTUALBOY, "Nintendo - Virtual Boy", new[] { "vb" });
    public static Console PHILLIPSCDI = new RetroConsole(CONSOLETYPE.PHILLIPSCDI, "Philips - CD-i", new[] { "" });
    public static Console PHILLIPSVIDEOPAC = new RetroConsole(CONSOLETYPE.PHILLIPSVIDEOPAC, "Philips - Videopac+", new[] { "" });
    public static Console QUAKE = new RetroConsole(CONSOLETYPE.QUAKE, "Quake", new[] { "" });
    public static Console QUAKE2 = new RetroConsole(CONSOLETYPE.QUAKE2, "Quake II", new[] { "" });
    public static Console QUAKE3 = new RetroConsole(CONSOLETYPE.QUAKE3, "Quake III", new[] { "" });
    public static Console STUDIO2 = new RetroConsole(CONSOLETYPE.STUDIO2, "RCA - Studio II", new[] { "" });
    public static Console RPGMAKER = new RetroConsole(CONSOLETYPE.RPGMAKER, "RPG Maker", new[] { "" });
    public static Console RICKDANGEROUS = new RetroConsole(CONSOLETYPE.RICKDANGEROUS, "Rick Dangerous", new[] { "" });
    public static Console NEOGEO = new RetroConsole(CONSOLETYPE.NEOGEO, "SNK - Neo Geo", new[] { "" });
    public static Console NEOGEOCD = new RetroConsole(CONSOLETYPE.NEOGEOCD, "SNK - Neo Geo CD", new[] { "" });
    public static Console NEOGEOPOCKET = new RetroConsole(CONSOLETYPE.NEOGEOPOCKET, "SNK - Neo Geo Pocket", new[] { "ngp" });
    public static Console NEOGEOPOCKETCOLOR = new RetroConsole(CONSOLETYPE.NEOGEOPOCKETCOLOR, "SNK - Neo Geo Pocket Color", new[] { "ngc", "npc" });
    public static Console SCUMMVM = new RetroConsole(CONSOLETYPE.SCUMMVM, "ScummVM", new[] { "" });
    public static Console SEGA32X = new RetroConsole(CONSOLETYPE.SEGA32X, "Sega - 32X", new[] { "32x" });
    public static Console DREAMCAST = new RetroConsole(CONSOLETYPE.DREAMCAST, "Sega - Dreamcast", new[] { "" });
    public static Console GAMEGEAR = new RetroConsole(CONSOLETYPE.GAMEGEAR, "Sega - Game Gear", new[] { "gg" });
    public static Console MASTERSYSTEMMARK3 = new RetroConsole(CONSOLETYPE.MASTERSYSTEMMARK3, "Sega - Master System - Mark III", new[] { "sms" });
    public static Console MEGADRIVEGENESIS = new RetroConsole(CONSOLETYPE.MEGADRIVEGENESIS, "Sega - Mega Drive - Genesis", new[] { "gen","smd","md" });
    public static Console SEGAMEGACD = new RetroConsole(CONSOLETYPE.SEGAMEGACD, "Sega - Mega-CD - Sega CD", new[] { "cue" });
    public static Console SEGANAOMI = new RetroConsole(CONSOLETYPE.SEGANAOMI, "Sega - Naomi", new[] { "" });
    public static Console SEGANAOMI2 = new RetroConsole(CONSOLETYPE.SEGANAOMI2, "Sega - Naomi 2", new[] { "" });
    public static Console SEGAPICO = new RetroConsole(CONSOLETYPE.SEGAPICO, "Sega - PICO", new[] { "" });
    public static Console SEGASG1000 = new RetroConsole(CONSOLETYPE.SEGASG1000, "Sega - SG-1000", new[] { "" });
    public static Console SEGASATURN = new RetroConsole(CONSOLETYPE.SEGASATURN, "Sega - Saturn", new[] { "" });
    public static Console SHARPX1 = new RetroConsole(CONSOLETYPE.SHARPX1, "Sharp - X1", new[] { "" });
    public static Console SHARPX68000 = new RetroConsole(CONSOLETYPE.SHARPX68000, "Sharp - X68000", new[] { "" });
    public static Console SINCLAIRZX81 = new RetroConsole(CONSOLETYPE.SINCLAIRZX81, "Sinclair - ZX 81", new[] { "" });
    public static Console SINCLAIRZXSPECTRUM = new RetroConsole(CONSOLETYPE.SINCLAIRZXSPECTRUM, "Sinclair - ZX Spectrum", new[] { "" });
    public static Console PS1 = new RetroConsole(CONSOLETYPE.PSX, "Sony - PlayStation", new[] { "" });
    public static Console PS2 = new RetroConsole(CONSOLETYPE.PS2, "Sony - PlayStation 2", new[] { "" },
        BOXTYPE.DVD, new Vector3(1f, 1f, 1f), new Color(0.18f, 0.37f, 0.67f, 0.8f));
    public static Console PS4 = new RetroConsole(CONSOLETYPE.PS4, "Sony - PlayStation 4", new[] { "" },
        BOXTYPE.DVD, new Vector3(1f, 0.895f, 1f), new Color(0.18f, 0.37f, 0.67f, 0.8f));
    public static Console PSP = new RetroConsole(CONSOLETYPE.PSP, "Sony - PlayStation Portable", new[] { "cso" },
        BOXTYPE.DVD, new Vector3(0.73f, 0.885f, 0.93f), new Color(1f, 1f, 0.97f, 0.1f));
    public static Console PSVITA = new RetroConsole(CONSOLETYPE.PSVITA, "Sony - PlayStation Vita", new[] { "vpk" },
        BOXTYPE.DVD, new Vector3(0.78f, 0.71f, 0.8f), new Color(1f, 1f, 0.97f, 0.1f));
    public static Console TIC80 = new RetroConsole(CONSOLETYPE.TIC80, "TIC-80", new[] { "" });
    public static Console THE3DO = new RetroConsole(CONSOLETYPE.THE3DO, "The 3DO Company - 3DO", new[] { "" });
    public static Console THOMSONMOTO = new RetroConsole(CONSOLETYPE.THOMSONMOTO, "Thomson - MOTO", new[] { "" });
    public static Console TIGERGAME = new RetroConsole(CONSOLETYPE.TIGERGAME, "Tiger - Game.com", new[] { "" });
    public static Console TOMBRAIDER = new RetroConsole(CONSOLETYPE.TOMBRAIDER, "Tomb Raider", new[] { "" });
    public static Console VTECHCREATIVISION = new RetroConsole(CONSOLETYPE.VTECHCREATIVISION, "VTech - CreatiVision", new[] { "" });
    public static Console VTECHVSMILE = new RetroConsole(CONSOLETYPE.VTECHVSMILE, "VTech - V.Smile", new[] { "" });
    public static Console VIRCON32 = new RetroConsole(CONSOLETYPE.VIRCON32, "Vircon32", new[] { "" });
    // No covers:
    // public static Console WASM4 = new RetroConsole(CONSOLETYPE.WASM4, "WASM-4", new[] { "" });
    public static Console WATARASUPERVISION = new RetroConsole(CONSOLETYPE.WATARASUPERVISION, "Watara - Supervision", new[] { "" });
    public static Console WOLFSTEIN3D = new RetroConsole(CONSOLETYPE.WOLFENSTEIN3D, "Wolfenstein 3D", new[] { "" });
    
    // Others
    public static Console STEAM = new RetroConsole(CONSOLETYPE.STEAM, "Steam", new[] { "" }, false);
    public static Console J2ME = new RetroConsole(CONSOLETYPE.J2ME, "J2ME", new[] { "jar", "jad", "kjx" }, false);
    

    private static List<Console> AllConsoles = new List<Console>();
    private static List<string> AllConsoleNames = new List<string>();

    public static List<Console> GetAllConsoles()
    {
        if (AllConsoles.IsNullOrEmpty())
        {
            foreach (FieldInfo info in typeof(CONSOLES).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    AllConsoles.Add((Console)info.GetValue(null));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        return AllConsoles;
    }

    public static CONSOLETYPE[] GetAllConsoleTypes()
    {
        List<CONSOLETYPE> types = new List<CONSOLETYPE>(Enum.GetValues(typeof(CONSOLETYPE)).Cast<CONSOLETYPE>());
        types.Remove(CONSOLETYPE.UNKNOWN);
        return types.ToArray();
    }

    public static Console GetConsole(CONSOLETYPE consoleType)
    {
        foreach (Console console in GetAllConsoles())
        {
            if (console.type == consoleType)
            {
                return console;
            }
        }

        return null;
    }
    
    public static Console GetConsole(string name)
    {
        foreach (Console console in GetAllConsoles())
        {
            if (console.name == name)
            {
                return console;
            }
        }

        return null;
    }
    
    public static List<string> GetAllConsoleIds()
    {
        if (AllConsoleNames.IsNullOrEmpty())
        {
            foreach (Console console in GetAllConsoles())
            {
                AllConsoleNames.Add(console.name);
            }
            AllConsoleNames.Sort();
        }

        return AllConsoleNames;
    }
}

public enum BOXTYPE
{
    DVD, CARTRIDGE, GENERIC
}