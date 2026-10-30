namespace SuBilet.Models.ViewModels;

// Firma listesi ve silme sayfasında: firma + bağlı kayıt sayıları (tek sorguda hesaplanır)
public record FirmaSatiri(Firma Firma, int OtobusSayisi, int SeferSayisi, int YetkiliSayisi);
