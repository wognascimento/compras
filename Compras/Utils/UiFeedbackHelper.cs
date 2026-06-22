using System;
using System.Windows;
using System.Windows.Input;

namespace Compras.Utils
{
    public static class UiFeedbackHelper
    {
        public static IDisposable BeginBusyCursor()
        {
            var previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            return new BusyCursorScope(previousCursor);
        }

        public static void ShowError(Exception exception, string title = "Erro")
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private sealed class BusyCursorScope : IDisposable
        {
            private readonly Cursor? previousCursor;
            private bool disposed;

            public BusyCursorScope(Cursor? previousCursor)
            {
                this.previousCursor = previousCursor;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                Mouse.OverrideCursor = previousCursor;
                disposed = true;
            }
        }
    }
}
