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
using System.Text.RegularExpressions;

namespace Plugin_Entwicklung.Controller
{
    class NamingController
    {

        public void CheckNaming(Project project,List<string> permittedmethodstrings)
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
							methodnaming(list, permittedmethodstrings);
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

		public void methodnaming(List<MethodDeclarationSyntax> list, List<string> permittedmethodstrings)
		{
			list.ForEach(delegate (MethodDeclarationSyntax mds)
			{

				permittedmethodstrings.ForEach(delegate (string permittedstring)
				{
					string currentmethodname = mds.Identifier.ToString();
					string regexmatchstring = currentmethodname.Replace(permittedstring, "");
					if (!currentmethodname.Contains(permittedstring) && Regex.Matches(regexmatchstring, @"[a-zA-Z ]").Count == currentmethodname.Length)
					{
						
					}
					else
					{
						System.Diagnostics.Debug.WriteLine("Alarm: " + mds.Identifier);
					}
				});
				System.Diagnostics.Debug.WriteLine("Methodenname: " + mds.Identifier.ToString());
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
