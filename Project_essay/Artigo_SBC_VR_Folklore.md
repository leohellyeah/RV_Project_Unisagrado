# Artigo SBC — Explorando Histórias

> **Como usar este arquivo:** cole as seções abaixo, na ordem, no template oficial SBC em Word. As citações estão no formato `[Autor Ano]` (padrão SBC). As referências no final estão prontas pra colar na seção "Referências". Os blocos `[FIGURA X — placeholder]` indicam onde inserir imagens/diagramas — substitua por screenshots ou figuras quando definir.

---

## Sugestões de título (escolher uma)

1. **Explorando Histórias: Desenvolvimento de uma Experiência em Realidade Virtual com Narrativas Folclóricas Continentais** *(opção atual)*
2. **Explorando Histórias: Protótipo Desktop de uma Experiência Educacional em Realidade Virtual sobre Folclore Mundial**
3. **Folclores em Realidade Virtual: Arquitetura e Implementação de uma Experiência Interativa Multicultural**
4. **Storytelling Cultural em Realidade Virtual: Desenvolvimento do Protótipo Explorando Histórias**

---

## Autores e filiação

**Leonardo Buratto de Assis, Jennifer Leonora Galina Vieira, Leonardo Conti**

Centro Universitário Sagrado Coração (Unisagrado) — Bauru — SP — Brasil

---

## Abstract

This paper presents the development of *Explorando Histórias*, an interactive Virtual Reality experience designed for educational and cultural narrative purposes. The application is built in Unity 2022.3 LTS and allows a user, in a first-person perspective, to visit four scenes inspired by the European medieval period, Brazilian folklore, Japanese culture and ancient Egypt. In each scene, a non-player character (NPC) presents a narrative through spatial audio, while the user is able to interrupt and resume the storytelling. The work focuses on the architectural decisions that sustain the prototype: a state-machine-based dialogue system, a raycast-based interaction layer, a wrapper prefab pattern to manage GLB assets imported from Blender, and a portal-based scene navigation mechanism. The result is a functional desktop-first prototype intended as a foundation for a future immersive VR layer.

**Keywords:** Virtual Reality, Educational Games, Interactive Storytelling, Unity, Folklore.

## Resumo

Este trabalho apresenta o desenvolvimento do projeto *Explorando Histórias*, uma experiência interativa em Realidade Virtual voltada para fins educacionais e narrativos. A aplicação foi desenvolvida na engine Unity 2022.3 LTS e permite que o usuário, em perspectiva de primeira pessoa, visite quatro cenas inspiradas no período medieval europeu, no folclore brasileiro, na cultura japonesa e no Egito antigo. Em cada cena, um personagem não jogável (NPC) apresenta uma narrativa por meio de áudio espacial, sendo possível ao usuário interromper e retomar a contação. O trabalho enfatiza decisões arquiteturais: um sistema de diálogo baseado em máquina de estados, uma camada de interação por raycasting, um padrão de wrapper prefab para gestão de modelos GLB importados do Blender e um mecanismo de navegação entre cenas por meio de portais. O resultado é um protótipo funcional desktop-first, concebido como base para uma futura camada imersiva em RV.

**Palavras-chave:** Realidade Virtual, Jogos Educacionais, Narrativa Interativa, Unity, Folclore.

---

## 1. Introdução

A Realidade Virtual (RV) tem sido investigada como ferramenta para mediação de experiências educacionais e culturais, oferecendo formas de engajamento que rompem com a passividade típica de mídias tradicionais [Burdea e Coiffet 2003]. Ao colocar o usuário em primeira pessoa dentro de um ambiente tridimensional interativo, a RV explora o potencial cognitivo da imersão e da agência — dois atributos que, segundo [Murray 1997], caracterizam de forma particular a narrativa em ambientes computacionais.

Inserido nesse contexto, este trabalho apresenta o projeto *Explorando Histórias*, uma experiência interativa em RV cuja proposta é apresentar elementos culturais e folclóricos de quatro regiões do mundo — Europa medieval, Brasil, Japão e Egito antigo — por meio de personagens-narradores. Cada continente é representado em uma cena independente, na qual um NPC temático conta uma história ao usuário ao ser interpelado. A interação é deliberadamente simples: aproximar-se, mirar e ouvir.

