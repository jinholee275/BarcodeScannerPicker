﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BarcodeReaderPicker
{
    public class BarcodeReaderLoader
    {
        private static List<Type> plugIns = new List<Type>();

        public static List<string> PlugIns => plugIns.Select(x => x.Name).ToList();

        public static IBarcodeReaderPlugin GetPlugin(string name, BarcodeReaderConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Type plugInType = plugIns.FirstOrDefault(type => type.Name == name);

            IBarcodeReaderPlugin plugin = (IBarcodeReaderPlugin)Activator.CreateInstance(
                plugInType ?? throw new Exception($"No plugin found with name '{name}'"), config);

            return plugin;
        }

        public void LoadPluginAssemblies(string path)
            => Directory
            .GetFiles(path)
            .ToList()
            .Where(
                fileName =>
                Path.GetFileName(fileName).StartsWith("BarcodeReaderPicker.") &&
                Path.GetFileName(fileName).EndsWith("Adaptor.dll"))
            .ToList()
            .Select(
                fileName =>
                Assembly.LoadFile(Path.GetFullPath(fileName)))
            .SelectMany(assembly => assembly.GetTypes())
            .Where(p => typeof(IBarcodeReaderPlugin).IsAssignableFrom(p) && p.IsClass)
            .Where(type => !plugIns.Contains(type))
            .ToList()
            .ForEach(type => plugIns.Add(type));
    }
}
