#pragma warning disable CS8618
using GdkPixbuf;
using QRCoder;
using System;
using System.Threading.Tasks;

public class MainWindow : Adw.ApplicationWindow
{
    // UI Elements
    private Adw.HeaderBar headerBar;
    private Gtk.Button aboutButton;
    private Adw.ToastOverlay toastOverlay;
    private Gtk.Box mainBox;
    private Gtk.Box contentBox;
    private Adw.Clamp contentClamp;
    private Gtk.Picture qrPicture;
    private Gtk.Frame qrFrame;
    private Gtk.Entry textEntry;
    private Gtk.Box inputBox;
    private Gtk.Label inputLabel;
    private Gtk.Button generateButton;

    // QR Code settings
    private const int QR_DISPLAY_SIZE = 250;
    private const int CONTENT_MARGIN = 32;

    protected override void Initialize()
    {
        InitializeWindow();
        CreateHeaderBar();
        CreateMainLayout();
        SetupEventHandlers();

    }

    private void InitializeWindow()
    {
        SetTitle("TinyQR");
        SetDefaultSize(420, 550);
    }

    private void CreateHeaderBar()
    {
        headerBar = new Adw.HeaderBar();
        headerBar.SetTitleWidget(Adw.WindowTitle.New("TinyQR", "QR Code Generator"));
        headerBar.AddCssClass("flat");

        aboutButton = Gtk.Button.New();
        aboutButton.SetLabel("About");
        headerBar.PackStart(aboutButton);
    }

    private void CreateMainLayout()
    {
        // Toast overlay for notifications
        toastOverlay = Adw.ToastOverlay.New();

        // Main container
        mainBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        mainBox.Append(headerBar);

        // Content area 
        contentBox = Gtk.Box.New(Gtk.Orientation.Vertical, 24);
        contentBox.SetMarginTop(CONTENT_MARGIN);
        contentBox.SetMarginBottom(CONTENT_MARGIN);
        contentBox.SetMarginStart(CONTENT_MARGIN);
        contentBox.SetMarginEnd(CONTENT_MARGIN);
        contentBox.SetVexpand(true);

        //responsive width behavior
        contentClamp = Adw.Clamp.New();
        contentClamp.SetMaximumSize(600);
        contentClamp.SetTighteningThreshold(400);
        contentClamp.SetChild(contentBox);

        toastOverlay.SetChild(mainBox);
        mainBox.Append(contentClamp);

        // QR code display with frame and shadow
        qrFrame = Gtk.Frame.New(null);
        qrFrame.SetHalign(Gtk.Align.Center);
        qrFrame.SetValign(Gtk.Align.Center);
        qrFrame.SetMarginBottom(8);
        qrFrame.AddCssClass("card");

        qrPicture = Gtk.Picture.New();
        qrPicture.SetSizeRequest(QR_DISPLAY_SIZE, QR_DISPLAY_SIZE);
        qrPicture.SetHalign(Gtk.Align.Center);
        qrPicture.SetValign(Gtk.Align.Center);
        qrPicture.SetMarginTop(12);
        qrPicture.SetMarginBottom(12);
        qrPicture.SetMarginStart(12);
        qrPicture.SetMarginEnd(12);

        qrFrame.SetChild(qrPicture);
        contentBox.Append(qrFrame);

        // Create input area with label
        inputBox = Gtk.Box.New(Gtk.Orientation.Vertical, 8);
        inputBox.SetMarginTop(16);
        inputBox.SetMarginBottom(16);

        inputLabel = Gtk.Label.New("Enter URL or text to encode:");
        inputLabel.SetHalign(Gtk.Align.Start);
        inputLabel.AddCssClass("heading");
        inputBox.Append(inputLabel);

        // Text input field
        textEntry = Gtk.Entry.New();
        textEntry.SetHexpand(true);
        textEntry.SetPlaceholderText("Text or URL");
        inputBox.Append(textEntry);

        contentBox.Append(inputBox);

        // Generation button
        generateButton = Gtk.Button.New();
        generateButton.AddCssClass("suggested-action");
        generateButton.AddCssClass("pill");
        generateButton.SetHalign(Gtk.Align.Center);
        generateButton.SetLabel("Generate QR Code");
        contentBox.Append(generateButton);

        SetContent(toastOverlay);
    }

    private void SetupEventHandlers()
    {
        aboutButton.OnClicked += HandleAboutButtonClicked;
        generateButton.OnClicked += async (sender, args) => await HandleGenerateButtonClickedAsync(sender, args);
    }


    private void HandleAboutButtonClicked(Gtk.Button sender, EventArgs args)
    {
        var aboutWindow = Adw.AboutDialog.New();
        aboutWindow.ApplicationName = "TinyQR";
        aboutWindow.DeveloperName = "Mohammad Mahdi Karimi";
        aboutWindow.IssueUrl = "https://github.com/Karimiprogramer/TinyQR/issues";
        aboutWindow.SupportUrl = "https://github.com/Karimiprogramer/TinyQR";
        aboutWindow.Version = "2.0";
        aboutWindow.SetLicenseType(Gtk.License.Gpl30);
        aboutWindow.Present(this);
    }

    private async Task HandleGenerateButtonClickedAsync(object sender, EventArgs args)
    {
        string inputText = textEntry.GetText();

        if (!string.IsNullOrWhiteSpace(inputText))
        {
            await GenerateQRCodeAsync(inputText);
        }
        else
        {
            ShowEmptyInputToast();
        }
    }

    private async Task GenerateQRCodeAsync(string text)
    {
        SetGenerateButtonState(false, "Generating...");

        try
        {
            // Run the QR code generation on a background thread
            var pixbuf = await Task.Run(() => {
                var url = new PayloadGenerator.Url(text);
                var generator = new QRCodeGenerator();
                var qrCode = generator.CreateQrCode(url);

                // Create and scale the QR code
                var tempPixbuf = PixbufLoader.FromBytes(new PngByteQRCode(qrCode).GetGraphic(64));
                return tempPixbuf.ScaleSimple(QR_DISPLAY_SIZE, QR_DISPLAY_SIZE, GdkPixbuf.InterpType.Bilinear);
            });

            // Update UI on the main thread
            qrPicture.SetPixbuf(pixbuf);

            // Show success toast
            ShowToast("QR code generated successfully");
        }
        catch (Exception ex)
        {
            ShowToast("Error generating QR code: " + ex.Message);
        }
        finally
        {
            SetGenerateButtonState(true, "Generate QR Code");
        }
    }

    private void SetGenerateButtonState(bool isEnabled, string label)
    {
        generateButton.SetSensitive(isEnabled);
        generateButton.SetLabel(label);
    }

    private void ShowEmptyInputToast()
    {
        ShowToast("Please enter text to generate a QR code");
    }

    private void ShowToast(string message)
    {
        var toast = Adw.Toast.New(message);
        toast.SetTimeout(3);
        toastOverlay.AddToast(toast);
    }
}