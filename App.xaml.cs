// Authorship note:
// This file follows standard .NET MAUI application startup structure.
// Microsoft documentation and MAUI setup guidance were used for application initialization
// and window creation patterns.
// The use of AppShell as the main application shell in NutriAI was chosen by the author.

namespace NutriAI;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }
}