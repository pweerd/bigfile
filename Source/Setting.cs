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

using Bitmanager.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {

   /// <summary>
   /// Base class of individual settings
   /// The setting is always in string-format and can be serialized into/from the registry
   /// Before use, it needs to be converted into the actual value via convert()
   /// or via the implicit operators from the derived classes
   /// </summary>
   public abstract class Setting<T> {
      public readonly String Name;
      public readonly String Default;
      protected String value;
      public String Value => value;

      public Setting(String name, String def, String initial) {
         Name = name;
         Default = def;
         value = initial==null ? def : initial;
      }

      public void Set(String s) {
         convert(s);
         value = s;
      }

      public static String ColorToHtmlString(Color c) {
         return ColorTranslator.ToHtml(c);
      }

      protected virtual T convert(String s) {
         if (String.Equals(SettingsSource.AUTO, s, StringComparison.OrdinalIgnoreCase))
            s = Default;
         else {
            s = s.TrimToNull();
            if (s == null) s = Default;
         }
         return convertStr(s);
      }
      protected abstract T convertStr(String s);

      public void Load(RegistryKey k) {
         if (k == null) return;
         String v = ReadVal(k);
         try {
            convert(v);
         } catch (Exception e) {
            Globals.SettingsLogger.Log(_LogType.ltError, "Setting [{0}] has an invalid value: [{1}]. Replaced by default [{2}].",
               Name, v, Default);
            v = Default;
         }
         value = v;
      }
      public void Save(RegistryKey k) {
         if (k == null) return;
         k.SetValue(Name, value == null ? String.Empty : value);
      }

      protected String ReadVal(RegistryKey key) {
         if (key == null) return Default;
         var v = key.GetValue(Name, Default);
         if (v == null) return Default;

         String ret = v == null ? null : v.ToString().TrimToNull();
         return ret == null ? Default : ret;
      }

   }

   /// <summary>
   /// IntSetting.
   /// The converted actual value is an int
   /// </summary>
   public class IntSetting : Setting<int> {
      public IntSetting(String name, String def, String initial = null) : base(name, def, initial) { }

      protected override int convertStr(string s) {
         return Invariant.ToInt32(s);
      }
      public static implicit operator int(IntSetting x) {
         return x.convert(x.value);
      }
      public override string ToString() {
         return Invariant.Format("{0}: {1} ({2})", Name, value, Pretty.PrintNumber(convert(value)));
      }
   }

   /// <summary>
   /// ThreadsSetting.
   /// If the raw value <= 0, it is the relative #threads and needs to be added to the #cores
   /// Otherwise it is the #threads itself
   /// </summary>
   public class ThreadsSetting : Setting<int> {
      public ThreadsSetting(String name, String def, String initial = null) : base(name, def, initial) { }

      protected override int convertStr(string s) {
         int ret = String.Equals(s, SettingsSource.AUTO, StringComparison.OrdinalIgnoreCase) ? 0: Invariant.ToInt32(s);
         var N = Environment.ProcessorCount;
         if (ret <= 0) ret += N;

         if (ret < 1) ret = 1;
         else if (ret > N) ret = N;
         return ret;
      }
      public static implicit operator int(ThreadsSetting x) {
         return x.convert(x.value);
      }
      public override string ToString() {
         return Invariant.Format("{0}: {1} ({2})", Name, value, Pretty.PrintNumber(convert(value)));
      }
   }

   /// <summary>
   /// SizeSetting.
   /// Sizes are converted into a long. Values can be specified like 0.5MB.
   /// </summary>
   public class SizeSetting : Setting<long> {
      public SizeSetting(String name, String def, String initial = null) : base(name, def, initial) { }

      protected override long convertStr(string s) {
         if (String.Equals(s, "off")) return long.MaxValue;
         if (String.Equals(s, "on")) return -1;
         return Pretty.ParseSize(s);
      }

      public static implicit operator long(SizeSetting x) {
         return x.convert(x.value);
      }
      public static implicit operator int(SizeSetting x) {
         return (int)x.convert(x.value);
      }
      public override string ToString() {
         return Invariant.Format("{0}: {1} ({2})", Name, value, Pretty.PrintSize(convert(value)));
      }
   }

   /// <summary>
   /// ColorSetting.
   /// Colors are stored in the registry as HtmlColors.
   /// </summary>
   public class ColorSetting : Setting<Color> {
      public ColorSetting(String name, Color def, String initial = null) : base(name, ColorToHtmlString(def), initial) { }

      protected override Color convertStr(string s) {
         return ColorTranslator.FromHtml(s);
      }

      public static implicit operator Color(ColorSetting x) {
         return x.convert(x.value);
      }
      public override string ToString() {
         return Invariant.Format("{0}: {1}", Name, value);
      }
   }
}
