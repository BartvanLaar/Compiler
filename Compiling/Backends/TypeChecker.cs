using Lexing;
using Parsing.AbstractSyntaxTree.Expressions;
using Parsing.AbstractSyntaxTree.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Compiling.Backends
{
    internal class TypeChecker : IAbstractSyntaxTreeVisitor
    {

        private readonly Dictionary<string, FunctionDefinitionExpression> _functions = new();
        private readonly Dictionary<string, Token> _namedValues = new();
        private readonly Stack<Token> _valueStack = new();
        public string Name => "Type checker";

        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            var lhsValue = Convert.ToDouble(_valueStack.Pop());
            Visit(expression.RightHandSide);
            var rhsValue = Convert.ToDouble(_valueStack.Pop());

            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Add:
                    {
                        throw new NotImplementedException();
                        break;
                    }
                case ExpressionType.Subtract:
                    {
                        throw new NotImplementedException();
                        break;
                    }
                case ExpressionType.Multiply:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.Divide:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.Assignment:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.Equals:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.NotEquivalent: //todo: actually make this do a type compare? 
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.NotEquals:
                    {

                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        throw new NotImplementedException();


                        break;
                    }
                case ExpressionType.GreaterThanEqual:
                    {
                        throw new NotImplementedException();


                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.LessThanEqual:
                    {
                        throw new NotImplementedException();


                        break;
                    }
                case ExpressionType.LogicalOr:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.LogicalXOr:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.LogicalAnd:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.ConditionalOr:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.ConditionalAnd:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.BitShiftLeft:
                    {
                        throw new NotImplementedException();

                        break;
                    }
                case ExpressionType.BitShiftRight:
                    {
                        throw new NotImplementedException();
                        break;
                    }
                default:
                    throw new ArgumentException($"invalid binary operator {expression.NodeExpressionType}");
            }
        }

        public void VisitBodyExpression(BodyExpression expression)
        {
            foreach (var expr in expression.Body)
            {
                Visit(expr);
            }
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            Visit(expression.Condition);
            Visit(expression.Body);
        }

        public void VisitDoWhileStatementExpression(DoWhileStatementExpression expression)
        {
            Visit(expression.Body);
            Visit(expression.Condition);
        }

        public void VisitFloatExpression(ValueExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            Visit(expression.VariableDeclaration);
            Visit(expression.Condition);
            Visit(expression.VariableIncreaseExpression);
            Visit(expression.Body);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            // visit body first, cause defining a function is more important than the prototype..
            Visit(expression.Body);
            var typeTokens = expression.Arguments.Select(x => x.TypeToken);
            expression.FunctionName = CreateMangledName(expression.FunctionName, typeTokens);
            _functions.Add(expression.FunctionName, expression);
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            Visit(expression.ReturnExpr);
            var hasReturnExpressionResultToken = _valueStack.TryPop(out var returnExprToken);

            if (expression.FunctionReturnTypeIndicator is TypeIndicator.Void)
            {
                if (hasReturnExpressionResultToken)
                {
                    throw new Exception("void return func can't return a value"); // todo create custom exception for type error..
                }

                return;
            }

            Debug.Assert(returnExprToken is not null);
            Debug.Assert(expression.Token is not null);
            expression.Token.TypeIndicator = expression.FunctionReturnTypeIndicator;
            FixTypes(expression.Token, returnExprToken);
            if (expression.FunctionReturnTypeIndicator != returnExprToken.TypeIndicator)
            {
                throw new Exception($"Function with return type '{expression.FunctionReturnTypeIndicator}' can't return a value of type '{returnExprToken.TypeIndicator}'");
            }

            return;
        }

        private void FixTypes(Token lhs, Token rhs)// todo:rename
        {
            Debug.Assert(lhs.TypeIndicator is not TypeIndicator.None && rhs.TypeIndicator is not TypeIndicator.None, $"{nameof(FixTypes)} does not support inferring types (yet)");// todo: handle type inference.

            if (lhs.TypeIndicator == rhs.TypeIndicator)
            {
                return;
            }

            var supportedTypesOrdered = new[] { TypeIndicator.Double, TypeIndicator.Float, TypeIndicator.Integer };

            if (!supportedTypesOrdered.Contains(lhs.TypeIndicator) || !supportedTypesOrdered.Contains(rhs.TypeIndicator))
            {
                return; // can only fix these types, so if lhs and rhs are not both of the above typeIndicator then return.
            }

            foreach (var type in supportedTypesOrdered)
            {
                if (lhs.TypeIndicator == type || rhs.TypeIndicator == type)
                {
                    lhs.TypeIndicator = type;
                    rhs.TypeIndicator = type;
                    return;
                }
            }



        }

        public void VisitStringExpression(StringExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            throw new NotImplementedException();
        }

        public void VisitBooleanExpression(BooleanExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        public void VisitCharacterExpression(CharacterExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        public void VisitDoubleExpression(DoubleExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            _valueStack.Push(expression.Token);
        }

        private static string CreateMangledName(string baseName, IEnumerable<Token> typeTokens)
        {
            var name = baseName;
            foreach (var token in typeTokens)
            {
                name += token.Name.First();
            }
            return name;
        }

    }
}
