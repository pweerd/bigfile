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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.Query
{
   public abstract class ParserNode<T>
   {
      public abstract bool Evaluate(T test);
      public abstract bool EvaluateDeep(T test);

      public List<ParserValueNode<T>> CollectValueNodes()
      {
         var ret = new List<ParserValueNode<T>>();
         collectValueNodes(ret);
         return ret;
      }

      public override String ToString()
      {
         var sb = toString(new StringBuilder());
         int start, end;
         for (start = 0; start < sb.Length; start++)
         {
            if (sb[start] != ' ') break;
         }
         for (end = sb.Length; end > start; end--)
         {
            if (sb[end - 1] != ' ') break;
         }
         var ret = sb.ToString(start, end - start);
         if (ret != sb.ToString().Trim())
         {
            Console.WriteLine("trimmed: [{0}]", sb.ToString().Trim());
            Console.WriteLine("ret: [{0}]", ret);
         }
         return sb.ToString(start, end - start);
      }

      public String DumpTree()
      {
         String lvl = String.Empty;
         var sb = new StringBuilder();
         dump(sb, lvl);
         return sb.ToString();
      }
      internal protected abstract StringBuilder dump(StringBuilder sb, String lvl);
      internal protected abstract StringBuilder toString(StringBuilder sb);
      internal protected abstract void collectValueNodes(List<ParserValueNode<T>> list);
   }



   public class ParserValueNode<T> : ParserNode<T>
   {
      public readonly String Field;
      public readonly String Value;
      protected readonly String fieldPlusValue;
      protected readonly int hashcode;

      public ParserValueNode(String field, String value)
      {
         if (String.IsNullOrEmpty(field)) field = null;
         this.Field = field;
         this.Value = value;
         fieldPlusValue = field==null ? value : field + ":" + value;
         hashcode = fieldPlusValue.GetHashCode();
      }

      public override bool Evaluate(T test) { var v = test.ToString(); return v == "true" || v == "1"; }
      public override bool EvaluateDeep(T test) { return Evaluate(test); }
      internal protected override StringBuilder toString(StringBuilder sb)
      {
         return sb.Append(fieldPlusValue);
      }
      internal protected override StringBuilder dump(StringBuilder sb, String lvl)
      {
         return sb.AppendLine().Append(lvl).Append(fieldPlusValue);
      }

      protected internal override void collectValueNodes(List<ParserValueNode<T>> list)
      {
         list.Add(this);
      }


      public override int GetHashCode()
      {
         return hashcode;
      }
      public override bool Equals(object obj)
      {
         if (obj == null || obj.GetType() != GetType()) return false;

         return String.Equals(fieldPlusValue, ((ParserValueNode<T>)obj).fieldPlusValue);
      }
   }

   public class ParserOrNode<T> : ParserNode<T>
   {
      public readonly ParserNode<T> Left, Right;

      public ParserOrNode(ParserNode<T> left, ParserNode<T> right)
      {
         this.Left = left;
         this.Right = right;
      }

      public override bool Evaluate(T test) { return Left.Evaluate(test) || Right.Evaluate(test); }
      public override bool EvaluateDeep(T test) { return Left.Evaluate(test) | Right.Evaluate(test); }

      internal protected override StringBuilder toString(StringBuilder sb)
      {
         sb.Append("(");
         Left.toString(sb);
         sb.Append(" OR ");
         Right.toString(sb);
         sb.Append(")");
         return sb;
      }
      internal protected override StringBuilder dump(StringBuilder sb, String lvl)
      {
         String lvl2 = lvl + "-- ";
         sb.AppendLine().Append(lvl).Append("OR");
         Left.dump(sb, lvl2);
         Right.dump(sb, lvl2);
         return sb;
      }
      protected internal override void collectValueNodes(List<ParserValueNode<T>> list)
      {
         Left.collectValueNodes(list);
         Right.collectValueNodes(list);
      }

   }

   public class ParserAndNode<T> : ParserNode<T>
   {
      public readonly ParserNode<T> Left, Right;

      public ParserAndNode(ParserNode<T> left, ParserNode<T> right)
      {
         this.Left = left;
         this.Right = right;
      }

      public override bool Evaluate(T test) { return Left.Evaluate(test) && Right.Evaluate(test); }
      public override bool EvaluateDeep(T test) { return Left.Evaluate(test) & Right.Evaluate(test); }

      internal protected override StringBuilder toString(StringBuilder sb)
      {
         sb.Append("(");
         Left.toString(sb);
         sb.Append(" AND ");
         Right.toString(sb);
         sb.Append(")");
         return sb;
      }
      internal protected override StringBuilder dump(StringBuilder sb, String lvl)
      {
         String lvl2 = lvl + "-- ";
         sb.AppendLine().Append(lvl).Append("AND");
         Left.dump(sb, lvl2);
         Right.dump(sb, lvl2);
         return sb;
      }
      protected internal override void collectValueNodes(List<ParserValueNode<T>> list)
      {
         Left.collectValueNodes(list);
         Right.collectValueNodes(list);
      }
   }


   public class ParserNot2Node<T> : ParserNode<T>
   {
      public readonly ParserNode<T> Left, Right;
      public ParserNot2Node(ParserNode<T> left, ParserNode<T> right)
      {
         this.Left = left;
         this.Right = right;
      }

      public override bool Evaluate(T test) { return Left.Evaluate(test) && !Right.Evaluate(test); }
      public override bool EvaluateDeep(T test) { return Left.Evaluate(test) & !Right.Evaluate(test); }

      internal protected override StringBuilder toString(StringBuilder sb)
      {
         sb.Append("(");
         Left.toString(sb);
         sb.Append(" NOT ");
         Right.toString(sb);
         sb.Append(")");
         return sb;
      }
      internal protected override StringBuilder dump(StringBuilder sb, String lvl)
      {
         String lvl2 = lvl + "-- ";
         sb.AppendLine().Append(lvl).Append("NOT2");
         Left.dump(sb, lvl2);
         Right.dump(sb, lvl2);
         return sb;
      }
      protected internal override void collectValueNodes(List<ParserValueNode<T>> list)
      {
         Left.collectValueNodes(list);
         Right.collectValueNodes(list);
      }
   }

   public class ParserNot1Node<T> : ParserNode<T>
   {
      public readonly ParserNode<T> Sub;
      public ParserNot1Node(ParserNode<T> node)
      {
         this.Sub = node;
      }

      public override bool Evaluate(T test) { return !Sub.Evaluate(test); }
      public override bool EvaluateDeep(T test) { return !Sub.Evaluate(test); }

      internal protected override StringBuilder toString(StringBuilder sb)
      {
         sb.Append("NOT ");
         Sub.toString(sb);
         return sb;
      }
      internal protected override StringBuilder dump(StringBuilder sb, String lvl)
      {
         String lvl2 = lvl + "-- ";
         sb.AppendLine().Append(lvl).Append("NOT1");
         Sub.dump(sb, lvl2);
         return sb;
      }
      protected internal override void collectValueNodes(List<ParserValueNode<T>> list)
      {
         Sub.collectValueNodes(list);
      }
   }
}
