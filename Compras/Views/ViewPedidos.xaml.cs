using Compras.DataBase.Model;
using Compras.Utils;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Compras.Views
{
    public partial class ViewPedidos : UserControl
    {
        public ViewPedidos()
        {
            InitializeComponent();
            DataContext = new QryPedidoViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (QryPedidoViewModel)DataContext;
                await vm.GetPedidosAsync();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void pedidos_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var vm = (QryPedidoViewModel)DataContext;
            if (vm.Pedido?.idpedido is null)
            {
                vm.PedidoDets = [];
                return;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                await vm.GetPedidoDetalhesAsync(vm.Pedido.idpedido);
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }
    }

    public class QryPedidoViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private QryPedidoModel? pedido;
        public QryPedidoModel? Pedido
        {
            get => pedido;
            set
            {
                pedido = value;
                RaisePropertyChanged(nameof(Pedido));
            }
        }

        private ObservableCollection<QryPedidoModel> pedidos = [];
        public ObservableCollection<QryPedidoModel> Pedidos
        {
            get => pedidos;
            set
            {
                pedidos = value;
                RaisePropertyChanged(nameof(Pedidos));
            }
        }

        private QryPedidosDet? pedidoDet;
        public QryPedidosDet? PedidoDet
        {
            get => pedidoDet;
            set
            {
                pedidoDet = value;
                RaisePropertyChanged(nameof(PedidoDet));
            }
        }

        private ObservableCollection<QryPedidosDet> pedidoDets = [];
        public ObservableCollection<QryPedidosDet> PedidoDets
        {
            get => pedidoDets;
            set
            {
                pedidoDets = value;
                RaisePropertyChanged(nameof(PedidoDets));
            }
        }

        public async Task GetPedidosAsync()
        {
            const string sql = """
                SELECT *
                FROM compras.qry_pedidos;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<QryPedidoModel>(sql);
            Pedidos = new ObservableCollection<QryPedidoModel>(data);
        }

        public async Task GetPedidoDetalhesAsync(long? idpedido)
        {
            if (idpedido is null)
            {
                PedidoDets = [];
                return;
            }

            const string sql = """
                SELECT *
                FROM compras.qry_pedidosdet
                WHERE idpedido = @idpedido;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<QryPedidosDet>(sql, new { idpedido });
            PedidoDets = new ObservableCollection<QryPedidosDet>(data);
        }
    }
}
