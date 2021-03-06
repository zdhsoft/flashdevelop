using System;
using System.ComponentModel;
using PluginCore.Localization;

namespace BasicCompletion
{
    public enum AutoInsert
    {
        CPP = 0,
        Never = 1,
        Always = 2,
    }

    [Serializable]
    public class Settings
    {
        bool disableAutoCompletion;
        AutoInsert autoInsertType = AutoInsert.CPP;

        /// <summary> 
        /// Get and sets the AutoInsertType
        /// </summary>
        [DisplayName("Auto Insert Type"), DefaultValue(AutoInsert.CPP)]
        [LocalizedDescription("BasicCompletion.Description.AutoInsertType")]
        public AutoInsert AutoInsertType
        {
            get => autoInsertType;
            set => autoInsertType = value;
        }

        /// <summary> 
        /// Get and sets the DisableAutoCompletion
        /// </summary>
        [DisplayName("Disable Auto Completion"), DefaultValue(false)]
        [LocalizedDescription("BasicCompletion.Description.DisableAutoCompletion")]
        public bool DisableAutoCompletion
        {
            get => disableAutoCompletion;
            set => disableAutoCompletion = value;
        }

    }
}