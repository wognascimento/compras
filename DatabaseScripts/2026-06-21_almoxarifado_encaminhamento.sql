ALTER TABLE compras.solicitacao_material_itens
    ADD COLUMN IF NOT EXISTS status_fluxo character varying(30) DEFAULT 'SOLICITADO';

ALTER TABLE compras.solicitacao_material_itens
    ADD COLUMN IF NOT EXISTS id_almox_item bigint;

ALTER TABLE compras.solicitacao_material_itens
    ADD COLUMN IF NOT EXISTS quantidade_atendida_estoque double precision DEFAULT 0;

ALTER TABLE compras.solicitacao_material_itens
    ADD COLUMN IF NOT EXISTS quantidade_enviada_compra double precision DEFAULT 0;

ALTER TABLE compras.solicitacao_material_itens
    ADD COLUMN IF NOT EXISTS processado_almox_por character varying(50);

ALTER TABLE compras.solicitacao_material_itens
    ADD COLUMN IF NOT EXISTS processado_almox_em timestamp with time zone;

ALTER TABLE compras.solicitacao_material_itens
    ADD COLUMN IF NOT EXISTS pedido boolean NOT NULL DEFAULT false;

UPDATE compras.solicitacao_material_itens
SET status_fluxo = 'SOLICITADO'
WHERE status_fluxo IS NULL;

CREATE TABLE IF NOT EXISTS compras.almoxarifado_encaminhamento
(
    id_almox_encaminhamento bigserial PRIMARY KEY,
    almox_recebimento character varying(100) NOT NULL,
    status character varying(30) NOT NULL DEFAULT 'ABERTO',
    obs character varying(250),
    criado_por character varying(50) NOT NULL,
    criado_em timestamp with time zone NOT NULL DEFAULT now(),
    alterado_por character varying(50),
    alterado_em timestamp with time zone,
    enviado_compras_por character varying(50),
    enviado_compras_em timestamp with time zone
);

CREATE TABLE IF NOT EXISTS compras.almoxarifado_encaminhamento_itens
(
    id_almox_item bigserial PRIMARY KEY,
    id_almox_encaminhamento bigint NOT NULL REFERENCES compras.almoxarifado_encaminhamento (id_almox_encaminhamento),
    codcompleadicional bigint NOT NULL,
    codprodutocompra bigint,
    idfornecedor bigint,
    almox_recebimento character varying(100) NOT NULL,
    tipo character varying(30) NOT NULL,
    planilha character varying(50),
    descricao_completa character varying(250) NOT NULL,
    unidade character varying(20),
    quantidade_total_solicitada numeric(15,2) NOT NULL DEFAULT 0,
    quantidade_atendida_estoque numeric(15,2) NOT NULL DEFAULT 0,
    quantidade_enviar_compra numeric(15,2) NOT NULL DEFAULT 0,
    saldo_estoque_considerado numeric(15,2),
    obs_almoxarifado character varying(250),
    preco numeric(15,2),
    orientacao_compra text,
    orientacao_roteiro text,
    pedido boolean NOT NULL DEFAULT false,
    data_entrega date,
    resp_compra character varying(50),
    finalizado boolean NOT NULL DEFAULT false,
    finalizado_por character varying(50),
    finalizado_em timestamp with time zone,
    status character varying(30) NOT NULL DEFAULT 'CONSOLIDADO',
    criado_por character varying(50) NOT NULL,
    criado_em timestamp with time zone NOT NULL DEFAULT now(),
    alterado_por character varying(50),
    alterado_em timestamp with time zone
);

-- Garante a evolução de instalações onde a tabela já existia antes destes campos.
ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS preco numeric(15,2);

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS orientacao_compra text;

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS orientacao_roteiro text;

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS pedido boolean NOT NULL DEFAULT false;

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS data_entrega date;

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS resp_compra character varying(50);

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS finalizado boolean NOT NULL DEFAULT false;

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS finalizado_por character varying(50);

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS finalizado_em timestamp with time zone;

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS alterado_por character varying(50);

ALTER TABLE compras.almoxarifado_encaminhamento_itens
    ADD COLUMN IF NOT EXISTS alterado_em timestamp with time zone;

