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
        public void CheckNaming(List<Document> documents, List<string> permittedMethods,
            List<string> permittedVariables, List<string> permittedProperties,
            bool isMethodChecked, bool isVariableChecked, bool isClassChecked, bool isPropertyChecked, bool isNamespaceChecked
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
                    if (isMethodChecked)
                    {
                        List<MethodDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
                        Methodnaming(list, permittedMethods, document, nameCaseMethod);
                    }
                    if (isVariableChecked)
                    {
                        List<VariableDeclaratorSyntax> list = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
                        Variablenaming(list, permittedVariables, document, nameCaseVariable);
                    }
                    if (isClassChecked)
                    {
                        List<ClassDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                        Classnaming(list, document);
                    }
                    if (isPropertyChecked)
                    {
                        List<PropertyDeclarationSyntax> list = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
                        Propertynaming(list, permittedProperties, document, nameCaseProperty);
                    }
                    if (isNamespaceChecked)
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
        public void Methodnaming(List<MethodDeclarationSyntax> list, List<string> permittedMethods,
			Document document, MainWindowControl.NameCase nameCaseMethod)
        {
            list.ForEach(delegate (MethodDeclarationSyntax mds)
            {
                string currentMethodName = mds.Identifier.ToString();
                string regexmatchstring = currentMethodName;
                //permitted special signs get treated like they are not part of the methodname 
                if (permittedMethods.Equals(""))
                {
					permittedMethods.ForEach(delegate (string permitted)
                    {
                        regexmatchstring = regexmatchstring.Replace(permitted, "");
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
        public void Variablenaming(List<VariableDeclaratorSyntax> list, List<string> permittedVariables, 
			Document document, MainWindowControl.NameCase nameCaseVariable)
        {
            list.ForEach(delegate (VariableDeclaratorSyntax vds)
            {
                string currentVariableName = vds.Identifier.ToString();
                string regexmatchstring = currentVariableName;
                //permitted special signs get treated like they are not part of the variablename 
                if (permittedVariables.Equals(""))
                {
					permittedVariables.ForEach(delegate (string permitted)
                    {
                        regexmatchstring = regexmatchstring.Replace(permitted, "");
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
                string currentClassName = cds.Identifier.ToString();
                //determin whether the class name only contains letters of the abc and if the first letter is in uppercase
                if (char.IsUpper(currentClassName[0]) && Regex.Matches(currentClassName, @"[a-zA-Z ]").Count == currentClassName.Length)
                {

                }
                else
                {
                    ErrorReporter.AddWarning("Flawed class name : " + cds.Identifier, document.FilePath);
                }
            });
        }

        //this method checks if the namingconventions for propertys set by the user are met for all documents in the project
        public void Propertynaming(List<PropertyDeclarationSyntax> list, List<string> permittedProperties,
			Document document, MainWindowControl.NameCase nameCaseProperty)
        {
            list.ForEach(delegate (PropertyDeclarationSyntax pds)
            {
                string currentVariableName = pds.Identifier.ToString();
                string regexmatchstring = currentVariableName;
                //permitted special signs get treated like they are not part of the propertyname
                if (permittedProperties.Equals(""))
                {
					permittedProperties.ForEach(delegate (string permitted)
                    {
                        regexmatchstring = regexmatchstring.Replace(permitted, "");
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
            String desiredNamespace = projekt;
            if (list.Count > 1)
            {
                System.Diagnostics.Debug.WriteLine("Nur 1 Namespace im Dokument ");
            }
            list.ForEach(delegate (NamespaceDeclarationSyntax nds)
            {
                folders.ForEach(delegate (string folder)
                {
					desiredNamespace += "." + folder;
                });

                if (!nds.Name.Equals(desiredNamespace))
                {
                    ErrorReporter.AddWarning("Wrong namespace : " + nds.Name + "! Should be : " + desiredNamespace + "!", document.FilePath);
                }
            });
        }
    }
}
