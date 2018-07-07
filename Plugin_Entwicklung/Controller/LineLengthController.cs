using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Entwicklung.Controller
{
    class LineLengthController
    {
		//The CheckLineLenght Method which is called from the MainWindowControl.xaml.cs when the assigned button is pressed
		public void CheckLineLength(Project project, int numChars)
        {
            
            foreach (var document in project.Documents)
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
                                System.Diagnostics.Debug.WriteLine(document.Name + "Zu lang in Zeile" + line.LineNumber + 1);
                            }
                        }
                    }
                    
                }
            }
        }
    }
}
