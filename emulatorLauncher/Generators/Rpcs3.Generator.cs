﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using EmulatorLauncher.Common;
using EmulatorLauncher.Common.FileFormats;

namespace EmulatorLauncher
{
    partial class Rpcs3Generator : Generator
    {
        public Rpcs3Generator()
        {
            DependsOnDesktopResolution = true;
        }

        public override System.Diagnostics.ProcessStartInfo Generate(string system, string emulator, string core, string rom, string playersControllers, ScreenResolution resolution)
        {
            SimpleLogger.Instance.Info("[Generator] Getting " + emulator + " path and executable name.");

            string path = AppConfig.GetFullPath(emulator);
            if (string.IsNullOrEmpty(path) && emulator != "rpcs3")
                path = AppConfig.GetFullPath("rpcs3");

            string exe = Path.Combine(path, "rpcs3.exe");
            if (!File.Exists(exe))
                return null;

            rom = this.TryUnZipGameIfNeeded(system, rom);
            string savesPath = Path.Combine(AppConfig.GetFullPath("saves"), "ps3", "rpcs3");
            if (!Directory.Exists(savesPath))
                savesPath = path;

            if (Directory.Exists(rom))
            {
                rom = Directory.GetFiles(rom, "EBOOT.BIN", SearchOption.AllDirectories).FirstOrDefault();

                if (!File.Exists(rom))
                    throw new ApplicationException("Unable to find any game in the provided folder");
            }

            else if (Path.GetExtension(rom).ToLower() == ".m3u")
            {
                string romPath = Path.GetDirectoryName(rom);
                rom = File.ReadAllText(rom);

                if (rom.StartsWith(".\\") || rom.StartsWith("./"))
                    rom = Path.Combine(romPath, rom.Substring(2));
                else if (rom.StartsWith("\\") || rom.StartsWith("/"))
                    rom = Path.Combine(savesPath, rom.Substring(1));
            }

            // Fullscreen
            bool fullscreen = !IsEmulationStationWindowed() || SystemConfig.getOptBoolean("forcefullscreen");

            var commandArray = new List<string>
            {
                "\"" + rom + "\""
            };
            
            if (fullscreen)
            {
                commandArray.Add("--no-gui");
                commandArray.Add("--fullscreen");
            }

            string args = string.Join(" ", commandArray);
            
            // If game was uncompressed, say we are going to launch, so the deletion will not be silent
            ValidateUncompressedGame();
            
            // Configuration
            if (!SystemConfig.getOptBoolean("disableautoconfig"))
            {
                SetupGuiConfiguration(path);
                SetupConfiguration(path, fullscreen);
                SetupVFSConfiguration(path, savesPath);
                CreateControllerConfiguration(path);

                // Check if firmware is installed in emulator, if not and if firmware is available in \bios path then install it instead of running the game
                string firmware = Path.Combine(path, "dev_flash", "vsh", "etc", "version.txt");
                string biosPath = AppConfig.GetFullPath("bios");
                string biosPs3 = Path.Combine(biosPath, "PS3UPDAT.PUP");

                if (!File.Exists(firmware) && !File.Exists(biosPs3))
                    throw new ApplicationException("PS3 firmware is not installed in rpcs3 emulator, either place it in \\bios folder, or launch the emulator and install the firware.");
            
                else if (!File.Exists(firmware) && File.Exists(biosPs3))
                {
                    SimpleLogger.Instance.Info("[INFO] Firmware not installed, launching RPCS3 with 'installfirmware' command.");
                    List<string> commandArrayfirmware = new List<string>
                    {
                        "--installfw",
                        biosPs3
                    };
                    
                    string argsfirmware = string.Join(" ", commandArrayfirmware);
                    return new ProcessStartInfo()
                    {
                        FileName = exe,
                        WorkingDirectory = path,
                        Arguments = argsfirmware,
                    };
                }
            }

            return new ProcessStartInfo()
            {
                FileName = exe,
                WorkingDirectory = path,
                Arguments = args,
                //WindowStyle = args.Contains("--fullscreen") ? ProcessWindowStyle.Maximized : ProcessWindowStyle.Minimized
            };
        }

