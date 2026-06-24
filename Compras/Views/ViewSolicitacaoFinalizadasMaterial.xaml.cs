using Compras.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewSolicitacaoFinalizadasMaterial : UserControl
    {
        private SolicitacaoNovoEncaminhamentoViewModel ViewModel =>
            (SolicitacaoNovoEncaminhamentoViewModel)DataContext;

        public ViewSolicitacaoFinalizadasMaterial()
        {
            InitializeComponent();
            DataContext = new SolicitacaoNovoEncaminhamentoViewModel("MATERIAIS");
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                await ViewModel.CarregarFinalizadasAsync();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void ItensSolicitados_RowEditEnded(
            object sender,
            GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit ||
                e.NewData is not SolicitacaoEncaminhadaModel item)
            {
                return;
            }

            if (item.finalizado == true)
            {
                item.finalizado_por ??= DataBaseSettings.Instance.Username;
                item.finalizado_em ??= DateTime.Now;
            }
            else
            {
                item.finalizado_por = null;
                item.finalizado_em = null;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                await ViewModel.SalvarSolicitacaoAsync(item);

                if (item.finalizado == false)
                {
                    ViewModel.SolicitacoesPendentes.Remove(item);
                }
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
                await ViewModel.CarregarFinalizadasAsync();
            }
        }
    }
}
