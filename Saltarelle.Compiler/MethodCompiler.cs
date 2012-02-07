﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
    public class MethodCompiler : DepthFirstAstVisitor<object, object> {
        private readonly INamingConventionResolver _namingConvention;
        private readonly IErrorReporter _errorReporter;
        private ICompilation _compilation;
        private CSharpAstResolver _resolver;

        internal IDictionary<IVariable, string> variableNameMap;

        public MethodCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver) {
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
            _compilation = compilation;
            _resolver = resolver;
        }

        public JsFunctionDefinitionExpression CompileMethod(AttributedNode methodNode, IMethod method, MethodImplOptions impl) {
            var usedNames   = new HashSet<string>(method.DeclaringTypeDefinition.TypeParameters.Concat(method.TypeParameters).Select(p => _namingConvention.GetTypeParameterName(p)));
            variableNameMap = new VariableGatherer(_resolver, _namingConvention, _errorReporter).GatherVariables(methodNode, method, usedNames);
            var paramNames  = new List<string>();
            
            foreach (var p in method.Parameters) {
                paramNames.Add(variableNameMap[p]);
            }

            methodNode.AcceptVisitor(this);

            return new JsFunctionDefinitionExpression(paramNames, JsBlockStatement.EmptyStatement, "X");
        }
    }
}