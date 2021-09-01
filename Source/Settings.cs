/*
 * Licensed to De Bitmanager under one or more contributor
 * license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright
 * ownership. De Bitmanager licenses this file to you under
 * the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using Bitmanager.Core;
using Bitmanager.IO;
using Microsoft.Win32;

namespace Bitmanager.BigFile
{
   /// <summary>
   /// Readonly object with all current settings replaced with their defaults
   /// Created from a SettingsSource
   /// </summary>
   public class Settings
   {
      public readonly SettingsSource Source;
      public readonly long TotalPhysicalMemory;
      public readonly long AvailablePhysicalMemory;

      public readonly long CompressMemoryIfBigger;
      public readonly long LoadMemoryIfBigger;

      public readonly Color HighlightColor;
      public readonly Color SelectedHighlightColor;
      public readonly Color ContextColor;
      public readonly int NumContextLines;
      public readonly int SearchThreads;
      public readonly int MaxLineLength;
      public readonly int MaxCopyLines;
      public readonly int MaxCopySize;

      public Settings(SettingsSource src)
      {
         Source = src;
         TotalPhysicalMemory = src.TotalPhysicalMemory;
         AvailablePhysicalMemory = src.AvailablePhysicalMemory;
         Color tmp = src.HighlightColor;
         HighlightColor = tmp;
         SelectedHighlightColor = Color.FromArgb((int)(tmp.R * .7f), (int)(tmp.G * .7f), (int)(tmp.B * .7f));

         ContextColor = src.ContextColor;
         NumContextLines = src.NumContextLines;
         MaxCopyLines = src.MaxCopyLines;
         MaxCopySize = src.MaxCopySize;
         MaxLineLength = src.MaxLineLength;
         SearchThreads = src.SearchThreads;

         CompressMemoryIfBigger = src.CompressMemoryIfBigger;
         LoadMemoryIfBigger = src.LoadMemoryIfBigger;
      }

      public void Dump () {
         var logger = Globals.SettingsLogger;
         logger.Log("Dumping current ACTUAL settings:");
         logger.Log("-- HighlightColor: {0}, SelectedHighlightColor: {1}", HighlightColor, SelectedHighlightColor);
         logger.Log("-- ContextColor: {0}, NumContextLines={1}", ContextColor, NumContextLines);
         logger.Log("-- MaxCopyLines: {0}, MaxCopySize={1} ({2})", MaxCopyLines, MaxCopySize, Pretty.PrintSize(MaxCopySize));
         logger.Log("-- MaxLineLength: {0} ({1})", MaxLineLength, Pretty.PrintSize(MaxLineLength));
         logger.Log("-- SearchThreads: {0}", SearchThreads);
         logger.Log("-- CompressMemoryIfBigger: {0} ({1})", CompressMemoryIfBigger, Pretty.PrintSize(CompressMemoryIfBigger));
         logger.Log("-- LoadMemoryIfBigger: {0} ({1})", LoadMemoryIfBigger, Pretty.PrintSize(LoadMemoryIfBigger));
         logger.Log("-- PhysicalMem: total={0}, available={1}", Pretty.PrintSize(TotalPhysicalMemory), Pretty.PrintSize(AvailablePhysicalMemory));
      }
   }



   /// <summary>
   /// Save/load settings in registry
   /// </summary>
   public class SettingsSource {
      public const String AUTO = @"Auto";
      public Settings Settings { get; private set; }
      private const String _KEY = @"software\bitmanager\bigfile";
      public readonly long TotalPhysicalMemory;
      public readonly long AvailablePhysicalMemory;

      public readonly SizeSetting MaxLineLength = new SizeSetting("max_line_size", "10MB");
      public readonly ColorSetting HighlightColor = new ColorSetting("highlight_color", Color.Lime);
      public readonly ColorSetting ContextColor = new ColorSetting("context_color", Color.LightGray);
      public readonly IntSetting NumContextLines = new IntSetting("context_lines", "0");

      public readonly ThreadsSetting SearchThreads = new ThreadsSetting("search_threads", "0", AUTO);
      public readonly SizeSetting LoadMemoryIfBigger = new SizeSetting("load_memory", "32GB", AUTO);
      public readonly SizeSetting CompressMemoryIfBigger = new SizeSetting("compress_memory", "1GB", AUTO);

      public readonly IntSetting MaxCopyLines = new IntSetting("max_copy_lines", "100000");
      public readonly SizeSetting MaxCopySize = new SizeSetting("max_copy_size", "100MB");

      public SettingsSource(bool autoLoad=false)
      {
         if (autoLoad)
            Load();

         var ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
         this.TotalPhysicalMemory = (long)ci.TotalPhysicalMemory;
         this.AvailablePhysicalMemory = (long)Math.Min (ci.AvailablePhysicalMemory, ci.AvailableVirtualMemory);
         Globals.MainLogger.Log("Total memory={0}, available={1}.", Pretty.PrintSize(TotalPhysicalMemory), Pretty.PrintSize(AvailablePhysicalMemory));

         ActualizeDefaults();
      }



      public Settings ActualizeDefaults()
      {
         return Settings = new Settings(this); //actualize settings
      }

      public void Load()
      {
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.OpenSubKey(_KEY, false))
         {
            MaxLineLength.Load(key);
            ContextColor.Load(key);
            HighlightColor.Load(key);
            NumContextLines.Load(key);
            MaxCopyLines.Load(key);
            MaxCopySize.Load(key);
            SearchThreads.Load(key);
            LoadMemoryIfBigger.Load(key);
            CompressMemoryIfBigger.Load(key);

            FormLine.LoadState(key);
         }
      }

      public void Save()
      {
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, true))
         {
            MaxLineLength.Save(key);
            ContextColor.Save(key);
            HighlightColor.Save(key);
            NumContextLines.Save(key);
            MaxCopyLines.Save(key);
            MaxCopySize.Save(key);
            SearchThreads.Save(key);
            LoadMemoryIfBigger.Save(key);
            CompressMemoryIfBigger.Save(key);

            FormLine.SaveState(key);
         }
         Load();
      }

      public void Dump(String why) {
         var logger = Globals.SettingsLogger;
         logger.Log("Dumping current registry settings ({0}):", why);
         logger.Log("-- {0}", HighlightColor);
         logger.Log("-- {0}, {1}", ContextColor, NumContextLines);
         logger.Log("-- {0}, {1}", MaxCopyLines, MaxCopySize);
         logger.Log("-- {0}", MaxLineLength);
         logger.Log("-- {0}", SearchThreads);
         logger.Log("-- {0}", CompressMemoryIfBigger);
         logger.Log("-- {0}", LoadMemoryIfBigger);
         logger.Log("-- PhysicalMem: total={0}, available={1}", Pretty.PrintSize(TotalPhysicalMemory), Pretty.PrintSize(AvailablePhysicalMemory));
         if (Settings != null) Settings.Dump();
      }


      public static long GetActualSize (String size, String auto)
      {
         size = size.TrimToNull();
         if (size == null) size = auto;
         switch (size.ToLowerInvariant())
         {
            case "auto":
               return GetActualSize(auto, "off");
            case "off":
               return long.MaxValue;
            case "on":
               return -1;
            default:
               return Pretty.ParseSize(size);
         }
      }

      public static void SaveFormPosition(int left, int top, int w, int h)
      {
         String sizeStr = Invariant.Format("{0};{1};{2};{3}", w, h, left, top);
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, true))
         {
            WriteVal(key, "position", sizeStr);
         }
      }

      public static bool LoadFormPosition(out int left, out int top, out int w, out int h)
      {
         String sizeStr;
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, false))
         {
            sizeStr = ReadVal(key, "position", null);
         }
         Globals.MainLogger.Log("Loaded size str={0}", sizeStr);

         left = -1;
         top = -1;
         w = -1;
         h = -1;
         if (String.IsNullOrEmpty(sizeStr)) return false;

         String[] parts = sizeStr.Split(';');
         for (int i=0; i<parts.Length; i++)
         {
            int v;
            if (!int.TryParse(parts[i], out v)) break;
            if (v <= 0) break;

            switch (i)
            {
               case 0: w = v; continue;
               case 1: h = v; continue;
               case 2: left = v; continue;
               case 3: top = v; continue;
            }
            break;
         }
         return (w >= 0);
      }

      public static void SaveFileHistory (String[] list, String prefix)
      {
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, true))
         {
            for (int i=0; i<list.Length; i++)
            {
               if (list[i] == null) break;
               WriteVal(key, prefix + i.ToString(), list[i]);
            }
         }
      }
      public static String[] LoadFileHistory(String prefix)
      {
         String[] ret = new string[10];
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, false))
         {
            for (int i = 0; i < ret.Length; i++)
            {
               ret[i] = ReadVal(key, prefix + i.ToString(), null);
               if (ret[i] == null) break;
            }
         }
         return ret;
      }

      public static String ReadVal(RegistryKey key, String valName, String def)
      {
         if (key == null) return def;
         var v = key.GetValue(valName, def);
         if (v == null) return def;

         String ret = v.ToString();
         return String.IsNullOrEmpty(ret) ? def : ret;
      }
      public static Color ReadVal(RegistryKey key, String valName, Color def)
      {
         return ColorTranslator.FromHtml(ReadVal(key, valName, ColorTranslator.ToHtml(def)));
      }
      public static int ReadVal(RegistryKey key, String valName, int def)
      {
         if (key == null) return def;
         var v = key.GetValue(valName, def);
         return v == null ? def : (int)v;
      }
      public static bool ReadVal(RegistryKey key, String valName, bool def)
      {
         if (key == null) return def;
         var v = key.GetValue(valName, def ? 1 : 0);
         return v == null ? def : (0 != (int)v);
      }
      public static void WriteVal(RegistryKey key, String valName, String val)
      {
         if (key == null) return;
         if (val == null) val = String.Empty;
         key.SetValue(valName, val);
      }
      public static void WriteVal(RegistryKey key, String valName, int val)
      {
         if (key == null) return;
         key.SetValue(valName, val);
      }
      public static void WriteVal(RegistryKey key, String valName, bool val)
      {
         if (key == null) return;
         key.SetValue(valName, val ? 1 : 0);
      }
      public static void WriteVal(RegistryKey key, String valName, Color val)
      {
         if (key == null) return;
         WriteVal(key, valName, ColorTranslator.ToHtml(val));
      }

   }
}