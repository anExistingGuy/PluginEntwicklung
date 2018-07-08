using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatterNicer.Controller
{
    class LineLengthController
    {
		/* The CheckLineLenght Method which is called from 
         * MainWindowControl.xaml.cs when the assigned button is pressed 
         */
		public void CheckLineLength(List<Document> documents, int numChars)
        {
            
            foreach (var document in documents)
            {
                if (document != null)
                {
					/* convert the document to a text 
                     * so every line can be accesed 
                     */
                    Task<SourceText> t = document.GetTextAsync();
                    SourceText doctext = t.Result;

                    if (doctext != null)
                    {
                        foreach (var line in doctext.Lines)
                        {
                            if (line.ToString().Length > numChars)
                            {
                                ErrorReporter.AddWarning(
                                    "Line " + (line.LineNumber + 1) + 
                                    " in Document " + document.Name + 
                                    " is too long!", 
                                    document.FilePath, line.LineNumber
                                    );
                            }
                        }
                    }
                    
                }
            }
        }
    }
}
