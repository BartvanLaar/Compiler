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
            var lhsValue = _valueStack.Pop();
            Visit(expression.RightHandSide);
            _valueStack.TryPop(out var rhsValue);

            if (rhsValue == null)
            {
                return; // input = output
            }

            switch (expression.NodeExpressionType)
            {
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanEqual:
                    {
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeIndicator != rhsValue.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }

                        return;
                    }
                case ExpressionType.Assignment:
                    {
                        //todo: refactor and infer type before calling this method...
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeIndicator != rhsValue.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }
                        return;
                    }
                case ExpressionType.Equivalent: //todo: actually make this do a type compare? 
                case ExpressionType.NotEquivalent: //todo: actually make this do a type compare? 
                    {
                        //todo: refactor and infer type before calling this method...
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeIndicator != rhsValue.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }

                        return;
                    }
                case ExpressionType.Equals:
                case ExpressionType.NotEquals:
                    {
                        //todo: refactor and infer type before calling this method...
                        FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
                        if (lhsValue.TypeIndicator != rhsValue.TypeIndicator)
                        {
                            throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
                        }

                        return;
                    }
                case ExpressionType.BitShiftLeft:
                case ExpressionType.BitShiftRight:
                case ExpressionType.BitwiseOr:
                case ExpressionType.BitwiseAnd:
                    {

                        if (lhsValue.TypeIndicator != TypeIndicator.Integer)
                        {
                            throw new Exception($"lhs value of {expression.NodeExpressionType} must evaluate to an {TypeIndicator.Integer}.");// todo improve message.
                        }

                        if (rhsValue.TypeIndicator != TypeIndicator.Integer)
                        {
                            throw new Exception($"rhs value of{expression.NodeExpressionType} must evaluate to an {TypeIndicator.Integer}.");// todo improve message.
                        }

                        return;
                    }
                case ExpressionType.ConditionalOr:
                case ExpressionType.ConditionalXOr:
                case ExpressionType.ConditionalAnd:
                    {

                        if (lhsValue.TypeIndicator != TypeIndicator.Boolean)
                        {
                            throw new Exception($"lhs value of {expression.NodeExpressionType} must evaluate to a {TypeIndicator.Boolean}.");// todo improve message.

                        }

                        if (rhsValue.TypeIndicator != TypeIndicator.Boolean)
                        {
                            throw new Exception($"rhs value of{expression.NodeExpressionType} must evaluate to a {TypeIndicator.Boolean}.");// todo improve message.
                        }

                        return;
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
            //todo: make sure the arguments will be known before visting the body....

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
            Visit(expression.IfCondition);
            Visit(expression.IfBody);
            Visit(expression.ElseBody);
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
            FixNumericalTypesAndValuesIfRequired(expression.Token, returnExprToken);
            if (expression.FunctionReturnTypeIndicator != returnExprToken.TypeIndicator)
            {
                throw new Exception($"Function with return type '{expression.FunctionReturnTypeIndicator}' can't return a value of type '{returnExprToken.TypeIndicator}'");
            }

            return;
        }

        private static void FixNumericalTypesAndValuesIfRequired(Token lhs, Token rhs)// todo:rename
        {
            //todo: refactor and infer type before calling this method...
            var isTypeInferred = lhs.TypeIndicator is TypeIndicator.Inferred || rhs.TypeIndicator is TypeIndicator.Inferred;
            if (lhs.TypeIndicator == rhs.TypeIndicator)
            {
                if(isTypeInferred) // not sure if this is actually valid... get rid of it if required -BvL 13-11-2021.
                {
                    throw new Exception("Unable to infer type, both lhs and rhs are type inferred.");
                }

                return;
            }

            var supportedTypesOrdered = new (TypeIndicator TypeIndicator, Type TypeInfo)[] { (TypeIndicator.Double, typeof(double)), (TypeIndicator.Float, typeof(float)), (TypeIndicator.Integer, typeof(int)) };
            
            if (!isTypeInferred && !supportedTypesOrdered.Any(t => t.TypeIndicator == lhs.TypeIndicator || t.TypeIndicator == rhs.TypeIndicator))
            {
                return; // can only fix these types, so if lhs and rhs are not both of the above typeIndicator then return.
            }

            foreach (var typeData in supportedTypesOrdered)// don't filter list as its ordered..
            {
                if (lhs.TypeIndicator == typeData.TypeIndicator || rhs.TypeIndicator == typeData.TypeIndicator)
                {
                    lhs.TypeIndicator = typeData.TypeIndicator;
                    rhs.TypeIndicator = typeData.TypeIndicator;

                    if (lhs.TokenType is TokenType.Value)
                    {
                        lhs.Value = Convert.ChangeType(lhs.Value, typeData.TypeInfo);
                    }

                    if (rhs.TokenType is TokenType.Value)
                    {
                        rhs.Value = Convert.ChangeType(rhs.Value, typeData.TypeInfo);
                    }

                    return;
                }
            }

            Debug.Assert(lhs.TypeIndicator == rhs.TypeIndicator);
        }

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            var lhsValue = expression.DeclarationTypeToken;
            Visit(expression.ValueExpression);
            var rhsValue = _valueStack.Pop();

            FixNumericalTypesAndValuesIfRequired(lhsValue, rhsValue);
            if (lhsValue.TypeIndicator != rhsValue.TypeIndicator)
            {
                throw new Exception("rhs and lhs value are not of the same type.");// todo improve message.
            }

            return;
        }

        public void VisitValueExpression(ValueExpression expression)
        {
            Debug.Assert(expression?.Token is not null);
            if (expression.Token.TypeIndicator is TypeIndicator.Character && expression.Token?.ValueAsString?.Length > 1)
            {
                throw new Exception($"Character can only have a length of one but was {expression.Token?.ValueAsString?.Length}"); // todo: Custom exception type.
            }
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
