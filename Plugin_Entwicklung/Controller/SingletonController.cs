using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Entwicklung.Controller
{
    class SingletonController
    {
        public void CheckSingleton(Project project, ObservableCollection<Document> documents)
        {
            foreach (Document d in documents)
            {
                Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
                SyntaxTree tree = t.Result;

                List<ClassDeclarationSyntax> classDeclarations =
                    tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

                List<ConstructorDeclarationSyntax> constructorDeclarations = 
                    tree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();

                foreach (ClassDeclarationSyntax classDec in classDeclarations)
                {
                    bool isSingleton = true;
                    foreach (ConstructorDeclarationSyntax constDec in constructorDeclarations)
                    {
                        List<SyntaxToken> tokens = constDec.ChildTokens().ToList();
                        bool isPrivate = false;
                        foreach (SyntaxToken token in tokens)
                        {
                            if (token.IsKind(SyntaxKind.PrivateKeyword))
                            {
                                isPrivate = true;
                            }
                        }
                        if (!isPrivate)
                        {
                            isSingleton = false;
                        }
                    }

                    string className = classDec.Identifier.Text;
                    if (isSingleton)
                    {
                        // With information from http://csharpindepth.com/Articles/General/Singleton.aspx

                        System.Diagnostics.Debug.WriteLine("Class " + className + " contained in document " + d.Name + " is a Singleton!");
                        if (IsDoubleCheckLocked(classDec))
                        {
                            System.Diagnostics.Debug.WriteLine("Class " + className + " contained in document " + d.Name + " is double check locked!");
                        } else if (IsLazy(classDec))
                        {
                            System.Diagnostics.Debug.WriteLine("Class " + className + " contained in document " + d.Name + " is lazy!");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Class " + className + " contained in document " + d.Name + " is not a Singleton!");
                    }
                }
            }
        }

        public bool IsDoubleCheckLocked(ClassDeclarationSyntax classDec)
        {
            List<MethodDeclarationSyntax> possibleInstanceMethods = new List<MethodDeclarationSyntax>();

            List<MethodDeclarationSyntax> methodDeclarations =
                classDec.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            bool classContainsValidMethod = false;

            foreach (MethodDeclarationSyntax methodDec in methodDeclarations)
            {
                bool isPublic = false;
                bool isStatic = false;

                // Check if method returns an instance of class first, otherwise don't do more taxing operations
                if (methodDec.ReturnType.ToString() == classDec.Identifier.Text)
                {
                    SyntaxTokenList modifiers = methodDec.Modifiers;
                    foreach (SyntaxToken modifier in modifiers)
                    {
                        if (modifier.IsKind(SyntaxKind.PublicKeyword))
                        {
                            isPublic = true;
                        }
                        else if (modifier.IsKind(SyntaxKind.StaticKeyword))
                        {
                            isStatic = true;
                        }
                    }
                }

                // isPublic and isStatic can only have been changed to true if returnType is correct.
                if (isPublic && isStatic)
                {
                    possibleInstanceMethods.Add(methodDec);
                }
            }

            foreach (MethodDeclarationSyntax methodDec in possibleInstanceMethods)
            {
                // Can choose .First() because method can syntactically only have one Block
                BlockSyntax block = methodDec.ChildNodes().OfType<BlockSyntax>().First();

                string identifier = "";
                bool containsFirstCheck = false;
                List<SyntaxNode> nodes =
                    block.ChildNodes().OfType<SyntaxNode>().ToList();

                BlockSyntax secondBlock = null;
                foreach (SyntaxNode node in nodes)
                {
                    if (node.IsKind(SyntaxKind.IfStatement))
                    {
                        List<SyntaxNodeOrToken> subNodes =
                            node.ChildNodesAndTokens().OfType<SyntaxNodeOrToken>().ToList();

                        bool isEqualsEquals = false;
                        bool checksForNull = false;

                        foreach (SyntaxNodeOrToken subNode in subNodes)
                        {
                            if (subNode.IsKind(SyntaxKind.EqualsExpression))
                            {
                                List<SyntaxNodeOrToken> subSubNodes =
                                    subNode.ChildNodesAndTokens().OfType<SyntaxNodeOrToken>().ToList();

                                foreach (SyntaxNodeOrToken subSubNode in subSubNodes)
                                {
                                    if (subSubNode.IsKind(SyntaxKind.IdentifierName))
                                    {
                                        identifier = subSubNode.ToString();
                                    }
                                    else if (subSubNode.IsKind(SyntaxKind.EqualsEqualsToken))
                                    {
                                        isEqualsEquals = true;
                                    }
                                    else if (subSubNode.IsKind(SyntaxKind.NullLiteralExpression))
                                    {
                                        checksForNull = true;
                                    }
                                }
                            }

                        }

                        if (isEqualsEquals && checksForNull)
                        {
                            containsFirstCheck = true;
                            secondBlock = node.ChildNodes().OfType<BlockSyntax>().First();
                        }
                    }
                }

                bool containsLock = false;
                BlockSyntax thirdBlock = null;
                if (containsFirstCheck)
                {
                    List<SyntaxNode> subNodes =
                        secondBlock.ChildNodes().OfType<SyntaxNode>().ToList();

                    foreach (SyntaxNode subNode in subNodes)
                    {
                        if (subNode.IsKind(SyntaxKind.LockStatement))
                        {
                            containsLock = true;
                            thirdBlock = subNode.ChildNodes().OfType<BlockSyntax>().First();
                        }
                    }
                }

                bool containsSecondCheck = false;
                BlockSyntax fourthBlock = null;
                if (containsLock)
                {
                    List<SyntaxNode> subNodes =
                        thirdBlock.ChildNodes().OfType<SyntaxNode>().ToList();

                    foreach (SyntaxNode subNode in subNodes)
                    {
                        if (subNode.IsKind(SyntaxKind.IfStatement))
                        {
                            List<SyntaxNode> subSubNodes =
                                subNode.ChildNodes().OfType<SyntaxNode>().ToList();

                            foreach (SyntaxNode subSubNode in subSubNodes)
                            {
                                if (subSubNode.IsKind(SyntaxKind.EqualsExpression))
                                {
                                    List<SyntaxNodeOrToken> subSubSubNodes =
                                        subSubNode.ChildNodesAndTokens().OfType<SyntaxNodeOrToken>().ToList();


                                    bool correctIdentifier = false;
                                    bool isEqualsEquals = false;
                                    bool checksForNull = false;

                                    foreach (SyntaxNodeOrToken subSubSubNode in subSubSubNodes)
                                    {
                                        if (subSubSubNode.IsKind(SyntaxKind.IdentifierName) && subSubSubNode.ToString() == identifier)
                                        {
                                            correctIdentifier = true;
                                        }
                                        else if (subSubSubNode.IsKind(SyntaxKind.EqualsEqualsToken))
                                        {
                                            isEqualsEquals = true;
                                        }
                                        else if (subSubSubNode.IsKind(SyntaxKind.NullLiteralExpression))
                                        {
                                            checksForNull = true;
                                        }
                                    }

                                    if (correctIdentifier && isEqualsEquals && checksForNull)
                                    {
                                        containsSecondCheck = true;
                                        fourthBlock = subNode.ChildNodes().OfType<BlockSyntax>().First();
                                    }
                                }
                            }
                        }
                    }
                }

                bool isValidDoubleCheck = false;
                if (containsSecondCheck)
                {
                    List<SyntaxNode> subNodes =
                        fourthBlock.ChildNodes().OfType<SyntaxNode>().ToList();

                    foreach (SyntaxNode subNode in subNodes)
                    {
                        if (subNode.IsKind(SyntaxKind.ExpressionStatement))
                        {
                            List<SyntaxNode> subSubNodes =
                                subNode.ChildNodes().OfType<SyntaxNode>().ToList();

                            foreach (SyntaxNode subSubNode in subSubNodes)
                            {
                                if (subSubNode.IsKind(SyntaxKind.SimpleAssignmentExpression))
                                {
                                    // SimpleAssignmentExpression can only have an identifier name
                                    if (subSubNode.ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.Text == identifier)
                                    {
                                        // Only one ObjectCreationExpression can be attached to a SimpleAssignmentExpression
                                        ObjectCreationExpressionSyntax objectCreationExp =
                                            subSubNode.ChildNodes().OfType<ObjectCreationExpressionSyntax>().First();

                                        // ObjectCreationExpression can only have one IdentifierName attached to it
                                        if (objectCreationExp.ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.Text == classDec.Identifier.Text)
                                        {
                                            isValidDoubleCheck = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (isValidDoubleCheck)
                    {
                        classContainsValidMethod = true;
                    }
                }

            }

            return classContainsValidMethod;
        }

        public bool IsLazy(ClassDeclarationSyntax classDec)
        {
            string identifier = "";
            bool classContainsValidMethod = false;

            List<FieldDeclarationSyntax> classFieldDeclarations =
                classDec.ChildNodes().OfType<FieldDeclarationSyntax>().ToList();

            foreach (FieldDeclarationSyntax classFieldDec in classFieldDeclarations)
            {
                List<SyntaxNodeOrToken> nodesAndTokens =
                    classFieldDec.ChildNodesAndTokens().OfType<SyntaxNodeOrToken>().ToList();

                bool isPrivate = false;
                bool isStatic = false;
                bool isReadOnly = false;

                bool fieldIsValid = false;

                foreach (SyntaxNodeOrToken nodeOrToken in nodesAndTokens)
                {
                    if (nodeOrToken.IsKind(SyntaxKind.PrivateKeyword))
                    {
                        isPrivate = true;
                    } else if (nodeOrToken.IsKind(SyntaxKind.StaticKeyword))
                    {
                        isStatic = true;
                    } else if (nodeOrToken.IsKind(SyntaxKind.ReadOnlyKeyword))
                    {
                        isReadOnly = true;
                    } else if (nodeOrToken.IsKind(SyntaxKind.VariableDeclaration))
                    {
                        SyntaxNode varDec = nodeOrToken.AsNode();

                        List<SyntaxNodeOrToken> varNodesAndTokens =
                            varDec.ChildNodesAndTokens().OfType<SyntaxNodeOrToken>().ToList();

                        string varIdentifier = "";
                        string typeOfGeneric = "";
                        bool isDeclaredLazy = false;

                        foreach (SyntaxNodeOrToken varNodeOrToken in varNodesAndTokens)
                        {
                            if (varNodeOrToken.IsKind(SyntaxKind.GenericName))
                            {
                                if (((GenericNameSyntax)varNodeOrToken).Identifier.Text.Equals("Lazy"))
                                {
                                    isDeclaredLazy = true;

                                    List<SyntaxNodeOrToken> genericNodesAndTokens =
                                        varNodeOrToken.ChildNodesAndTokens().OfType<SyntaxNodeOrToken>().ToList();

                                    foreach (SyntaxNodeOrToken genericNodeOrToken in genericNodesAndTokens)
                                    {
                                        if (genericNodeOrToken.IsKind(SyntaxKind.TypeArgumentList))
                                        {
                                            // Lazy<T> can only take one argument and must be supplied one argument so there will always be only and exactly one argument.
                                            typeOfGeneric = ((TypeArgumentListSyntax)genericNodeOrToken).Arguments.First().ToString();
                                        }
                                    }

                                }
                            } else if (varNodeOrToken.IsKind(SyntaxKind.VariableDeclarator))
                            {
                                varIdentifier = ((VariableDeclaratorSyntax)varNodeOrToken).Identifier.Text;
                            }
                        }

                        if (isDeclaredLazy)
                        {
                            if (!varIdentifier.Equals("") && !typeOfGeneric.Equals(""))
                            {
                                identifier = varIdentifier;
                            }
                        }
                    }
                    
                }
            }

            List<MethodDeclarationSyntax> methodDeclarations =
                classDec.ChildNodes().OfType<MethodDeclarationSyntax>().ToList();

            foreach (MethodDeclarationSyntax methodDec in methodDeclarations)
            {
                List<SyntaxNodeOrToken> methodNodesAndTokens =
                    methodDec.ChildNodesAndTokens().OfType<SyntaxNodeOrToken>().ToList();

                bool isPublic = false;
                bool isStatic = false;
                bool correctType = false;

                foreach (SyntaxNodeOrToken methodNodeOrToken in methodNodesAndTokens)
                {
                    if (methodNodeOrToken.IsKind(SyntaxKind.PublicKeyword))
                    {
                        isPublic = true;
                    } else if (methodNodeOrToken.IsKind(SyntaxKind.StaticKeyword))
                    {
                        isStatic = true;
                    } else if (methodNodeOrToken.IsKind(SyntaxKind.IdentifierName))
                    {
                        // If method return type equals this classes' name
                        if (((IdentifierNameSyntax)methodNodeOrToken).Identifier.Text.Equals(classDec.Identifier.Text))
                        {
                            correctType = true;
                        }
                    }
                }

                if (isPublic && isStatic && correctType)
                {
                    // There can only and will always be one Block as a direct child of a MethodDeclaration
                    BlockSyntax methodBlock =
                        methodDec.ChildNodes().OfType<BlockSyntax>().First();

                    // Check all descendantNodes instead of just childNodes incase there are additional conditions for returning the value
                    List<SyntaxNode> blockNodes =
                        methodBlock.DescendantNodes().OfType<SyntaxNode>().ToList();

                    foreach (SyntaxNode blockNode in blockNodes)
                    {
                        if (blockNode.IsKind(SyntaxKind.ReturnStatement))
                        {
                            List<SyntaxNode> returnNodes =
                                blockNode.ChildNodes().OfType<SyntaxNode>().ToList();

                            foreach (SyntaxNode returnNode in returnNodes)
                            {
                                if (returnNode.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                                {
                                    MemberAccessExpressionSyntax memberAccess = (MemberAccessExpressionSyntax)returnNode;

                                    /* 
                                     * MemberAccessExpressionSyntax does not have "Identifier"-Property, so it must be cast to "IdentifierNameSyntax" 
                                     * in order to access the Property we need to compare. 
                                    */

                                    string expressionIdentifier = ((IdentifierNameSyntax)memberAccess.Expression).Identifier.Text;
                                    string memberName = ((IdentifierNameSyntax)memberAccess.Name).Identifier.Text;

                                    if (expressionIdentifier.Equals(identifier) && memberName.Equals("Value"))
                                    {
                                        classContainsValidMethod = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return classContainsValidMethod;
        }
    }
}
