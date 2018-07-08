using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatterNicer
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

        // Method to add an error to VS error list supplying only the message
        public static void AddError(string message)
        {
            AddTask(message, TaskErrorCategory.Error);
        }

        /* Method to add an error to VS error list 
         * supplying message, docPath and startline 
         */
        public static void AddError
            (
            string message, 
            string documentPath, 
            int line = 0
            )
        {
            AddTask(message, line, documentPath, TaskErrorCategory.Error);
        }

        /* Method to add a warning to 
         * VS warning list supplying only the message 
         */
        public static void AddWarning(string message)
        {
            AddTask(message, TaskErrorCategory.Warning);
        }

        /* Method to add a warning to VS warning list 
         * supplying message, docPath and startline 
         */
        public static void AddWarning
            (
            string message, 
            string documentPath, 
            int line = 0
            )
        {
            AddTask(message, line, documentPath, TaskErrorCategory.Warning);
        }

        // Method to clear all errors created by this class
        public static void ClearErrors()
        {
            foreach (ErrorTask error in errors)
            {
                errorListProvider.Tasks.Remove(error);
            }
        }

        /* General method for adding tasks to the errorListProvider 
         * provided only the message and the errorCategory 
         */
        private static void AddTask
            (
            string message, 
            TaskErrorCategory errorCategory
            )
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

        /* General method for adding tasks to the errorListProvider 
         * provided the message, startline, docPath and errorCategory 
         */
        private static void AddTask
            (
            string message, 
            int line, 
            string documentPath, 
            TaskErrorCategory errorCategory
            )
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
