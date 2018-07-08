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
		//The CheckMVVM Method which is called from the MainWindowControl.xaml.cs when the assigned button is pressed
		public void CheckMVVM(Project project, ObservableCollection<Document> modelDocuments, ObservableCollection<Document> viewDocuments, ObservableCollection<Document> viewmodelDocuments)
		{
			List<ClassDeclarationSyntax> classDeclarationsinModel = null;
			List<ClassDeclarationSyntax> classDeclarationsinView = null;

			List<ObjectCreationExpressionSyntax> idDeclarationsinModel = null;
			List<ObjectCreationExpressionSyntax> idDeclarationsinView = null;

			//create a list of tuples so the document can still be accesed later on
            List<Tuple<ObjectCreationExpressionSyntax, Document>> idDeclarationsModel = new List<Tuple<ObjectCreationExpressionSyntax, Document>>();
            List<Tuple<ObjectCreationExpressionSyntax, Document>> idDeclarationsView = new List<Tuple<ObjectCreationExpressionSyntax, Document>>();

			foreach (Document d in modelDocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;

				//get the classdeclaration of the model documents and a list of the objects that are created within it
				classDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().ToList();
				idDeclarationsinModel =
	                tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().ToList();

                foreach (ObjectCreationExpressionSyntax oces in idDeclarationsinModel)
                {
                    idDeclarationsModel.Add(new Tuple<ObjectCreationExpressionSyntax, Document>(oces, d));
                }
			}

			foreach (Document d in viewDocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;

				//get the classdeclaration of the view documents and a list of the objects that are created within it
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

		/*this method checks whether the view knows the model or the other way around. This will be the case
		 * if an object of the other is instantiated anywhere within it.
		*/
		public void ModelknowsViews(List<ClassDeclarationSyntax> classModellist, List<ClassDeclarationSyntax> classViewlist,
			List<Tuple<ObjectCreationExpressionSyntax, Document>> idModellist, List<Tuple<ObjectCreationExpressionSyntax, Document>> idViewlist)
		{
			classModellist.ForEach(delegate (ClassDeclarationSyntax cdsm)
			{
				string modelClassname = cdsm.Identifier.ToString();
				idViewlist.ForEach(delegate (Tuple<ObjectCreationExpressionSyntax, Document> tuple)
				{
					string createdObject = tuple.Item1.ToString();
					if (createdObject.Contains(modelClassname))
					{
                        ErrorReporter.AddWarning("Model accessing View!", tuple.Item2.FilePath);
					}
				});
			});

			classViewlist.ForEach(delegate (ClassDeclarationSyntax cdsv)
			{
				string viewClassname = cdsv.Identifier.ToString();
				idModellist.ForEach(delegate (Tuple<ObjectCreationExpressionSyntax, Document> tuple)
				{
					string createdObject = tuple.Item1.ToString();
					if (createdObject.Contains(viewClassname))
					{
                        ErrorReporter.AddWarning("View accessing model!", tuple.Item2.FilePath);
					}
				});
			});
		}

		/*this method checks if the code behind file is left empty. Every code behind file has a .xaml as its parent
		 * which is treatet like its folder.
		 */
		public void CheckCodeBehind(Project project)
		{
			foreach (var document in project.Documents)
			{
				if (document != null)
				{
					Task<SyntaxTree> t = document.GetSyntaxTreeAsync();
					SyntaxTree tree = t.Result;

					// get every method/variable and propertydeclaration of the current document
					List<MethodDeclarationSyntax> methodDeclarations =
						tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
					List<VariableDeclarationSyntax> variableDeclarations =
						tree.GetRoot().DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>().ToList();
					List<PropertyDeclarationSyntax> propertyDeclarations =
						tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>().ToList();

					List<string> folders = document.Folders.ToList();
					folders.ForEach(delegate (string folder)
					{
						//check if any declarations are in the code behind file
						if (folder.Contains(".xaml")&& (methodDeclarations.Any()||variableDeclarations.Any()||propertyDeclarations.Any()))
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
