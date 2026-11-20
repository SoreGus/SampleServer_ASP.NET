# Checklist — Visão de negócio

## Visão

Checklist é uma plataforma genérica para empresas definirem, executarem e auditarem processos operacionais recorrentes. Restaurantes são o mercado inicial, mas o produto não depende desse segmento.

## Estrutura da empresa

Uma organização possui setores hierárquicos, permitindo representar desde estruturas simples até árvores com vários níveis. Cada setor pode possuir turnos e seus próprios managers e operadores.

A organização pode definir uma localização padrão. Um setor pode usar essa localização, herdar a localização de um setor ancestral ou definir a sua própria.

## Usuários e acesso

- **Owner:** controle total da organização, inclusive gestão de administradores.
- **Admin:** administra usuários, setores, turnos e processos da organização.
- **Manager:** gerencia os setores aos quais está associado e acompanha suas execuções.
- **Operator:** executa os checklists atribuídos aos seus setores e turnos.

## Processos

Um checklist representa um processo e contém tarefas ordenadas. Cada tarefa define instruções, horário, obrigatoriedade, tipo de entrada e, opcionalmente, exigência de localização.

Os tipos de entrada iniciais incluem booleano, texto, número, seleção, foto, vídeo, data, horário e assinatura.

Checklists publicados são imutáveis. Qualquer alteração produz uma nova versão, preservando o significado dos registros históricos.

## Execução e acompanhamento

O agendamento determina quando, em qual setor e, opcionalmente, em qual turno ou por qual operador um checklist deve ser executado.

`ChecklistRecord` registra uma execução esperada ou realizada. `TaskRecord` registra as respostas de suas tarefas. Esses registros permitem acompanhar atrasos, não execução, conformidade, localização, revisão e evidências.

Relatórios são produzidos a partir dos registros existentes e não constituem entidades próprias inicialmente.

## Princípios

- Isolamento completo dos dados de cada organização.
- Permissões aplicadas conforme papel, setor e turno.
- Histórico imutável dos processos publicados e executados.
- Auditoria das ações relevantes.
- Coleta apenas dos dados necessários.
- Arquivos armazenados fora do banco de dados.
- Logs e observabilidade separados dos dados de negócio.
