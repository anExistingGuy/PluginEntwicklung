using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Plugin_Entwicklung.Controller
{
    class NamingController
    {
        public void CheckNaming(Project project)
        {
            foreach (var document in project.Documents)
            {
                if (document != null)
                {
                    SourceText doctext;
                    document.TryGetText(out doctext);
                    if (doctext != null)
                    {
                        //SyntaxTree tree = CSharpSyntaxTree.ParseText(doctext);
                        SyntaxTree tree;
                        Task<SyntaxTree> t = document.GetSyntaxTreeAsync();
                        tree = t.Result;
                        List<MethodDeclarationSyntax>list = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
                        list.ForEach(delegate (MethodDeclarationSyntax mds) 
                        {
                             System.Diagnostics.Debug.WriteLine("Methodenname: " +mds.Identifier);
                        });
                    }
                }
            }
        }
    }
}
