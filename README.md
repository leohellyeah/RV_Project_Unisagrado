# Projeto_RV

Projeto acadêmico de **Realidade Virtual** desenvolvido em **Unity 2022.3 LTS**, com foco educacional e narrativo. O jogador assume o papel de um explorador que visita ambientes inspirados em diferentes continentes e interage com NPCs que contam histórias culturais e folclóricas.

> **Status:** em desenvolvimento estrutural. Sistemas principais funcionando; ambientes e polimento ainda em andamento.

---

## Conceito

Uma experiência **exploratória e narrativa**, em que o jogador funciona como observador. A interação com o mundo é deliberadamente simples: aproximar-se de um NPC, mirar e ouvir uma história. Cada continente é uma cena independente com a mesma base mecânica e um NPC temático diferente.

### Continentes / Cenas

| Cena | Tema |
|------|------|
| `Scene_Europe`  | Europa medieval (cavaleiro) |
| `Scene_America` | Brasil |
| `Scene_Asia`    | Japão |
| `Scene_Africa`  | Egito |

Todas as cenas compartilham o mesmo Player, UI e sistema de interação — o que muda é o **ambiente, o modelo do NPC e os áudios**.

---

## Stack

- **Engine:** Unity 2022.3.62f3 (Built-in Render Pipeline)
- **Modelagem:** Blender (modelos GLB; futura migração para FBX com rig, UV e textura)
- **Áudio:** narração do NPC + SFX de interrupção/retomada
- **Plataforma:** Desktop first; camada VR planejada como evolução futura

---

## Sistemas Implementados

### Player
Movimento (andar, pular), mouse look (yaw no Player, pitch na câmera). Empacotado como `Player.prefab` com a hierarquia:

```
Player
├── Head
│   └── Player_Cam (Main Camera)
└── Player_Body
```

### UI / HUD
Canvas em Screen Space – Overlay com **crosshair central** (sprite 64x64). Empacotado como `UI_HUD.prefab`.

### Interação
Raycast a partir da `Player_Cam`, mira pelo crosshair, tecla **E** para interagir. Quando o jogador aponta para um objeto na layer `Interactable`, o crosshair muda de cor.

### NPC + Storytelling
Padrão **Wrapper Prefab** para evitar conflitos com importação GLB:

```
NPC_Model (root, layer Interactable, Collider, NpcStory)
├── ModelRoot      (encaixe do GLB)
└── SpeakerPoint   (origem do áudio)
```

O script `NpcStory` é uma máquina de estados:

- **Idle** → primeira interação inicia a história
- **PlayingStory** → nova interação pausa e toca SFX (*"Alguma dúvida?"*)
- **Paused** → próxima interação toca SFX de retomada (*"Continuando..."*) e retoma do ponto pausado
- **PlayingSfx** → reprodução dos SFX de reação
- Ao terminar, a próxima interação reinicia a história

---

## Estrutura de Prefabs

| Prefab | Papel |
|--------|-------|
| `Player.prefab`     | Comportamento de movimentação e câmera |
| `UI_HUD.prefab`     | Canvas + crosshair |
| `NPC_Model.prefab`  | Wrapper do NPC + storytelling |
| `GameManager.prefab` *(opcional)* | Coordenação de cena |

**Regra geral:** prefab carrega comportamento e estrutura; a cena fornece conteúdo (modelo, áudio, ambiente).

---

## Status

**Pronto**
- Player, mouse look e crosshair funcionais
- Interação com NPC e storytelling com pausa/SFX
- Importação de GLB funcionando
- Estrutura de prefabs e cenas-base definidas

**Em andamento**
- Montagem mínima dos ambientes por continente
- Ajuste fino de colliders e escala dos NPCs
- Props narrativos no ambiente
- Triggers ambientais (saudações ao entrar em áreas)
- Polimento visual no Blender

---

## Filosofia

- Projeto acadêmico, **escopo controlado**
- **Funcionar antes de embelezar** — estrutura sólida antes de decoração
- **Desktop first**, VR como camada futura
