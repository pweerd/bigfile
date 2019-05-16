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
    public class Parser<TArg> 
   {
      protected readonly Lexer lexer;
      protected readonly Func<String, String, ParserValueNode<TArg>> valueFactory;
      protected Lexer.Token _token;

      public Parser (Lexer lexer, Func<String, String, ParserValueNode<TArg>> valueFactory=null)
      {
         this.lexer = lexer;
         this.valueFactory = valueFactory == null ? ValueNodeFactory : valueFactory;
         _token = lexer.NextToken();
      }

      public Lexer.Token peekNextToken()
      {
         return _token;
      }
      public Lexer.Token readNextToken()
      {
         var ret = _token;
         _token = lexer.NextToken();
         return ret;
      }

      public ParserNode<TArg> Parse ()
      {
         return peekNextToken().Type== Lexer.TokenType.Eof ? null : readExpression();
      }

      //exp::term {OR expr};
      //exp::term {IMPLIED expr};
      protected virtual ParserNode<TArg> readExpression()
      {
         var term = readTerm();
         var next = peekNextToken();
         switch (next.Type)
         {
            case Lexer.TokenType.Eof:
            case Lexer.TokenType.RParen:
               return term;
            case Lexer.TokenType.Or:
               readNextToken();
               return new ParserOrNode<TArg>(term, readExpression());
            default:
               return new ParserOrNode<TArg>(term, readExpression());
         }
      }

      //term::factor {AND factor};
      //term::factor {NOT factor};
      protected virtual ParserNode<TArg> readTerm()
      {
         var fact = readFactor();
         var next = peekNextToken();

         switch (next.Type)
         {
            case Lexer.TokenType.And:
               readNextToken();
               return createOptimizedAnd (fact, readTerm());
            case Lexer.TokenType.Not:
               readNextToken();
               return new ParserNot2Node<TArg>(fact, readTerm());
            default:
               return fact;
         }
      }

      //factor::id;
      //factor::NOT factor;
      //factor::LPAREN exp RPAREN;
      protected virtual ParserNode<TArg> readFactor()
      {
         var token = readNextToken();
         switch (token.Type)
         {
            case Lexer.TokenType.LParen:
               var expr = readExpression();
               token = readNextToken();
               if (token != null && token.Type != Lexer.TokenType.RParen)
                  throw new Exception(") expected");
               return expr;

            case Lexer.TokenType.Not:
               var fact = readFactor();
               return createOptimizedNot1(fact);

            case Lexer.TokenType.Value:
            case Lexer.TokenType.Field:
               return readFieldValue(token);

            default:
               throw new Exception("Unexpected token: " + token);
         }
      }

      protected ParserNode<TArg> createOptimizedAnd(ParserNode<TArg> left, ParserNode<TArg> right)
      {
         bool leftIsNot = left is ParserNot1Node<TArg>;
         bool rightIsNot = right is ParserNot1Node<TArg>;

         if (leftIsNot && !rightIsNot)
            return new ParserNot2Node<TArg>(right, ((ParserNot1Node<TArg>)left).Sub);
         if (rightIsNot && !leftIsNot)
            return new ParserNot2Node<TArg>(left, ((ParserNot1Node<TArg>)right).Sub);
         return new ParserAndNode<TArg>(left, right);
      }
      protected ParserNode<TArg> createOptimizedNot1(ParserNode<TArg> node)
      {
         if (node is ParserNot1Node<TArg>)
         {
            return ((ParserNot1Node<TArg>)node).Sub;
         }
         return new ParserNot1Node<TArg>(node);
      }

      protected ParserValueNode<TArg> readFieldValue (Lexer.Token token)
      {
         if (token.Type == Lexer.TokenType.Field)
         {
            var valueToken = readNextToken();
            if (valueToken.Type != Lexer.TokenType.Value) throw new Exception("Unexpected token: " + token);
            return valueFactory(token.Text, valueToken.Text);
         }
         return valueFactory(null, token.Text);
      }

      protected static ParserValueNode<TArg> ValueNodeFactory(String field, String text)
      {
         return new ParserValueNode<TArg>(field, text);
      }
   }
}
