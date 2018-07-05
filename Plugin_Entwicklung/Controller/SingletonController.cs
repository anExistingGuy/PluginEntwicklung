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
                List<ConstructorDeclarationSyntax> constructorDeclarations = 
                    tree.GetRoot().DescendantNodesAndSelf().OfType<ConstructorDeclarationSyntax>().ToList();

                bool isSingleton = true;
                foreach (ConstructorDeclarationSyntax cds in constructorDeclarations)
                {
                    List<SyntaxToken> tokens = cds.ChildTokens().ToList();
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
                if (!isSingleton)
                {
                    System.Diagnostics.Debug.WriteLine("Class contained in " + d.Name + " is not a Singleton!");
                } else
                {
                    System.Diagnostics.Debug.WriteLine("Class contained in " + d.Name + " is Singleton!");
                }
            }
        }
    }
}
