# Checklist — Casos de uso

## Objetivo

Este documento define o que os usuários e o sistema podem fazer. Ele descreve comportamento e regras de negócio, sem determinar telas, endpoints ou detalhes de implementação.

## Atores

- **Owner:** possui controle total da organização, inclusive sobre administradores e configurações gerais.
- **Admin:** administra pessoas, estrutura, processos e operação da organização, exceto ações exclusivas do Owner.
- **Manager:** gerencia sua equipe e os processos dos setores aos quais está associado.
- **Operator:** executa os checklists atribuídos a ele, ao seu setor ou ao seu turno.
- **System:** executa agendamentos, validações, escaladas, notificações e registros automáticos.

## Visão geral de acesso

| Capacidade | Owner | Admin | Manager | Operator |
|---|:---:|:---:|:---:|:---:|
| Gerenciar a organização | Sim | Parcial | Não | Não |
| Gerenciar administradores | Sim | Não | Não | Não |
| Gerenciar managers e operators | Sim | Sim | Apenas sua equipe | Não |
| Gerenciar setores e turnos | Sim | Sim | Nos seus setores | Não |
| Criar checklists | Sim | Sim | Sim, com aprovação | Não |
| Aprovar e publicar checklists | Sim | Sim | Não | Não |
| Executar checklists | Se atribuído | Se atribuído | Se atribuído | Sim, se atribuído |
| Verificar execuções | Sim | Sim | Nos seus setores | Não |
| Ver dashboards e relatórios | Todos os dados | Todos os dados | Seus setores | Próprios registros |

As permissões sempre respeitam a organização, os setores e os turnos do membro. Possuir um papel não concede acesso a dados de outra organização.

## 1. Autenticação e acesso

### UC-AUTH-01 — Criar conta e organização

**Atores:** Owner.

**Objetivo:** cadastrar a primeira conta e criar uma organização.

**Regras:**

- O usuário criador torna-se Owner da organização.
- O sistema gera um código de acesso único para a organização.
- A organização deve possuir ao menos um Owner ativo.
- A localização da organização é opcional.

### UC-AUTH-02 — Enviar convite para a organização

**Atores:** Owner e Admin.

**Objetivo:** enviar um link para facilitar o ingresso de uma pessoa na organização.

**Regras:**

- O convite define previamente e-mail, organização e papel.
- Owner pode convidar Admin, Manager e Operator.
- Admin pode convidar Manager e Operator.
- O link possui token de uso único, prazo de validade e opção de cancelamento.
- O token original não é armazenado no banco de dados.
- Reenviar um convite invalida ou substitui o convite anterior ainda pendente.

### UC-AUTH-03 — Aceitar convite

**Atores:** pessoa convidada.

**Objetivo:** entrar na organização por meio do link recebido.

**Regras:**

- O convite deve estar válido, não utilizado e não revogado.
- O usuário pode criar uma conta ou entrar em uma conta existente.
- A conta autenticada deve possuir o mesmo e-mail do convite.
- O papel definido no convite não pode ser alterado pelo convidado.
- Após a aceitação, o convite não pode ser reutilizado.

### UC-AUTH-04 — Criar conta usando o código da organização

**Atores:** Admin, Manager ou Operator.

**Objetivo:** criar uma conta vinculada a uma organização existente sem convite direto.

**Regras:**

- O código deve identificar uma organização ativa.
- O código não autentica o usuário e não concede permissões sozinho.
- O papel e o acesso devem ser definidos ou aprovados por Owner ou Admin.
- Um usuário pode participar de mais de uma organização com papéis diferentes.

### UC-AUTH-05 — Criar conta com e-mail e senha

**Atores:** todos os usuários.

**Objetivo:** criar uma conta utilizando credenciais próprias do sistema.

**Regras:**

- O e-mail deve ser único e possuir formato válido.
- A senha deve respeitar a política de segurança vigente.
- O sistema envia um link de confirmação para o e-mail informado.
- A conta não pode acessar o sistema até que o e-mail seja confirmado.
- O link de confirmação deve ser temporário e não pode confirmar a conta mais de uma vez.

### UC-AUTH-06 — Confirmar e-mail

**Atores:** usuário cadastrado com e-mail e senha.

**Objetivo:** comprovar que o usuário controla o endereço informado.

**Regras:**

- O link deve ser válido e estar dentro do prazo.
- A confirmação registra a data em que o e-mail foi verificado.
- Um link expirado pode ser reenviado.
- Solicitar novo link invalida ou torna desnecessário o anterior.

