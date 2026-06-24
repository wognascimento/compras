using System.Windows.Controls;

namespace Compras.Views
{
    public partial class ViewSolicitacaoEncaminhamentoMaterial : UserControl
    {
        public ViewSolicitacaoEncaminhamentoMaterial()
        {
            InitializeComponent();
            conteudo.Content = new ViewSolicitacaoNovoEncaminhamento(
                "MATERIAIS",
                habilitarMontarPedido: false);
        }
    }
}
