using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compras.Views.PopUp
{
    /// <summary>
    /// Interação lógica para PopUpNovaSolicitacao.xam
    /// </summary>
    public partial class PopUpNovaSolicitacao : UserControl
    {

        SolicitacaoSolicitanteModel Solicitante { get; set; }
        public PopUpNovaSolicitacao(object DataContext)
        {
            InitializeComponent();
            this.DataContext = DataContext;
        }

        private async void OnClickGravar(object sender, RoutedEventArgs e)
        {

            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                var solicitacao = new SolicitacaoMaterialModel
                {
                    tipo = (string)tipo.SelectionBoxItem,
                    almox_recebimento = almoxRecebimento.Text,
                    //cod_solicitante = Solicitante.cod_solicitante,
                    data_solicitacao = DateTime.Now
                };

                vm.SolicitacaoMaterial =  await Task.Run(async () => await vm.CreateSolicitacaoMaterialAsync(solicitacao));
                btnGrevar.IsEnabled= false;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }

            //MessageBox.Show((string)tipo.SelectionBoxItem);
        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                //Solicitante = await Task.Run(async () => await vm.GetSolicitacaoAsync());
                btnGrevar.IsEnabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
