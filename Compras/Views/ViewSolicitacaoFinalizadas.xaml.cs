using Compras.Utils;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewSolicitacaoFinalizadas : UserControl
    {
        private DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public ViewSolicitacaoFinalizadas()
        {
            InitializeComponent();
            DataContext = new SolicitacaoEncaminhadaViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (SolicitacaoEncaminhadaViewModel)DataContext;
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                vm.SolicitacoesEncaminhadas = await vm.GetSolicitacaoFinalizadasAsync();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void itensSolicitados_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit || e.NewData is not SolicitacaoEncaminhadaModel record)
            {
                return;
            }

            if (record.finalizado == true)
            {
                record.finalizado_por = BaseSettings.Username;
                record.finalizado_em = DateTime.Now;
            }
            else
            {
                record.finalizado = false;
                record.finalizado_por = null;
                record.finalizado_em = null;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (SolicitacaoEncaminhadaViewModel)DataContext;
                await vm.FinalizarItemSolicitadoAsync(record);
                if (record.finalizado == false)
                    vm.SolicitacoesEncaminhadas.Remove(record);
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }
    }
}
