using System.Windows.Controls;

namespace Compras.Views
{
    public partial class ViewSolicitacaoEncaminhamentoMaterialLegado : UserControl
    {
        public ViewSolicitacaoEncaminhamentoMaterialLegado()
        {
            InitializeComponent();
            conteudo.Content = new ViewSolicitacaoEncaminhamento(
                "MATERIAIS",
                habilitarMontarPedido: false);
        }
    }
}