### UC-AUTH-07 — Entrar com e-mail e senha

**Atores:** usuários com credencial local.

**Objetivo:** autenticar-se com e-mail e senha.

**Regras:**

- O e-mail precisa estar confirmado.
- Somente usuários e vínculos ativos podem acessar dados da organização.
- Tentativas inválidas são limitadas e podem bloquear temporariamente a conta.
- Após o login, o usuário é direcionado para a experiência adequada ao seu papel.
- O sistema registra o login e as tentativas relevantes na auditoria.

### UC-AUTH-08 — Entrar com Google ou Apple

**Atores:** todos os usuários.

**Objetivo:** criar ou acessar uma conta usando SSO do Google ou da Apple.

**Regras:**

- O sistema valida identidade, assinatura, emissor, audiência e validade da resposta do provedor.
- A identidade externa é vinculada pelo identificador estável fornecido pelo provedor, não apenas pelo e-mail.
- O e-mail só pode vincular automaticamente uma conta existente quando o provedor afirmar que ele foi verificado.
- Uma conta criada somente por SSO não precisa possuir senha local.
- O sistema não armazena a senha do Google ou da Apple.
- Entrar com SSO não concede acesso automático a nenhuma organização.

### UC-AUTH-09 — Vincular Google ou Apple a uma conta existente

**Atores:** usuário autenticado.

**Objetivo:** adicionar uma forma alternativa de login à própria conta.

**Regras:**

- O usuário deve confirmar novamente sua identidade quando necessário.
- A mesma identidade externa não pode estar vinculada a duas contas.
- A vinculação deve ser auditada.

### UC-AUTH-10 — Usar autenticação multifator

**Atores:** todos os usuários.

**Objetivo:** proteger a conta com um segundo fator de autenticação.

**Regras:**

- O usuário pode ativar ou desativar o método após confirmar sua identidade.
- A organização pode exigir autenticação multifator para papéis administrativos.
- Segredos de autenticação nunca são armazenados em texto puro.

### UC-AUTH-11 — Recuperar acesso

**Atores:** usuários com credencial local.

**Objetivo:** recuperar a conta quando a senha for esquecida.

**Regras:**

- A recuperação deve usar um mecanismo temporário e de uso único.
- A resposta não deve revelar se um e-mail está cadastrado.
- A troca de senha pode revogar sessões anteriores por segurança.

### UC-AUTH-12 — Encerrar sessão

**Atores:** todos os usuários.

**Objetivo:** sair do sistema ou revogar uma sessão ativa.

**Regras:**

- O token de renovação da sessão deve ser revogado.
- O usuário pode continuar conectado em outros dispositivos, salvo quando escolher encerrar todas as sessões.

## 2. Organização

### UC-ORG-01 — Visualizar organização

**Atores:** todos os membros ativos.

**Objetivo:** consultar os dados básicos da organização atual.

**Regras:**

- Cada usuário visualiza somente organizações às quais pertence.
- Informações administrativas podem ser ocultadas de Manager e Operator.

### UC-ORG-02 — Atualizar organização

**Atores:** Owner e Admin.

**Objetivo:** alterar nome, dados legais, fuso horário e localização padrão.

**Regras:**

- Alterações devem ser auditadas.
- O fuso horário é usado nos agendamentos e registros da organização.
- A localização é opcional e serve como padrão quando o setor não possuir localização própria.

### UC-ORG-03 — Consultar ou renovar código de acesso

**Atores:** Owner e Admin.

**Objetivo:** consultar ou substituir o código usado para localizar a organização no cadastro.

**Regras:**

- O código deve ser único.
- Renovar o código invalida o anterior.
- O código não concede papel nem substitui autenticação ou aprovação.

### UC-ORG-04 — Gerenciar administradores

**Atores:** Owner.

**Objetivo:** conceder ou remover o papel de Admin.

**Regras:**

- Admin não pode promover outro usuário a Owner ou Admin.
- A organização deve manter ao menos um Owner ativo.
- Toda alteração de papel deve ser auditada.

### UC-ORG-05 — Alternar organização

**Atores:** usuários pertencentes a mais de uma organização.

**Objetivo:** escolher em qual organização deseja trabalhar.

**Regras:**

- Papéis, setores, turnos e permissões são resolvidos novamente ao alternar.
- Nenhum dado da organização anterior pode permanecer acessível fora de seu contexto.