CREATE TABLE IF NOT EXISTS compras.almoxarifado_encaminhamento_origem
(
    id_almox_origem bigserial PRIMARY KEY,
    id_almox_item bigint NOT NULL REFERENCES compras.almoxarifado_encaminhamento_itens (id_almox_item),
    cod_item bigint NOT NULL REFERENCES compras.solicitacao_material_itens (cod_item),
    quantidade_solicitada_origem numeric(15,2) NOT NULL,
    quantidade_atendida_estoque_origem numeric(15,2) NOT NULL DEFAULT 0,
    quantidade_enviada_compra_origem numeric(15,2) NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_almox_item_encaminhamento
    ON compras.almoxarifado_encaminhamento_itens (id_almox_encaminhamento);

CREATE INDEX IF NOT EXISTS idx_almox_origem_item
    ON compras.almoxarifado_encaminhamento_origem (id_almox_item);

CREATE INDEX IF NOT EXISTS idx_almox_origem_cod_item
    ON compras.almoxarifado_encaminhamento_origem (cod_item);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_solicitacao_material_itens_almox_item'
    ) THEN
        ALTER TABLE compras.solicitacao_material_itens
            ADD CONSTRAINT fk_solicitacao_material_itens_almox_item
            FOREIGN KEY (id_almox_item)
            REFERENCES compras.almoxarifado_encaminhamento_itens (id_almox_item);
    END IF;
END $$;

CREATE OR REPLACE VIEW compras.qry_almoxarifado_solicitacoes_pendentes AS
SELECT
    item.cod_item,
    item.cod_solicitacao,
    item.codcompleadicional,
    item.codprodutocompra,
    item.codfornecedor AS idfornecedor,
    solicitacao.almox_recebimento,
    solicitacao.tipo,
    descricao.planilha,
    descricao.descricao_completa,
    descricao.unidade,
    descricao.saldo_estoque,
    item.quantidade,
    item.obs_solicitacao,
    item.data_informado,
    item.data_utilizacao,
    item.solicitante,
    item.cliente,
    fornecedor.nomefantasia,
    solicitacao.data_solicitacao,
    item.status_fluxo
FROM compras.solicitacao_material_itens item
JOIN compras.solicitacao_material solicitacao
    ON solicitacao.cod_solicitacao = item.cod_solicitacao
LEFT JOIN producao.qry3descricoes descricao
    ON descricao.codcompladicional = item.codcompleadicional
LEFT JOIN compras.fornecedores fornecedor
    ON fornecedor.idfornecedor = item.codfornecedor
WHERE COALESCE(item.finalizado, false) = false
  AND COALESCE(solicitacao.tipo, '') <> 'SERVIÇO'
  AND COALESCE(item.status_fluxo, 'SOLICITADO') IN ('SOLICITADO', 'EM_ALMOX');