> **[FIGURA 1 — placeholder]** Screenshot da visão geral do Lobby central, mostrando o jogador em primeira pessoa e os quatro portais identificados pelas etiquetas Europa, Ásia, África e América. *Sugestão: capturar in-game durante o Play Mode no Unity.*

O escopo está delimitado a uma versão *desktop-first*: a camada de interação utiliza mouse e teclado, e a câmera é controlada em primeira pessoa. Essa decisão permitiu concentrar esforço em três pilares: (i) a *arquitetura do sistema de narração*, capaz de iniciar, interromper, retomar e reiniciar histórias de forma coerente; (ii) a *integração de assets externos*, com foco em modelos exportados em formato GLB a partir do Blender; e (iii) a *navegação entre cenas*, mediada por portais interativos posicionados em uma cena central (*Lobby*). Uma camada imersiva em RV, com suporte a headsets, é prevista como evolução futura.

A motivação principal do projeto não está em demonstrar a fronteira técnica da RV, mas em construir uma base arquitetural sólida sobre a qual conteúdo educacional possa ser plugado: cada cena segue um mesmo padrão estrutural, variando apenas o ambiente, o modelo do NPC e os áudios das narrativas. Tal abordagem prioriza, conforme a filosofia adotada no projeto, *funcionar antes de embelezar* — pressuposto que orientou todas as decisões de escopo descritas adiante.

A organização deste artigo se dá da seguinte forma. A Seção 2 contextualiza o trabalho frente à literatura sobre RV educacional e narrativa interativa. A Seção 3 descreve a metodologia adotada e a divisão de atividades. A Seção 4 detalha a implementação dos sistemas centrais. A Seção 5 apresenta os resultados obtidos no protótipo. A Seção 6 traz as considerações finais e identifica trabalhos futuros.

## 2. Fundamentação Teórica e Trabalhos Relacionados

### 2.1. Realidade Virtual e Educação

A Realidade Virtual pode ser entendida como um meio caracterizado pela combinação de três elementos fundamentais — imersão, interatividade e imaginação — os chamados "três Is" [Burdea e Coiffet 2003]. No contexto educacional, esses elementos têm sido explorados em aplicações que vão da simulação procedimental ao treinamento de habilidades comportamentais, com evidências de que ambientes imersivos podem aumentar o engajamento e a retenção de conteúdo [Freina e Ott 2015].

Especificamente em RV narrativa, a literatura discute o conceito de *agência* enquanto sensação do usuário de que suas ações têm efeito significativo sobre o mundo [Murray 1997]. Mesmo em experiências contemplativas — nas quais o usuário não é protagonista de uma trama complexa — pequenos atos de interação, como aproximar-se, mirar e escolher quando iniciar uma narração, já contribuem para essa sensação. Esse é precisamente o tipo de agência mínima que orienta o design do *Explorando Histórias*: o usuário não decide o conteúdo da história, mas decide quando, em que ritmo e por quanto tempo escutá-la.

### 2.2. Narrativa em Jogos e Storytelling Interativo

A literatura em design de jogos discute o equilíbrio entre regras, narrativa e experiência. [Schell 2008] propõe quatro elementos básicos — mecânica, estética, história e tecnologia — que devem dialogar de forma coerente. Em jogos de foco narrativo, a história não precisa ser ramificada para gerar engajamento; o engajamento pode emergir do controle temporal que o jogador exerce sobre a apresentação da narrativa, como ocorre neste trabalho.

[Salen e Zimmerman 2003] discutem o conceito de *jogo significativo* (meaningful play), no qual cada ação do jogador é discernível e integrada ao sistema. Aplicado ao *Explorando Histórias*, esse princípio orienta o design da interação: a tecla de interação tem sempre uma resposta clara e contextualmente coerente — iniciar a história, interrompê-la (com um SFX de reação do NPC) ou retomá-la (com um SFX de retomada) — sem ambiguidade quanto ao estado atual do sistema.

### 2.3. Trabalhos Relacionados

