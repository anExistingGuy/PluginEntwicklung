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
			List<ClassDeclarationSyntax> classDeclarationsinModel = null;
			List<ClassDeclarationSyntax> classDeclarationsinView = null;

			List<ObjectCreationExpressionSyntax> idDeclarationsinModel = null;
			List<ObjectCreationExpressionSyntax> idDeclarationsinView = null;

			foreach (Document d in modeldocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				classDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().ToList();
				idDeclarationsinModel =
	                tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().ToList();
			}

			foreach (Document d in viewdocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				classDeclarationsinView =
					tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().ToList();
				idDeclarationsinView =
					tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().ToList();
			}

			ModelknowsViews(classDeclarationsinModel, classDeclarationsinView, idDeclarationsinModel, idDeclarationsinView);
			CheckCodeBehind(project);
		}

		public void ModelknowsViews(List<ClassDeclarationSyntax> classmodellist, List<ClassDeclarationSyntax> classviewlist,
			List<ObjectCreationExpressionSyntax> idmodellist, List<ObjectCreationExpressionSyntax> idviewlist)
		{
			classmodellist.ForEach(delegate (ClassDeclarationSyntax cdsm)
			{
				string modelclassname = cdsm.Identifier.ToString();
				idviewlist.ForEach(delegate (ObjectCreationExpressionSyntax oces)
				{
					string createdobject = oces.ToString();
					if (createdobject.Contains(modelclassname))
					{
						System.Diagnostics.Debug.WriteLine("Model sollte das View nicht kennen");
					}
				});
			});

			classviewlist.ForEach(delegate (ClassDeclarationSyntax cdsv)
			{
				string viewclassname = cdsv.Identifier.ToString();
				idmodellist.ForEach(delegate (ObjectCreationExpressionSyntax oces)
				{
					string createdobject = oces.ToString();
					if (createdobject.Contains(viewclassname))
					{
						System.Diagnostics.Debug.WriteLine("View sollte das Model nicht kennen");
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
