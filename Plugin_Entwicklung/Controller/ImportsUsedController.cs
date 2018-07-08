using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin_Entwicklung.Controller
{
	class ImportsUsedController
	{
		//The CheckImports Method which is called from the MainWindowControl.xaml.cs when the assigned button is pressed
		public void CheckImports(List<Document> documents)
		{
			foreach (var document in documents)
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
					//part of this snippet is taken from a solution provided by Jon Skeet in his Stackoverflow post
					//https://stackoverflow.com/questions/44058243/how-can-i-detect-unused-imports-in-a-script-rather-than-a-document-with-roslyn
					var unusedImportNodes = compilation.GetDiagnostics()
						.Where(d => d.Id == "CS8019")
						.Where(d => d.Location?.SourceTree == tree)
						.Select(d => root.FindNode(d.Location.SourceSpan))
						.ToList();
					unusedImportNodes.ForEach(delegate (SyntaxNode nd)
					{
                        ErrorReporter.AddWarning("Unused import : " + nd.ToString(), document.FilePath);
					});
				}
			}
		}
	}
}
