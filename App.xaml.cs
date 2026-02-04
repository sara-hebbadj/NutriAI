namespace NutriAI;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;   // store injected shell
        MainPage = new AppShell();

    }

}

