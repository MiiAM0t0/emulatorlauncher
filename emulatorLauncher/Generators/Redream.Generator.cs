using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using EmulatorLauncher.Common;
using EmulatorLauncher.Common.FileFormats;
using System.Windows.Forms;
using System.Linq;

namespace EmulatorLauncher
{
    partial class RedreamGenerator : Generator
    {
        private BezelFiles _bezelFileInfo;
        private ScreenResolution _resolution;

        public override System.Diagnostics.ProcessStartInfo Generate(string system, string emulator, string core, string rom, string playersControllers, ScreenResolution resolution)
        {
            string path = AppConfig.GetFullPath("redream");

            string exe = Path.Combine(path, "redream.exe");
            if (!File.Exists(exe))
                return null;

            bool fullscreen = !IsEmulationStationWindowed() || SystemConfig.getOptBoolean("forcefullscreen");
            bool wideScreen = SystemConfig["redream_aspect"] == "16:9" || SystemConfig["redream_aspect"] == "stretch";
            if (wideScreen)
                SystemConfig["forceNoBezel"] = "1";

            //Applying bezels
            if (!ReshadeManager.Setup(ReshadeBezelType.opengl, ReshadePlatform.x64, system, rom, path, resolution, emulator))
                _bezelFileInfo = BezelFiles.GetBezelFiles(system, rom, resolution, emulator);

            _resolution = resolution;

            // Decompression Logic
            rom = HandleDecompression(rom, system);

            SetupConfiguration(path, fullscreen, resolution);

            return new ProcessStartInfo()
            {
                FileName = exe,
                WorkingDirectory = path,
                Arguments = "\"" + rom + "\"",
            };
        }

        private string HandleDecompression(string rom, string system)
        {
            string[] compressedExtensions = new string[] { ".zip", ".7z", ".squashfs" };
            string[] gameExtensions = new string[] { ".mds", ".mdf", ".cue", ".cdi", ".gdi", ".chd" };

            // Check if the ROM is compressed and needs to be extracted
            if (compressedExtensions.Contains(Path.GetExtension(rom).ToLowerInvariant()))
            {
                string uncompressedRomPath = this.TryUnZipGameIfNeeded(system, rom, false, false);
                if (Directory.Exists(uncompressedRomPath))
                {
                    // Search for valid game files after extraction
                    string[] romFiles = Directory.GetFiles(uncompressedRomPath, "*.*", SearchOption.AllDirectories)
                        .OrderBy(file => Array.IndexOf(gameExtensions, Path.GetExtension(file).ToLowerInvariant()))
                        .ToArray();

                    rom = romFiles.FirstOrDefault(file => gameExtensions.Any(ext => Path.GetExtension(file).Equals(ext, StringComparison.OrdinalIgnoreCase)));

                    ValidateUncompressedGame();
                }
            }

            return rom;
        }

        private void SetupConfiguration(string path, bool fullscreen, ScreenResolution resolution)
        {
            string conf = Path.Combine(path, "redream.cfg");

            using (var ini = IniFile.FromFile(conf))
            {
                ini.WriteValue("", "fullmode", fullscreen ? "borderless fullscreen" : "windowed");
                ini.WriteValue("", "mode", fullscreen ? "borderless fullscreen" : "windowed");
                ini.WriteValue("", "gamedir", "./../../roms/dreamcast");

                BindIniFeatureSlider(ini, "", "res", "redream_res", "2");
                BindIniFeature(ini, "", "cable", "redream_cable", "rgb");
                BindIniFeature(ini, "", "broadcast", "redream_broadcast", "ntsc");
                BindIniFeature(ini, "", "language", "redream_language", "english");
                BindIniFeature(ini, "", "region", "redream_region", "japan");
                BindBoolIniFeatureOn(ini, "", "vsync", "redream_vsync", "1", "0");
                BindBoolIniFeature(ini, "", "frameskip", "redream_frameskip", "1", "0");
                BindIniFeature(ini, "", "aspect", "redream_aspect", "4:3");

                ini.WriteValue("", "fullwidth", (resolution == null ? Screen.PrimaryScreen.Bounds.Width : resolution.Width).ToString());
                ini.WriteValue("", "fullheight", (resolution == null ? Screen.PrimaryScreen.Bounds.Height : resolution.Height).ToString());
                ini.WriteValue("", "width", (resolution == null ? Screen.PrimaryScreen.Bounds.Width : resolution.Width).ToString());
                ini.WriteValue("", "height", (resolution == null ? Screen.PrimaryScreen.Bounds.Height : resolution.Height).ToString());

                ConfigureControllers(ini);
            }
        }

        public override int RunAndWait(ProcessStartInfo path)
        {
            FakeBezelFrm bezel = null;

            if (_bezelFileInfo != null)
                bezel = _bezelFileInfo.ShowFakeBezel(_resolution);

            int ret = base.RunAndWait(path);

            bezel?.Dispose();

            if (ret == 1)
            {
                ReshadeManager.UninstallReshader(ReshadeBezelType.opengl, path.WorkingDirectory);
                return 0;
            }
            ReshadeManager.UninstallReshader(ReshadeBezelType.opengl, path.WorkingDirectory);
            return ret;
        }
    }
}
