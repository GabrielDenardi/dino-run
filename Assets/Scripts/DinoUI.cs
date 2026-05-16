using UnityEngine;
using UnityEngine.UI;

public class DinoUI : MonoBehaviour
{
    private Text scoreText;
    private Text bestText;
    private Text messageText;

    public void Bind(Text scoreText, Text bestText, Text messageText)
    {
        this.scoreText = scoreText;
        this.bestText = bestText;
        this.messageText = messageText;
    }

    private void Update()
    {
        var manager = DinoGameManager.Instance;
        if (manager == null || scoreText == null || bestText == null || messageText == null)
        {
            return;
        }

        scoreText.text = $"SCORE {manager.Score:0000}";
        bestText.text = $"BEST {manager.BestScore:0000}";
        messageText.text = manager.IsGameOver
            ? "GAME OVER  |  PRESSIONE R PARA REINICIAR"
            : "PULAR: ESPACO / W / SETA PARA CIMA";
    }
}
