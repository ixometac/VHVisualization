﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VHVisualisation.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.0.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("H:\\VggFc7.features")]
        public string FeaturesFileName {
            get {
                return ((string)(this["FeaturesFileName"]));
            }
            set {
                this["FeaturesFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("H:\\TrecVid-96x72-1sec.thumbs")]
        public string ThumbnailsFileName {
            get {
                return ((string)(this["ThumbnailsFileName"]));
            }
            set {
                this["ThumbnailsFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public int videoCountToRead {
            get {
                return ((int)(this["videoCountToRead"]));
            }
            set {
                this["videoCountToRead"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("220")]
        public double LoadThreshold {
            get {
                return ((double)(this["LoadThreshold"]));
            }
            set {
                this["LoadThreshold"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("16")]
        public int InitGridSize {
            get {
                return ((int)(this["InitGridSize"]));
            }
            set {
                this["InitGridSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50000")]
        public int IMSwitchRuns {
            get {
                return ((int)(this["IMSwitchRuns"]));
            }
            set {
                this["IMSwitchRuns"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int MaxFramesPerVideo {
            get {
                return ((int)(this["MaxFramesPerVideo"]));
            }
            set {
                this["MaxFramesPerVideo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int skipframes {
            get {
                return ((int)(this["skipframes"]));
            }
            set {
                this["skipframes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int NeighbouthoodSize {
            get {
                return ((int)(this["NeighbouthoodSize"]));
            }
            set {
                this["NeighbouthoodSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("35345")]
        public int TrecVidBaseID {
            get {
                return ((int)(this["TrecVidBaseID"]));
            }
            set {
                this["TrecVidBaseID"] = value;
            }
        }
    }
}
