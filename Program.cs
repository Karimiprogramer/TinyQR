using Adw;

var application = Application.New("ir.thedimension.tinyqr", Gio.ApplicationFlags.FlagsNone);
application.OnActivate += (sender, args) =>
{
    MainWindow window = new MainWindow();
    window.Application = application;
    window.Title = "Tiny QR";
    window.SetDefaultSize(360, 600);
    window.SetResizable(true);
    window.Show();
};
return application.RunWithSynchronizationContext(args);
