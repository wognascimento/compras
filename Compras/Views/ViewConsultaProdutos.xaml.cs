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
    public partial class ViewConsultaProdutos : UserControl
    {
        public ViewConsultaProdutos()
        {
            InitializeComponent();
            DataContext = new TodosProdutosViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (TodosProdutosViewModel)DataContext;
                vm.Descricoes = await vm.GetDescricaosAsync();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }
    }

    public class TodosProdutosViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private DescricaoProducaoModel? descricao;
        public DescricaoProducaoModel? Descricao
        {
            get => descricao;
            set
            {
                descricao = value;
                RaisePropertyChanged(nameof(Descricao));
            }
        }

        private ObservableCollection<DescricaoProducaoModel> descricoes = [];
        public ObservableCollection<DescricaoProducaoModel> Descricoes
        {
            get => descricoes;
            set
            {
                descricoes = value;
                RaisePropertyChanged(nameof(Descricoes));
            }
        }

        public async Task<ObservableCollection<DescricaoProducaoModel>> GetDescricaosAsync()
        {
            const string sql = """
                SELECT *
                FROM producao.qry3descricoes;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<DescricaoProducaoModel>(sql);
            return new ObservableCollection<DescricaoProducaoModel>(data);
        }
    }
}
