using UnityEngine;

public class SceneContentHelperTask1 : SceneTaskHelper
{
    [SerializeField]
    private InstructionsPanel instructions;

    public override void Show()
    {
        base.Show();
        instructions.Show();
    }

    public override void Hide()
    {
        base.Hide();
        instructions.Hide();
    }

    public override void HideInstantly()
    {
        base.HideInstantly();
        instructions.HideInstantly();
    }
}