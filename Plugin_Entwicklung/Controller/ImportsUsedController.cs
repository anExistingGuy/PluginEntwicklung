using System;
using Microsoft.CodeAnalysis;

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
	class ImportsUsedController
	{

		public void CheckImports(Project project)
		{
			List<Document> listdoc = null;
			foreach (var document in project.Documents)
			{
				if (document != null)
				{
					Compilation compilation;
					Task<Compilation> t1 = document.Project.GetCompilationAsync();
					compilation = t1.Result;
					SyntaxTree tree;
					Task<SyntaxTree> t2 = document.GetSyntaxTreeAsync();
					tree = t2.Result;
					var root = tree.GetRoot();
					var unusedImportNodes = compilation.GetDiagnostics()
						.Where(d => d.Id == "CS8019")
						.Where(d => d.Location?.SourceTree == tree)
						.Select(d => root.FindNode(d.Location.SourceSpan))
						.ToList();
					unusedImportNodes.ForEach(delegate (SyntaxNode nd)
					{
						System.Diagnostics.Debug.WriteLine("Methodenname: " + nd.ToString());
					});
					System.Diagnostics.Debug.WriteLine("Hallo ImportsUsed"+ unusedImportNodes);
				//	listdoc.Add(document.WithSyntaxRoot(
				//		root.RemoveNodes(unusedImportNodes, SyntaxRemoveOptions.KeepNoTrivia)));
				}
			}
			//return listdoc;
		}
	}
}
