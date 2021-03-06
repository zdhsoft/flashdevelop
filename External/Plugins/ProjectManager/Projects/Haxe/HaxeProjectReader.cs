using System;
using System.Collections.Generic;
using System.IO;
using PluginCore;

namespace ProjectManager.Projects.Haxe
{
    public class HaxeProjectReader : ProjectReader
    {
        readonly HaxeProject project;

        public HaxeProjectReader(string filename) : base(filename, new HaxeProject(filename)) => project = Project as HaxeProject;

        public new HaxeProject ReadProject() => base.ReadProject() as HaxeProject;

        protected override void PostProcess()
        {
            var options = project.MovieOptions;
            if (version > 1)
            {
                var needSave = false;
                //TODO slavara: old projects fix
                switch (options.Platform)
                {
                    case "NME":
                        if (project.TargetBuild is null
                            && project.TestMovieCommand != ""
                            && project.TestMovieBehavior != TestMovieBehavior.OpenDocument)
                            project.TestMovieCommand = string.Empty;
                        options.Platform = GetBuilder(project.OutputPath);
                        options.Version = "1.0";
                        needSave = true;
                        break;
                    case "Lime":
                        if (project.TargetBuild is null)
                        {
                            project.TargetBuild = options.PlatformSupport.Targets[0];
                            needSave = true;
                        }
                        break;
                    case null:
                        options.Platform = PlatformData.FLASHPLAYER_PLATFORM;
                        needSave = true;
                        break;
                }
                if (options.HasPlatformSupport)
                {
                    options.TargetBuildTypes = options.PlatformSupport.Targets;
                    needSave = true;
                }
                if (needSave)
                {
                    try
                    {
                        project.Save();
                    }
                    catch { }
                }
                return;
            }

            //TODO slavara: actualize for haxe 4.1.4
            if (options.MajorVersion > 10)
            {
                //TODO slavara: old projects fix
                string platform = null;
                switch (options.MajorVersion)
                {
                    case 11: 
                        platform = "JavaScript"; 
                        options.MajorVersion = 0; 
                        break;
                    case 12: 
                        platform = "Neko"; 
                        options.MajorVersion = 0; 
                        break;
                    case 13: 
                        platform = "PHP"; 
                        options.MajorVersion = 0; 
                        break;
                    case 14: 
                        platform = "C++"; 
                        options.MajorVersion = 0; 
                        break;
                }
                if (platform is null)
                {
                    platform = PlatformData.FLASHPLAYER_PLATFORM;
                    //TODO slavara: actualize for haxe 4.1.4
                    options.MajorVersion = 14;
                }
                options.Platform = platform;
            }
            try { project.Save(); } 
            catch { }
        }

        static string GetBuilder(string projectFile)
        {
            if (string.IsNullOrEmpty(projectFile)) return "Lime";
            return Path.GetExtension(projectFile).ToLower() switch
            {
                ".nmml" => "Nme",
                _ => "Lime",
            };
        }

        // process Haxe-specific stuff
        protected override void ProcessNode(string name)
        {
            switch (name)
            {
                case "build": ReadBuildOptions(); break;
                case "library": ReadLibraryAssets(); break;
                case "haxelib": ReadLibraries(); break;
                default: base.ProcessNode(name); break;
            }
        }

        public void ReadLibraries()
        {
            var libraries = new List<string>();
            ReadStartElement("haxelib");
            while (Name == "library")
            {
                libraries.Add(GetAttribute("name"));
                Read();
            }
            ReadEndElement();
            project.CompilerOptions.Libraries = libraries.ToArray();
        }

        public void ReadBuildOptions()
        {
            var options = project.CompilerOptions;
            ReadStartElement("build");
            while (Name == "option")
            {
                MoveToFirstAttribute();
                switch (Name)
                {
                    case "directives": options.Directives = (Value=="") ? Array.Empty<string>() : Value.Split('\n'); break;
                    case "flashStrict": options.FlashStrict = BoolValue; break;
                    case "noInlineOnDebug": options.NoInlineOnDebug = BoolValue; break;
                    case "mainClass": options.MainClass = Value; break;
                    case "enabledebug": options.EnableDebug = BoolValue; break;
                    case "additional": options.Additional = Value.Split('\n'); break;
                }
                Read();
            }
            ReadEndElement();
        }

        public void ReadLibraryAssets()
        {
            ReadStartElement("library");
            while (Name == "asset")
            {
                string path = OSPath(GetAttribute("path"));
                string mode = GetAttribute("mode");

                if (path is null) throw new Exception("All library assets must have a 'path' attribute.");

                var asset = new LibraryAsset(project, path);
                project.LibraryAssets.Add(asset);

                asset.ManualID = GetAttribute("id"); // could be null
                asset.UpdatePath = OSPath(GetAttribute("update")); // could be null
                asset.FontGlyphs = GetAttribute("glyphs"); // could be null

                if (mode != null)
                    asset.SwfMode = (SwfAssetMode)Enum.Parse(typeof(SwfAssetMode), mode, true);

                if (asset.SwfMode == SwfAssetMode.Shared)
                    asset.Sharepoint = GetAttribute("sharepoint"); // could be null

                if (asset.IsImage && GetAttribute("bitmap") != null)
                    asset.BitmapLinkage = bool.Parse(GetAttribute("bitmap"));

                Read();
            }
            ReadEndElement();
        }
    }
}