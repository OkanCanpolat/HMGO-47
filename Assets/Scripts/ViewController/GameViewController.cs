
public class GameViewController : ViewController
{
    private InputManager inputManager;

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        inputManager.Activate();
        ViewManager.NotifyEnterGame();
    }

    public override void OnLeave()
    {
        inputManager.Deactivate();
        base.OnLeave();
    }
}
