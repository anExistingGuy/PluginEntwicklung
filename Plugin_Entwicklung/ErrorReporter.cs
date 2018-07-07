using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Entwicklung
{
    static class ErrorReporter
    {
        // https://softwareproduction.eu/2013/05/30/write-to-visual-studios-error-list/
        private static ErrorListProvider errorListProvider;
        private static List<ErrorTask> errors;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            errorListProvider = new ErrorListProvider(serviceProvider);
            errors = new List<ErrorTask>();
        }

        public static void AddError(string message)
        {
            AddTask(message, TaskErrorCategory.Error);
        }

        public static void AddError(string message, int line, string documentPath)
        {
            AddTask(message, line, documentPath, TaskErrorCategory.Error);
        }

        public static void AddWarning(string message)
        {
            AddTask(message, TaskErrorCategory.Warning);
        }

        public static void AddWarning(string message, int line, string documentPath)
        {
            AddTask(message, line, documentPath, TaskErrorCategory.Warning);
        }

        public static void ClearErrors()
        {
            foreach (ErrorTask error in errors)
            {
                errorListProvider.Tasks.Remove(error);
            }
        }

        private static void AddTask(string message, TaskErrorCategory errorCategory)
        {
            ErrorTask newTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = errorCategory,
                Text = message
            };

            errorListProvider.Tasks.Add(newTask);

            errors.Add(newTask);
        }

        private static void AddTask(string message, int line, string documentPath, TaskErrorCategory errorCategory)
        {
            ErrorTask newTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = errorCategory,
                Text = message,
                Line = line,
                Document = documentPath,
            };

            errorListProvider.Tasks.Add(newTask);
            errorListProvider.Show();

            errors.Add(newTask);
        }
    }
}
