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
	class MVVMConntroller
	{
		public void CheckMVVM(ObservableCollection<Document> modeldocuments, ObservableCollection<Document> viewdocuments, ObservableCollection<Document> viewmodeldocuments)
		{
			foreach (Document d in modeldocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				List<MethodDeclarationSyntax> methodDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
			}

			foreach (Document d in viewdocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				List<MethodDeclarationSyntax> methodDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
			}

			foreach (Document d in viewmodeldocuments)
			{
				Task<SyntaxTree> t = d.GetSyntaxTreeAsync();
				SyntaxTree tree = t.Result;
				List<MethodDeclarationSyntax> methodDeclarationsinModel =
					tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().ToList();
			}
		}
	}
}
