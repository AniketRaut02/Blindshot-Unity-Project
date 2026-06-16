namespace SciFiGame.Interaction
{
    public interface IInteractable
    {
        // Called when the player presses the interact button
        void Interact();

        // Optional: Useful if you want to show a UI prompt like "Press E to Steal"
        string GetInteractionPrompt();
    }
}