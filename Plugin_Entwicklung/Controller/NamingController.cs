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

        public void CheckNaming(Project project,List<string> permittedmethodstrings,
			List<string> permittedvariablestrings, List<string> permittedpropertystrings,
			bool ismethodchecked, bool isvariablechecked, bool isclasschecked, bool ispropertychecked, bool isnamespacechecked)
        {
            foreach (var document in project.Documents)
            {
                if (document != null)
                {
                        SyntaxTree tree;
                        Task<SyntaxTree> t = document.GetSyntaxTreeAsync();
                        tree = t.Result;
						if (ismethodchecked)
						{
							List<MethodDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
							Methodnaming(list, permittedmethodstrings);
						}
					    if (isvariablechecked)
					    {
						List<VariableDeclaratorSyntax> list = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
						Variablenaming(list, permittedvariablestrings);
					    }
					    if (isclasschecked)
						{
							List<ClassDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
							Classnaming(list);
						}
					    if (ispropertychecked)
					    {
						    List<PropertyDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
						    Propertynaming(list, permittedpropertystrings);
					    }
				     	if (isnamespacechecked)
						{
							List<NamespaceDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
						    List<string> folders = document.Folders.ToList();
						    String projekt = document.Project.Name;
						    Namespacenaming(list, projekt, folders);
						}
                }
            }
        }

		public void Methodnaming(List<MethodDeclarationSyntax> list, List<string> permittedmethodstrings)
		{
			list.ForEach(delegate (MethodDeclarationSyntax mds)
			{
				string currentmethodname = mds.Identifier.ToString();
				string regexmatchstring = currentmethodname;
				System.Diagnostics.Debug.WriteLine("Counter: " + permittedmethodstrings.Count());
				if (permittedmethodstrings.Equals(""))
				{
					permittedmethodstrings.ForEach(delegate (string permittedstring)
					{
						regexmatchstring = regexmatchstring.Replace(permittedstring, "");
					});
				}
				if (Regex.Matches(regexmatchstring, @"[a-zA-Z ]").Count == regexmatchstring.Length)
				{

				}
				else
				{
					System.Diagnostics.Debug.WriteLine("Fehlerhafter Methodenname: " + mds.Identifier);
				}
			});
		}

		public void Variablenaming(List<VariableDeclaratorSyntax> list, List<string> permittedvariablestrings)
		{
			list.ForEach(delegate (VariableDeclaratorSyntax vds)
			{
				string currentvariablename = vds.Identifier.ToString();
				string regexmatchstring = currentvariablename;
				if (permittedvariablestrings.Equals(""))
				{
					permittedvariablestrings.ForEach(delegate (string permittedstring)
					{
						regexmatchstring = regexmatchstring.Replace(permittedstring, "");
					});
				}
						if (Regex.Matches(regexmatchstring, @"[a-zA-Z ]").Count == regexmatchstring.Length)
						{

						}
						else
						{
							System.Diagnostics.Debug.WriteLine("Fehlerhafter Variablenname: " + vds.Identifier);
						}
			});
		}

		public void Classnaming(List<ClassDeclarationSyntax> list)
		{
			list.ForEach(delegate (ClassDeclarationSyntax cds)
			{
					string currentclassname = cds.Identifier.ToString();
					if (char.IsUpper(currentclassname[0]) && Regex.Matches(currentclassname, @"[a-zA-Z ]").Count == currentclassname.Length)
					{

					}
					else
					{
					System.Diagnostics.Debug.WriteLine("Klassennamefehler: " + cds.Identifier);
				}
			});
		}

		public void Propertynaming(List<PropertyDeclarationSyntax> list, List<string> permittedpropertystrings)
		{
			list.ForEach(delegate (PropertyDeclarationSyntax vds)
			{
				string currentvariablename = vds.Identifier.ToString();
				string regexmatchstring = currentvariablename;
				if (permittedpropertystrings.Equals(""))
				{
					permittedpropertystrings.ForEach(delegate (string permittedstring)
					{
						regexmatchstring = regexmatchstring.Replace(permittedstring, "");
					});
				}
						if (Regex.Matches(regexmatchstring, @"[a-zA-Z ]").Count == regexmatchstring.Length)
						{

						}
						else
						{
							System.Diagnostics.Debug.WriteLine("Fehlerhafter Propertyname: " + vds.Identifier);
						}
			});
		}

		public void Namespacenaming(List<NamespaceDeclarationSyntax> list, String projekt, List<string> folders)
		{
			String desirednamespace=projekt;
			if (list.Count > 1) {
				System.Diagnostics.Debug.WriteLine("Nur 1 Namespace im Dokument ");
			}
			list.ForEach(delegate (NamespaceDeclarationSyntax nds)
			{
				folders.ForEach(delegate (string fldr)
				{
					desirednamespace += "." + fldr;
				});

				System.Diagnostics.Debug.WriteLine("Namespacename: " + nds.Name + "Desirednamespace: "+ desirednamespace);
			});
		}
	}
}
