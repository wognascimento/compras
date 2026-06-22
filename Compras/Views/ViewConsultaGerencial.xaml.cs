using Compras.Utils;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Compras.Views
{
    public partial class ViewConsultaGerencial : UserControl
    {
        public ViewConsultaGerencial()
        {
            InitializeComponent();
            DataContext = new ConsultaGerencialViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (ConsultaGerencialViewModel)DataContext;
                vm.Detalhes = await vm.GeDetalhesAsync();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }
    }

    public class ConsultaGerencialViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private SolicitacaoDetalheItem? detalhe;
        public SolicitacaoDetalheItem? Detalhe
        {
            get => detalhe;
            set
            {
                detalhe = value;
                RaisePropertyChanged(nameof(Detalhe));
            }
        }

        private ObservableCollection<SolicitacaoDetalheItem> detalhes = [];
        public ObservableCollection<SolicitacaoDetalheItem> Detalhes
        {
            get => detalhes;
            set
            {
                detalhes = value;
                RaisePropertyChanged(nameof(Detalhes));
            }
        }

        public async Task<ObservableCollection<SolicitacaoDetalheItem>> GeDetalhesAsync()
        {
            const string sql = """
                SELECT *
                FROM compras.qry_solicitacoes_detalhes_itens;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<SolicitacaoDetalheItem>(sql);
            return new ObservableCollection<SolicitacaoDetalheItem>(data);
        }
    }
}