        public override int RunAndWait(System.Diagnostics.ProcessStartInfo path)
        {
            foreach (var px in Process.GetProcessesByName("rpcs3"))
            {
                try { px.Kill(); }
                catch { }
            }

            var process = Process.Start(path);
            process.WaitForExit();

            // In some cases, the process seems to be launched again by the main one
            process = Process.GetProcessesByName("rpcs3").FirstOrDefault();
            process?.WaitForExit();

            return 0;
        }

        /// <summary>
        /// Set 6 options in rpcs3 GUI settings to disable prompts (updates, exit, launching game...)
        /// </summary>
        /// <param name="path"></param>
        private void SetupGuiConfiguration(string path)
        {
            SimpleLogger.Instance.Info("[GENERATOR] Writing to GuiConfigs\\CurrentSettings.ini file.");

            string guiSettings = Path.Combine(path, "GuiConfigs", "CurrentSettings.ini");
            using (var ini = new IniFile(guiSettings))
            {
                ini.WriteValue("main_window", "confirmationBoxExitGame", "false");
                ini.WriteValue("main_window", "infoBoxEnabledInstallPUP", "false");
                ini.WriteValue("main_window", "infoBoxEnabledWelcome", "false");
                ini.WriteValue("main_window", "confirmationBoxBootGame", "false");
                ini.WriteValue("main_window", "infoBoxEnabledInstallPKG", "false");
                ini.WriteValue("Meta", "checkUpdateStart", "false");

                if (SystemConfig.isOptSet("discord") && SystemConfig.getOptBoolean("discord"))
                    ini.WriteValue("Meta", "useRichPresence", "true");
                else
                    ini.WriteValue("Meta", "useRichPresence", "false");

                if (SystemConfig.isOptSet("rpcs3_guns") && SystemConfig.getOptBoolean("rpcs3_guns"))
                    ini.WriteValue("GSFrame", "lockMouseInFullscreen", "false");
                else
                    ini.WriteValue("GSFrame", "lockMouseInFullscreen", "true");

                ini.WriteValue("GSFrame", "disableMouse", "true");
            }
        }

        private void SetupVFSConfiguration(string path, string savesPath)
        {
            SimpleLogger.Instance.Info("[GENERATOR] Writing to vfs.yml file.");

            var yml = YmlFile.Load(Path.Combine(path, "config", "vfs.yml"));

            string hdd0Path = Path.Combine(savesPath, "dev_hdd0");
            if (!Directory.Exists(hdd0Path))
                try { Directory.CreateDirectory(hdd0Path); }
                catch { }

            yml["/dev_hdd0/"] = hdd0Path.Replace("\\", "/");

            SimpleLogger.Instance.Info("[Generator] Setting '" + hdd0Path + "' as content path for the emulator");

            // Save to yml file
            yml.Save();
        }