## 3. Pessoas e organograma

### UC-MEMBER-01 — Listar membros

**Atores:** Owner, Admin e Manager.

**Objetivo:** consultar pessoas da organização e seus papéis.

**Regras:**

- Owner e Admin podem visualizar toda a organização.
- Manager visualiza apenas sua equipe e os membros de seus setores.

### UC-MEMBER-02 — Adicionar ou aprovar membro

**Atores:** Owner e Admin.

**Objetivo:** permitir que um usuário participe da organização e definir seu papel.

**Regras:**

- Owner pode criar ou aprovar qualquer papel.
- Admin pode criar ou aprovar Manager e Operator.
- O papel escolhido pelo próprio usuário durante o cadastro não é concedido automaticamente.
- O mesmo usuário não pode possuir dois vínculos com a mesma organização.

### UC-MEMBER-03 — Alterar papel

**Atores:** Owner e Admin.

**Objetivo:** promover ou rebaixar um membro conforme sua responsabilidade.

**Regras:**

- Apenas Owner gerencia Admin.
- Admin gerencia Manager e Operator.
- A alteração não pode deixar a organização sem Owner.
- A alteração deve ser auditada.

### UC-MEMBER-04 — Suspender ou reativar membro

**Atores:** Owner e Admin.

**Objetivo:** bloquear ou restaurar o acesso de um membro à organização.

**Regras:**

- Suspensão não apaga registros históricos.
- O membro suspenso não pode iniciar novas execuções.
- Registros já realizados continuam associados ao membro original.
- Admin não pode suspender Owner.

### UC-MEMBER-05 — Remover membro

**Atores:** Owner e Admin.

**Objetivo:** encerrar a participação de um usuário na organização.

**Regras:**

- A remoção é lógica e preserva o histórico.
- Associações futuras a setores, turnos e agendamentos devem deixar de produzir novas atribuições.
- Owner não pode ser removido enquanto for o único Owner ativo.

### UC-MEMBER-06 — Definir líder direto

**Atores:** Owner, Admin e Manager dentro de sua equipe.

**Objetivo:** configurar o organograma e a relação direta entre líder e subordinado.

**Regras:**

- Líder e subordinado devem pertencer à mesma organização.
- Um membro não pode ser líder de si mesmo.
- A relação não pode formar ciclos.
- O organograma é opcional.
- O sistema pode usar o líder direto para sugerir a cadeia de escalada.

### UC-MEMBER-07 — Visualizar organograma

**Atores:** Owner, Admin e Manager.

**Objetivo:** visualizar a hierarquia de pessoas da organização.

**Regras:**

- Owner e Admin visualizam a árvore completa.
- Manager visualiza sua equipe e o contexto necessário de seus superiores.
- Membros sem líder definido continuam utilizáveis no sistema.

## 4. Setores

### UC-SECTOR-01 — Criar setor

**Atores:** Owner e Admin.

**Objetivo:** criar uma área operacional dentro da organização.

**Regras:**

- O setor pode ser raiz ou filho de outro setor.
- O setor pai deve pertencer à mesma organização.
- A hierarquia não pode formar ciclos.
- A localização própria é opcional.

### UC-SECTOR-02 — Atualizar setor

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** alterar nome, descrição, posição hierárquica ou localização.

**Regras:**

- Manager altera somente setores sob sua responsabilidade quando possuir permissão.
- Mover o setor não pode criar ciclos.
- Alterar a localização não modifica resultados geográficos já registrados.

### UC-SECTOR-03 — Desativar ou reativar setor

**Atores:** Owner e Admin.

**Objetivo:** retirar temporariamente um setor da operação sem apagar seu histórico.

**Regras:**

- Setor desativado não recebe novos agendamentos ou membros.
- Registros anteriores permanecem disponíveis.
- A desativação de um setor com filhos deve ser tratada explicitamente pelo usuário.

### UC-SECTOR-04 — Associar membro ao setor

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** definir em quais setores um Manager ou Operator atua.

**Regras:**

- O membro e o setor devem pertencer à mesma organização.
- Manager associa apenas membros de sua equipe e de seus setores.
- Um membro pode participar de vários setores.

### UC-SECTOR-05 — Resolver localização efetiva

**Atores:** System.

**Objetivo:** determinar a localização e o raio aplicáveis a uma execução.

**Regras:**

