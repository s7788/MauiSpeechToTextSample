﻿using MauiSpeechToTextSample.Platforms;
using Microsoft.Extensions.Logging;

namespace MauiSpeechToTextSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
#if ANDROID
        builder.Services.AddSingleton<ISpeechToText, SpeechToTextImplementation>();
#endif

        builder.Services.AddTransient<MainPage>();
        //builder.Services.AddSingleton<ISpeechToText, SpeechToTextImplementation>();

		return builder.Build();
	}
}
