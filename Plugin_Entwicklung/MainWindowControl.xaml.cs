namespace Plugin_Entwicklung
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell.Interop;
	using System;
	using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
            IComponentModel componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

            var projects = workspace.CurrentSolution.Projects;

            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "MainWindow");

			checklinelenght();

			foreach (var project in projects)
			{
				foreach (var document in project.Documents)
				{
					System.Diagnostics.Debug.WriteLine("Kappachino" + "\t\t\t" + document.Name);
					
				
				}
			}

		}

		private void checklinelenght()
		{
			IComponentModel componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
			var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

			var projects = workspace.CurrentSolution.Projects;
			foreach (var project in projects)
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
								if (line.ToString().Length > 60)
								{
									System.Diagnostics.Debug.WriteLine(document.Name+"Zu lang in Zeile" + line.LineNumber+1);
								}
							}
						}


					}
				}
			}

		}
    }
}