- O sistema usa primeiro a localização do setor da execução.
- Na ausência dela, procura o primeiro setor ancestral com localização.
- Na ausência de localização nos setores, usa a organização.
- Latitude, longitude e raio são herdados como uma única configuração.
- Se nenhuma localização estiver configurada, não há restrição geográfica.

## 5. Turnos

### UC-SHIFT-01 — Criar turno

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** criar um turno para um setor.

**Regras:**

- Todo turno pertence a um único setor.
- O turno define horário, recorrência e período de validade.
- Horários que atravessam a meia-noite devem ser interpretados no fuso da organização.

### UC-SHIFT-02 — Atualizar turno

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** alterar nome, horários, recorrência ou vigência.

**Regras:**

- Alterações afetam somente atribuições futuras.
- Registros históricos mantêm os horários com os quais foram gerados.

### UC-SHIFT-03 — Desativar ou reativar turno

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** retirar ou recolocar um turno na operação.

**Regras:**

- Turnos desativados não geram novas atribuições.
- O histórico do turno permanece disponível.

### UC-SHIFT-04 — Associar membro ao turno

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** alocar managers e operators em um turno durante um período.

**Regras:**

- O membro deve estar associado ao setor do turno.
- A associação pode ter início e término.
- Um membro pode participar de vários turnos quando não houver conflito operacional definido pela organização.

## 6. Checklists e tarefas

### UC-CHECKLIST-01 — Listar checklists

**Atores:** Owner, Admin e Manager.

**Objetivo:** consultar processos por status, setor, criador ou versão.

**Regras:**

- Owner e Admin consultam todos os checklists da organização.
- Manager consulta checklists relacionados aos seus setores e os que criou.
- Rascunhos e versões arquivadas não aparecem para Operator.

### UC-CHECKLIST-02 — Criar checklist

**Atores:** Owner, Admin e Manager.

**Objetivo:** criar um processo em rascunho.

**Regras:**

- O checklist começa como `Draft`.
- O criador deve pertencer à organização.
- O checklist precisa de nome e ao menos uma tarefa antes da publicação.

### UC-CHECKLIST-03 — Configurar tarefa

**Atores:** Owner, Admin e Manager criador.

**Objetivo:** adicionar ou alterar uma tarefa enquanto o checklist está em rascunho.

**Regras:**

- A tarefa possui nome, instruções, ordem e obrigatoriedade.
- Uma tarefa pode solicitar mais de um tipo de entrada.
- Tipos iniciais: booleano, texto, número, seleção, foto, vídeo, data, horário, data e horário e assinatura.
- Configurações específicas podem definir opções, valor esperado, mínimo, máximo, unidade e comportamento da observação.
- O valor esperado é somente leitura durante a execução.
- A tarefa pode exigir localização.
- A tarefa pode possuir material explicativo.

### UC-CHECKLIST-04 — Ordenar ou remover tarefa

**Atores:** Owner, Admin e Manager criador.

**Objetivo:** organizar o fluxo do checklist em rascunho.

**Regras:**

- A ordem deve ser única dentro do checklist.
- Apenas checklists em rascunho podem ser alterados diretamente.
- Remover uma tarefa de uma nova versão não altera versões publicadas anteriormente.

### UC-CHECKLIST-05 — Configurar níveis de escalada

**Atores:** Owner, Admin e Manager criador.

**Objetivo:** definir quem será avisado e após quanto tempo uma pendência sobe de nível.

**Regras:**

- Os níveis são ordenados e não podem se repetir.
- Cada nível possui um responsável e um tempo de espera.
- O organograma pode sugerir os responsáveis, mas o usuário autorizado confirma a configuração.
- Todos os responsáveis devem pertencer à organização.

### UC-CHECKLIST-06 — Enviar checklist para aprovação

**Atores:** Manager.

**Objetivo:** solicitar a aprovação de um checklist criado ou alterado.

**Regras:**

- O checklist deve estar válido e completo.
- O status muda de `Draft` para `PendingApproval`.
- O checklist não pode ser executado antes da publicação.

### UC-CHECKLIST-07 — Aprovar checklist

**Atores:** Owner e Admin.

**Objetivo:** validar um checklist pendente e autorizá-lo para publicação.

**Regras:**

- O sistema registra aprovador e horário.
- A aprovação deve ser auditada.
- Owner e Admin podem publicar diretamente checklists próprios válidos.

### UC-CHECKLIST-08 — Devolver checklist para ajustes

**Atores:** Owner e Admin.