        /// <summary>
        /// Setup config.yml file
        /// </summary>
        /// <param name="path"></param>
        private void SetupConfiguration(string path, bool fullscreen)
        {
            SimpleLogger.Instance.Info("[GENERATOR] Writing to config.yml file.");

            var yml = YmlFile.Load(Path.Combine(path, "config.yml"));
            // Initialize IO settings
            var io = yml.GetOrCreateContainer("Input/Output");
            BindFeature(io, "Keyboard", "rpcs3_keyboard", "\"Null\"");
            BindFeature(io, "Mouse", "rpcs3_mouse", "\"Null\"");
            BindFeature(io, "Move", "rpcs3_move", "\"Null\"");
            BindFeature(io, "Keyboard", "rpcs3_keyboard", "\"Null\"");
            BindFeature(io, "Camera", "rpcs3_camera", "\"Null\"");

            if (SystemConfig.isOptSet("rpcs3_cameraType") && !string.IsNullOrEmpty(SystemConfig["rpcs3_cameraType"]))
            {
                io["Camera type"] = SystemConfig["rpcs3_cameraType"];
                if (!SystemConfig.isOptSet("rpcs3_camera"))
                    io["Camera"] = "Fake";

                string camera = io["Camera ID"];

                if (string.IsNullOrEmpty(camera) || camera == "\"\"")
                    io["Camera ID"] = "Default";
            }

            // Handle Core part of yml file
            var core = yml.GetOrCreateContainer("Core");
            BindFeature(core, "PPU Decoder", "ppudecoder", "Recompiler (LLVM)");
            BindFeature(core, "LLVM Precompilation", "lvmprecomp", "true");
            BindFeature(core, "SPU Decoder", "spudecoder", "Recompiler (LLVM)");
            BindFeature(core, "Preferred SPU Threads", "sputhreads", "0");
            BindFeature(core, "SPU loop detection", "spuloopdetect", "false");
            BindFeature(core, "SPU Block Size", "spublocksize", "Safe");
            BindFeature(core, "Accurate RSX reservation access", "accuratersx", "false");
            BindFeature(core, "PPU Accurate Vector NaN Values", "vectornan", "false");
            BindFeature(core, "Full Width AVX-512", "fullavx", "false");
            BindFeature(core, "XFloat Accuracy", "rpcs3_xfloat", "Accurate");

            // Handle Video part of yml file
            var video = yml.GetOrCreateContainer("Video");
            BindFeature(video, "Renderer", "gfxbackend", "Vulkan");
            video["Resolution"] = "1280x720";
            BindFeature(video, "Resolution Scale", "rpcs3_internal_resolution", "100");
            BindFeature(video, "Aspect ratio", "rpcs3_ratio", "16:9");
            BindFeature(video, "Frame limit", "framelimit", "Auto");
            BindFeature(video, "MSAA", "msaa", "Auto");
            BindFeature(video, "Shader Mode", "shadermode", "Async Shader Recompiler");
            BindFeature(video, "Write Color Buffers", "writecolorbuffers", "false");
            BindFeature(video, "Write Depth Buffer", "writedepthbuffers", "false");
            BindFeature(video, "Read Color Buffers", "readcolorbuffers", "false");
            BindFeature(video, "Read Depth Buffer", "readdepthbuffers", "false");
            BindFeature(video, "VSync", "rpcs3_vsync", "true");
            BindFeature(video, "Stretch To Display Area", "stretchtodisplay", "false");
            BindFeature(video, "Strict Rendering Mode", "strict_rendering", "false");
            BindFeature(video, "Disable Vertex Cache", "disablevertex", "false");
            BindFeature(video, "Multithreaded RSX", "multithreadedrsx", "false");
            BindFeature(video, "Output Scaling Mode", "rpcs3_scaling_filter", "Nearest");
            BindFeature(video, "3D Display Mode", "enable3d", "Disabled");
            
            BindFeature(video, "Anisotropic Filter Override", "anisotropicfilter", "0");
            BindFeature(video, "Shader Precision", "shader_quality", "Auto");
            BindFeature(video, "Driver Wake-Up Delay", "driver_wake", "1");
            BindBoolFeature(video, "Force CPU Blit", "cpu_blit", "true", "false");
            BindBoolFeature(video, "Disable ZCull Occlusion Queries", "disable_zcull_queries", "true", "false");

            // ZCULL Accuracy
            if (SystemConfig.isOptSet("zcull_accuracy") && (SystemConfig["zcull_accuracy"] == "Approximate"))
            {
                video["Relaxed ZCULL Sync"] = "false";
                video["Accurate ZCULL stats"] = "false";
            }
            else if (SystemConfig.isOptSet("zcull_accuracy") && (SystemConfig["zcull_accuracy"] == "Relaxed"))
            {
                video["Relaxed ZCULL Sync"] = "true";
                video["Accurate ZCULL stats"] = "false";
            }
            else if (Features.IsSupported("zcull_accuracy"))
            {
                video["Relaxed ZCULL Sync"] = "false";
                video["Accurate ZCULL stats"] = "true";
            }

            // Handle Vulkan part of yml file
            var vulkan = video.GetOrCreateContainer("Vulkan");
            BindFeature(vulkan, "Asynchronous Texture Streaming 2", "asynctexturestream", "false");
            BindFeature(vulkan, "Exclusive Fullscreen Mode", "rpcs3_fullscreen_mode", "Automatic");

            // Handle Performance Overlay part of yml file
            var performance = video.GetOrCreateContainer("Performance Overlay");
            if (SystemConfig.isOptSet("performance_overlay") && (SystemConfig["performance_overlay"] == "detailed"))
            {
                performance["Enabled"] = "true";
                performance["Enable Framerate Graph"] = "true";
                performance["Enable Frametime Graph"] = "true";
            }
            else if (SystemConfig.isOptSet("performance_overlay") && (SystemConfig["performance_overlay"] == "simple"))
            {
                performance["Enabled"] = "true";
                performance["Enable Framerate Graph"] = "false";
                performance["Enable Frametime Graph"] = "false";
            }
            else if (Features.IsSupported("performance_overlay"))
            {
                performance["Enabled"] = "false";
                performance["Enable Framerate Graph"] = "false";
                performance["Enable Frametime Graph"] = "false";
            }

            // Handle Audio part of yml file
            var audio = yml.GetOrCreateContainer("Audio");
            BindFeature(audio, "Renderer", "audiobackend", "Cubeb");
            BindFeature(audio, "Audio Format", "audiochannels", "Stereo");
            BindFeature(audio, "Enable Buffering", "audio_buffering", "true");
            if (SystemConfig.isOptSet("time_stretching") && (SystemConfig["time_stretching"] == "low"))
            {
                audio["Enable time stretching"] = "true";
                audio["Time Stretching Threshold"] = "25";
            }
            else if (SystemConfig.isOptSet("time_stretching") && (SystemConfig["time_stretching"] == "medium"))
            {
                audio["Enable time stretching"] = "true";
                audio["Time Stretching Threshold"] = "50";
            }
            else if (SystemConfig.isOptSet("time_stretching") && (SystemConfig["time_stretching"] == "high"))
            {
                audio["Enable time stretching"] = "true";
                audio["Time Stretching Threshold"] = "75";
            }
            else if (Features.IsSupported("time_stretching"))
            {
                audio["Enable time stretching"] = "false";
                audio["Time Stretching Threshold"] = "75";
            }

            // Handle System part of yml file
            var system_region = yml.GetOrCreateContainer("System");
            BindFeature(system_region, "License Area", "ps3_region", "SCEE");
            BindFeature(system_region, "Language", "ps3_language", GetDefaultPS3Language());

            // Handle Miscellaneous part of yml file
            var misc = yml.GetOrCreateContainer("Miscellaneous");
            misc["Start games in fullscreen mode"] = fullscreen ? "true" : "false";
            BindFeature(misc, "Show trophy popups", "show_trophy", "true");
            misc["Automatically start games after boot"] = "true";
            misc["Exit RPCS3 when process finishes"] = "true";
            misc["Prevent display sleep while running games"] = "true";
            BindBoolFeature(misc, "Show shader compilation hint", "rpcs3_hidehints", "false", "true");
            BindBoolFeature(misc, "Show PPU compilation hint", "rpcs3_hidehints", "false", "true");

            SetupGuns(yml, vulkan);

            // Save to yml file
            yml.Save();
        }

