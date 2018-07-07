using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Entwicklung.Controller
{
	class MVVMConntroller
	{
		public void CheckMVVM(Project project, ObservableCollection<Document> modeldocuments, ObservableCollection<Document> viewdocuments, ObservableCollection<Document> viewmodeldocuments)
		{
			List<MethodDeclarationSyntax> methodDeclarationsinModel = null;
			List<MethodDeclarationSyntax> methodDeclarationsinView = null;

			List<PropertyDeclarationSyntax> propertyDeclarationsinModel = null;
			List<PropertyDeclarationSyntax> propertyDeclarationsinView = null;

			foreach (Document d in modeldocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				methodDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
				propertyDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>().ToList();
			}

			foreach (Document d in viewdocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				methodDeclarationsinView =
					tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
				propertyDeclarationsinView =
	                tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>().ToList();
			}

			foreach (Document d in viewmodeldocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				List<MethodDeclarationSyntax> methodDeclarationsinViewModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
			}
			ModelknowsViews(methodDeclarationsinModel, methodDeclarationsinView, propertyDeclarationsinModel, propertyDeclarationsinView);
			CheckCodeBehind(project);
		}

		public void ModelknowsViews(List<MethodDeclarationSyntax> modellist, 
			List<MethodDeclarationSyntax> viewlist, List<PropertyDeclarationSyntax> propertyDeclarationsinModel
			, List<PropertyDeclarationSyntax> propertyDeclarationsinView)
		{
			viewlist.ForEach(delegate (MethodDeclarationSyntax mdsv)
			{
				string viewmethodname = mdsv.Identifier.ToString();
				modellist.ForEach(delegate (MethodDeclarationSyntax mdsm)
				{
					string modelmethodname = mdsv.Identifier.ToString();
					if (modelmethodname.Equals(viewmethodname))
					{

					}
				});
			});

			viewlist.ForEach(delegate (MethodDeclarationSyntax mdsv)
			{
				string viewmethodname = mdsv.Identifier.ToString();
				modellist.ForEach(delegate (MethodDeclarationSyntax mdsm)
				{
					string modelmethodname = mdsv.Identifier.ToString();
					if (modelmethodname.Equals(viewmethodname))
					{

					}
				});
			});
		}

		public void CheckCodeBehind(Project project)
		{
			foreach (var document in project.Documents)
			{
				if (document != null)
				{
					Task<SyntaxTree> t = document.GetSyntaxTreeAsync();
					SyntaxTree tree = t.Result;

					List<MethodDeclarationSyntax> methodDeclarations =
						tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
					List<VariableDeclarationSyntax> variableDeclarations =
						tree.GetRoot().DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>().ToList();
					List<PropertyDeclarationSyntax> propertyDeclarations =
						tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>().ToList();

					List<string> folders = document.Folders.ToList();
					folders.ForEach(delegate (string fldr)
					{
						if (fldr.Contains(".xaml")&& (methodDeclarations.Any()||variableDeclarations.Any()||propertyDeclarations.Any()))
						{
							System.Diagnostics.Debug.WriteLine("Codebehind sollte Leer sein");
						}

					});
				}
			}
		}
	}
}