**Objetivo:** retornar um checklist pendente ao criador para correções.

**Regras:**

- O checklist retorna para `Draft`.
- A devolução não publica nem cria agendamentos.
- A ação deve ser auditada.

### UC-CHECKLIST-09 — Publicar checklist

**Atores:** Owner e Admin.

**Objetivo:** tornar uma versão disponível para agendamento e execução.

**Regras:**

- O checklist deve possuir ao menos uma tarefa válida.
- A publicação registra data e responsável pela ação.
- Uma versão publicada é imutável.
- Apenas versões publicadas podem gerar novas execuções.

### UC-CHECKLIST-10 — Criar nova versão

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** alterar um processo já publicado sem modificar seu histórico.

**Regras:**

- O sistema cria um novo rascunho ligado à versão anterior.
- Tarefas e configurações são copiadas para o novo rascunho.
- Registros anteriores continuam apontando para a versão executada.
- A nova versão segue o mesmo fluxo de aprovação e publicação.

### UC-CHECKLIST-11 — Arquivar checklist

**Atores:** Owner e Admin.

**Objetivo:** impedir o uso futuro de uma versão sem apagar seu histórico.

**Regras:**

- Versões arquivadas não podem gerar novos agendamentos.
- Agendamentos ativos devem ser desativados ou migrados conscientemente.
- Registros históricos permanecem disponíveis.

## 7. Agendamentos e atribuições

### UC-SCHEDULE-01 — Criar agendamento

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** definir quando e onde um checklist publicado deve acontecer.

**Regras:**

- O agendamento pertence a um checklist publicado e a um setor.
- O turno é opcional.
- A frequência pode ser diária, semanal, mensal ou em data específica.
- O horário é interpretado no fuso da organização.
- O período de validade é obrigatório no início e opcional no fim.

### UC-SCHEDULE-02 — Definir executores específicos

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** selecionar membros específicos para executar um agendamento.

**Regras:**

- Todos os executores devem estar ativos e vinculados ao contexto permitido.
- Quando houver executores específicos, o sistema gera uma execução para cada um.
- Quando não houver, o sistema resolve os executores pelo setor e, quando informado, pelo turno.

### UC-SCHEDULE-03 — Atualizar agendamento

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** alterar horário, recorrência, vigência, turno ou executores.

**Regras:**

- Alterações afetam ocorrências futuras.
- Registros já gerados não são reescritos automaticamente.
- O checklist associado deve continuar publicado e ativo.

### UC-SCHEDULE-04 — Desativar ou reativar agendamento

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** interromper ou retomar a geração de execuções.

**Regras:**

- Desativar não apaga registros já gerados.
- Reativar passa a valer para as próximas ocorrências dentro da vigência.

### UC-SCHEDULE-05 — Gerar execuções previstas

**Atores:** System.

**Objetivo:** criar os registros que representam checklists esperados para cada horário e executor.

**Regras:**

- Cada ocorrência deve apontar para a versão publicada correta.
- A geração deve ser idempotente e não pode duplicar uma ocorrência.
- A execução nasce com status `Pending`.
- O horário limite considera a tolerância configurada.

## 8. Execução operacional

### UC-EXEC-01 — Visualizar checklists do dia

**Atores:** qualquer membro com execução atribuída.

**Objetivo:** consultar checklists pendentes, em andamento, atrasados e concluídos.

**Regras:**

- O usuário visualiza somente execuções que pode realizar ou acompanhar.
- Atrasados e urgentes devem ser priorizados.
- A lista informa prazo, setor, turno, status e quantidade de tarefas.

### UC-EXEC-02 — Iniciar checklist

**Atores:** membro autorizado para a execução.

**Objetivo:** iniciar o preenchimento de um checklist disponível.

**Regras:**

- A execução deve estar disponível e não concluída ou cancelada.
- O sistema registra executor e horário de início.
- A localização inicial é capturada quando exigida.
- Um usuário não pode assumir execução atribuída exclusivamente a outro sem autorização.

### UC-EXEC-03 — Responder tarefa

**Atores:** executor da execução.

**Objetivo:** preencher as entradas exigidas por uma tarefa.

**Regras:**

- Os valores devem respeitar os tipos e configurações da tarefa.
- O executor não pode alterar valor esperado, mínimo, máximo ou unidade.
- A resposta pode combinar, por exemplo, número, foto e observação.
- O sistema registra responsável e horário.
- Uma tarefa obrigatória só é concluída quando todas as entradas obrigatórias estiverem válidas.

