using Lexing;
using Parsing.AbstractSyntaxTree.Expressions;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public interface IScope
    {
        IReadOnlyDictionary<string, ExpressionBase> UserDefinedTypes { get; }
        IReadOnlyDictionary<string, ExpressionBase> Variables { get; }
        IReadOnlyDictionary<string, FunctionDefinitionExpression> Functions { get; }
        public string Namespace { get; }
        public string ClassName { get; }
        public string ScopeIdentifier { get; }
    }

    public class Scope : IScope
    {
        // saving their entire expressions is a bit overkill as we only want to access the metadata...
        // for now just use the expressions. Shouldn't matter memory wise as it's all references anyway...
        private readonly Dictionary<string, FunctionDefinitionExpression> _functions = new();
        private readonly Dictionary<string, ExpressionBase> _variables = new();
        private readonly Dictionary<string, ExpressionBase> _userDefinedTypes = new();

        public Scope(string @namespace, string className)
        {
            Namespace = @namespace;
            ClassName = className;
        }
        public IReadOnlyDictionary<string, ExpressionBase> UserDefinedTypes => _userDefinedTypes;
        public IReadOnlyDictionary<string, ExpressionBase> Variables => _variables;
        public IReadOnlyDictionary<string, FunctionDefinitionExpression> Functions => _functions;

        public string Namespace { get; }
        public string ClassName { get; }
        public string ScopeIdentifier => ToString();

        public void AddFunction(string functionName, FunctionDefinitionExpression expression)
        {
            _functions.Add(functionName, expression);
        }

        public void AddVariable(string variableName, ExpressionBase expression)
        {
            _variables.Add(variableName, expression);
        }

        public void AddUserDefinedType(string typeName, ExpressionBase expression)
        {
            _userDefinedTypes.Add(typeName, expression);
        }

        public override string ToString()
        {
            return $"{Namespace}.{ClassName}";
        }
    }

    public class ASTCrawler : IAbstractSyntaxTreeVisitor
    {
        private readonly Dictionary<string, Scope> _scopes = new();
        public string Name => "AST Crawler";
        public IReadOnlyDictionary<string, IScope> Scopes => _scopes.ToDictionary(kv => kv.Key, kv => (IScope)kv.Value);

        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            Visit(expression.RightHandSide);
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

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            Visit(expression.VariableDeclaration);
            Visit(expression.Condition);
            Visit(expression.VariableIncreaseExpression);
            Visit(expression.Body);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {

        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            Visit(expression.Body);

            expression.FunctionName = CreateMangledName(expression.FunctionName, expression.Arguments.Select(a => a.TypeToken));
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {

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
        }

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {

        }

        public void VisitValueExpression(ValueExpression expression)
        {

        }

        private static string CreateMangledName(string baseName, IEnumerable<Token> typeTokens)
        {
            //todo: code below doesnt really work for user defined types as the name can have the same starting value....?
            var name = baseName;

            foreach (var token in typeTokens)
            {
                var typeName = token.TokenType is TokenType.VariableIdentifier ? token.Name : ConvertTypeIndicatorToString(token.TypeIndicator);
                name += $"<{typeName}>";
            }

            return name;
        }

        private static string ConvertTypeIndicatorToString(TypeIndicator typeIndicator)
        {
            return typeIndicator switch
            {
                TypeIndicator.Float => "float",
                TypeIndicator.Double => "double",
                TypeIndicator.Boolean => "bool",
                TypeIndicator.Integer => "int",
                TypeIndicator.Character => "char",
                TypeIndicator.String => "string",
                TypeIndicator.Inferred => throw new NotImplementedException(),
                TypeIndicator.DateTime => throw new NotImplementedException(),
                TypeIndicator.Void => throw new NotImplementedException(),
                TypeIndicator.None => throw new NotImplementedException(),
                _ => throw new NotImplementedException($"Exhaustive use of {nameof(ConvertTypeIndicatorToString)}."),
            };
        }

        public void Initialize(IReadOnlyDictionary<string, IScope> scopes)
        {
            throw new NotImplementedException("This method should not be called on the crawler thats providing the scopes!");
        }

        public void VisitNamespaceExpression(NamespaceDefinitionExpression expression)
        {
            
        }

        public void VisitClassExpression(ClassDefinitionExpression expression)
        {
            
        }

        public void VisitImportExpression(ImportStatementExpression expression)
        {
            
        }
    }
}