        private string GetDefaultPS3Language()
        {
            Dictionary<string, string> availableLanguages = new Dictionary<string, string>()
            {
                { "en", "English (US)" },
                { "fr", "French" },
                { "de", "German" },
                { "zh", "Chinese (Simplified)" },
                { "nl", "Dutch" },
                { "es", "Spanish" },
                { "fi", "Finnish" },
                { "it", "Italian" },
                { "jp", "Japanese" },
                { "ja", "Japanese" },
                { "ko", "Korean" },
                { "pl", "Polish" },
                { "pt", "Portuguese (Portugal)" },
                { "ru", "Russian" },
                { "sv", "Swedish" },
                { "tr", "Turkish" },
                { "nn", "Norwegian" },
                { "nb", "Norwegian" }
            };

            // Special case for some variances
            if (SystemConfig["Language"] == "zh_TW")
                return "Chinese (Traditional)";
            else if (SystemConfig["Language"] == "pt_BR")
                return "Portuguese (Brazil)";
            else if (SystemConfig["Language"] == "en_GB")
                return "English (UK)";

            string lang = GetCurrentLanguage();
            if (!string.IsNullOrEmpty(lang))
            {
                if (availableLanguages.TryGetValue(lang, out string ret))
                    return ret;
            }
            return "English (US)";
        }
    }
}
