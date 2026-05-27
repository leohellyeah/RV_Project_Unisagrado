# Knight — Scene_Europe

**Continente:** Europa medieval
**Personagem:** Cavaleiro de uma ordem
**Tom:** solene, voz grave, dicção formal
**TTS sugerido:** voz masculina grave pt-BR (ex: Azure `pt-BR-AntonioNeural`)
**Pasta de áudio:** `Assets/Audio/Voz/Knight/`

---

## Story (`Knight_Story.mp3` — substituir `Interact_SFX_PH.mp3`)

> Detenha-se, viajante. Vejo em teus olhos a curiosidade dos que nada sabem da Ordem.
>
> Eu sou um cavaleiro do reino, e meu juramento é mais antigo que esta armadura. Quando completei dezesseis invernos, ajoelhei-me diante do meu senhor, com a espada apoiada no ombro, e jurei os cinco votos: proteger os fracos, honrar a palavra dada, jamais recuar diante da injustiça, servir ao rei e zelar pela fé.
>
> Lembro-me da minha primeira batalha. O céu estava cinzento, a terra encharcada de orvalho. Éramos cinquenta homens contra duzentos. Quando o conde caiu ferido, peguei seu estandarte do chão e ergui-o bem alto, para que todos os soldados vissem que ainda havia quem lutasse.
>
> Vencemos naquele dia. Mas não foi pela força das armas, viajante. Foi porque um cavaleiro não jura à sua espada, jura ao que ela protege. E enquanto houver alguém para proteger, esta armadura jamais será deposta.

## Interrupt (`Interrupt.mp3` — já existe, pode substituir)

> Dize, viajante. Tens alguma pergunta?

## Resume (`Resume.mp3` — já existe, pode substituir)

> Pois bem. Onde estávamos...

---

## Notas de produção

- Reverb leve (sala/caverna) ajuda no clima de castelo.
- Mono 44.1 kHz, `.mp3` ou `.wav`.
- No `NpcStory` do prefab da cena: arrastar `Knight_Story.mp3` em **Story Clip**, `Interrupt.mp3` em **Interrupt Sfx**, `Resume.mp3` em **Resume Sfx**.
