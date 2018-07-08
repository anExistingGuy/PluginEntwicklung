using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Plugin_Entwicklung.Controller
{
    class LineLengthController
    {
		//The CheckLineLenght Method which is called from the MainWindowControl.xaml.cs when the assigned button is pressed
		public void CheckLineLength(List<Document> documents, int numChars)
        {
            
            foreach (var document in documents)
            {
                if (document != null)
                {
					//convert the document to a text so every line can be accesed
                    SourceText doctext;
                    document.TryGetText(out doctext);
                    if (doctext != null)
                    {
                        foreach (var line in doctext.Lines)
                        {
                            if (line.ToString().Length > numChars)
                            {
                                ErrorReporter.AddWarning("Line " + (line.LineNumber + 1) + " in Document " + document.Name + " is too long!", document.FilePath, line.LineNumber);
                            }
                        }
                    }
                    
                }
            }
        }
    }
}
