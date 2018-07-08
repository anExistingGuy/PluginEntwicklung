using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            List<Tuple<ObjectCreationExpressionSyntax, Document>> idDeclarationsModel = new List<Tuple<ObjectCreationExpressionSyntax, Document>>();
            List<Tuple<ObjectCreationExpressionSyntax, Document>> idDeclarationsView = new List<Tuple<ObjectCreationExpressionSyntax, Document>>();

			foreach (Document d in modeldocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;


				classDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().ToList();
				idDeclarationsinModel =
	                tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().ToList();

                foreach (ObjectCreationExpressionSyntax oces in idDeclarationsinModel)
                {
                    idDeclarationsModel.Add(new Tuple<ObjectCreationExpressionSyntax, Document>(oces, d));
                }
			}

			foreach (Document d in viewdocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;

				classDeclarationsinView =
					tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().ToList();
				idDeclarationsinView =
					tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().ToList();

                foreach (ObjectCreationExpressionSyntax oces in idDeclarationsinView)
                {
                    idDeclarationsView.Add(new Tuple<ObjectCreationExpressionSyntax, Document>(oces, d));
                }
			}

			ModelknowsViews(classDeclarationsinModel, classDeclarationsinView, idDeclarationsModel, idDeclarationsView);
			CheckCodeBehind(project);
		}

		public void ModelknowsViews(List<ClassDeclarationSyntax> classmodellist, List<ClassDeclarationSyntax> classviewlist,
			List<Tuple<ObjectCreationExpressionSyntax, Document>> idmodellist, List<Tuple<ObjectCreationExpressionSyntax, Document>> idviewlist)
		{
			classmodellist.ForEach(delegate (ClassDeclarationSyntax cdsm)
			{
				string modelclassname = cdsm.Identifier.ToString();
				idviewlist.ForEach(delegate (Tuple<ObjectCreationExpressionSyntax, Document> tuple)
				{
					string createdobject = tuple.Item1.ToString();
					if (createdobject.Contains(modelclassname))
					{
                        ErrorReporter.AddWarning("Model accessing View!", tuple.Item2.FilePath);
					}
				});
			});

			classviewlist.ForEach(delegate (ClassDeclarationSyntax cdsv)
			{
				string viewclassname = cdsv.Identifier.ToString();
				idmodellist.ForEach(delegate (Tuple<ObjectCreationExpressionSyntax, Document> tuple)
				{
					string createdobject = tuple.Item1.ToString();
					if (createdobject.Contains(viewclassname))
					{
                        ErrorReporter.AddWarning("View accessing model!", tuple.Item2.FilePath);
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
                            ErrorReporter.AddWarning("Codebehind is not empty!", document.FilePath);
						}

					});
				}
			}
		}
	}
}
