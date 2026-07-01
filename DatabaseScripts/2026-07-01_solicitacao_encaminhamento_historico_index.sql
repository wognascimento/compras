-- Execute com o dono da tabela ou usuário administrador do banco.
CREATE INDEX IF NOT EXISTS idx_solicitacao_encaminhamento_historico_item
    ON compras.solicitacao_encaminhamento_historico (origem_tabela, cod_item, alterado_em DESC);