### UC-EXEC-04 — Adicionar observação

**Atores:** executor da execução.

**Objetivo:** fornecer contexto adicional sobre uma resposta.

**Regras:**

- A observação pode ser opcional, sempre obrigatória ou obrigatória apenas em divergência, conforme a tarefa.
- A interface deve usar linguagem neutra e não acusatória.

### UC-EXEC-05 — Capturar foto ou vídeo

**Atores:** executor da execução.

**Objetivo:** registrar evidência visual solicitada pela tarefa.

**Regras:**

- O arquivo deve pertencer à mesma organização da execução.
- O conteúdo fica em armazenamento privado.
- Quando a tarefa exigir evidência capturada no momento, o frontend não deve aceitar seleção da galeria.
- O envio só é considerado válido após o arquivo estar disponível no armazenamento.

### UC-EXEC-06 — Validar localização

**Atores:** System.

**Objetivo:** verificar se a execução ocorreu dentro da área permitida.

**Regras:**

- A validação usa a localização efetiva do setor ou da organização.
- A coordenada capturada é preservada no registro.
- O resultado da validação também é preservado e não muda quando a configuração futura mudar.
- A ausência de geofence não impede a execução.

### UC-EXEC-07 — Calcular divergência

**Atores:** System.

**Objetivo:** classificar uma resposta comparando-a com a configuração da tarefa.

**Regras:**

- O resultado pode estar dentro do esperado, diferente do esperado, abaixo do mínimo ou acima do máximo.
- Valores abaixo do mínimo ou acima do máximo podem ser considerados críticos.
- Divergência pode tornar observação ou evidência obrigatória.
- Divergência crítica pode iniciar escalada imediatamente.
- O sistema utiliza o termo “divergência”, nunca “erro” ou “desvio”, na comunicação operacional.

### UC-EXEC-08 — Concluir checklist

**Atores:** executor da execução.

**Objetivo:** entregar o checklist preenchido.

**Regras:**

- Todas as tarefas obrigatórias devem estar válidas.
- O sistema registra executor, horário e localização final quando exigida.
- O status será `Completed` ou `CompletedWithIssues`, conforme as divergências.
- Após a conclusão, respostas não podem ser alteradas livremente.
- A conclusão disponibiliza a execução para verificação quando aplicável.

### UC-EXEC-09 — Marcar execução como perdida

**Atores:** System.

**Objetivo:** identificar checklists não concluídos dentro do prazo aplicável.

**Regras:**

- O sistema considera horário previsto, tolerância e estado atual.
- O registro passa para `Missed` quando não puder mais ser entregue conforme a regra do processo.
- O atraso ou perda pode iniciar escalada e notificação.

### UC-EXEC-10 — Cancelar execução

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** cancelar uma ocorrência que não deve mais ser executada.

**Regras:**

- Apenas execuções não concluídas podem ser canceladas.
- O cancelamento deve ser auditado.
- Cancelar uma ocorrência não desativa o agendamento inteiro.

## 9. Verificação

### UC-REVIEW-01 — Visualizar fila de verificação

**Atores:** Owner, Admin e Manager.

**Objetivo:** consultar execuções e tarefas que aguardam conferência.

**Regras:**

- Manager visualiza apenas seus setores e sua equipe.
- Divergências críticas e verificações atrasadas são priorizadas.
- A fila informa executor, horário, setor, respostas e evidências.

### UC-REVIEW-02 — Conferir resposta

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** analisar valor, observação, localização e evidências de uma tarefa.

**Regras:**

- O verificador não altera a resposta original.
- O sistema preserva o executor e o horário da resposta.
- O acesso a arquivos deve respeitar organização e escopo do verificador.

### UC-REVIEW-03 — Confirmar tarefa

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** registrar que uma tarefa foi verificada e aceita.

**Regras:**

- O sistema registra verificador e horário.
- A confirmação muda a verificação da tarefa para `Confirmed`.
- A ação deve ser auditada.

### UC-REVIEW-04 — Escalar tarefa

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** encaminhar uma tarefa para o próximo responsável quando exigir atenção superior.

**Regras:**

- A tarefa muda para `Escalated`.
- O sistema registra a escalada e notifica o responsável configurado.
- A resposta original permanece inalterada.
- A ação deve ser auditada.

### UC-REVIEW-05 — Finalizar verificação do checklist

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** aprovar ou rejeitar a execução após revisar suas tarefas.

