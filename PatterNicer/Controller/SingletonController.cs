using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatterNicer.Controller
{
    class SingletonController
    {
        public void CheckSingleton(
            Project project, 
            ObservableCollection<Document> documents,
            bool checkThreadSafety)
        {
            foreach (Document d in documents)
            {
                // Get SyntaxTree for Document
                Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
                SyntaxTree tree = t.Result;

                // Get List of class declarations from SyntaxTree
                List<ClassDeclarationSyntax> classDeclarations =
                    tree.GetRoot().DescendantNodes().
                    OfType<ClassDeclarationSyntax>().ToList();
                
                foreach (ClassDeclarationSyntax classDec in classDeclarations)
                {
                    // Get List of constructor declarations in class
                    List<ConstructorDeclarationSyntax> 
                        constructorDeclarations =
                        classDec.DescendantNodes().
                        OfType<ConstructorDeclarationSyntax>().ToList();

                    string className = classDec.Identifier.Text;
                    bool isSingleton = true;

                    /* Check all class constructors for 
                     * whether or not they are private 
                     */
                    if (!constructorDeclarations.Any())
                    {
                        isSingleton = false;
                    } else
                    {
                        foreach (
                            ConstructorDeclarationSyntax constDec in 
                            constructorDeclarations)
                        {
                            List<SyntaxToken> tokens = 
                                constDec.ChildTokens().ToList();

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
                    }
                    
                    if (!isSingleton)
                    {
                        int line = classDec.GetLocation().GetMappedLineSpan().
                            StartLinePosition.Line;
                        ErrorReporter.AddWarning(
                            "Class " + className + 
                            " contained in document " + d.Name + 
                            " is not singleton but it should be!", 
                            d.FilePath, 
                            line);
                    }

                    /* Consider the class Singleton 
                     * as long as all constructors are private
                     */
                    if (isSingleton && checkThreadSafety)
                    {
                        /* With information from 
                         * http://csharpindepth.com/Articles/General/Singleton.aspx 
                         */
                        
                        /* Check whether Singleton is thread safe 
                         * by way of double check locking 
                         */
                        if (IsDoubleCheckLocked(classDec))
                        {
                        }

                        /* If it is not, check if it is thread safe 
                         * through use of .NET's Lazy<T>-class 
                         */
                        else if (IsLazy(classDec))
                        {
                        } 

                        /* If singleton is neither double check locked 
                         * nor Lazy-instantiated, add warning
                         * */
                        else
                        {
                            int line = classDec.GetLocation().
                                GetMappedLineSpan().StartLinePosition.Line;

                            ErrorReporter.AddWarning(
                                "Class " + className + 
                                " contained in document " + d.Name + 
                                " is neither double check locked nor lazy! \n"+
                                "Consider adding either one for thread safety."
                                ,d.FilePath, 
                                line);
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        /* Method that checks whether or not Singleton class is thread safe 
         * by way of double check locking 
         */
        public bool IsDoubleCheckLocked(ClassDeclarationSyntax classDec)
        {
            /* List of methods that might be used to 
             * return instances of singleton class 
             */
            List<MethodDeclarationSyntax> possibleInstanceMethods = 
                new List<MethodDeclarationSyntax>();

            // Get List of all methodDeclarations in class
            List<MethodDeclarationSyntax> methodDeclarations =
                classDec.DescendantNodes().
                OfType<MethodDeclarationSyntax>().ToList();

            bool classContainsValidMethod = false;

            foreach (MethodDeclarationSyntax methodDec in methodDeclarations)
            {
                bool isPublic = false;
                bool isStatic = false;

                /* Check if method returns an instance of class first, 
                 * otherwise don't do more taxing operations 
                 */
                if (methodDec.ReturnType.ToString().
                    Equals(classDec.Identifier.Text))
                {
                    /* Check whether or not method is public and static, 
                     * both of which instance method should be 
                     */

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

                /* isPublic and isStatic can only have been changed to true 
                 * if returnType is correct. If conditions are fulfilled, 
                 * add checked method to list of possible instance methods
                 */
                if (isPublic && isStatic)
                {
                    possibleInstanceMethods.Add(methodDec);
                }
            }

            foreach (
                MethodDeclarationSyntax methodDec in 
                possibleInstanceMethods)
            {
                /* Can choose .First() because method can 
                 * syntactically only have one Block 
                 */
                BlockSyntax block = methodDec.ChildNodes().
                    OfType<BlockSyntax>().First();

                string identifier = "";
                bool containsFirstCheck = false;
                List<SyntaxNode> nodes =
                    block.ChildNodes().OfType<SyntaxNode>().ToList();

                BlockSyntax secondBlock = null;
                foreach (SyntaxNode node in nodes)
                {
                    /* Check whether or not the block contains an if-stmt 
                     * that checks the instance variable for null 
                     */
                    if (node.IsKind(SyntaxKind.IfStatement))
                    {
                        List<SyntaxNodeOrToken> subNodes =
                            node.ChildNodesAndTokens().
                            OfType<SyntaxNodeOrToken>().ToList();

                        bool isEqualsEquals = false;
                        bool checksForNull = false;

                        foreach (SyntaxNodeOrToken subNode in subNodes)
                        {
                            if (subNode.IsKind(SyntaxKind.EqualsExpression))
                            {
                                List<SyntaxNodeOrToken> subSubNodes =
                                    subNode.ChildNodesAndTokens().
                                    OfType<SyntaxNodeOrToken>().ToList();

                                foreach (
                                    SyntaxNodeOrToken subSubNode in 
                                    subSubNodes)
                                {
                                    if (subSubNode.IsKind(
                                        SyntaxKind.IdentifierName))
                                    {
                                        identifier = subSubNode.ToString();
                                    }
                                    else if (subSubNode.IsKind(
                                        SyntaxKind.EqualsEqualsToken))
                                    {
                                        isEqualsEquals = true;
                                    }
                                    else if (subSubNode.IsKind(
                                        SyntaxKind.NullLiteralExpression))
                                    {
                                        checksForNull = true;
                                    }
                                }
                            }

                        }

                        if (isEqualsEquals && checksForNull)
                        {
                            containsFirstCheck = true;
                            secondBlock = node.ChildNodes().
                                OfType<BlockSyntax>().First();
                        }
                    }
                }

                bool containsLock = false;
                BlockSyntax thirdBlock = null;

                /* If instance variable is checked for null first, 
                 * check if after that check, a lock occurs 
                 */
                if (containsFirstCheck)
                {
                    List<SyntaxNode> subNodes =
                        secondBlock.ChildNodes().OfType<SyntaxNode>().ToList();

                    foreach (SyntaxNode subNode in subNodes)
                    {
                        if (subNode.IsKind(SyntaxKind.LockStatement))
                        {
                            containsLock = true;
                            thirdBlock = subNode.ChildNodes().
                                OfType<BlockSyntax>().First();
                        }
                    }
                }

                bool containsSecondCheck = false;
                BlockSyntax fourthBlock = null;
                /* If the lock occurs, check if another 
                 * null-check on instance variable happens 
                 */
                if (containsLock)
                {
                    List<SyntaxNode> subNodes =
                        thirdBlock.ChildNodes().
                        OfType<SyntaxNode>().ToList();

                    foreach (SyntaxNode subNode in subNodes)
                    {
                        if (subNode.IsKind(
                            SyntaxKind.IfStatement))
                        {
                            List<SyntaxNode> subSubNodes =
                                subNode.ChildNodes().
                                OfType<SyntaxNode>().ToList();

                            foreach (SyntaxNode subSubNode in subSubNodes)
                            {
                                if (subSubNode.IsKind(
                                    SyntaxKind.EqualsExpression))
                                {
                                    List<SyntaxNodeOrToken> subSubSubNodes =
                                        subSubNode.ChildNodesAndTokens().
                                        OfType<SyntaxNodeOrToken>().ToList();


                                    bool correctIdentifier = false;
                                    bool isEqualsEquals = false;
                                    bool checksForNull = false;

                                    foreach (
                                        SyntaxNodeOrToken subSubSubNode in 
                                        subSubSubNodes)
                                    {
                                        if (subSubSubNode.IsKind(
                                            SyntaxKind.IdentifierName) &&
                                            subSubSubNode.ToString() == 
                                            identifier)
                                        {
                                            correctIdentifier = true;
                                        }
                                        else if (subSubSubNode.IsKind(
                                            SyntaxKind.EqualsEqualsToken))
                                        {
                                            isEqualsEquals = true;
                                        }
                                        else if (subSubSubNode.IsKind(
                                            SyntaxKind.NullLiteralExpression))
                                        {
                                            checksForNull = true;
                                        }
                                    }

                                    if (correctIdentifier && 
                                        isEqualsEquals && 
                                        checksForNull)
                                    {
                                        containsSecondCheck = true;
                                        fourthBlock = subNode.ChildNodes().
                                            OfType<BlockSyntax>().First();
                                    }
                                }
                            }
                        }
                    }
                }

                bool isValidDoubleCheck = false;

                /* If second null-check happens, finally check whether or not
                 * after that second null-check, a value is assigned to
                 * the instance variable of type {ClassName} 
                 * in which case it can be determined that a valid
                 * double check lock has occured
                 */
                if (containsSecondCheck)
                {
                    List<SyntaxNode> subNodes =
                        fourthBlock.ChildNodes().
                        OfType<SyntaxNode>().ToList();

                    foreach (SyntaxNode subNode in subNodes)
                    {
                        if (subNode.IsKind(SyntaxKind.ExpressionStatement))
                        {
                            List<SyntaxNode> subSubNodes =
                                subNode.ChildNodes().
                                OfType<SyntaxNode>().ToList();

                            foreach (SyntaxNode subSubNode in subSubNodes)
                            {
                                if (subSubNode.IsKind
                                    (SyntaxKind.SimpleAssignmentExpression))
                                {
                                    /* SimpleAssignmentExpression can 
                                     * only have an identifier name 
                                     */
                                    if (subSubNode.ChildNodes().
                                        OfType<IdentifierNameSyntax>().
                                        First().Identifier.Text == identifier)
                                    {
                                        /* Only one ObjectCreationExpression 
                                         * can be attached to a 
                                         * SimpleAssignmentExpression 
                                         */
                                        ObjectCreationExpressionSyntax
                                            objectCreationExp =
                                            subSubNode.ChildNodes().
                                            OfType<ObjectCreationExpressionSyntax>().
                                            First();

                                        /* ObjectCreationExpression can only 
                                         * have one IdentifierName 
                                         * attached to it 
                                         */
                                        if (objectCreationExp.ChildNodes().
                                            OfType<IdentifierNameSyntax>().
                                            First().Identifier.Text == 
                                            classDec.Identifier.Text)
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

        /* Method that checks whether or not 
         * Singleton class is thread safe 
         * by way of Lazy instantiation 
         */
        public bool IsLazy(ClassDeclarationSyntax classDec)
        {
            string identifier = "";
            bool classContainsValidMethod = false;

            // Get List of FieldDeclarations on class-level
            List<FieldDeclarationSyntax> classFieldDeclarations =
                classDec.ChildNodes().
                OfType<FieldDeclarationSyntax>().ToList();

            /* Check whether or not field is private, static and readonly 
             * which for proper Lazy-Instantiation it needs to be 
             */
            foreach (
                FieldDeclarationSyntax classFieldDec in 
                classFieldDeclarations)
            {
                List<SyntaxNodeOrToken> nodesAndTokens =
                    classFieldDec.ChildNodesAndTokens().
                    OfType<SyntaxNodeOrToken>().ToList();

                bool isPrivate = false;
                bool isStatic = false;
                bool isReadOnly = false;

                bool fieldIsValid = false;

                foreach (SyntaxNodeOrToken nodeOrToken in nodesAndTokens)
                {
                    if (nodeOrToken.IsKind(
                        SyntaxKind.PrivateKeyword))
                    {
                        isPrivate = true;
                    } else if (nodeOrToken.IsKind(
                        SyntaxKind.StaticKeyword))
                    {
                        isStatic = true;
                    } else if (nodeOrToken.IsKind(
                        SyntaxKind.ReadOnlyKeyword))
                    {
                        isReadOnly = true;
                    } else if (nodeOrToken.IsKind(
                        SyntaxKind.VariableDeclaration))
                    {
                        SyntaxNode varDec = nodeOrToken.AsNode();

                        // Get list of Child Nodes and Tokens of variable
                        List<SyntaxNodeOrToken> varNodesAndTokens =
                            varDec.ChildNodesAndTokens().
                            OfType<SyntaxNodeOrToken>().ToList();

                        string varIdentifier = "";
                        string typeOfGeneric = "";
                        bool isDeclaredLazy = false;

                        /* Check whether or not Field is 
                         * declared with GenericType Lazy 
                         */
                        foreach (
                            SyntaxNodeOrToken varNodeOrToken in 
                            varNodesAndTokens)
                        {
                            if (varNodeOrToken.IsKind(SyntaxKind.GenericName))
                            {
                                if (((GenericNameSyntax)varNodeOrToken).
                                    Identifier.Text.Equals("Lazy"))
                                {
                                    isDeclaredLazy = true;

                                    List<SyntaxNodeOrToken> 
                                        genericNodesAndTokens =
                                        varNodeOrToken.ChildNodesAndTokens().
                                        OfType<SyntaxNodeOrToken>().ToList();

                                    foreach (
                                        SyntaxNodeOrToken genericNodeOrToken in 
                                        genericNodesAndTokens)
                                    {
                                        if (genericNodeOrToken.IsKind(
                                            SyntaxKind.TypeArgumentList))
                                        {
                                            /* Lazy<T> can only take one 
                                             * argument and must be supplied 
                                             * one argument so there will 
                                             * always be only and 
                                             * exactly one argument. 
                                             */
                                            typeOfGeneric = 
                                                ((TypeArgumentListSyntax)
                                                genericNodeOrToken)
                                                .Arguments.First().ToString();
                                        }
                                    }

                                }
                            } else if (varNodeOrToken.IsKind(
                                SyntaxKind.VariableDeclarator))
                            {
                                varIdentifier = 
                                    ((VariableDeclaratorSyntax)
                                    varNodeOrToken).Identifier.Text;
                            }
                        }

                        /* If variable is declared as with GenericType Lazy 
                         * and is private, static and readonly,
                         * it can be considered the instance variable of 
                         * the singleton class, so save it's identifiername
                         */
                        if (isDeclaredLazy && 
                            isPrivate && 
                            isStatic && 
                            isReadOnly)
                        {
                            if (!varIdentifier.Equals("") && 
                                !typeOfGeneric.Equals(""))
                            {
                                identifier = varIdentifier;
                            }
                        }
                    }
                    
                }
            }

            // Get list of method declarations in class
            List<MethodDeclarationSyntax> methodDeclarations =
                classDec.ChildNodes().
                OfType<MethodDeclarationSyntax>().ToList();

            foreach (MethodDeclarationSyntax methodDec in methodDeclarations)
            {
                List<SyntaxNodeOrToken> methodNodesAndTokens =
                    methodDec.ChildNodesAndTokens().
                    OfType<SyntaxNodeOrToken>().ToList();

                bool isPublic = false;
                bool isStatic = false;
                bool correctType = false;

                /* Check whether method is public, static 
                 * and has this class as return type 
                 */
                foreach (
                    SyntaxNodeOrToken methodNodeOrToken in 
                    methodNodesAndTokens)
                {
                    if (methodNodeOrToken.IsKind(
                        SyntaxKind.PublicKeyword))
                    {
                        isPublic = true;
                    } else if (methodNodeOrToken.IsKind(
                        SyntaxKind.StaticKeyword))
                    {
                        isStatic = true;
                    } else if (methodNodeOrToken.IsKind(
                        SyntaxKind.IdentifierName))
                    {
                        // If method return type equals this classes' name
                        if (((IdentifierNameSyntax)methodNodeOrToken).
                            Identifier.Text.Equals(classDec.Identifier.Text))
                        {
                            correctType = true;
                        }
                    }
                }

                if (isPublic && isStatic && correctType)
                {
                    /* There can only and will always be one Block 
                     * as a direct child of a MethodDeclaration 
                     */
                    BlockSyntax methodBlock =
                        methodDec.ChildNodes().
                        OfType<BlockSyntax>().First();

                    /* Check all descendantNodes instead of just childNodes 
                     * incase there are additional conditions 
                     * for returning the value 
                     */
                    List<SyntaxNode> blockNodes =
                        methodBlock.DescendantNodes().
                        OfType<SyntaxNode>().ToList();

                    /* If the method is public, static and has the 
                     * correct return type, check if method contains 
                     * a return-stmt that returns the value of
                     * the Lazy instance-variable whose 
                     * identifier name we safed earlier
                     */
                    foreach (SyntaxNode blockNode in blockNodes)
                    {
                        if (blockNode.IsKind(
                            SyntaxKind.ReturnStatement))
                        {
                            List<SyntaxNode> returnNodes =
                                blockNode.ChildNodes().
                                OfType<SyntaxNode>().ToList();

                            foreach (SyntaxNode returnNode in returnNodes)
                            {
                                if (returnNode.IsKind(
                                    SyntaxKind.SimpleMemberAccessExpression))
                                {
                                    MemberAccessExpressionSyntax memberAccess = 
                                        (MemberAccessExpressionSyntax)
                                        returnNode;

                                    /* 
                                     * MemberAccessExpressionSyntax does not 
                                     * have "Identifier"-Property, so it 
                                     * must be cast to "IdentifierNameSyntax" 
                                     * in order to access the Property 
                                     * we need to compare. 
                                    */

                                    string expressionIdentifier = 
                                        ((IdentifierNameSyntax
                                        )memberAccess.Expression).
                                        Identifier.Text;

                                    string memberName = 
                                        ((IdentifierNameSyntax)
                                        memberAccess.Name).
                                        Identifier.Text;

                                    /* If return-stmt looks like 
                                     * return {identifier}.Value, 
                                     * assume that this method is a valid 
                                     * instance-method returning the 
                                     * lazy instance variable, 
                                     * thus making the singleton threadsafe 
                                     * by way of Lazy-Instantiation
                                     */
                                    if (expressionIdentifier.Equals(identifier) 
                                        && memberName.Equals("Value"))
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
