using System.Globalization;
using Google.Cloud.Translation.V2;

namespace MauiSpeechToTextSample;

public partial class MainPage : ContentPage
{
    private ISpeechToText speechToText;
    private CancellationTokenSource tokenSource = new CancellationTokenSource();

    public Command ListenCommand { get; set; }
    public Command ListenCancelCommand { get; set; }
    public string RecognitionText { get; set; }
    public string TranslatedText { get; set; }
    private string _selectedLanguage;
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value)
            {
                _selectedLanguage = value;
                OnPropertyChanged();
            }
        }
    }
    private const string GOOGLE_API_KEY = @"";
    public MainPage(ISpeechToText speechToText)
	{
		InitializeComponent();

        this.speechToText = speechToText;

        ListenCommand = new Command(Listen);
		ListenCancelCommand = new Command(ListenCancel);
        BindingContext = this;
    }

    private async void Listen()
    {
        var isAuthorized = await speechToText.RequestPermissions();
        if (isAuthorized)
        {
            try
            {
                var culture = "en-us";
                culture = "zh-hant";
                RecognitionText = await speechToText.Listen(CultureInfo.GetCultureInfo(culture),
                    new Progress<string>(partialText =>
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        RecognitionText = partialText;
                    }
                    else
                    {
                        RecognitionText += partialText + " ";
                    }

                    OnPropertyChanged(nameof(RecognitionText));
                }), tokenSource.Token);
                // translated jp
                TranslatedText = await TranslateTextAsync(RecognitionText, "ja");
                OnPropertyChanged(nameof(TranslatedText));
                await SpeakTranslatedTextAsync(TranslatedText);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        else
        {
            await DisplayAlert("Permission Error", "No microphone access", "OK");
        }
    }

    private void ListenCancel()
    {
        tokenSource?.Cancel();
    }
    public async Task<string> TranslateTextAsync(string text, string targetLanguage)
    {
        var client = TranslationClient.CreateFromApiKey(GOOGLE_API_KEY);
        var response = await client.TranslateTextAsync(text, targetLanguage);
        return response.TranslatedText;
    }
    public async Task SpeakTranslatedTextAsync(string text)
    {
        try
        {
            var locales = await TextToSpeech.GetLocalesAsync();

            // jp Locale
            var japaneseLocale = locales.FirstOrDefault(l => l.Language == "ja");

            var speakOptions = new SpeechOptions()
            {
                Locale = japaneseLocale ?? throw new InvalidOperationException("Japanese locale not found.")
            };

            await TextToSpeech.SpeakAsync(text, speakOptions);
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            Console.WriteLine(fnsEx);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

public class LanguageToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == (string)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? parameter.ToString() : null;
    }
}

