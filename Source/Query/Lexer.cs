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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.Query
{
   public class Lexer
   {
      private readonly String expr;
      private int nextIndex = 0;
      private int endIndex;
      private readonly bool isSimple;
      private readonly Token eof;

      public Lexer (String expr)
      {
         this.expr = expr;
         //this.isSimple = IsSimple(expr);
         this.nextIndex = 0;
         this.endIndex = expr==null ? 0 : expr.Length;
         this.eof = new Token(TokenType.Eof);
      }

      public Token NextToken()
      {
         if (nextIndex >= endIndex) return eof;
         if (isSimple)
         {
            nextIndex = endIndex;
            if (expr == null) return eof;
            var trimmed = expr.Trim();
            return String.IsNullOrEmpty(trimmed) ? eof : new Token (TokenType.Value, expr);
         }

         int i;
         for (i=nextIndex; i<endIndex; i++)
         {
            if (!char.IsWhiteSpace(expr[i])) break; 
         }

         if (i >= endIndex) return eof;
         switch (expr[i])
         {
            case '\'':
            case '"':
               return readQuotedValue(i);

            case '(':
               nextIndex = i + 1;
               return new Token(TokenType.LParen);
            case ')':
               nextIndex = i + 1;
               return new Token(TokenType.RParen);
            case 'A':
               if (i + 2 >= endIndex) break;
               if (expr[i + 1] == 'N' && expr[i + 2] == 'D' && isWhiteOrEndOrParenthesis(i + 3))
               {
                  nextIndex = i + 3;
                  return new Token(TokenType.And);
               }
               break;
            case 'N':
               if (i + 2 >= endIndex) break;
               if (expr[i + 1] == 'O' && expr[i + 2] == 'T' && isWhiteOrEndOrParenthesis(i + 3))
               {
                  nextIndex = i + 3;
                  return new Token(TokenType.Not);
               }
               break;
            case 'O':
               if (i + 1 >= endIndex) break;
               if (expr[i + 1] == 'R' && isWhiteOrEndOrParenthesis(i + 2))
               {
                  nextIndex = i + 2;
                  return new Token(TokenType.Or);
               }
               break;
         }

         int j;
         for (j=i+1; j < endIndex; j++)
         {
            char ch = expr[j];
            if (ch == ':')
            {
               nextIndex = j + 1;
               return new Token (TokenType.Field, expr.Substring(i, j - i));
            }
            if (ch == ':' || ch == ')' || char.IsWhiteSpace(ch)) break;
         }
         nextIndex = j;
         return new Token(expr.Substring(i, j - i));
      }

      private bool isWhiteOrEnd(int index)
      {
         return index >= endIndex || char.IsWhiteSpace(expr[index]);
      }
      private bool isWhiteOrEndOrParenthesis(int index)
      {
         return index >= endIndex || char.IsWhiteSpace(expr[index]) || expr[index] == '(';
      }

      private Token readQuotedValue (int from)
      {
         char quoteChar = expr[from];
         int end = expr.IndexOf(quoteChar, from + 1);
         if (end < 0 || end + 1 >= endIndex)  //Didn't find the end or it was the last char in the string
         {
            nextIndex = endIndex;
            if (end < 0) end = endIndex;
            return new Token(expr.Substring(from + 1, end - (from + 1)));
         }

         //Check if we found the end-quote: simple case
         if (expr[end-1] != '\\' && char.IsWhiteSpace(expr[end + 1]))
         {
            nextIndex = end+1;
            return new Token(expr.Substring(from + 1, end - (from+1)));
         }

         //More complex case: we need to un-escape 
         var sb = new StringBuilder(endIndex - from);
         sb.Append(expr, from + 1, end - from - 2);
         int i=end-1;
         for (i=end-1; i<endIndex; i++)
         {
            char ch = expr[i];
            if (ch == '\\')
            {
               if (i + 1 < endIndex && expr[i + 1] == quoteChar)
               {
                  i++;
                  sb.Append(quoteChar);
                  continue;
               }
               sb.Append(ch);
               continue;
            }
            if (ch == quoteChar)
            {
               if (i + 1 >= endIndex)
               {
                  i = endIndex;
                  break;
               }

               char chNext = expr[i + 1];
               if (chNext == quoteChar)
               {
                  i++;
                  sb.Append(quoteChar);
                  continue;
               }
               if (char.IsWhiteSpace(chNext)) break;
            }

            sb.Append(ch);
         }

         nextIndex = i + 1;
         return new Token(sb.ToString());
      }

      public static bool IsSimple (String expr)
      {
         if (expr == null) return true;
         return
            expr.IndexOf('"') < 0 &&
            expr.IndexOf('\'') < 0 &&
            expr.IndexOf(':') < 0 &&
            expr.IndexOf("NOT", StringComparison.Ordinal) < 0 &&
            expr.IndexOf("AND", StringComparison.Ordinal) < 0 &&
            expr.IndexOf("OR", StringComparison.Ordinal) < 0;
      }

      public enum TokenType { And, Or, Not, LParen, RParen, Field, Value, Eof};
      public class Token
      {
         public readonly TokenType Type;
         public readonly String Text;
         public Token(TokenType type, String text)
         {
            this.Type = type;
            this.Text = text;
         }
         public Token(TokenType type)
         {
            this.Type = type;
            this.Text = null;
         }
         public Token(String text)
         {
            this.Type = TokenType.Value;
            this.Text = text;
         }

         public override String ToString ()
         {
            return Type == TokenType.Value ? Type + ":" + Text : Type.ToString();
         }
      }
   }


}