Trabalhos acadêmicos brasileiros têm explorado a RV em contextos educacionais. [Nipo et al. 2023] apresentam o *Robo-Think*, um jogo em RV para o ensino de habilidades de pensamento computacional, no qual o usuário interage com elementos tridimensionais em uma narrativa estruturada para gerar engajamento pedagógico. O trabalho ilustra como ambientes imersivos podem ser empregados para internalizar conteúdo de forma mais ativa do que abordagens tradicionais baseadas em texto.

O presente projeto se diferencia em três pontos principais: (i) adota a perspectiva *desktop-first* como decisão de escopo, focando inicialmente na arquitetura e na qualidade do conteúdo narrativo, e reservando a adaptação para hardware imersivo como etapa subsequente; (ii) emprega vozes sintéticas geradas por TTS neural como mecanismo de produção rápida de narrativas em português brasileiro, sem dependência de gravações profissionais; e (iii) explora o folclore comparado entre continentes como eixo curatorial, em vez de focar em um único contexto cultural ou disciplinar.

## 3. Metodologia

### 3.1. Visão Geral do Processo

O desenvolvimento seguiu uma abordagem incremental orientada por entregas funcionais. Em vez de buscar um produto polido em um único ciclo, o projeto foi estruturado em iterações: cada iteração entregava uma capacidade nova e testável (movimentação básica, interação por raycast, máquina de estados de narração, integração de áudio, navegação entre cenas), permitindo validação contínua antes de avançar ao próximo bloco.

Os entregáveis intermediários foram organizados em três frentes que progrediram em paralelo: (i) construção dos *sistemas técnicos reutilizáveis* — movimentação do jogador, sistema de interação e máquina de estados de narração; (ii) *produção de conteúdo* — roteiros narrativos das histórias, geração dos áudios em TTS e seleção dos modelos 3D; e (iii) *montagem das cenas*, com integração dos dois pilares anteriores em ambientes coerentes com cada cultura.

### 3.2. Stack Tecnológico

A engine escolhida foi o Unity 2022.3 LTS, na versão 2022.3.62f3, utilizando o *Built-in Render Pipeline*. A escolha priorizou estabilidade, ampla compatibilidade com assets de terceiros disponíveis na Asset Store e a curva de aprendizado já consolidada da equipe. O Unity foi também critério de viabilidade para a evolução futura prevista: o ecossistema de RV da engine, via XR Toolkit, oferece suporte nativo aos principais dispositivos imersivos do mercado.

A modelagem 3D dos personagens foi conduzida no Blender, com exportação para o formato **GLB**. Esse formato foi escolhido por ser leve, portátil e suportado nativamente pelo Unity por meio do *Scripted Importer* de glTF. A organização do projeto contempla a futura migração para o formato FBX, caso seja necessária maior fidelidade em *rigging*, *UV mapping* e *texturing*.

A produção dos áudios das narrativas foi realizada com a ferramenta **ElevenLabs**, que oferece síntese de voz neural em português brasileiro com timbres distintos. Cada voz foi selecionada manualmente para refletir o perfil cultural do NPC correspondente — uma voz masculina grave para o cavaleiro medieval, uma voz masculina regional para o Curupira, uma voz feminina suave para a gueixa e uma voz masculina dramática para o faraó. O uso de TTS neural foi uma decisão pragmática que viabilizou a produção de doze clipes de áudio (três por NPC: narrativa principal, SFX de interrupção e SFX de retomada) em tempo viável para o escopo acadêmico.

### 3.3. Divisão de Atividades

O projeto foi desenvolvido em equipe, com atribuições distribuídas conforme a especialidade de cada membro:

- **Leonardo Buratto de Assis**: organização geral, planejamento técnico, implementação dos sistemas de movimentação, interação e narração, padrão de wrapper prefab para NPCs, sistema de portais e integração final das cenas.
- **Jennifer Leonora Galina Vieira**: pesquisa sobre os folclores e culturas representadas, definição e revisão das narrativas apresentadas pelos NPCs, validação cultural dos roteiros gerados.
- **Leonardo Conti**: definição do estilo visual, curadoria e organização dos modelos 3D coletados, organização da documentação do projeto e suporte à pesquisa cultural.

## 4. Implementação