**Regras:**

- O sistema registra verificador, horário, resultado e observações.
- Uma execução pode ser aprovada quando todas as tarefas exigidas estiverem confirmadas.
- Tarefas escaladas impedem aprovação final enquanto permanecerem pendentes de tratamento.

## 10. Escaladas

### UC-ESC-01 — Escalar por atraso na entrega

**Atores:** System.

**Objetivo:** avisar os responsáveis quando uma execução não for concluída no prazo.

**Regras:**

- O primeiro aviso pode ser direcionado ao executor.
- Os níveis seguintes respeitam a ordem e o tempo configurados.
- Cada nível é registrado uma única vez por ocorrência.

### UC-ESC-02 — Escalar por atraso na verificação

**Atores:** System.

**Objetivo:** avisar responsáveis quando uma execução entregue não for verificada no prazo.

**Regras:**

- A cadeia configurada determina destinatários e intervalos.
- A escalada é interrompida quando a verificação necessária é concluída.
- Cada disparo deve ser registrado.

### UC-ESC-03 — Escalar divergência crítica

**Atores:** System.

**Objetivo:** alertar imediatamente sobre valores abaixo do mínimo, acima do máximo ou outra condição crítica.

**Regras:**

- O alerta crítico não precisa aguardar o prazo normal do nível.
- A divergência e a tarefa de origem devem ser identificáveis.
- O destinatário recebe notificação interna e, quando configurado, e-mail.

### UC-ESC-04 — Consultar histórico de escaladas

**Atores:** Owner, Admin e Manager autorizado.

**Objetivo:** acompanhar quando, por que e para quem uma pendência foi escalada.

**Regras:**

- Manager consulta somente escaladas de seus setores.
- O histórico não é substituído por logs técnicos.
- Escaladas alimentam indicadores operacionais.

## 11. Notificações

### UC-NOTIFY-01 — Receber notificação

**Atores:** todos os membros ativos.

**Objetivo:** ser informado sobre atribuições, atrasos, divergências, verificações e eventos do sistema.

**Regras:**

- A notificação pertence a um único membro e organização.
- Quando aplicável, a notificação referencia o registro relacionado.
- Alertas críticos podem também ser enviados por e-mail.

### UC-NOTIFY-02 — Consultar central de notificações

**Atores:** todos os membros ativos.

**Objetivo:** visualizar notificações próprias.

**Regras:**

- O usuário visualiza apenas suas notificações.
- A lista pode ser filtrada por alerta, checklist, divergência e sistema.
- Não lidas são destacadas e podem ser agrupadas por período.

### UC-NOTIFY-03 — Abrir notificação

**Atores:** destinatário da notificação.

**Objetivo:** marcar a notificação como lida e acessar seu contexto.

**Regras:**

- A referência só pode ser aberta quando o usuário possuir acesso ao recurso.
- Abrir uma notificação registra a data de leitura.

### UC-NOTIFY-04 — Marcar todas como lidas

**Atores:** todos os membros ativos.

**Objetivo:** encerrar o estado de não leitura das notificações próprias.

**Regras:**

- A ação afeta somente o usuário e a organização atuais.
- As notificações não são apagadas.

## 12. Dashboard e relatórios

### UC-DASH-01 — Visualizar resumo operacional

**Atores:** Owner, Admin e Manager.

**Objetivo:** acompanhar execuções entregues, pendentes, atrasadas, divergentes e verificadas.

**Regras:**

- Owner e Admin visualizam toda a organização.
- Manager visualiza apenas seus setores.
- Os indicadores são calculados a partir dos registros operacionais.

### UC-DASH-02 — Filtrar período e contexto

**Atores:** Owner, Admin e Manager.

**Objetivo:** analisar dados por período, setor, turno, checklist ou membro.

**Regras:**

- Períodos iniciais incluem hoje, semana e mês.
- Filtros nunca ampliam o acesso além do escopo do usuário.
- Datas são apresentadas no fuso da organização.

### UC-DASH-03 — Consultar indicadores

**Atores:** Owner, Admin e Manager.

**Objetivo:** acompanhar pontualidade, divergências, escaladas e aprovação.

**Regras:**

- Os cálculos devem ser reproduzíveis a partir de `ChecklistRecord`, `TaskRecord` e `EscalationRecord`.
- Indicadores não alteram os registros de origem.
- A definição de cada indicador deve ser consistente em todas as telas.

