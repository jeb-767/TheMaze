using UnityEngine;

public class Cowboy : NPC_Talk
{
    [Header("Quest Settings")]
    public string greetingText = "There are relics waiting to be found. Want to see available quests?";

    public override void Interact(Player player)
    {
        // Ejecuta la lógica base (guardar al jugador, reproducir sonido, etc.)
        base.Interact(player);

        // Abre el panel de diálogo con el texto de bienvenida usando el método que ya tienes[cite: 2]
        player.canvas.OpenDialoguePanel(greetingText);
    }

    public override void OnYes()
    {
        // 1. Cerramos el panel de diálogo normal[cite: 2]
        player.canvas.CloseDialoguePanel();

        // 2. Le decimos al Canvas del jugador que abra el menú de misiones.
        // (Tendrás que crear este método OpenQuestPanel() en el script de tu Canvas)
        player.canvas.OpenQuestPanel(); 
    }

    public override void OnNo()
    {
        // Si el jugador dice que no, simplemente cerramos la conversación
        player.canvas.CloseDialoguePanel();
    }
    
    void Update()
    {
        // Actualizamos las animaciones dependiendo de si el jugador está hablando con nosotros
        if (player != null && player.canvas != null)
        {
            // Asumiendo que DialoguePanel es tu Talk_Panel[cite: 2]
            UpdateAnimations(player.canvas.DialoguePanel); 
        }
    }
}