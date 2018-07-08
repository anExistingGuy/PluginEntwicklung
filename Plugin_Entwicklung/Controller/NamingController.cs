using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace Plugin_Entwicklung.Controller
{
    class NamingController
    {

        //The CheckNaming Method which is called from the MainWindowControl.xaml.cs when the assigned button is pressed
        public void CheckNaming(List<Document> documents, List<string> permittedmethodstrings,
            List<string> permittedvariablestrings, List<string> permittedpropertystrings,
            bool ismethodchecked, bool isvariablechecked, bool isclasschecked, bool ispropertychecked, bool isnamespacechecked
			,MainWindowControl.NameCase nameCaseMethod, MainWindowControl.NameCase nameCaseVariable, MainWindowControl.NameCase nameCaseProperty)
        {
            foreach (var document in documents)
            {
                if (document != null)
                {
                    //getting the Syntax Tree for the current document
                    SyntaxTree tree;
                    Task<SyntaxTree> t = document.GetSyntaxTreeAsync();
                    tree = t.Result;
                    if (ismethodchecked)
                    {
                        List<MethodDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
                        Methodnaming(list, permittedmethodstrings, document, nameCaseMethod);
                    }
                    if (isvariablechecked)
                    {
                        List<VariableDeclaratorSyntax> list = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
                        Variablenaming(list, permittedvariablestrings, document, nameCaseVariable);
                    }
                    if (isclasschecked)
                    {
                        List<ClassDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                        Classnaming(list, document);
                    }
                    if (ispropertychecked)
                    {
                        List<PropertyDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
                        Propertynaming(list, permittedpropertystrings, document, nameCaseProperty);
                    }
                    if (isnamespacechecked)
                    {
                        List<NamespaceDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
                        List<string> folders = document.Folders.ToList();
                        String projekt = document.Project.Name;
                        Namespacenaming(list, projekt, folders, document);
                    }
                }
            }
        }

        //this method checks if the namingconventions for methods set by the user are met for all documents in the project
        public void Methodnaming(List<MethodDeclarationSyntax> list, List<string> permittedmethodstrings,
			Document document, MainWindowControl.NameCase nameCaseMethod)
        {
            list.ForEach(delegate (MethodDeclarationSyntax mds)
            {
                string currentmethodname = mds.Identifier.ToString();
                string regexmatchstring = currentmethodname;
                //permitted special signs get treated like they are not part of the methodname 
                if (permittedmethodstrings.Equals(""))
                {
                    permittedmethodstrings.ForEach(delegate (string permittedstring)
                    {
                        regexmatchstring = regexmatchstring.Replace(permittedstring, "");
                    });
                }
                //determin whether the method name only contains letters of the abc
                if (Regex.Matches(regexmatchstring, @"[a-zA-Z ]").Count == regexmatchstring.Length)
                {
					//check if camelCase constraint is violatet
					if (char.IsUpper(regexmatchstring[0])&& nameCaseMethod.Equals(MainWindowControl.NameCase.camelCase))
					{
						ErrorReporter.AddWarning("Flawed method name : " + mds.Identifier, document.FilePath);
					}
					//check if PascalCase constraint is violatet
					if (char.IsLower(regexmatchstring[0]) && nameCaseMethod.Equals(MainWindowControl.NameCase.PascalCase))
					{
						ErrorReporter.AddWarning("Flawed method name : " + mds.Identifier, document.FilePath);
					}
				}
                else
                {
                    ErrorReporter.AddWarning("Flawed method name : " + mds.Identifier, document.FilePath);
                }
            });
        }

        //this method checks if the namingconventions for variables set by the user are met for all documents in the project
        public void Variablenaming(List<VariableDeclaratorSyntax> list, List<string> permittedvariablestrings, 
			Document document, MainWindowControl.NameCase nameCaseVariable)
        {
            list.ForEach(delegate (VariableDeclaratorSyntax vds)
            {
                string currentvariablename = vds.Identifier.ToString();
                string regexmatchstring = currentvariablename;
                //permitted special signs get treated like they are not part of the variablename 
                if (permittedvariablestrings.Equals(""))
                {
                    permittedvariablestrings.ForEach(delegate (string permittedstring)
                    {
                        regexmatchstring = regexmatchstring.Replace(permittedstring, "");
                    });
                }
                //determin whether the variable name only contains letters of the abc
                if (Regex.Matches(regexmatchstring, @"[a-zA-Z ]").Count == regexmatchstring.Length)
                {
					//check if camelCase constraint is violatet
					if (char.IsUpper(regexmatchstring[0]) && nameCaseVariable.Equals(MainWindowControl.NameCase.camelCase))
					{
						ErrorReporter.AddWarning("Flawed method name : " + vds.Identifier, document.FilePath);
					}
					//check if PascalCase constraint is violatet
					if (char.IsLower(regexmatchstring[0]) && nameCaseVariable.Equals(MainWindowControl.NameCase.PascalCase))
					{
						ErrorReporter.AddWarning("Flawed method name : " + vds.Identifier, document.FilePath);
					}
				}
                else
                {
                    ErrorReporter.AddWarning("Flawed variable name : " + vds.Identifier, document.FilePath);
                }
            });
        }

        //this method checks if the namingconventions for classes set by the user are met for all documents in the project
        public void Classnaming(List<ClassDeclarationSyntax> list, Document document)
        {
            list.ForEach(delegate (ClassDeclarationSyntax cds)
            {
                string currentclassname = cds.Identifier.ToString();
                //determin whether the class name only contains letters of the abc and if the first letter is in uppercase
                if (char.IsUpper(currentclassname[0]) && Regex.Matches(currentclassname, @"[a-zA-Z ]").Count == currentclassname.Length)
                {

                }
                else
                {
                    ErrorReporter.AddWarning("Flawed class name : " + cds.Identifier, document.FilePath);
                }
            });
        }

        //this method checks if the namingconventions for propertys set by the user are met for all documents in the project
        public void Propertynaming(List<PropertyDeclarationSyntax> list, List<string> permittedpropertystrings,
			Document document, MainWindowControl.NameCase nameCaseProperty)
        {
            list.ForEach(delegate (PropertyDeclarationSyntax pds)
            {
                string currentvariablename = pds.Identifier.ToString();
                string regexmatchstring = currentvariablename;
                //permitted special signs get treated like they are not part of the propertyname
                if (permittedpropertystrings.Equals(""))
                {
                    permittedpropertystrings.ForEach(delegate (string permittedstring)
                    {
                        regexmatchstring = regexmatchstring.Replace(permittedstring, "");
                    });
                }
                //determin whether the property name only contains letters of the abc
                if (Regex.Matches(regexmatchstring, @"[a-zA-Z ]").Count == regexmatchstring.Length)
                {
					//check if camelCase constraint is violatet
					if (char.IsUpper(regexmatchstring[0]) && nameCaseProperty.Equals(MainWindowControl.NameCase.camelCase))
					{
						ErrorReporter.AddWarning("Flawed method name : " + pds.Identifier, document.FilePath);
					}
					//check if PascalCase constraint is violatet
					if (char.IsLower(regexmatchstring[0]) && nameCaseProperty.Equals(MainWindowControl.NameCase.PascalCase))
					{
						ErrorReporter.AddWarning("Flawed method name : " + pds.Identifier, document.FilePath);
					}
				}
                else
                {
                    ErrorReporter.AddWarning("Flawed property name : " + pds.Identifier, document.FilePath);
                }
            });
        }

        //this method checks if the namingconventions for namespaces set by the user are met for all documents in the project
        public void Namespacenaming(List<NamespaceDeclarationSyntax> list, String projekt, List<string> folders, Document document)
        {
            String desirednamespace = projekt;
            if (list.Count > 1)
            {
                System.Diagnostics.Debug.WriteLine("Nur 1 Namespace im Dokument ");
            }
            list.ForEach(delegate (NamespaceDeclarationSyntax nds)
            {
                folders.ForEach(delegate (string fldr)
                {
                    desirednamespace += "." + fldr;
                });

                if (!nds.Name.Equals(desirednamespace))
                {
                    ErrorReporter.AddWarning("Wrong namespace : " + nds.Name + "! Should be : " + desirednamespace + "!", document.FilePath);
                }
            });
        }
    }
}
