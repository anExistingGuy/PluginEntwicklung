namespace Plugin_Entwicklung
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.Text;
	using Microsoft.VisualStudio.ComponentModelHost;
	using Microsoft.VisualStudio.LanguageServices;
	using Microsoft.VisualStudio.OLE.Interop;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for MainWindowControl.
	/// </summary>
	public partial class MainWindowControl : UserControl
	{
		// List<Project> Projects { get; set; }
		// public Package Package { get; set; }

		private Project selectedProject;
		public ObservableCollection<Project> Projects { get; set; }
		public Project SelectedProject
		{
			get { return selectedProject; }
			set
			{
				selectedProject = value;

				DocumentsUnassigned.Clear();
				DocumentsModel.Clear();
				DocumentsView.Clear();
				DocumentsViewModel.Clear();
				DocumentsSingleton.Clear();
				DocumentsNonSingleton.Clear();

				if (selectedProject != null)
				{
					List<Document> documentsFiltered = new List<Document>();

					foreach (Document d in selectedProject.Documents)
					{
						if (
							d.Name.EndsWith(".cs")
							&& !d.Name.EndsWith(".g.cs")
							&& !d.Name.EndsWith(".Designer.cs")
							&& !d.Name.EndsWith(".AssemblyAttributes.cs")
							&& !d.Name.StartsWith("TemporaryGeneratedFile_")
							&& !d.Name.Equals("AssemblyInfo.cs")
							)
						{
							documentsFiltered.Add(d);
						}
					}

					foreach (Document d in documentsFiltered)
					{
						DocumentsUnassigned.Add(d);
						DocumentsNonSingleton.Add(d);
					}
				}
			}
		}
		public ObservableCollection<Document> DocumentsUnassigned { get; set; }
		public ObservableCollection<Document> DocumentsModel { get; set; }
		public ObservableCollection<Document> DocumentsView { get; set; }
		public ObservableCollection<Document> DocumentsViewModel { get; set; }

		public ObservableCollection<Document> DocumentsSingleton { get; set; }
		public ObservableCollection<Document> DocumentsNonSingleton { get; set; }

		private ListBox lastFocusedListBoxMVVM;
		private ListBox lastFocusedListBoxSingleton;

		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindowControl"/> class.
		/// </summary>
		public MainWindowControl()
		{
			this.InitializeComponent();

			// IServiceProvider serviceProvider = new ServiceProvider(Application.Current);
			// vielleicht im MainwindowCommand.cs in der initializer()?

			// ErrorReporter.Initialize((System.IServiceProvider)serviceProvider);
			// Projects = new List<Project>();
			Projects = new ObservableCollection<Project>();

			DocumentsUnassigned = new ObservableCollection<Document>();
			DocumentsModel = new ObservableCollection<Document>();
			DocumentsView = new ObservableCollection<Document>();
			DocumentsViewModel = new ObservableCollection<Document>();

			DocumentsSingleton = new ObservableCollection<Document>();
			DocumentsNonSingleton = new ObservableCollection<Document>();

			IComponentModel componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
			var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

			var projects = workspace.CurrentSolution.Projects;
			foreach (Project p in projects)
			{
				this.Projects.Add(p);
			}

			DataContext = this;
		}

		/// <summary>
		/// Handles click on the button by displaying a message box.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event args.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
		[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
		private void CheckProject(object sender, RoutedEventArgs e)
		{
			#region errorTest
			// Test für errors
			/*
            Document testDoc = null;
            foreach (Document d in DocumentsUnassigned)
            {
                if (d.Name == "MainWindowControl.xaml.cs")
                {
                    testDoc = d;
                }
            }

            var ivsSolution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
            IVsHierarchy hierarchyItem;
            ivsSolution.GetProjectOfUniqueName("Plugin_Entwicklung", out hierarchyItem);

            ErrorTask testError = new ErrorTask()
            {
                ErrorCategory = TaskErrorCategory.Error,
                Category = TaskCategory.BuildCompile,
                Text = "This is a test error for testing purposes.",
                Document = testDoc.FilePath,
                Line = 2,
                Column = 6,
                HierarchyItem = hierarchyItem
            };


            ServiceProvider serviceProvider = new ServiceProvider(Application.Current as IServiceProvider);
            ErrorListProvider errorListProvider = new ErrorListProvider(serviceProvider);
            errorListProvider.ProviderName = "custom errors";
            errorListProvider.ProviderGuid = new System.Guid("xxxxxxxxx");
            testError.Navigate += (src, args) =>
            {
                //there are two Bugs in the errorListProvider.Navigate method:
                //    Line number needs adjusting
                //    Column is not shown
                testError.Line++;
                errorListProvider.Navigate(testError, new System.Guid(EnvDTE.Constants.vsViewKindCode));
                testError.Line--;
            };

            errorListProvider.Tasks.Clear();    // clear previously created
            errorListProvider.Tasks.Add(testError);  // add item
            errorListProvider.Show();       // make sure it is visible
            */
			#endregion errorTest

			/*
            Document testDoc = null;
            foreach (Document d in DocumentsUnassigned)
            {
                if (d.Name == "MainWindowControl.xaml.cs")
                {
                    testDoc = d;
                }
            }

            ErrorReporter.AddWarning("Testeroni");
            ErrorReporter.AddWarning("TestLine 52", 52, testDoc.FilePath);
            */

			MessageBox.Show(
				string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
				"MainWindow");

			if ((bool)checkBox_LineLength.IsChecked)
			{
				new Controller.LineLengthController().CheckLineLength(this.SelectedProject, int.Parse(this.textBox_lengthLines.Text));
			}

			if ((bool)checkBox_Naming.IsChecked)
			{
				// SplitString() für erlaubte Sonderzeichen, if-stmnts für einzelne Namings (Variablen, Properties, etc)
				List<string> permittedCharsMethods;
				permittedCharsMethods = SplitString(textBox_specialCharsMethods.Text);
				new Controller.NamingController().CheckNaming(SelectedProject, permittedCharsMethods);
			}

			if ((bool)checkBox_imports.IsChecked)
			{
				new Controller.ImportsUsedController().CheckImports(SelectedProject);
			}

			if ((bool)checkBox_Singleton.IsChecked)
			{
				new Controller.SingletonController().CheckSingleton(SelectedProject, DocumentsSingleton);
			}
		}

		private void RefreshWorkspace(object sender, RoutedEventArgs e)
		{
			IComponentModel componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
			var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

			ErrorReporter.ClearErrors();

			var projects = workspace.CurrentSolution.Projects;

			this.Projects.Clear();
			foreach (Project p in projects)
			{
				this.Projects.Add(p);
			}
		}

		private void MoveItem(object sender, RoutedEventArgs e)
		{
			ObservableCollection<Document> targetCollection;
			ObservableCollection<Document> sourceCollection;

			if (sender == button_ToModel)
			{
				targetCollection = DocumentsModel;
			}
			else if (sender == button_ToView)
			{
				targetCollection = DocumentsView;
			}
			else if (sender == button_ToVM)
			{
				targetCollection = DocumentsViewModel;
			}
			else
			{
				targetCollection = null;
			}

			if (lastFocusedListBoxMVVM == listBox_Items)
			{
				sourceCollection = DocumentsUnassigned;
			}
			else if (lastFocusedListBoxMVVM == listBox_Model)
			{
				sourceCollection = DocumentsModel;
			}
			else if (lastFocusedListBoxMVVM == listBox_View)
			{
				sourceCollection = DocumentsView;
			}
			else if (lastFocusedListBoxMVVM == listBox_ViewModel)
			{
				sourceCollection = DocumentsViewModel;
			}
			else
			{
				sourceCollection = null;
			}

			if (targetCollection != null && sourceCollection != null)
			{
				if (lastFocusedListBoxMVVM.SelectedItem != null)
				{
					targetCollection.Add((Document)lastFocusedListBoxMVVM.SelectedItem);
					sourceCollection.Remove((Document)lastFocusedListBoxMVVM.SelectedItem);
				}
			}
		}

		private void SwapItem(object sender, RoutedEventArgs e)
		{
			if (lastFocusedListBoxSingleton != null)
			{
				if (lastFocusedListBoxSingleton.SelectedItem != null)
				{
					if (lastFocusedListBoxSingleton == listBox_NonSingleton)
					{
						DocumentsSingleton.Add((Document)lastFocusedListBoxSingleton.SelectedItem);
						DocumentsNonSingleton.Remove((Document)lastFocusedListBoxSingleton.SelectedItem);
					}
					else if (lastFocusedListBoxSingleton == listBox_Singleton)
					{
						DocumentsNonSingleton.Add((Document)lastFocusedListBoxSingleton.SelectedItem);
						DocumentsSingleton.Remove((Document)lastFocusedListBoxSingleton.SelectedItem);
					}
				}
			}
		}

		private void ShiftFocus(object sender, RoutedEventArgs e)
		{
			ListBox listBox = sender as ListBox;
			if (listBox == listBox_Items || listBox == listBox_Model || listBox == listBox_View || listBox == listBox_ViewModel)
			{
				lastFocusedListBoxMVVM = sender as ListBox;
			}
			else if (listBox == listBox_NonSingleton || listBox == listBox_Singleton)
			{
				lastFocusedListBoxSingleton = listBox;
			}
		}

		private List<string> SplitString(string input)
		{
			List<string> res = new List<string>();

			string[] substrings = input.Split(' ');

			foreach (string s in substrings)
			{
				res.Add(s.Trim());
			}

			return res;
		}
	}
}