Esta seção descreve em detalhe os principais componentes do protótipo, com ênfase nas decisões arquiteturais que orientaram o código.

> **[FIGURA 2 — placeholder]** Diagrama geral da arquitetura do sistema, mostrando as três camadas: (i) Player (movimentação + câmera + interação por raycast); (ii) NPC (wrapper prefab + máquina de estados de narração + áudio espacial); (iii) Sistema de navegação (portais + spawn contextual). *Sugestão: pode ser feito em draw.io ou Lucidchart, como diagrama de blocos.*

### 4.1. Sistema de Movimentação e Câmera em Primeira Pessoa

O jogador é representado por um *prefab* contendo um `CharacterController` e dois scripts principais: `PlayerMovementCC_2` e `MouseLook360_2`.

O componente `PlayerMovementCC_2` processa a entrada WASD do teclado, aplica gravidade contínua, implementa salto via `Space` e suporta sprint via `Shift`. A movimentação é traduzida em chamadas a `CharacterController.Move`, o que garante interação correta com colisores do ambiente. Para evitar tremor vertical em superfícies planas, uma velocidade negativa residual é aplicada quando o jogador está em contato com o solo.

O componente `MouseLook360_2` implementa a câmera em primeira pessoa por meio de uma separação clara: a rotação horizontal (*yaw*) é aplicada ao GameObject raiz do Player, enquanto a rotação vertical (*pitch*) é aplicada apenas ao Transform local da câmera, com *clamp* de ±85 graus para evitar inversão. Essa separação garante que a movimentação WASD permaneça consistente com a orientação visual do usuário, e ao mesmo tempo desacopla a rotação vertical do corpo do personagem — exigência para uma futura integração com colisores não simétricos. Um *smoothing* opcional, baseado em `SmoothDampAngle`, suaviza a câmera em ambientes de baixa taxa de quadros.

A câmera é posicionada na altura aproximada da linha dos olhos (1,7 metros relativo à origem do *prefab*), refletindo uma escala realística de personagem humano.

### 4.2. Sistema de Interação por Raycasting

O componente `PlayerRayInteractor`, anexado à câmera do jogador, realiza, a cada frame, um *raycast* a partir da posição da câmera na direção *forward*, com distância máxima parametrizável (4 metros no protótipo atual), filtrado pela camada `Interactable`. Quando o raycast atinge um objeto cujo componente raiz implementa a interface `IInteractable`, o *crosshair* (renderizado em *Screen Space – Overlay*) muda de cor de branco para vermelho, sinalizando ao usuário que a interação está disponível. Ao pressionar a tecla **E**, o método `Interact()` do objeto-alvo é invocado.

> **[FIGURA 3 — placeholder]** Screenshot in-game mostrando o crosshair em estado normal (branco) ao apontar para o ambiente e em estado de destaque (vermelho) ao mirar em um NPC. *Sugestão: duas capturas lado a lado.*

Essa abordagem oferece três vantagens. Primeiro, permite que diferentes tipos de objetos — NPCs, portais entre cenas e, potencialmente, itens colecionáveis ou *triggers* ambientais — compartilhem a mesma camada de descoberta, sem duplicação de lógica de detecção. Segundo, desacopla totalmente a interação da física de colisão de proximidade, eliminando dependência de *triggers* e *OnTriggerEnter*. Terceiro, oferece *feedback* visual claro: o crosshair como indicador de interação possível é uma convenção amplamente reconhecida pelo público de jogos em primeira pessoa.

A interface `IInteractable` é definida da seguinte forma:

```csharp
public interface IInteractable
{
    void Interact();
}
```

Sua simplicidade é intencional: o contrato mínimo permite que qualquer objeto que implemente essa interface seja descoberto e ativado pelo sistema, sem assumir nada sobre o que a interação faz internamente.

### 4.3. Sistema de Narração com Máquina de Estados

O componente `NpcStory` modela o comportamento de narração por meio de uma máquina de estados explícita com quatro estados:

- **Idle**: o NPC está pronto para iniciar a história a partir do começo.
- **PlayingStory**: a narrativa principal está sendo reproduzida; uma nova interação irá pausá-la.
- **Paused**: a história foi pausada e aguarda comando para retomar a partir do mesmo ponto.
- **PlayingSfx**: um efeito sonoro de reação (interrupção ou retomada) está sendo executado, e novas interações são ignoradas para evitar sobreposição.

A transição entre os estados é representada na Figura 4.

> **[FIGURA 4 — placeholder]** Diagrama da máquina de estados do `NpcStory`, mostrando: `Idle` → (interação) → `PlayingStory` → (interação) → `PlayingSfx` (interrupt) → `Paused` → (interação) → `PlayingSfx` (resume) → `PlayingStory` → (fim natural) → `Idle`. *Sugestão: pode ser desenhado em draw.io como diagrama UML de máquina de estados.*

Cada NPC possui três clipes de áudio distintos: a narrativa principal (`storyClip`), um SFX curto de interrupção (por exemplo, "Alguma dúvida?", proferido pelo personagem) e um SFX de retomada (por exemplo, "Continuando..."). Esses SFX foram concebidos como reações naturais do narrador às ações do usuário, reforçando a impressão de interlocução genuína em vez de mera reprodução automática.

A implementação utiliza dois `AudioSources` distintos, criados em tempo de execução: um dedicado à narrativa principal — para permitir `Pause()` e `UnPause()` sem perder a posição temporal — e outro para os SFX, evitando que a reprodução de um SFX interfira no tempo de reprodução da história. Essa separação é uma decisão arquitetural deliberada: a tentativa inicial de utilizar um único `AudioSource` levou a comportamentos indesejados nos quais o SFX sobrescrevia o estado de reprodução da narrativa principal.

O áudio é configurado como espacial (`spatialBlend = 1`), de modo que a percepção sonora do usuário é coerente com a posição do NPC no espaço tridimensional — afastando-se do NPC, o som atenua de forma realística.

### 4.4. Padrão Wrapper Prefab para Modelos GLB

A engine Unity impõe restrições sobre a edição direta de hierarquias importadas a partir de arquivos GLB: não é possível adicionar componentes diretamente ao GameObject raiz do modelo importado, pois esse raiz é considerado imutável pelo importador. Para contornar essa limitação sem comprometer a integração com o pipeline do Blender, adotou-se o padrão **wrapper prefab**.

Nesse padrão, um GameObject vazio na cena assume o papel de raiz funcional, carregando os componentes lógicos (`NpcStory`, `BoxCollider`, `AudioSource` e a *layer* `Interactable`), enquanto o GLB é incorporado como filho desse wrapper. Visualmente, a hierarquia é a seguinte:

```
NPC_Root (Layer: Interactable, BoxCollider, NpcStory, AudioSource)
└── GLB_Model (Knight, Pharaoh, Geisha ou Curupira)
```

> **[FIGURA 5 — placeholder]** Screenshot da janela Hierarchy do Unity Editor, mostrando a estrutura wrapper prefab aplicada a um dos NPCs. Ao lado, o painel Inspector destacando os componentes anexados ao GameObject raiz. *Sugestão: capturar com a Scene_Europe aberta e o Knight selecionado.*

Esse padrão isola a configuração lógica do conteúdo visual, permitindo que a substituição de um modelo — por exemplo, durante iteração artística ou migração para FBX — não exija reconfiguração dos componentes funcionais. A *layer* `Interactable` aplicada ao wrapper garante que o raycast do `PlayerRayInteractor` detecte o NPC mesmo quando o usuário aponta diretamente para a malha do modelo, graças à propagação de hits para o componente raiz via `GetComponentInParent<IInteractable>()`.

### 4.5. Navegação entre Cenas via Portais Interativos

A navegação entre as quatro cenas-continente e a cena central (*Lobby*) é mediada por portais físicos no ambiente. Cada portal é um GameObject contendo um `BoxCollider` na camada `Interactable` e o componente `ScenePortal`, que implementa `IInteractable`. Ao ser ativado pelo jogador, o portal registra em uma variável estática a cena de origem e invoca `SceneManager.LoadScene` com o nome da cena de destino.

