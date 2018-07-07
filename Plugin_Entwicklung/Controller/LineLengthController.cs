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
        // public void CheckLineLength(VisualStudioWorkspace workspace)
        public void CheckLineLength(Project project, int numChars)
        {
            
            foreach (var document in project.Documents)
            {
                if (document != null)
                {
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
