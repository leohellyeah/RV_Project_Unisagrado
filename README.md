# Projeto_RV

Projeto acadêmico de **Realidade Virtual** desenvolvido em **Unity 2022.3 LTS**, com foco educacional e narrativo. O jogador assume o papel de um explorador que visita ambientes inspirados em diferentes continentes e interage com NPCs que contam histórias culturais e folclóricas.

> **🚧 Early Build VR (entrega P2 — 2026-05).** Esta versão **já inclui suporte mínimo a VR** (câmera *head-tracked*, locomoção por analógico, interação por *gaze* + gatilho) e um **APK** para **Meta Quest**. Como **não há hardware disponível para testar/refinar**, **bugs e crashes são esperados** no headset — trate como *early build*. O build **desktop original (WASD + mouse) segue funcional** como *fallback*.

---

## Conceito

Uma experiência **exploratória e narrativa**, em que o jogador funciona como observador. A interação com o mundo é deliberadamente simples: aproximar-se de um NPC, mirar e ouvir uma história. Cada continente é uma cena independente com a mesma base mecânica e um NPC temático diferente.

### Continentes / Cenas

| Cena | Tema |
|------|------|
| `Scene_Europe`  | Europa medieval|
| `Scene_America` | Brasil |
| `Scene_Asia`    | Japão |
| `Scene_Africa`  | Egito |

Todas as cenas compartilham o mesmo Player, UI e sistema de interação — o que muda é o **ambiente, o modelo do NPC e os áudios**.

---

## Stack

- **Engine:** Unity 2022.3.62f3 (Built-in Render Pipeline)
- **Modelagem:** Blender (modelos GLB; futura migração para FBX com rig, UV e textura)
- **Áudio:** narração do NPC + SFX de interrupção/retomada
- **Plataforma:** Desktop (WASD + mouse) **+ VR mínimo** via OpenXR / Meta Quest (*early build*); testável no Editor pelo XR Device Simulator sem necessidade de headset

---

## Sistemas Implementados

### Player
Movimento (andar, pular, sprint), mouse look (yaw na raiz, pitch na câmera). Estrutura enxuta após refator:

```
Player (CharacterController, PlayerMovementCC_2, MouseLook360_2)
└── Main Camera (PlayerRayInteractor, AudioListener, Crosshair UI no HUD)
```

No modo VR, esse rig é **neutralizado** (mantido como fallback) e um `XR Origin (VR)` toma o lugar — câmera *head-tracked* + analógico (locomoção) + *gaze* (interação).

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

**Pronto (P2 — 2026-05)**
- Player + mouse look + crosshair + interação por *raycast*
- Storytelling com pausa/SFX e máquina de estados (`NpcStory`)
- 4 cenas-continente + `Scene_Lobby` central com 4 portais e *spawn* contextual no retorno
- NPCs com áudios narrados próprios (Knight, Curupira, Geisha, Pharaoh) — falloff Linear (7m / 40m)
- Paleta de cores + *props* placeholder por cena via `SceneDresser` (Editor)
- **VR mínimo** (OpenXR + XR Origin head-tracked + `VrGazeInteractor` + `VrLocomotion`) — *early build*
- **Build APK** para Meta Quest configurado via `VrApkBuilder` (IL2CPP / ARM64 / Vulkan / Single Pass Instanced)
- Artigo SBC (`Project_essay/`) com pipeline `md` → `docx` → `pdf` automatizado

**Conhecido / em aberto**
- Sem texturas/assets de ambiente "reais" — só *props* placeholder por primitivas (cubos, esferas, pirâmide procedural)
- Em VR: `LobbySpawnController` não reposiciona o XR Origin (você reaparece sempre no *spawn* fixo da cena)
- Em VR: HUD em *screen-space* (crosshair) fica estranho no headset
- Refino do VR depende de teste em hardware real

---

## Como rodar

### Desktop (Editor)
1. Abra `Assets/Cenas/Scene_Lobby.unity`.
2. **Play.** WASD para mover, mouse para olhar, **E** para interagir, **Shift** para correr, **Space** para pular.

### VR no Editor (sem headset, via XR Device Simulator)
1. `Tools > VR Project > Setup VR Rig (current scene)` (ou *in ALL Scenes* para aplicar nas 5).
2. Arraste o prefab `XR Device Simulator` de `Assets/Samples/XR Interaction Toolkit/.../` para a cena.
3. **Play.** Mouse = cabeça, WASD = mover, botão esquerdo/direito do mouse = gatilho dos controles esquerdo/direito (interage com NPC/portal).

### VR no Meta Quest (early build, APK)
1. `Tools > VR Project > Build VR APK (Quest)` — gera `Builds/Android/ProjetoRV-VR.apk`.
2. Habilite modo desenvolvedor no Quest (via Meta Quest Developer Hub / app Meta no celular).
3. Sideload do APK por [SideQuest](https://sidequestvr.com/) ou `adb install`.

### Utilitários de Editor (menu `Tools > VR Project`)
| Menu | O que faz |
|------|-----------|
| `Dress ALL Scenes` / `Dress LOBBY` | Aplica paleta de cores + *props* placeholder |
| `Remove Dressing (current)` | Reverte o *dressing* da cena aberta |
| `Setup VR Rig (current scene)` / `... in ALL Scenes` | Monta o `XR Origin (VR)` |
| `Remove VR Rig (current)` | Reverte o rig VR e reativa o player desktop |
| `Configure Project for Quest` / `Build VR APK (Quest)` | Configura *Player Settings* + builda APK |

---

## Filosofia

- Projeto acadêmico, **escopo controlado**
- **Funcionar antes de embelezar** — estrutura sólida antes de decoração
- Desktop como base sólida; **VR adicionado em paralelo** (*early build*) sem quebrar o desktop

---

## Créditos

**Equipe (Unisagrado):** Leonardo Buratto de Assis · Jennifer Leonora Galina Vieira · Leonardo Conti

**Disciplina:** Realidade Virtual — Centro Universitário Sagrado Coração (Unisagrado), 2026/1