A cena central, *Scene_Lobby*, contém quatro portais — um para cada continente — dispostos em torno do ponto de spawn inicial do jogador. Cada portal é identificado por uma etiqueta tridimensional (*TextMesh*) posicionada acima dele, com comportamento de *billboard*: a etiqueta gira em torno do eixo vertical para sempre apresentar sua face frontal à câmera do jogador, garantindo legibilidade independentemente do ângulo de aproximação.

> **[FIGURA 6 — placeholder]** Vista aérea (top-down) da cena Lobby, mostrando a disposição em cruz dos quatro portais ao redor do ponto de spawn central, com legendas indicando o destino de cada um. *Sugestão: capturar com a Scene View do Unity, em projeção ortogonal vista de cima.*

Em cada cena de continente, um portal de retorno permite ao usuário voltar ao Lobby. Para preservar a continuidade espacial, um componente `LobbySpawnController`, executando em `Awake()` com prioridade explícita (`[DefaultExecutionOrder(-1000)]`), reposiciona o jogador em frente ao portal correspondente à cena de origem quando o Lobby é carregado a partir de um continente. Assim, ao voltar do Brasil, o jogador aparece no Lobby diretamente em frente ao portal da América; ao voltar do Egito, em frente ao portal da África; e assim por diante.

Adicionalmente, um componente `BootstrapToLobby`, presente em cada cena de continente, verifica em `Awake()` se a cena foi acessada via portal ou diretamente (por exemplo, ao executar a cena diretamente no Editor durante o desenvolvimento). No segundo caso, o componente redireciona automaticamente o jogador ao Lobby, garantindo que o fluxo de navegação seja sempre coerente: o usuário sempre inicia sua jornada no ponto central, podendo escolher livremente a ordem de exploração.

## 5. Resultados

### 5.1. Estado Atual do Protótipo

O protótipo encontra-se funcional em ambiente desktop, executável a partir de um *build standalone* para Windows. O fluxo completo de uso pode ser descrito da seguinte forma:

1. O usuário inicia a aplicação e é apresentado à cena *Lobby*, contendo quatro portais identificados por etiquetas tridimensionais (Europa, Ásia, África, América);
2. Ao mirar em um portal, o crosshair muda de cor, indicando que a interação está disponível. Ao pressionar **E**, a cena correspondente é carregada;
3. Na cena do continente, o usuário pode se movimentar livremente, aproximar-se do NPC temático e interpelá-lo via **E**;
4. O NPC inicia sua narrativa, que pode ser interrompida e retomada por novas interações;
5. Um portal de retorno permite voltar ao Lobby, com o jogador sendo reposicionado em frente ao portal correspondente.

### 5.2. NPCs Implementados

Os quatro NPCs implementados, suas localizações e os temas de suas narrativas são apresentados na Tabela 1.

> **[TABELA 1 — placeholder]** Inserir como tabela formatada no Word com colunas: Cena, NPC, Tema da narrativa.
>
> | Cena | NPC | Tema da narrativa |
> |------|-----|-------------------|
> | Scene_Europe | Knight | Juramento e código de honra dos cavaleiros medievais |
> | Scene_America | Curupira | O guardião da floresta no folclore brasileiro |
> | Scene_Asia | Geisha | O conceito estético japonês *mono no aware* |
> | Scene_Africa | Pharaoh | A pesagem do coração no julgamento egípcio |

> **[FIGURA 7 — placeholder]** Painel com quatro screenshots (2x2) mostrando cada NPC em sua cena respectiva: Knight, Curupira, Geisha e Pharaoh. *Sugestão: capturar in-game de uma distância média, com o NPC centralizado e o ambiente visível ao fundo.*

Cada narrativa tem entre noventa segundos e dois minutos de duração, equivalentes a aproximadamente cento e oitenta a duzentas e cinquenta palavras. Os roteiros foram redigidos em primeira pessoa, em registro coerente com o personagem (formal e solene para o cavaleiro e o faraó, contemplativo para a gueixa, regional para o Curupira), e produzidos como áudio por meio da síntese neural de voz da plataforma ElevenLabs.

### 5.3. Validação Funcional