### UC-DASH-04 — Analisar atrasos e desempenho

**Atores:** Owner, Admin e Manager.

**Objetivo:** identificar setores, turnos, processos e membros com atrasos ou resultados recorrentes.

**Regras:**

- Comparações respeitam o mesmo período e contexto.
- Resultados devem ser apresentados como apoio à gestão, não como alteração dos registros.
- Manager visualiza somente pessoas sob seu escopo.

### UC-DASH-05 — Identificar divergências recorrentes

**Atores:** Owner, Admin e Manager.

**Objetivo:** encontrar tarefas, itens ou períodos com padrões de divergência.

**Regras:**

- A análise utiliza os valores e classificações já registrados nas tarefas.
- Não é necessária uma entidade própria de relatório.
- O sistema pode agrupar resultados por checklist, tarefa, setor, turno e período.

### UC-DASH-06 — Consultar o próprio histórico

**Atores:** Operator.

**Objetivo:** consultar suas execuções concluídas e respectivos estados.

**Regras:**

- Operator visualiza apenas registros que executou ou que foram atribuídos a ele.
- Dados de outros operadores não são exibidos.

## 13. Arquivos e evidências

### UC-FILE-01 — Enviar arquivo

**Atores:** membros autorizados.

**Objetivo:** adicionar instrução ou evidência a uma tarefa.

**Regras:**

- O arquivo é enviado para armazenamento privado.
- Apenas metadados e a chave do objeto ficam no banco relacional.
- Tipo e tamanho devem ser validados antes da aceitação.
- O arquivo deve pertencer à mesma organização do recurso relacionado.

### UC-FILE-02 — Visualizar arquivo

**Atores:** membros autorizados ao checklist ou registro relacionado.

**Objetivo:** acessar uma instrução ou evidência.

**Regras:**

- O sistema valida a autorização antes de liberar o conteúdo.
- O acesso deve usar mecanismo temporário ou fluxo autenticado.
- Arquivos não possuem endereço público permanente.

## 14. Auditoria e segurança

### UC-AUDIT-01 — Registrar ação auditável

**Atores:** System.

**Objetivo:** preservar evidência de ações relevantes de segurança e negócio.

**Regras:**

- Devem ser auditados login, acesso negado, mudança de papel, alteração estrutural, aprovação, publicação, cancelamento, conclusão, verificação e escalada manual.
- O registro identifica ação, usuário, entidade, organização, data, IP e correlação quando disponíveis.
- Senhas, tokens, segredos e conteúdo sensível desnecessário não são registrados.
- Auditoria é imutável.

### UC-AUDIT-02 — Consultar auditoria

**Atores:** Owner e Admin autorizado.

**Objetivo:** investigar alterações e ações relevantes da organização.

**Regras:**

- A consulta nunca ultrapassa o limite da organização.
- O acesso à auditoria também deve ser auditado quando necessário.
- Logs técnicos não substituem registros de auditoria.

### UC-SEC-01 — Aplicar autorização

**Atores:** System.

**Objetivo:** permitir cada ação somente ao usuário autorizado.

**Regras:**

- A verificação considera usuário, organização, papel, setor, turno e vínculo ativo.
- Toda operação lê e grava dados dentro da organização atual.
- Identificadores enviados pelo cliente nunca são considerados autorização suficiente.
- A autorização é validada no backend, independentemente da interface.

### UC-SEC-02 — Proteger dados e sessões

**Atores:** System.

**Objetivo:** proteger credenciais, sessões, arquivos e informações organizacionais.

**Regras:**

- Senhas são armazenadas somente como hash seguro.
- Tokens persistidos são armazenados como hash e podem ser revogados.
- Segredos de MFA são criptografados.
- Arquivos permanecem privados.
- Logs não devem conter senhas, tokens ou respostas sensíveis.

## 15. Regras gerais de experiência

- A experiência do Operator deve ser mobile-first, direta e utilizável com poucos toques.
- Ações principais devem permanecer claras em telas pequenas.
- Tarefas obrigatórias incompletas impedem a entrega.
- Valores esperados são visíveis, mas nunca editáveis durante a execução.
- A linguagem deve ser humana, simples, positiva e não acusatória.
- Deve-se usar “divergência”, “diferente do esperado” ou “fora do padrão”, evitando “erro”, “culpa” e “desvio”.
- Confirmações de conclusão devem informar claramente que os dados foram entregues.
