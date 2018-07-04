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
                        //SyntaxTree tree = CSharpSyntaxTree.ParseText(doctext);
                        SyntaxTree tree;
                        Task<SyntaxTree> t = document.GetSyntaxTreeAsync();
                        tree = t.Result;
						if (true)
						{
							List<MethodDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
							methodnaming(list);
						}
						if (true)
						{
							List<ClassDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
							classnaming(list);
						}
						if (true)
						{
							List<VariableDeclaratorSyntax> list = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
							variablenaming(list);
						}
						if (true)
						{
							List<NamespaceDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
							namespacenaming(list);
						}
                }
            }
        }

		public void methodnaming(List<MethodDeclarationSyntax> list)
		{
			list.ForEach(delegate (MethodDeclarationSyntax mds)
			{
				System.Diagnostics.Debug.WriteLine("Methodenname: " + mds.Identifier);
			});
		}

		public void classnaming(List<ClassDeclarationSyntax> list)
		{
			list.ForEach(delegate (ClassDeclarationSyntax cds)
			{
				System.Diagnostics.Debug.WriteLine("Klassenname: " + cds.Identifier);
			});
		}

		public void variablenaming(List<VariableDeclaratorSyntax> list)
		{
			list.ForEach(delegate (VariableDeclaratorSyntax vds)
			{
				System.Diagnostics.Debug.WriteLine("Variablenname: " + vds.Identifier);
			});
		}

		public void namespacenaming(List<NamespaceDeclarationSyntax> list)
		{
			list.ForEach(delegate (NamespaceDeclarationSyntax nds)
			{
				System.Diagnostics.Debug.WriteLine("Namespacename: " + nds.Name);
			});
		}
	}
}
