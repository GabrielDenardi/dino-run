# Dino Run

Projeto de um endless runner em Unity inspirado no jogo do dinossauro do Chrome.

## Sobre

Este repositório contém um protótipo jogável feito em Unity 6, com:

- personagem dinossauro com animações de corrida, pulo e morte;
- geração procedural de obstáculos;
- aumento progressivo de velocidade;
- sistema de pontuação e melhor pontuação;
- reinício rápido da partida.

## Requisitos

- Unity `6000.4.7f1`

## Como abrir

1. Abra o Unity Hub.
2. Adicione este projeto localmente.
3. Abra o projeto com a versão `6000.4.7f1`.
4. Carregue a cena `Assets/Scenes/DinoRunner.unity`.
5. Execute no Play Mode.

## Controles

- `Espaço`, `W` ou `Seta para cima`: pular
- `A` / `D` ou `Seta para esquerda` / `Seta para direita`: mover horizontalmente
- `R`: reiniciar após game over

## Estrutura principal

- `Assets/Scripts/`: lógica principal do jogo
- `Assets/Scenes/`: cena jogável
- `Assets/Resources/`: sprites e recursos carregados em runtime
- `ProjectSettings/`: configurações do projeto Unity

## Observações

Arquivos gerados pelo editor, como `Library/`, `Temp/`, `Logs/` e `UserSettings/`, estão ignorados no versionamento.
