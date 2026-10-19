using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace GDImproved
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();
            builder.ConfigureFonts(RegistrarFuentes);
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
        static void RegistrarFuentes(IFontCollection fonts)
        {
            fonts.AddFont("FuenteGoodMoov.otf", "FuenteGoodMoov");
        }
    }
}