Foram realizados testes manuais de *playthrough* completo para validar (i) a coerência dos estados da máquina de narração — verificando que as quatro transições principais (iniciar, interromper, retomar e finalizar) funcionam sem falhas; (ii) o funcionamento dos portais e do spawn contextual; (iii) a estabilidade do sistema de áudio em transições de cena, garantindo que reproduções residuais não se sobrepõem à carga da próxima cena; e (iv) o comportamento do sistema de interação em diferentes distâncias e ângulos de aproximação ao NPC. Não foram observadas falhas de regressão no escopo testado.

## 6. Considerações Finais e Trabalhos Futuros

Este trabalho apresentou o desenvolvimento do projeto *Explorando Histórias*, descrevendo as decisões arquiteturais que sustentam seu funcionamento atual e os resultados obtidos no protótipo desktop. A combinação de uma máquina de estados explícita para narração, *raycasting* para interação universal por meio da interface `IInteractable`, o padrão *wrapper prefab* para integração de modelos GLB e a navegação por portais com spawn contextual demonstrou-se eficaz para manter o código organizado, o conteúdo facilmente substituível e o fluxo de uso coerente.

Em termos pedagógicos, a experiência cumpre seu propósito mínimo: o usuário é exposto a quatro narrativas culturais distintas em uma única sessão, podendo controlar o ritmo de cada exposição. Em termos arquiteturais, o projeto estabelece uma base que pode ser expandida sem reescrita estrutural, graças ao desacoplamento entre lógica e conteúdo proporcionado pelos prefabs reutilizáveis.

Como evolução natural, três frentes de trabalho futuro são identificadas:

- **Camada imersiva em RV**: integração com o Unity XR Toolkit e adaptação dos controles para entrada de headset e *controllers*, mantendo a base arquitetural já estabelecida. A camada visual e a máquina de estados de narração não exigem mudanças; apenas o sistema de entrada do jogador precisa ser reescrito para o paradigma de RV;
- **Polimento dos ambientes**: substituição dos cenários minimalistas atuais por ambientes ricos em props culturais (castelo medieval, floresta tropical, templo japonês, vale de pirâmides), com iluminação dedicada por continente e *skybox* temático. Isso amplia o valor cultural percebido pelo usuário sem alterar a arquitetura;
- **Expansão narrativa**: inclusão de múltiplas histórias por NPC, com escolha temática pelo usuário, ampliando a profundidade educacional da experiência. Como a máquina de estados é parametrizada pelos clipes de áudio injetados via Inspector, essa extensão é trivial em termos de código.

A arquitetura modular adotada — com prefabs reutilizáveis, máquina de estados desacoplada da apresentação visual e navegação baseada em interface comum — foi pensada precisamente para suportar essas extensões. O projeto, em seu estado atual, oferece um *baseline* funcional e auditável a partir do qual essas evoluções podem ser conduzidas de forma incremental.

---

## Referências

Burdea, G. C. e Coiffet, P. (2003). *Virtual Reality Technology*. 2ª ed. Wiley-IEEE Press, Hoboken, NJ.

Freina, L. e Ott, M. (2015). A literature review on immersive virtual reality in education: state of the art and perspectives. In *Proceedings of the International Scientific Conference eLearning and Software for Education*, volume 1, pages 133–141.

Murray, J. H. (1997). *Hamlet on the Holodeck: The Future of Narrative in Cyberspace*. Free Press, New York.

Nipo, D. T., Rodrigues, R. L., França, R., Nascimento, J. B. e Pereira, M. (2023). Robo-Think: um jogo de realidade virtual para o ensino de habilidades de pensamento computacional. In *Anais Estendidos do XXII Simpósio Brasileiro de Jogos e Entretenimento Digital (SBGames)*, pages 915–924, Rio Grande/RS. SBC. DOI: 10.5753/sbgames_estendido.2023.232694.

Salen, K. e Zimmerman, E. (2003). *Rules of Play: Game Design Fundamentals*. MIT Press, Cambridge, MA.

Schell, J. (2008). *The Art of Game Design: A Book of Lenses*. CRC Press, Boca Raton, FL.

Unity Technologies (2022). *Unity 2022.3 LTS Documentation*. Disponível em: https://docs.unity3d.com/2022.3/Documentation/Manual/. Acesso em: maio de 2026.