CREATE OR REPLACE VIEW compras.qry_solicitacoes_encaminhadas_almox AS
SELECT
    almox_item.id_almox_item AS cod_item,
    MIN(origem.cod_solicitacao) AS cod_solicitacao,
    almox_item.data_entrega,
    almox_item.almox_recebimento,
    string_agg(DISTINCT COALESCE(origem.solicitante, ''), ', ') FILTER (WHERE COALESCE(origem.solicitante, '') <> '') AS username,
    MIN(solicitacao.data_solicitacao) AS data_solicitacao,
    almox_item.quantidade_total_solicitada AS quantidade,
    string_agg(DISTINCT COALESCE(origem.obs_solicitacao, ''), ' | ') FILTER (WHERE COALESCE(origem.obs_solicitacao, '') <> '') AS obs_solicitacao,
    NULL::bigint AS n_servico,
    NULL::character varying AS sugestao_fornecedor,
    NULL::character varying AS amostra,
    NULL::character varying AS setor,
    NULL::character varying AS observacao_compra,
    string_agg(DISTINCT COALESCE(origem.cliente, ''), ', ') FILTER (WHERE COALESCE(origem.cliente, '') <> '') AS cliente,
    MIN(origem.data_utilizacao) AS data_utilizacao,
    NULL::character varying AS saldo_atende,
    'SIM'::character varying AS enviar_compra,
    almox_item.quantidade_enviar_compra AS quantidade_compra,
    almox_item.obs_almoxarifado,
    NULL::character varying AS enviar_pedido,
    MIN(origem.data_informado) AS data_informado,
    NULL::character varying AS enviado_compra_por,
    NULL::timestamp with time zone AS enviado_compra_em,
    NULL::character varying AS enviado_pedido_por,
    NULL::timestamp with time zone AS enviado_pedido_em,
    NULL::character varying AS informado_por,
    almox_item.codprodutocompra,
    almox_item.codcompleadicional,
    NULL::bigint AS idpedido,
    almox_item.tipo,
    NULL::character varying AS resp_compra,
    NULL::character varying AS status_compra,
    NULL::bigint AS codfornecedor,
    NULL::bigint AS codempresa,
    NULL::bigint AS codlocalcompra,
    NULL::timestamp with time zone AS data_pedido_gerado,
    descricao.familia,
    almox_item.planilha,
    almox_item.descricao_completa,
    almox_item.unidade,
    descricao.saldo_estoque,
    almox_item.quantidade_enviar_compra AS qtde_compra_final,
    almox_item.preco,
    descricao.custo,
    NULL::character varying AS aprovacao,
    NULL::double precision AS limite,
    NULL::character varying AS etapa,
    NULL::character varying AS classificacao,
    NULL::character varying AS descricao_dsl,
    NULL::double precision AS ultimo_valor_compra,
    NULL::bigint AS codcentro_custo,
    NULL::character varying AS prioridade,
    NULL::character varying AS aprovado_por,
    NULL::timestamp with time zone AS aprovado_em,
    NULL::bigint AS id_cond_pagamento,
    NULL::character varying AS classificacao_cipolatti,
    NULL::date AS fechamento_shopp,
    NULL::character varying AS numero_nf,
    NULL::date AS data_de_expedicao,
    NULL::date AS data_emissao_nf,
    NULL::bigint AS linha_fluxo,
    fornecedor.nomefantasia,
    almox_item.idfornecedor,
    almox_item.orientacao_compra,
    almox_item.orientacao_roteiro,
    almox_item.pedido,
    string_agg(DISTINCT COALESCE(origem.solicitante, ''), ', ') FILTER (WHERE COALESCE(origem.solicitante, '') <> '') AS solicitante,
    almox_item.finalizado,
    almox_item.finalizado_por,
    almox_item.finalizado_em
FROM compras.almoxarifado_encaminhamento_itens almox_item
JOIN compras.almoxarifado_encaminhamento_origem vinculo
    ON vinculo.id_almox_item = almox_item.id_almox_item
JOIN compras.solicitacao_material_itens origem
    ON origem.cod_item = vinculo.cod_item
JOIN compras.solicitacao_material solicitacao
    ON solicitacao.cod_solicitacao = origem.cod_solicitacao
LEFT JOIN producao.qry3descricoes descricao
    ON descricao.codcompladicional = almox_item.codcompleadicional
LEFT JOIN compras.fornecedores fornecedor
    ON fornecedor.idfornecedor = almox_item.idfornecedor
WHERE COALESCE(almox_item.finalizado, false) = false
GROUP BY
    almox_item.id_almox_item,
    almox_item.data_entrega,
    almox_item.almox_recebimento,
    almox_item.quantidade_total_solicitada,
    almox_item.quantidade_enviar_compra,
    almox_item.obs_almoxarifado,
    almox_item.codprodutocompra,
    almox_item.codcompleadicional,
    almox_item.tipo,
    descricao.familia,
    almox_item.planilha,
    almox_item.descricao_completa,
    almox_item.unidade,
    descricao.saldo_estoque,
    almox_item.preco,
    descricao.custo,
    fornecedor.nomefantasia,
    almox_item.idfornecedor,
    almox_item.orientacao_compra,
    almox_item.orientacao_roteiro,
    almox_item.pedido,
    almox_item.finalizado,
    almox_item.finalizado_por,
    almox_item.finalizado_em;
