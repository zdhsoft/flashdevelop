﻿using CodeRefactor.Managers;
using System.Collections.Generic;
using PluginCore;
using PluginCore.Localization;

namespace CodeRefactor.BatchProcessors
{
    internal class ConsistentEOLProcessor : IBatchProcessor
    {
        public bool IsAvailable => true;

        public string Text => TextHelper.GetString("Info.ConsistentEOLs");

        public void Process(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                var document = (ITabbedDocument) PluginBase.MainForm.OpenEditableDocument(file);
                var sci = document?.SciControl;
                sci?.ConvertEOLs(sci.EOLMode);
            }
        }

        public void ProcessProject(IProject project) => Process(BatchProcessManager.GetAllProjectFiles(project));
    }
}